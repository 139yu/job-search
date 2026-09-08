# Agent 入口

本仓库的开发与协作规范以 **[PROJECT_STANDARDS.md](./PROJECT_STANDARDS.md)** 为唯一权威来源。

- 新增或修改规范：只改 `PROJECT_STANDARDS.md`。
- 本文件与 `CLAUDE.md`、`.cursor/rules/project-standards.mdc` 仅作索引，不维护第二份正文。
- 仓库是什么、脚本怎么跑：见 `CLAUDE.md`。
- 中文是本仓库工作语言（回复、文档、commit message 均默认中文）。

## 仓库地图

- **求职数据管线**（仓库根）：Python 3 纯标准库脚本，采集/规范化/筛选 BOSS 直聘、智联、51job 的 C#/上位机岗位；`analyze_market.py` 是共享规范化库。无包管理器、无测试框架。
- **面试准备知识库**（仓库根）：中文 Markdown（`story_cards/`、`coding_drills.md`、`skill_improvement_plan*.md` 等），多处引用仓库外的源码路径。
- **VisionBench**（`VisionBench/`）：.NET 6 WPF 求职练习解决方案。`Commons`（错误码/日志/基类）、`CommonUI`（主题与通用控件）、`Vision`（相机抽象 + 海康 MVS 实现）、`MainApp`（主程序，含 NLog.config）。依赖方向：MainApp → Vision/CommonUI → Commons；`Libs/Halcon` 为本地 DLL。

## 改动前必读

1. `PROJECT_STANDARDS.md` 全文，尤其：
   - §1.0 每段新对话首条回复开头喊「牢大」。
   - §2 动手闸门（「分析一下 / 先看看 / 为什么」= 本回合禁改文件）；**§2.1 VisionBench 练习代码默认不代写 `.cs`/`.xaml`**，用户明确授权才可写。
   - §4 临时产物只进仓库根 `_build_verify/`，用完即删、禁止提交。
   - §6 UI 硬约束（语义主题键、留白、禁横向滚动、大改先出 HTML 稿）与 §7 注释/日志/异常。
   - §8 默认不自动 commit；commit 消息中文、`feat:`/`fix:`/`chore:`/`refactor:`/`docs:` 前缀；push/PR 仅用户明确要求。
2. `CLAUDE.md` 的 Gotchas：脚本的浏览器代理依赖（`http://localhost:3456` + 会话相关 tab TARGET）、根目录 JSON 是快照。注意其中「硬编码路径/目录缺失」两条写于仓库旧位置，现已部分过时。

## 运行与验证

- Python 脚本：`python <脚本>.py` 直接跑，需本地浏览器代理在线（细节见 CLAUDE.md）。
- VisionBench：`dotnet build VisionBench/VisionBench.sln`；`Vision.csproj` 引用海康 MVS SDK（HintPath 指向 `C:\Program Files (x86)\MVS\...`），机器上须装 MVS 才能编译；编译验证输出按 §4 进 `_build_verify/`。

## 其他

- `docs/mockups/`：HTML 设计稿，命名 `功能-场景.html`，默认不提交。
- `.agents/skills/`：项目级技能（`skills-lock.json` 记录来源，`npx skills experimental_install` 恢复）。
- `resume/`、`interviews/`、`job-posts/`、`applications/`、`skills/` 目前是空占位目录。
