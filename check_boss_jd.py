import json
import time
import urllib.request

TARGET = "F4DD5F8ABA1969FDFF72381BC715B1A7"
PROXY = "http://localhost:3456"


def eval_js(js_code):
    req = urllib.request.Request(
        f"{PROXY}/eval?target={TARGET}",
        data=js_code.encode("utf-8"),
        method="POST",
    )
    with urllib.request.urlopen(req, timeout=30) as resp:
        payload = json.loads(resp.read().decode("utf-8"))
    try:
        return json.loads(payload["value"])
    except (json.JSONDecodeError, TypeError):
        return payload["value"]


def nav(url):
    req = urllib.request.Request(
        f"{PROXY}/navigate?target={TARGET}",
        data=url.encode("utf-8"),
        method="POST",
    )
    try:
        with urllib.request.urlopen(req, timeout=30) as resp:
            return resp.read().decode("utf-8")
    except urllib.error.HTTPError as e:
        return f"HTTP {e.code}"


jobs = [
    ("海目星", "https://www.zhipin.com/job_detail/3dd00328181794270nF_2NW0F1pX.html"),
    ("大族数控", "https://www.zhipin.com/job_detail/dfd3cb116ea0d81a0nJ73N60FFRY.html"),
    ("正业科技", "https://www.zhipin.com/job_detail/531351330799792903J729q9EFVU.html"),
    ("恒翼能", "https://www.zhipin.com/job_detail/6d9b81a580d8475603x63t28GFVU.html"),
    ("汇成真空", "https://www.zhipin.com/job_detail/c68fbde6f72721f703xy2tm_EVFU.html"),
    ("沃德精密", "https://www.zhipin.com/job_detail/b244518b2791a21a1HZz2920EF_.html"),
]

code = """
(() => {
  const t = document.body.innerText;
  const idx = t.indexOf('职位描述');
  const desc = idx >= 0 ? t.slice(idx, idx + 2500) : t.slice(0, 2500);
  return JSON.stringify({
    hasTongzhao: t.includes('统招'),
    hasQuanri: t.includes('全日制'),
    hasBenke: t.includes('本科'),
    hasDazhuan: t.includes('大专'),
    matches: [...new Set(['统招', '全日制', '本科及以上', '本科', '大专']).filter(k => t.includes(k))],
    desc: desc.replace(/\\s+/g, ' ')
  });
})()
"""

results = []
for name, url in jobs:
    nav_result = nav(url)
    time.sleep(2)
    if nav_result.startswith("HTTP"):
        results.append({"company": name, "url": url, "error": nav_result})
        print(name, nav_result)
        continue
    try:
        info = eval_js(code)
    except Exception as e:
        results.append({"company": name, "url": url, "error": str(e)})
        print(name, "eval error", e)
        continue
    results.append({"company": name, "url": url, **info})
    print(json.dumps(results[-1], ensure_ascii=False)[:500])

with open(r"D:\Code\job-search\boss_jd_checks.json", "w", encoding="utf-8") as f:
    json.dump(results, f, ensure_ascii=False, indent=1)
