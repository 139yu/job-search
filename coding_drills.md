# 面试实操写代码清单

> 来源：`ui_newmainuc_story_card.md`、`ui_program_checkpoint_story_card.md`
> 原则：面试时只背书不写代码容易露馅。每一项都做成“能跑、能讲、能测”的小 demo。
> 建议：所有练习放在独立目录，例如 `D:\Code\job-search\coding-demos`，不要写进公司仓库。

## P0：最可能被深挖，先做

### 1. FPD 会话控制器最小状态机

目标：不依赖硬件，用 C# 写一个可运行的 `FpdSessionController`，配假 `IImagePipeline` 和 `IFpdCameraGate`。

必须实现：

- `FpdSession` 枚举：`Idle / Live / Static / EditFreeze / ExclusiveGrab`
- `EnterLive / EnterIdle / EnterExclusiveGrab / LeaveExclusiveGrab`
- `ComputeEffectiveTarget`：`Live + DisplayType==0 + !IsCncDrawMode + DispSource==Live` 才进入主 Live
- `ObserveActual`：循环是否在跑、相机是否开流、600ms 心跳是否新鲜、本世代是否出过帧
- `PostConditionMet`：主 Live 必须“循环 + 开流 + 新鲜心跳”都满足
- `Reconcile`：2 秒健康检查、短采忙跳过、SettleWindow、降级退避、never-framed 硬重开
- 分级重试：软等心跳、只补 `StartLive`、hard `RestartStreaming`，禁止无脑二次 Restart
- `RequestExclusiveCaptureAsync`：忙标志防重入、取消、finally 恢复连续采

验收：单测覆盖“软等心跳不二次 Restart”“owner 不匹配 no-op”“never-framed 硬重开”“ExclusiveGrab 不改成 Idle”。

可抄源码：

- [FpdSessionController.cs](D:/Code/RMI/ImgApp-online/ImgApp/Modules/SystemSharedUI/Services/FpdSessionController.cs)
- [IFpdSessionController.cs](D:/Code/RMI/ImgApp-online/ImgApp/ImgApp.Core/Interfaces/IFpdSessionController.cs)
- [IImagePipeline.cs](D:/Code/RMI/ImgApp-online/ImgApp/ImgApp.Core/Interfaces/IImagePipeline.cs)
- [IFpdCameraGate.cs](D:/Code/RMI/ImgApp-online/ImgApp/ImgApp.Core/Interfaces/IFpdCameraGate.cs)
- [FpdSessionControllerTests.cs](D:/Code/RMI/ImgApp-online/ImgApp.Tests/FpdSessionControllerTests.cs)

预计：2-3 个晚上。

### 2. WPF + Prism Region 最小主壳

目标：新建最小 WPF + Prism 工程，主壳用 `RegionManager`，两个模块注册到 Region，能导航和清理。

必须实现：

- 模块类实现 `IModule.RegisterTypes`
- `RegionManager.RequestNavigate` 切换中心页
- 切换前清理旧模式页拥有的嵌套 Region，防视图残留和事件泄漏
- `ConfirmNavigationRequest` 拦截离开主壳
- 用“运行中、回零、报警、急停”模拟不可切换状态

验收：页面能切换；状态锁定时不能切；切走后旧页面不再收到事件。

可抄源码：

- [NewMainUC.xaml](D:/Code/RMI/ImgApp-online/ImgApp/Modules/SystemMainUI/Views/NewMainUC.xaml)
- [NewMainUCViewModel.cs](D:/Code/RMI/ImgApp-online/ImgApp/Modules/SystemMainUI/ViewModels/NewMainUCViewModel.cs)

预计：1 个晚上。

### 3. 动态 RBAC 菜单 + 权限双校验

目标：用 `IMenuService` 返回带 `PermissionKey` 的菜单项，登录后重建模式 Tab 和菜单；点击前再校验一次。

必须实现：

- `IMenuItem`：标题、`PermissionKey`、命令
- `IPermissionService`：当前角色权限集合
- 可见性过滤：没有权限的菜单不显示
- 运行时二次校验：`ValidateAndExecute(PermissionKey, action)`，越权点击被拦截
- 模拟“操作员 / 工程师”两种角色切换

