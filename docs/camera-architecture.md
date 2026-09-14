# 相机模块架构设计（VisionBench）

> 本文档是相机模块重构的设计收敛结果，记录最终架构、关键决策、实现进度与待办。用于替代聊天记录，作为后续开发与协作的对照蓝本。
>
> 说明：文中「契约」是设计目标形态，未必与当前代码完全一致；「当前实现」标注实际落地状态。

---

## 1. 背景与目标

原相机模块已有一层 `ICameraDevice` 抽象 + `CameraFactory` 工厂 + `CameraConfigStore` 持久化，但存在以下缺口：

1. 没有「工位」概念，持久化是扁平的相机列表，业务无法按工位稳定拿到某台相机。
2. `CameraService` 是空壳，从未真正管理相机生命周期。
3. 设备状态缺少统一入口与变更通知，业务难监控。

**重构目标：**

- 引入「工位」：多工位，每工位绑一台相机；多相机通过多工位表达。
- 业务通过工位服务拿相机句柄，直接操作相机（控制/状态/参数/取帧）。
- 状态统一管理，支持监控与变更通知。
- 绑定关系持久化，支持设置页改绑定。

---

## 2. 最终架构（分层 + 依赖方向）

```
业务层：只依赖「ICameraStationService」+「ICameraDevice」，不碰具体厂商
   ↓ 依赖
Station 域：工位服务（装配/解析，唯一运行时权威）
   ↓ 复用
底层：ICameraDevice（唯一功能面）· ICameraFactory（造设备）
      ICameraManager（按品牌枚举）· ICameraConfigStore（落盘绑定）
```

```
枚举/绑定     StationEnum(MainCamera…)        +  CameraConfig.json(工位→相机)
装配/解析     ICameraStationService ── CameraStationService
唯一功能面     ICameraDevice                  (业务控制句柄，厂商无关)
帧交付        IImageProvider ← CameraProvider  (包住 device，推帧给管线)
建造/枚举     ICameraFactory                 ·  ICameraManagerRegistry(品牌→枚举器)
持久化        ICameraConfigStore              (唯一落盘方)
```

**依赖方向单向**：业务 → service/device → 底层设备/工厂/存储。不反向、不横向绕。

---

## 3. 核心模型

### 3.1 工位枚举（代码写死）

```csharp
// Vision/Enums/StationEnum.cs（namespace: Vision.Camera）
public enum StationEnum { MainCamera }   // 硬件扩展就加一项
```

- 工位清单是**编译期决策**（硬件结构固定），写死在枚举里。
- 每工位一台相机；多相机 = 多工位枚举项。
- 建议补一个 `GetDisplayName()` 扩展方法给界面用。

### 3.2 持久化模型

```csharp
public class CameraProfile {                     // 一台相机的绑定凭据
    CameraInfo Info;                            // 静态属性（类型/序列号/型号/接口）
    CameraParam Param;                          // 运行参数（曝光/增益/翻转/区域）
}

public class StationProfile {                    // 一个工位的绑定
    StationEnum StationName;
    CameraProfile Camera;                        // 未绑定为 null
}
```

### 3.3 状态模型

```csharp
public enum CameraStateEnum { Disconnected, Connected, Ready, Grabbing }
```

- 四态是「相位」，可被三信号唯一推导，不要混入 `Error`（`Error` 是叠加条件，无法由三信号推导；需要时单独用 `LastError`/`HasError` 挂在别处）。
- `StateChangedEventArgs { OldState, NewState }` 用于变更通知。

---

## 4. 关键决策与理由

| 决策 | 结论 | 理由 |
|------|------|------|
| 多工位 × 每工位一相机 | ✅ | 多相机 = 多工位枚举项，不在工位内部加集合 |
| 工位清单写死（enum） | ✅ | 硬件结构固定，清单是编译期决策；「哪台绑哪个工位」才进配置 |
| 业务控制句柄 | **业务握 `ICameraDevice`** | 设备是唯一功能面，握它就是握这台相机，无重复；厂商无关、可 mock |
| 工位对象抽象 | `Station` 用**具体类**，不抽 `IStation` | 单实现、无 mock/换实现需求，避免无消费者先抽象（§3 反模式） |
| 服务抽象 | `ICameraStationService` **加接口** | 被广泛注入的协作对象，mock 测试 / 将来离线模式需要替换 |
| 状态源 | **单一 `State` 枚举为权威**，三 bool 派生 | 避免双源漂移（见 §6） |
| 单写权威 | 配置存储=磁盘权威，工位服务=运行时权威 | 设置页「改→存→Reload」，杜绝两份互不同步的当前数据 |

