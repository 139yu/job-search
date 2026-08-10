import json

KEYWORDS = ["大族", "海目星", "比亚迪", "欣旺达", "中科飞测", "博众", "杰普特", "精实", "恒翼能", "立讯", "正业", "东博", "汇成真空", "先进芯测", "冠佳"]

for city, path in (("深圳", r"D:\Code\job-search\boss_sz_api.json"), ("东莞", r"D:\Code\job-search\boss_dg_api.json")):
    rows = json.load(open(path, encoding="utf-8"))
    print("=" * 20, city, "rows:", len(rows))
    seen = set()
    for r in rows:
        name = r.get("brandName", "")
        if any(k in name for k in KEYWORDS) and (name, r.get("jobName")) not in seen:
            seen.add((name, r.get("jobName")))
            print(
                name,
                "|",
                r.get("jobName"),
                "|",
                r.get("salaryDesc"),
                "|",
                r.get("jobDegree"),
                "|",
                r.get("brandScaleName"),
                "|",
                r.get("encryptJobId"),
            )
