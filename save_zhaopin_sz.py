import json
import time
import urllib.request

TARGET = "D478788BDF0A8745CD9E404A00F77EE7"
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
    with urllib.request.urlopen(req, timeout=30) as resp:
        return resp.read().decode("utf-8")


scrape = open(r"D:\Code\job-search\scrape_zhaopin.js", encoding="utf-8").read()
all_rows = []
for page in (1, 2, 3):
    if page > 1:
        nav(f"https://www.zhaopin.com/sou/jl765/kw9O54UJB778/p{page}")
        time.sleep(2)
    rows = eval_js(scrape)
    all_rows.extend(rows)
    print("page", page, "rows:", len(rows))
    time.sleep(1)

with open(r"D:\Code\job-search\zhaopin_sz.json", "w", encoding="utf-8") as f:
    json.dump(all_rows, f, ensure_ascii=False, indent=1)
print("total:", len(all_rows))
