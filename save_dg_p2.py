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


def click_page(num):
    code = f"(() => {{ const lis = [...document.querySelectorAll('.el-pager li.number')]; const l = lis.find(x => x.textContent.trim() === '{num}'); if (l) l.click(); return 'ok'; }})()"
    eval_js(code)
    time.sleep(2.5)


scrape = open(r"D:\Code\job-search\scrape51job.js", encoding="utf-8").read()
for page in (1, 2, 3):
    if page > 1:
        click_page(page)
    rows = eval_js(scrape)
    path = rf"D:\Code\job-search\51job_dg_p{page}.json"
    with open(path, "w", encoding="utf-8") as f:
        json.dump(rows, f, ensure_ascii=False, indent=1)
    dg = [r for r in rows if (r.get("entity") or {}).get("jobArea", "").startswith("东莞")]
    print(f"page {page}: total={len(rows)} dg={len(dg)} first={dg[0]['entity']['jobTitle'] if dg else '-'}")
