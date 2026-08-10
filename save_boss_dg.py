import json
import time
import urllib.request

TARGET = "8CF7653F4DB006C3E0D431ED14CB237D"
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


scrape = open(r"D:\Code\job-search\scrape_boss.js", encoding="utf-8").read()
rows = eval_js(scrape)
with open(r"D:\Code\job-search\boss_dg_p1.json", "w", encoding="utf-8") as f:
    json.dump(rows, f, ensure_ascii=False, indent=1)
print("count:", len(rows))
print("first:", json.dumps(rows[0], ensure_ascii=False)[:300])

# check for pagination
code = """
(() => {
  const pages = [...document.querySelectorAll('.options-pages a, .page a, [class*=pagination] a, [class*=page] button')];
  const all = [...document.querySelectorAll('*')].filter(e => /下一页|下一页>|第\\d+页/.test(e.textContent || '') && e.children.length <= 1);
  return JSON.stringify({pages: pages.map(p=>({txt:p.textContent.trim(), cls:(p.className||'').toString().slice(0,50)})).slice(0,20), candidates: all.slice(0,10).map(e=>({txt:(e.textContent||'').trim().slice(0,20), tag:e.tagName, cls:(e.className||'').toString().slice(0,50)}))});
})()
"""
print(eval_js(code))
