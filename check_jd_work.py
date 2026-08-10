import json
import re
import time
import urllib.parse
import urllib.request

PROXY = "http://localhost:3456"


def http_json(url, data=None, timeout=40):
    req = urllib.request.Request(url, data=data, method="POST" if data is not None else "GET")
    with urllib.request.urlopen(req, timeout=timeout) as resp:
        raw = resp.read().decode("utf-8", errors="replace")
    try:
        return json.loads(raw)
    except json.JSONDecodeError:
        return {"raw": raw}


def eval_js(target, js_code):
    payload = http_json(f"{PROXY}/eval?target={target}", js_code.encode("utf-8"))
    if "value" in payload:
        try:
            return json.loads(payload["value"])
        except (json.JSONDecodeError, TypeError):
            return payload["value"]
    return payload


def new_tab(url):
    payload = http_json(f"{PROXY}/new", url.encode("utf-8"))
    return payload.get("targetId")


def close_tab(target):
    try:
        http_json(f"{PROXY}/close?target={target}")
    except Exception:
        pass


READ_JD = """
(() => {
  const t = document.body.innerText;
  const idx = t.indexOf('职位描述');
  const head = idx >= 0 ? t.slice(idx, idx + 1600) : t.slice(0, 1600);
  const keys = ['统招', '全日制', '双休', '周末双休', '大小周', '单休', '单双休', '六天', '五天工作制', '5天工作制', '996', '加班'];
  const m = {};
  for (const k of keys) {
    const c = t.split(k).length - 1;
    if (c > 0) m[k] = c;
  }
  const sizeM = t.match(/(10000人以上|\\d+-\\d+人)/);
  const stageM = t.match(/(已上市|上市公司|未融资|A轮|B轮|C轮|D轮|拟上市)/);
  return JSON.stringify({
    title: document.title,
    hasTongzhao: t.includes('统招'),
    hasQuanri: t.includes('全日制'),
    hits: m,
    size: sizeM ? sizeM[1] : '',
    stage: stageM ? stageM[1] : '',
    desc: head.replace(/\\s+/g, ' ')
  });
})()
"""


def main():
    jobs = json.load(open(r"D:\Code\job-search\verify_jobs.json", encoding="utf-8"))
    results = []
    for i, item in enumerate(jobs):
        target = new_tab(item["url"])
        if not target:
            results.append({**item, "error": "no target"})
            print(item["company"], "no target")
            continue
        time.sleep(2.2)
        try:
            info = eval_js(target, READ_JD)
            results.append({**item, **info})
            print(
                item["platform"],
                item["city"],
                item["company"][:10],
                "|",
                "统招" if info.get("hasTongzhao") else "-",
                "|",
                "全日制" if info.get("hasQuanri") else "-",
                "|",
                info.get("hits", {}),
            )
        except Exception as e:
            results.append({**item, "error": str(e)})
            print(item["company"], "error", e)
        finally:
            close_tab(target)
        time.sleep(0.6)
    with open(r"D:\Code\job-search\jd_details.json", "w", encoding="utf-8") as f:
        json.dump(results, f, ensure_ascii=False, indent=1)
    print("done", len(results))


if __name__ == "__main__":
    main()