---

## 5. 各层类型清单

### 5.1 Model 层

| 类型 | 文件 | 职责 |
|------|------|------|
| `StationEnum` | `Vision/Enums/StationEnum.cs` | 工位枚举（namespace 当前为 `Vision.Camera`） |
| `CameraProfile` | `Vision/Models/CameraProfile.cs` | 相机绑定凭据（Info+Param） |
| `StationProfile` | `Vision/Models/StationProfile.cs` | 工位绑定（StationName+Camera） |

### 5.2 设备契约层（唯一功能面）

| 类型 | 职责 | 状态 |
|------|------|------|
| `ICameraDevice` | 生命周期/采集/参数/状态/取帧统一契约 | 已加 `State` + `StateChanged` |
| `HikVisionCamera` | 海康实现 | 已实现状态迁移（见 §6 待办） |
| `ICameraFactory` / `CameraFactory` | 按 `CameraEnum` 造设备 | 已有 |
| `ICameraManager` / `HikVisionManager` | 按品牌枚举可用设备 | 已有 |

### 5.3 Station 域

| 类型 | 职责 | 状态 |
|------|------|------|
| `ICameraStationService` | 运行时单写权威接口 | 已建（接口） |
| `CameraStationService` | 实现：读配置→建工位→工厂造设备 | ⚠️ 空壳（未实现） |

契约目标：

```csharp
public interface ICameraStationService {
    ICameraDevice? GetCamera(StationEnum station);   // 未绑定返回 null
    IReadOnlyList<StationProfile> GetStations();      // 含未绑定工位
    void OpenCamera();    // 打开所有已绑定相机（懒加载后聚合）
    void CloseCamera();
    void Reload();        // 配置变更后重建运行时（见 §7）
}
```

### 5.4 持久化层

| 类型 | 职责 | 状态 |
|------|------|------|
| `ICameraConfigStore` | 工位粒度配置接口 | 已改 `LoadStations/SaveStations` |
| `CameraConfigStore` | JSON 实现 `{ stations: [...] }` | 已改（解析失败当前返回 null，建议改空列表） |

### 5.5 枚举调度 + 帧交付

| 类型 | 职责 | 状态 |
|------|------|------|
| `ICameraManagerRegistry` | `CameraEnum → ICameraManager` 分发表（给设置页「选类型→枚举」） | ⚠️ 未建 |
| `CameraProvider : IImageProvider` | 包 device，把采集转成 `FrameReady` 推给管线 | ⚠️ 空壳 |

---

## 6. 状态管理设计（重点）

### 6.1 单源原则

- **权威 = 单一 `State` 枚举**（`_state` 字段，只读暴露）。
- **三 bool 是派生态**，不再独立存字段：

```csharp
public CameraStateEnum State { get; }             // 只读，无 set

public bool IsConnected   => State is Connected or Ready or Grabbing;
public bool IsInitialized => State is Ready or Grabbing;
public bool IsGrabbing    => State == Grabbing;
```

- 所有状态迁移走集中式 `SetState(newState)`：值没变不发事件，变了才发 `StateChanged`。

### 6.2 迁移矩阵（线性，失败不前进）

| 位置 | 成功 → | 失败 → |
|------|--------|--------|
| 构造 | `Disconnected` | —— |
| `Open()` | `Connected` | 抛异常，停在 `Disconnected` |
| `Init()` | `Ready` | 抛异常，停在 `Connected` |
| `StartAcquisition()` | `Grabbing` | 抛异常，停在 `Ready` |
| `StopAcquisition()` | `Ready` | 抛异常，停在 `Grabbing` |
| `Close()` | `Disconnected` | —— |

### 6.3 当前实现遗留（待优化，非阻塞）

1. `Init()` 成功后当前写成了 `State = Connected`，**应为 `Ready`**（`IsInitialized=true` 与 `Connected` 自相矛盾）。
2. `HikVisionCamera` 目前 **`State` 枚举与三 bool 双源并存**，`State` 有公开 setter、`Close()` 未重置 `IsInitialized`，存在漂移。按 §6.1 收口成单一 `State` 源即可消除。
3. 三个 bool 当前仍是独立字段（`public bool IsInitialized { get; private set; }`），非派生 getter。

