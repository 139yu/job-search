# CLAUDE.md

@PROJECT_STANDARDS.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

协作、动手闸门、UI 通用原则、Git 与验证以 **[PROJECT_STANDARDS.md](./PROJECT_STANDARDS.md)** 为准（`AGENTS.md` 与 `.cursor/rules/project-standards.mdc` 仅索引）。本文件只说明本仓库是什么、脚本怎么跑。

## What this repo is

A personal job-search workspace (求职资料库), not a deployable application. It has two halves:

1. **Market-data pipeline** — Python 3 scripts that collect C#/上位机 (industrial host-computer software) job postings from three Chinese platforms (BOSS 直聘, 智联招聘, 前程无忧/51job) for Shenzhen (深圳) and Dongguan (东莞), then normalize, filter, and verify them.
2. **Interview-prep knowledge base** — Chinese-language Markdown notes: STAR story cards, a coding-drill checklist, a 7-week study plan, Halcon 3D review cards, and a finished market report.

Chinese is the working language throughout; all JSON data files are written with `ensure_ascii=False` (UTF-8).

## Running scripts

No build system, package manager, or test suite. Every script is a self-contained Python 3 stdlib-only program (imports are `json`, `re`, `time`, `urllib.request`/`urllib.parse`):

```
python analyze_market.py
python build_shortlist.py
```

The `.js` files are **not** Node programs — they are DOM-scraping snippets (`(() => {...})()`) that get injected into a browser page and evaluated remotely (see "Scraping mechanism" below). `test_eval.py` is a diagnostic for that proxy, not a unit test.

## Architecture: the data pipeline

Raw data is collected, then transformed through progressively smaller stages. Each stage reads one set of files and writes the next:

```
save_*.py         (scrape/collect)   -> raw JSON   (boss_*_api.json, zhaopin_*.json, job51_sz.json, 51job_dg_p*.json)
analyze_market.py (normalize)        -> console report only (counts, salary/degree/size distributions)
collect_key_jobs.py (keyword filter) -> key_jobs.json     (target-company keyword match)
build_shortlist.py  (filter)         -> shortlist.json    (salary>=15k + company>=100人, deduped)
check_jd_*.py / probe_*.py (verify)  -> jd_checks.json / jd_details.json / boss_jd_checks.json
```

`analyze_market.py` is the **shared normalization library** — other scripts import from it. Its key pieces:

- `boss_rows(path, city)`, `zhaopin_rows(path, city)`, `wj_rows(path, city)` — each flattens one platform's raw JSON into a common row dict with fields `platform/city/company/size/stage/job/salary/salary_k/degree/exp/area/days/url`.
- `parse_salary_k(s)` — parses salary strings like `"15-25K"`, `"20-30K·13薪"`, `"1.5-2万"` into a lower-bound salary in thousands.
- `norm_size(s)` — buckets company-size strings (`"100-499人"`, `"10000人以上"`) into coarse tiers.

Only these `*_rows` functions (plus the two helpers) understand the raw per-platform JSON shapes; everything downstream consumes the normalized rows.

## Scraping mechanism (external dependency)

All collection (`save_*.py`) and JD-verification (`check_*.py`, `probe_*.py`, `refetch_*.py`) scripts depend on a local **browser-automation proxy** at `http://localhost:3456` (the `PROXY` constant). It drives a real, logged-in browser tab:

- `POST /eval?target=<TARGET>` — evaluate a JS snippet in the tab, return its JSON-string result (`eval_js()`).
- `POST /navigate?target=<TARGET>` — navigate the tab to a URL (`nav()`).
- `POST /new` / `POST /close?target=...` — open/close tabs (`new_tab()` / `close_tab()`).

Each script hardcodes a `TARGET` — a hex browser-tab ID (e.g. `"A00D84F6A9817D78385D9F007F40FCB8"`). These are session-specific and won't match a fresh browser. The proxy must be running against a logged-in session for scraping to work; without it, scripts fail at their first `urlopen` call.

## Gotchas

- **Hardcoded data paths are stale.** Nearly every script reads/writes `D:\Code\job-search\...`, but this repo (and its JSON data files) live at `D:\Code\github\job-search-main\`. Update the path prefix in a script before re-running it, or mirror the data files at `D:\Code\job-search\`.
- **The committed JSON files are snapshots**, not live data. Re-running `save_*` overwrites them and requires the proxy.
- **`.agents/skills/` holds three project-scoped Codex-style skills** (`tailored-resume-generator`, `career-changer-translator`, `salary-negotiation-prep`), tracked by `skills-lock.json` (restore with `npx skills experimental_install`). They are reference prompt files, not Claude Code slash commands.
- **The README's described subdirectories (`resume/`, `interviews/`, `job-posts/`, `applications/`, `skills/`) are not present** — actual content is flat in the repo root.

## Reference files worth knowing

- `market_report.md` — the finished analysis: final conclusions, target-company shortlist, and degree/work-schedule findings for the 深圳/东莞 C#/上位机 market.
- `story_cards/*.md` — STAR-format interview stories tied to the RMI `ImgApp-online` WPF codebase; their source paths point outside this repo.
- `coding_drills.md` / `skill_improvement_plan.md` — the hands-on demo checklist and 7-week study plan; they reference `D:\Code\RMI\...` and `D:\Code\Github\halcon_3d\...` source paths outside this repo.
