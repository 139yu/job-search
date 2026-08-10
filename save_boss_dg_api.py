import json
import time
import urllib.parse
import urllib.request

TARGET = "8CF7653F4DB006C3E0D431ED14CB237D"
PROXY = "http://localhost:3456"


def eval_js(js_code):
    req = urllib.request.Request(
        f"{PROXY}/eval?target={TARGET}",
        data=js_code.encode("utf-8"),
        method="POST",
    )
    with urllib.request.urlopen(req, timeout=40) as resp:
        payload = json.loads(resp.read().decode("utf-8"))
    return payload["value"]


def fetch_page(page):
    q = urllib.parse.quote("C# 上位机")
    url = (
        "https://www.zhipin.com/wapi/zpgeek/search/joblist.json"
        f"?scene=1&query={q}&city=101281600&page={page}&pageSize=30"
    )
    code = (
        f"fetch('{url}', {{headers: {{'accept': 'application/json'}}}})"
        ".then(r => r.text()).then(t => t).catch(e => 'ERR:' + e.message)"
    )
    text = eval_js(code)
    if text.startswith("ERR:"):
        return text
    return text


all_jobs = []
for page in (1, 2, 3):
    text = fetch_page(page)
    data = json.loads(text)
    if data.get("code") != 0:
        print("page", page, "failed:", text[:200])
        break
    jobs = data["zpData"]["jobList"]
    all_jobs.extend(jobs)
    print(
        "page",
        page,
        "jobs:",
        len(jobs),
        "hasMore:",
        data["zpData"].get("hasMore"),
        "resCount:",
        data["zpData"].get("resCount"),
    )
    time.sleep(1.5)

with open(r"D:\Code\job-search\boss_dg_api.json", "w", encoding="utf-8") as f:
    json.dump(all_jobs, f, ensure_ascii=False, indent=1)
print("total saved:", len(all_jobs))