---

## 7. 运行时流程

### 7.1 加载与打开

```
懒加载：遍历 StationEnum → 查配置该工位的 Camera → 有则 factory.Create(Info,Param) 建设备，无则空
打开：OpenCamera() → 对每台 Camera != null 的设备 device.Open() + Init()，不自动开始采集
```

- **服务构造函数不做硬件操作**：`factory.Create` 只造实例，`Open` 才是连硬件。

### 7.2 Reload 语义

`Reload()` = 重读配置、重建设备、替换内存工位表，让运行时跟上磁盘。

- 用前必须先 `CloseCamera()` 保证没有相机在用；**别在采集进行中 Reload**（会换走业务手里的设备引用）。
- MVP 默认「重启生效」最稳；要热生效才用 `Reload`。

---

## 8. 两条时序

### 8.1 设置页改绑定

```
选相机类型 → registry.Resolve(type).ListAvailable()      // 枚举
选一台 + 填参数 → factory.Create(info, param)             // 临时设备
验证 → device.Open() → Init() → StartAcquisition() → TryGetFrame() → Close()   // 用完即弃
绑定落盘 → configStore.SaveStations(...)
重建 → stationService.Reload()                            // 或重启生效
```

要点：**「打开/初始化」发生在工厂造的临时设备上，不写进工位**；工位只承载「这个槽位绑了谁」。

### 8.2 业务拿帧

```csharp
_main = stationService.GetCamera(StationEnum.MainCamera);  // 拿一次，缓存
_main.Open();
_main.StartAcquisition();
var frame = _main.TryGetFrame();                          // 轮询取帧
var st = _main.State;                                     // 读状态（或订阅 StateChanged）
```

- 控制/状态/取帧走设备；看图像（推送）走 `IImageProvider.FrameReady`。两条路并行，不重叠。
- `GetCamera` 返回 `null` = 未配置，业务拿一次判一次空即可。

---

## 9. 持久化 schema

```json
{
  "stations": [
    {
      "stationName": "MainCamera",
      "camera": {
        "info": { "cameraType": "HikVision", "serialNum": "...", "typeModel": "...", "interfaceType": "..." },
        "param": { "exposureTime": 5000, "gain": 1, "reverseX": true, "reverseY": true }
      }
    }
  ]
}
```

> 与旧 `List<CameraProfile>` 是破坏性变更；解析失败应降级为空列表 + 日志（当前实现返回 null，建议改空列表）。

---

## 10. 实现进度与待办

**已完成**
- `StationEnum`、`StationProfile`、`CameraProfile`、`CameraStateEnum`、`StateChangedEventArgs`
- `ICameraDevice` 加 `State`/`StateChanged`
- `ICameraConfigStore`/`CameraConfigStore` 改工位粒度
- 删除空壳 `CameraService`

**待办（按建议顺序）**
1. `HikVisionCamera` 状态收口成单源（§6.3）——消除漂移 + 修 `Init()=Ready`
2. 实现 `CameraStationService`（懒加载 + factory 注入 + GetCamera/GetStations/Open/Close/Reload）
3. `Station` 运行时类（若需给设置页/状态面板用身份聚合；业务若直接握 device，可暂缓）
4. `ICameraManagerRegistry` + 实现（塞 `HikVisionManager`）
5. `CameraProvider` 实现 `IImageProvider`
6. DI 注册（`App.xaml.cs`：store/factory/registry/stationService）
7. 设置页绑定 UI（§6.6 可先出 HTML 稿）

---

## 11. 注意事项

- **单写权威**：配置存储 = 磁盘，工位服务 = 运行时；设置页「改→存→Reload」，别出现双份当前数据。
- **线程（§6.7）**：`StateChanged` 可能在非 UI 线程触发，订阅方切回 UI 线程。
- **未绑定识别**：`GetCamera`/`GetStations` 返回未绑定工位（相机为 null），让业务识别「未配置」而非异常。
- **命名一致性**：`StationEnum` 现放 `Vision/Enums/` 目录但 namespace 是 `Vision.Camera`——目录与命名空间不一致，建议统一（要么移到 `Vision/Camera/`，要么 namespace 改 `Vision.Enums`）。
