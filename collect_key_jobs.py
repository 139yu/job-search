import json

KEYWORDS = [
    "大族",
    "海目星",
    "比亚迪",
    "欣旺达",
    "中科飞测",
    "博众",
    "杰普特",
    "精实",
    "恒翼能",
    "立讯",
    "正业",
    "东博",
    "汇成真空",
    "先进芯测",
    "冠佳",
    "麦逊",
    "中软国际",
    "沃德精密",
    "诺唯赞",
    "强瑞",
    "吉阳",
    "台德",
]


def hit(name):
    return any(k in name for k in KEYWORDS)


out = []

for city, path in (("深圳", r"D:\Code\job-search\job51_sz.json"), ("东莞", r"D:\Code\job-search\51job_dg_p1.json")):
    rows = json.load(open(path, encoding="utf-8"))
    for r in rows:
        e = r.get("entity") or {}
        name = e.get("companyId", "")
        company = r.get("name", "")
        if hit(company):
            out.append(
                {
                    "platform": "51job",
                    "city": city,
                    "company": company,
                    "job": e.get("jobTitle"),
                    "salary": e.get("jobSalary"),
                    "degree": e.get("jobDegree"),
                    "year": e.get("jobYear"),
                    "area": e.get("jobArea"),
                    "time": e.get("jobTime"),
                    "url": "",
                }
            )

for city, path in (("深圳", r"D:\Code\job-search\zhaopin_sz.json"), ("东莞", r"D:\Code\job-search\zhaopin_dg.json")):
    rows = json.load(open(path, encoding="utf-8"))
    for r in rows:
        if hit(r.get("company", "")):
            out.append(
                {
                    "platform": "zhaopin",
                    "city": city,
                    "company": r.get("company", ""),
                    "job": r.get("title"),
                    "salary": r.get("salary"),
                    "degree": r.get("other", [])[-1] if r.get("other") else "",
                    "year": r.get("other", [])[-2] if len(r.get("other", [])) > 1 else "",
                    "area": r.get("other", [])[0] if r.get("other") else "",
                    "time": "",
                    "url": r.get("href", ""),
                }
            )

for city, path in (("深圳", r"D:\Code\job-search\boss_sz_api.json"), ("东莞", r"D:\Code\job-search\boss_dg_api.json")):
    rows = json.load(open(path, encoding="utf-8"))
    for r in rows:
        if hit(r.get("brandName", "")):
            out.append(
                {
                    "platform": "boss",
                    "city": city,
                    "company": r.get("brandName"),
                    "job": r.get("jobName"),
                    "salary": r.get("salaryDesc"),
                    "degree": r.get("jobDegree"),
                    "year": r.get("jobExperience"),
                    "area": r.get("cityName") + " " + r.get("areaDistrict", ""),
                    "time": "",
                    "url": "https://www.zhipin.com/job_detail/" + r.get("encryptJobId", "") + ".html",
                }
            )

with open(r"D:\Code\job-search\key_jobs.json", "w", encoding="utf-8") as f:
    json.dump(out, f, ensure_ascii=False, indent=1)

for row in out:
    print(
        row["platform"],
        row["city"],
        "|",
        row["company"][:20],
        "|",
        row["job"][:20],
        "|",
        row["salary"],
        "|",
        row["degree"],
        "|",
        row["url"][:60],
    )