验收：切换角色后菜单不同；越权命令被拦截并提示。

可抄源码：

- [IMenuService.cs](D:/Code/RMI/ImgApp-online/ImgApp/Services/ServiceDatabase/Interfaces/IMenuService.cs)
- [IPermissionService.cs](D:/Code/RMI/ImgApp-online/ImgApp/Services/ServiceDatabase/Interfaces/IPermissionService.cs)
- [PermissionManager.cs](D:/Code/RMI/ImgApp-online/ImgApp/Services/ServiceDatabase/Permissions/PermissionManager.cs)

预计：1-1.5 个晚上。

### 4. 右侧工具 Catalog 驱动工具条

目标：用一个描述器列表生成右侧工具按钮，支持开闭、互斥、宽度记忆、权限键。

必须实现：

- `IRightToolDescriptor`：标题、图标、`PermissionKey`、打开命令
- `IRightToolCatalog.ForToolbar()` 投影工具条项
- `ItemsControl` + DataTemplate 渲染，不写死按钮
- 当前工具互斥：打开 A 时自动关闭 B
- 抽屉宽度持久化：可用 JSON 或 `Properties.Settings`

验收：新增一个工具只加描述器，不改工具条 XAML。

可抄源码：

- [IRightToolCatalog.cs](D:/Code/RMI/ImgApp-online/ImgApp/Modules/SystemMainUI/RightTools/IRightToolCatalog.cs)
- [RightToolCatalog.cs](D:/Code/RMI/ImgApp-online/ImgApp/Modules/SystemMainUI/RightTools/RightToolCatalog.cs)
- [ModeRightToolShellViewModel.cs](D:/Code/RMI/ImgApp-online/ImgApp/Modules/SystemMainUI/ViewModels/ModeRightToolShellViewModel.cs)

预计：1 个晚上。

## P1：核心业务链路，面试官可能顺着追

### 5. 程序树与检查点编辑最小模型

目标：用 `TreeView` + MVVM 模拟 `ProgramUC`：树节点包含检查点、点位、阵列、分组，支持新增、重命名、复制、粘贴、删除、启用开关。

必须实现：

- 数据模型：`ProgramNode / CheckpointItem / PointItem`
- 层级 `ObservableCollection`
- 复制粘贴时深拷贝，不共享引用
- 节点启用开关影响后续运行逻辑

验收：树操作不破坏父子关系；复制出来的节点改一个不影响原节点。

可抄源码：

- [ProgramUC.xaml](D:/Code/RMI/ImgApp-online/ImgApp/Modules/SystemCNCUI/Views/ProgramUC.xaml)
- [ProgramUCViewModel.cs](D:/Code/RMI/ImgApp-online/ImgApp/Modules/SystemCNCUI/ViewModels/ProgramUCViewModel.cs)

预计：1.5 个晚上。

### 6. 点位阵列生成器

目标：给定原点、方向、数量、间隔，批量生成点位；支持 X、Y、斜线等规则排列。

必须实现：

- 纯 C# 生成类，不依赖 UI
- 参数校验：数量为 0、间隔为 0、重复点
- 生成结果可直接绑定到 `TreeView` 或 `DataGrid`

验收：单测覆盖方向、数量、间隔、重复点、越界。

预计：1 个晚上。

### 7. 同组/同阵列同步器

目标：模拟“一键同步点位数据、算法、ROI 到同组或同阵列”，带范围确认。

必须实现：

- 明确同步源和同步范围
- 只同步允许字段，不做全量覆盖
- 同步前展示影响节点数并要求确认
- ROI 同步时深拷贝

验收：单测覆盖“范围外不同步”“ROI 不共享引用”“同步前可取消”。

预计：1 个晚上。

### 8. 算法预览与参数编辑隔离

目标：编辑参数时走预览副本，点“应用”才写正式配置；结果表展示参考值、上下限、输出启用、判定启用。

必须实现：

- `TryBuildPreviewDict`：深拷贝副本后应用 ROI 参数
- 伪算法：输入 ROI + 参数，输出检测数据
- 正式应用走另一条写入链路
- 预览修改不污染已保存配置

验收：预览后不点应用，正式配置保持不变。

可抄源码：

