import urllib.request
import json

PROXY = "http://localhost:3456"


def http_json(url, data=None, headers=None, timeout=30):
    req = urllib.request.Request(url, data=data, method="POST" if data is not None else "GET")
    for k, v in (headers or {}).items():
        req.add_header(k, v)
    with urllib.request.urlopen(req, timeout=timeout) as resp:
        raw = resp.read().decode("utf-8", errors="replace")
    try:
        return json.loads(raw)
    except json.JSONDecodeError:
        return {"raw": raw}


READ_JD = """
(() => {
  const t = document.body.innerText;
  const idx = t.indexOf('职位描述');
  const head = idx >= 0 ? t.slice(idx, idx + 1600) : t.slice(0, 1600);
  return JSON.stringify({
    hasTongzhao: t.includes('统招'),
    hasQuanri: t.includes('全日制'),
    degreeMentions: [...new Set(['统招', '全日制', '本科及以上', '本科', '大专', '学历']).filter(k => t.includes(k))],
    desc: head.replace(/\\s+/g, ' ')
  });
})()
"""


def main():
    target = "C407E9F3C6AFA2E12B8DB627AC956D49"
    tests = [
        ("set6-nofilter", "JSON.stringify([...new Set(['统招', '全日制', '本科及以上', '本科', '大专', '学历'])])"),
        ("set2-filter", "JSON.stringify([...new Set(['统招', '全日制']).filter(k => document.body.innerText.includes(k))])"),
        ("set3-filter", "JSON.stringify([...new Set(['统招', '全日制', '本科及以上']).filter(k => document.body.innerText.includes(k))])"),
        ("set4-filter", "JSON.stringify([...new Set(['统招', '全日制', '本科及以上', '本科']).filter(k => document.body.innerText.includes(k))])"),
    ]
    for label, js in tests:
        for hlabel, headers in (("plain", None), ("text/plain", {"Content-Type": "text/plain"})):
            try:
                res = http_json(f"{PROXY}/eval?target={target}", js.encode("utf-8"), headers)
                print(label, hlabel, "OK", str(res)[:300])
            except Exception as e:
                print(label, hlabel, "ERR", e)


if __name__ == "__main__":
    main()
