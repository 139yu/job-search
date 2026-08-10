import json
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


code = """
fetch('https://www.zhipin.com/wapi/zpgeek/search/jobdetail.json?encryptJobId=b492042b80e8c71f0nB70ty7GVJT', {headers: {'accept': 'application/json'}}).then(r => r.text()).then(t => t.slice(0, 3000)).catch(e => 'ERR:' + e.message)
"""
print(eval_js(code))