- [CNCItemEditUCViewModel.cs](D:/Code/RMI/ImgApp-online/ImgApp/Modules/SystemCNCUI/ViewModels/CNCItemEditUCViewModel.cs)
- [RoiComponentAlgoHelper.cs](D:/Code/RMI/ImgApp-online/ImgApp/Modules/SystemCNCUI/Helpers/RoiComponentAlgoHelper.cs)

预计：1.5 个晚上。

## P2：加分项或未来方案

### 9. 导航图选点原型

目标：用一张拼接产品图，支持放大缩小、选点、保存坐标，并把图像坐标转成轴坐标，加偏移量。

必须实现：

- `ScrollViewer` + `Canvas` 选点
- 缩放、平移、坐标换算
- 选点后保存偏移量
- 模拟“模板取图验证，坐标不对就修正偏移”

验收：选点后能换算成轴坐标，并能手工修正。

预计：2-3 个晚上，可放在面试后半段准备。

### 10. 只背不写的部分

以下内容不需要写代码，但需要每天口述：

- 两个故事卡的 30 秒版和 2 分钟 STAR 版
- 为什么先做元件库
- 阵列同步怎么保证不误改
- 算法预览为什么不用正式参数
- FPD 会话为什么做成控制器收口
- 现场反馈的诚实回答：还没去过现场，不编造客户原话

## Halcon 相关实操

> 来源：`D:\Code\Github\halcon_3d\hdev\05点云案例处理`、`halcon_3d_interview_notes.md`
> 原则：3D 不是背算子，而是“深度图 -> 点云 -> 去飞点 -> ROI -> 拟合/测量 -> 单位换算”一条链。每个案例都要能改参数、能默写、能讲易错点。
> 建议：HDevelop 练习直接在 Halcon 里跑；C# 封装放到 `D:\Code\job-search\coding-demos\Halcon3dDemo`，不写进公司仓库。

### 11. 2D 测量最小闭环（离线 X-Ray 优先）

目标：不整套重写离线机的 `MeasureCanvas`，只写一个最小可运行测量工具：读图 -> 鼠标画测量图元 -> 算像素结果 -> 按标定比例转 mm -> 显示结果。这套代码就是“交互测量怎么实现”的面试证据。

必须实现：

- 在 Halcon 窗口/控件上画点、线、角度、三点圆（任选 2-3 种）
- 几何计算独立成类：两点距离、点到线垂距、三点角度、多边形面积
- `MeasureVisual` 保存类型和点列，负责结果文本计算
- `MeasureCanvas` 只做绘制、命中、拖动，不掺业务逻辑
- 像素比例 `PixChange`（mm/px）从标定参数传入，结果同时显示像素值和 mm 值
- 最小 WPF 或 WinForm 壳，不接真实相机，用离线项目里的样例图或截图

验收：几何函数有单测（距离、角度、面积、单位换算）；能讲清“图像坐标 -> 像素结果 -> 实际尺寸”的链路；不依赖相机跑通。

可抄源码（只参考，不整段照抄）：

- [MeasureVisual.cs](D:/Code/RMI/ImgApp/SystemUI/HWindow_Tool/Model/MeasureVisual.cs)
- [MeasureCanvas.cs](D:/Code/RMI/ImgApp/SystemUI/HWindow_Tool/Model/MeasureCanvas.cs)
- [MeasureShapeUnit.cs](D:/Code/RMI/ImgApp/SystemUI/HWindow_Tool/Model/Shape/MeasureShapeUnit.cs)
- [AlgoModule_ManualDistLine.cs](D:/Code/RMI/ImgApp/SystemUI/Algo/AlgoModule_ManualDistLine.cs)
- [AlgoModule_ManualDistPP.cs](D:/Code/RMI/ImgApp/SystemUI/Algo/AlgoModule_ManualDistPP.cs)
- [AlgoModule_ManualAngle.cs](D:/Code/RMI/ImgApp/SystemUI/Algo/AlgoModule_ManualAngle.cs)
- [AlgoModule_ManualP3Circle.cs](D:/Code/RMI/ImgApp/SystemUI/Algo/AlgoModule_ManualP3Circle.cs)
- [X7100.cs](D:/Code/RMI/ImgApp/SystemUI/X7100.cs)（看 `SyncMeasurePixChange` 和测量入口）

