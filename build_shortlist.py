import json
from analyze_market import boss_rows, zhaopin_rows, wj_rows, norm_size, parse_salary_k


def main():
    rows = []
    rows += boss_rows(r"D:\Code\job-search\boss_sz_api.json", "深圳")
    rows += boss_rows(r"D:\Code\job-search\boss_dg_api.json", "东莞")
    rows += zhaopin_rows(r"D:\Code\job-search\zhaopin_sz.json", "深圳")
    rows += zhaopin_rows(r"D:\Code\job-search\zhaopin_dg.json", "东莞")
    for p in ("job51_sz.json", "51job_dg_p1.json", "51job_dg_p2.json", "51job_dg_p3.json"):
        city = "深圳" if "sz" in p else "东莞"
        rows += wj_rows(rf"D:\Code\job-search\{p}", city)

    high = [r for r in rows if r["salary_k"] and r["salary_k"] >= 15 and r["url"]]
    big = [r for r in high if norm_size(r["size"]) in ("100-499人", "500-999人", "1000-9999人", "10000人以上")]

    by_company = {}
    for r in big:
        by_company.setdefault((r["company"], r["city"], norm_size(r["size"])), []).append(r)

    out = []
    for key, jobs in sorted(by_company.items(), key=lambda x: (-len(x[1]), x[0][1])):
        # 同公司同岗位跨平台去重，最多取 2 条代表 JD
        seen_job = set()
        picked = []
        for j in jobs:
            if j["job"] in seen_job:
                continue
            seen_job.add(j["job"])
            picked.append(j)
            if len(picked) >= 2:
                break
        for j in picked:
            out.append(
                {
                    "platform": j["platform"],
                    "city": j["city"],
                    "company": j["company"],
                    "size": j["size"],
                    "job": j["job"],
                    "salary": j["salary"],
                    "degree": j["degree"],
                    "exp": j["exp"],
                    "url": j["url"],
                }
            )

    with open(r"D:\Code\job-search\shortlist.json", "w", encoding="utf-8") as f:
        json.dump(out, f, ensure_ascii=False, indent=1)

    print("短名单岗位数:", len(out))
    for r in out:
        print(r["platform"], r["city"], r["company"], "|", r["job"], "|", r["salary"], "|", r["degree"], "|", r["url"])


if __name__ == "__main__":
    main()
