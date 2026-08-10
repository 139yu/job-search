import json
import re


SIZE_PATTERN = re.compile(r"(\d+-\d+人|10000人以上)")


def parse_salary_k(s):
    if not s:
        return None
    s = s.replace(" ", "")
    m = re.search(r"(\d+(?:\.\d+)?)-(\d+(?:\.\d+)?)", s)
    if not m:
        return None
    lo = float(m.group(1))
    if "万" in s:
        return lo * 10
    if s.lower().endswith("k") or "k" in s.lower():
        return lo
    if "元" in s:
        return lo / 1000
    return None


def norm_size(s):
    if not s:
        return ""
    if "10000人以上" in s:
        return "10000人以上"
    m = re.search(r"(\d+)-(\d+)人", s)
    if m:
        lo, hi = int(m.group(1)), int(m.group(2))
        if hi < 20:
            return "0-20人"
        if lo < 100 and hi <= 99:
            return "20-99人"
        if hi <= 499:
            return "100-499人"
        if hi <= 999:
            return "500-999人"
        return "1000-9999人"
    return s


def boss_rows(path, city):
    out = []
    for r in json.load(open(path, encoding="utf-8")):
        out.append(
            {
                "platform": "boss",
                "city": city,
                "company": r.get("brandName", ""),
                "size": r.get("brandScaleName", ""),
                "stage": r.get("brandStageName", ""),
                "job": r.get("jobName", ""),
                "salary": r.get("salaryDesc", ""),
                "salary_k": parse_salary_k(r.get("salaryDesc", "")),
                "degree": r.get("jobDegree", ""),
                "exp": r.get("jobExperience", ""),
                "area": (r.get("cityName", "") + " " + r.get("areaDistrict", "")).strip(),
                "days": r.get("daysPerWeekDesc", ""),
                "url": "https://www.zhipin.com/job_detail/" + r.get("encryptJobId", "") + ".html",
            }
        )
    return out


def zhaopin_rows(path, city):
    out = []
    for r in json.load(open(path, encoding="utf-8")):
        other = r.get("other", []) or []
        out.append(
            {
                "platform": "zhaopin",
                "city": city,
                "company": r.get("companyName", ""),
                "size": (SIZE_PATTERN.search(r.get("company", "")) or [None, ""])[1],
                "stage": "",
                "job": r.get("title", ""),
                "salary": r.get("salary", ""),
                "salary_k": parse_salary_k(r.get("salary", "")),
                "degree": other[-1] if other else "",
                "exp": other[-2] if len(other) > 1 else "",
                "area": other[0] if other else "",
                "days": "",
                "url": r.get("href", ""),
            }
        )
    return out


def wj_rows(path, city):
    out = []
    for r in json.load(open(path, encoding="utf-8")):
        e = r.get("entity") or {}
        out.append(
            {
                "platform": "51job",
                "city": city,
                "company": r.get("name", ""),
                "size": (SIZE_PATTERN.search(r.get("company", "")) or [None, ""])[1],
                "stage": "",
                "job": e.get("jobTitle", ""),
                "salary": e.get("jobSalary", ""),
                "salary_k": parse_salary_k(e.get("jobSalary", "")),
                "degree": e.get("jobDegree", ""),
                "exp": e.get("jobYear", ""),
                "area": e.get("jobArea", ""),
                "days": "",
                "url": "",
            }
        )
    return out


def main():
    rows = []
    rows += boss_rows(r"D:\Code\job-search\boss_sz_api.json", "深圳")
    rows += boss_rows(r"D:\Code\job-search\boss_dg_api.json", "东莞")
    rows += zhaopin_rows(r"D:\Code\job-search\zhaopin_sz.json", "深圳")
    rows += zhaopin_rows(r"D:\Code\job-search\zhaopin_dg.json", "东莞")
    for p in ("job51_sz.json", "51job_dg_p1.json", "51job_dg_p2.json", "51job_dg_p3.json"):
        city = "深圳" if "sz" in p else "东莞"
        rows += wj_rows(rf"D:\Code\job-search\{p}", city)

    print("总岗位数:", len(rows))
    for platform in ("boss", "zhaopin", "51job"):
        for city in ("深圳", "东莞"):
            sub = [r for r in rows if r["platform"] == platform and r["city"] == city]
            print(f"{platform} {city}: {len(sub)}")

    high = [r for r in rows if r["salary_k"] and r["salary_k"] >= 15]
    print("\n起薪>=15k:", len(high))
    for platform in ("boss", "zhaopin", "51job"):
        for city in ("深圳", "东莞"):
            sub = [r for r in high if r["platform"] == platform and r["city"] == city]
            print(f"  {platform} {city}: {len(sub)}")

    print("\n起薪>=15k 学历分布:")
    deg = {}
    for r in high:
        deg[r["degree"]] = deg.get(r["degree"], 0) + 1
    for k, v in sorted(deg.items(), key=lambda x: -x[1]):
        print(f"  {k or '(未标)'}: {v}")

    print("\n起薪>=15k 公司规模分布:")
    size = {}
    for r in high:
        s = norm_size(r["size"]) or "(未标)"
        size[s] = size.get(s, 0) + 1
    for k, v in sorted(size.items(), key=lambda x: -x[1]):
        print(f"  {k}: {v}")

    big = [r for r in high if norm_size(r["size"]) in ("100-499人", "500-999人", "1000-9999人", "10000人以上")]
    print("\n起薪>=15k 且规模>=100人:", len(big))
    by_company = {}
    for r in big:
        key = (r["company"], r["size"], r["city"])
        by_company.setdefault(key, []).append(r)
    print("\n公司清单(公司|城市|规模|岗位数):")
    for (name, size_s, city), jobs in sorted(by_company.items(), key=lambda x: -len(x[1])):
        sample = " / ".join(f"{j['job']} {j['salary']} {j['degree']}" for j in jobs[:3])
        print(f"  {name} | {city} | {size_s} | {len(jobs)} | {sample}")

    small = [r for r in high if norm_size(r["size"]) == "20-99人"]
    small_companies = {}
    for r in small:
        small_companies.setdefault((r["company"], r["city"]), []).append(r)
    print("\n起薪>=15k 且规模20-99人(需核小巨人):", len(small_companies), "家公司")
    for (name, city), jobs in sorted(small_companies.items()):
        sample = " / ".join(f"{j['job']} {j['salary']} {j['degree']}" for j in jobs[:2])
        print(f"  {name} | {city} | {len(jobs)} | {sample}")


if __name__ == "__main__":
    main()