暂不手写：气泡分割、BGA、透锡率、金线弧度等业务算法，能讲清流程和关键参数即可。

预计：2-3 个晚上；拆分：第 1 晚 HDevelop/几何流程，第 2-3 晚 C# 交互与单测。

### 12. HDevelop 复跑：平面拟合 / 距离 / 平面度 / 夹角

目标：把 01、02、04、05 四个案例完整复跑，不看原文件能默写出主流程。

必须实现：

- 深度图按 Scale 转点云，能说清 XYZ 单位和像素比例
- 裁剪 ROI + 高低 10% 高度过滤去飞点
- 平面拟合（Huber/Tukey），点到平面距离公式
- 主惯性轴归一化坐标系，避免摆放姿态影响
- 平面夹角：先统一法向量方向再点积，避免算出补角

验收：改 `NumNeighbor`、`InlierRate`、裁剪比例后结果变化可解释；关掉原案例能默写 `read_image` -> `depth_image_to_pointcloud` -> `select_points_object_model_3d` -> `fit_primitives_object_model_3d` -> `moments_object_model_3d` -> 距离/平面度/夹角计算。

可抄源码：

- [点云平面拟合.hdev](D:/Code/Github/halcon_3d/hdev/05点云案例处理/01-点云平面拟合/点云平面拟合.hdev)
- [点到平面的距离.hdev](D:/Code/Github/halcon_3d/hdev/05点云案例处理/02-点到平面的距离/点到平面的距离.hdev)
- [点云平面度测量.hdev](D:/Code/Github/halcon_3d/hdev/05点云案例处理/04-点云平面度测量/点云平面度测量.hdev)
- [点云平面夹角测量.hdev](D:/Code/Github/halcon_3d/hdev/05点云案例处理/05-点云平面夹角测量/点云平面夹角测量.hdev)
- [get_plane_func.hdvp](D:/Code/Github/halcon_3d/func/get_plane_func.hdvp)

预计：3-4 个晚上，每晚会跑 1-2 个案例。

### 13. HDevelop 复跑：体积

目标：把 06 计算点云体积完整复跑，重点讲“相对参考平面”和“单位换算”。

必须实现：

- 点云采样 `sample_object_model_3d('fast', 3)` 后贪心三角化
- ROI 提取、底部参考平面生成
- `volume_object_model_3d_relative_to_plane` 有符号体积 -> 绝对值
- 把像素灰度值换算成 mm / mm³ 讲清楚

验收：改采样级别和 ROI，看体积变化并解释；能回答“为什么必须先有参考平面”。

可抄源码：

- [计算点云体积.hdev](D:/Code/Github/halcon_3d/hdev/05点云案例处理/06-计算点云体积/计算点云体积.hdev)
- [gen_3d_volumn.hdvp](D:/Code/Github/halcon_3d/func/gen_3d_volumn.hdvp)

预计：1-1.5 个晚上。

### 14. HDevelop 复跑：空间直线 + 平面交线

目标：08 案例完整复跑，并且能回答三个关键追问：Halcon 没有 3D 直线拟合算子怎么办、为什么先三角化、为什么投影 2D 再反变换回 3D。

必须实现：

- 点云去噪 -> 三角化 -> ROI
- 两个特征区域质心确定空间直线，或两个平面求交线
- 30 个平行平面切片，`intersect_plane_object_model_3d` 求每条交线
- 交线投影 2D，一阶/二阶导数找边缘点，再反变换回 3D

验收：默写主流程；能指出“交线提取对象是三角网格，不是原始点云”；能说出单位换算陷阱。

可抄源码：

- [点云直线拟合.hdev](D:/Code/Github/halcon_3d/hdev/05点云案例处理/08-点云直线拟合/点云直线拟合.hdev)
- [点云定向切割.hdev](D:/Code/Github/halcon_3d/hdev/05点云案例处理/07-点云定向切割/点云定向切割.hdev)
- [halcon_3d_interview_notes.md](halcon_3d_interview_notes.md)
- [halcon_3d_review_cards.md](halcon_3d_review_cards.md)

预计：2-3 个晚上，是 3D 里最值得深挖的一段。

