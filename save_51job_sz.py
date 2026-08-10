import json
import time
import urllib.request

TARGET = "9DAC431A2687DF5535E47AFB9DC48C31"
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


def click_page(num):
    code = f"(() => {{ const lis = [...document.querySelectorAll('.el-pager li.number')]; const l = lis.find(x => x.textContent.trim() === '{num}'); if (l) l.click(); return 'ok'; }})()"
    eval_js(code)
    time.sleep(2.5)


scrape = open(r"D:\Code\job-search\scrape51job.js", encoding="utf-8").read()
clear_dg = "(() => { const a = [...document.querySelectorAll('a.ch')].find(e => e.textContent.trim() === '东莞'); if (a && a.className.includes('on')) a.click(); return 'ok'; })()"
eval_js(clear_dg)
time.sleep(2)
all_rows = []
for page in (1, 2, 3, 4):
    if page > 1:
        click_page(page)
    rows = eval_js(scrape)
    all_rows.extend(rows)
    print("page", page, "rows:", len(rows))
    time.sleep(1)

with open(r"D:\Code\job-search\job51_sz.json", "w", encoding="utf-8") as f:
    json.dump(all_rows, f, ensure_ascii=False, indent=1)
print("total:", len(all_rows))
