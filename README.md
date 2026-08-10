# 求职资料库

本目录专门存放求职相关内容，所有技能均为项目级安装，只对这个目录生效，不会污染全局环境。

## 目录结构

- `resume/`：简历各版本、按 JD 定制的终版
- `interviews/`：面试准备、复盘记录、模拟面试稿
- `job-posts/`：目标公司调研、岗位 JD 存档
- `applications/`：投递记录、薪资谈判记录
- `skills/`：预留目录，可放自建技能或参考资料
- `.agents/skills/`：已安装的项目级技能

## 已安装技能

- `tailored-resume-generator`：根据具体 JD 定制简历，突出匹配经验
- `career-changer-translator`：把 Java 转 C# 的经历翻译成目标岗位语言
- `salary-negotiation-prep`：市场薪资调研、谈判策略与话术准备

使用方式：在 Codex 中打开本目录作为工作目录，或把简历、JD 文件放进对应子目录后直接让 Codex 调用相关技能。

## 说明

- 目前没有找到安装量足够高的模拟面试技能，需要模拟面试时直接让 Codex 扮演面试官即可，素材可以用 `D:\Code\RMI\Interview_Guide.md`
- 技能内容以提示词为主，使用前建议人工浏览一遍；`salary-negotiation-prep` 被扫描为中等风险，注意不要向技能提供身份证号、银行卡号等敏感信息
- `skills-lock.json` 记录了技能来源，换机器时可在本目录执行 `npx skills experimental_install` 恢复