### 15. C# + Halcon.NET 点云测量服务

目标：把 Halcon 算子收进一个不依赖 UI 的 C# 服务，演示“上位机里不是算子堆页面”。

必须实现：

- `DepthImageToPointCloud`：读图、转点云、单位换算
- `PointCloudFilter`：裁剪 ROI、邻域去噪、高度过滤
- `PlaneMeasurementService`：拟合平面、点到平面距离、平面度、夹角、体积
- `MeasurementResult`：结果对象化，带单位字段（mm、mm³、度）
- 控制台或最小 WPF 壳调用，不接真实相机，用 `test.tif` / `test.om3` 或合成数据

验收：不依赖相机能跑通“读图 -> 点云 -> 拟合 -> 输出结果”；每个方法的输入输出有类型，不传裸 `HObject`；单测覆盖单位换算和结果边界。

可抄源码：第 12-14 项列出的 `.hdev` 和 `.hdvp` 案例。

预计：2-3 个晚上；面试价值高于再跑一个案例。

### 16. 合成数据鲁棒拟合对比

目标：用合成深度图制造飞点，对比普通最小二乘、Huber、Tukey 的平面拟合结果，把“为什么要鲁棒拟合”变成可验证 demo。

必须实现：

- 生成理想平面 + 少量随机飞点
- 同一组数据用不同鲁棒模式拟合，输出平面参数和残差
- 断言：含飞点时 Huber/Tukey 比普通最小二乘更接近理想平面
- 注释说明“去飞点、裁剪、鲁棒拟合是三道防线，不能只靠一个”

验收：单测或 HDevelop 脚本能打印对比结果，面试时直接讲数据结论。

可抄源码：同第 12 项案例，算子 `fit_primitives_object_model_3d(..., 'least_squares_huber' / 'least_squares_tukey')`。

预计：1 个晚上。

### 17. 3D 只背不写的部分

- 通用框架：深度图 -> 点云 -> 去飞点 -> ROI -> 拟合/测量 -> 单位换算
- 为什么去飞点、为什么鲁棒拟合、为什么统一坐标系
- Halcon 没有 3D 直线拟合算子，两种替代方案
- 为什么交线提取必须先三角化
- 为什么 3D 交线投影成 2D 后还要反变换回 3D
- 单位换算最容易说错：像素灰度值 vs mm vs mm³

## 建议排期（2026-08-14 重排，与 skill_improvement_plan.md 对齐）

> VisionBench 为整合壳：有 UI 案例做成壳内可导航模块，纯逻辑案例进服务层类库 + 单测，Halcon 复跑独立练。完成阈值：壳 + 3~4 个高质量模块。

WPF 整合壳线（VisionBench）：

- 第 3 周（8/17-8/23）：第 2 项，Region 导航 + 状态锁 → 壳可讲
- 第 4 周（8/24-8/30）：第 3 项 RBAC 菜单、第 4 项 Catalog 工具条
- 第 5 周（8/31-9/6）：第 1 项 FPD 状态机 → 服务层类库 + 单测
- 第 6 周（9/7-9/13）：第 11 项 2D 测量 → Halcon 窗口区模块
- 第 7 周（9/14-9/20）：第 15 项点云测量服务 → 视觉模块进壳
- 有余力再做：第 5、7、8、9 项

Halcon 线（独立练，不进壳）：

- 第 3 周：第 12 项，平面拟合 + 点到平面距离
- 第 4 周：第 12 项剩余（平面度、夹角）+ 第 13 项体积
- 第 6 周：第 14 项直线与交线
- 第 7 周：第 15 项 C# + Halcon.NET 封装（进壳）
- 第 17 项每天口述 5 分钟，不占整块时间

## 面试怎么用

每个 demo 都要能回答三个问题：

1. 你写了什么：一句话说清功能
2. 为什么这么设计：解决什么实际场景
3. 怎么验证：哪些单测覆盖了关键边界

Halcon demo 额外加三个问题：

1. 结果单位是什么，怎么从像素灰度值换算到 mm
2. 为什么用鲁棒拟合而不是普通最小二乘
3. 为什么交线提取前必须先三角化

写代码的时间控制在 20 分钟、40 分钟、1 小时三档，方便上班忙里偷闲分块做。
