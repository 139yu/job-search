import json
import time
import urllib.parse
import urllib.request

TARGET = "A00D84F6A9817D78385D9F007F40FCB8"
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
        f"?scene=1&query={q}&city=101280600&page={page}&pageSize=30"
    )
    code = (
        f"fetch('{url}', {{headers: {{'accept': 'application/json'}}}})"
        ".then(r => r.text()).then(t => t).catch(e => 'ERR:' + e.message)"
    )
    return eval_js(code)


all_jobs = []
for page in (1, 2, 3):
    text = fetch_page(page)
    if text.startswith("ERR:"):
        print("page", page, "failed:", text)
        break
    data = json.loads(text)
    if data.get("code") != 0:
        print("page", page, "code:", data.get("code"), data.get("message"))
        break
    jobs = data["zpData"]["jobList"]
    all_jobs.extend(jobs)
    print("page", page, "jobs:", len(jobs), "resCount:", data["zpData"].get("resCount"))
    time.sleep(1.5)

with open(r"D:\Code\job-search\boss_sz_api.json", "w", encoding="utf-8") as f:
    json.dump(all_jobs, f, ensure_ascii=False, indent=1)
print("total saved:", len(all_jobs))
