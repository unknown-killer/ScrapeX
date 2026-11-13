# Source discovery audit — 2026-11-13

Discovery searches were run in Chinese, Russian, Spanish and English. Downloads were parsed with ScrapeX SourceParser, not an IP regex over arbitrary page text. No authentication, CAPTCHA bypass, or website JavaScript execution was used.

- Previous catalog: 168 sources; 164 returned parseable proxies during this audit.
- Candidates downloaded: 271; new parseable candidates: 86.
- Enabled additions: 12; new active catalog: 180.
- Baseline union: 331,458 public endpoints; union with additions: 611,698; additional unique endpoints: 280,240.
- These are source snapshots, not health-test results. The application retains its 500,000-candidate session limit; large bulk feeds can reach that limit.
- Equivalent and subset feeds are kept below as alternatives instead of inflating the active source count.
- File freshness is based on the exact-file GitHub commit where available. A reachable feed does not prove recent maintenance or proxy health.

## Enabled additions

| Feed | Parsed | Additional unique | Freshness |
| --- | ---: | ---: | --- |
| [raw.githubusercontent.com](https://raw.githubusercontent.com/hproxy-com/free-proxy-list/main/all.txt) | 29,481 | 7,559 | [recent (<=7 days)](https://github.com/hproxy-com/free-proxy-list/commit/fa0036b2407d3808e6b0eb7eba529cd549a45d1e) |
| [raw.githubusercontent.com](https://raw.githubusercontent.com/xyzs996/free-proxy-health-list/main/all.txt) | 5,874 | 443 | [recent (<=7 days)](https://github.com/xyzs996/free-proxy-health-list/commit/797f501c7abcfb9c0c0577a1ae2b508b0a53b795) |
| [freeproxyhub.net](https://freeproxyhub.net/api/v1/proxies) | 2,952 | 4 | Not established (public download tested) |
| [raw.githubusercontent.com](https://raw.githubusercontent.com/Moleway/Free-Proxy-List/main/all.txt) | 1,111 | 53 | [stale (>30 days)](https://github.com/Moleway/Free-Proxy-List/commit/5761116516264052baf6691c99ad19f1090b1afa) |
| [flashproxy.com](https://flashproxy.com/api/proxies/txt?limit=1000) | 977 | 11 | Not established (public download tested) |
| [proxywing.com](https://proxywing.com/api/free-proxy-list?format=txt&proxy_format=protocolipport) | 191 | 1 | Not established (public download tested) |
| [proxylister.com](https://proxylister.com/free-proxies) | 50 | 2 | Not established (public download tested) |
| [remproxy.com](https://remproxy.com/cn/freeproxy) | 50 | 3 | Not established (public download tested) |
| [raw.githubusercontent.com](https://raw.githubusercontent.com/wiki/gfpcom/free-proxy-list/lists/http.txt) | 548,309 | 253,370 | Not established (public download tested) |
| [raw.githubusercontent.com](https://raw.githubusercontent.com/wiki/gfpcom/free-proxy-list/lists/socks5.txt) | 503,779 | 13,863 | Not established (public download tested) |
| [raw.githubusercontent.com](https://raw.githubusercontent.com/wiki/gfpcom/free-proxy-list/lists/https.txt) | 502,160 | 1,751 | Not established (public download tested) |
| [raw.githubusercontent.com](https://raw.githubusercontent.com/wiki/gfpcom/free-proxy-list/lists/socks4.txt) | 487,436 | 3,180 | Not established (public download tested) |

## Valid alternatives (not enabled automatically)

- https://raw.githubusercontent.com/hproxy-com/free-proxy-list/main/by-country/US.txt — 12,743 parsed; no additional endpoints after the selected feeds at audit time.
- https://raw.githubusercontent.com/xyzs996/free-proxy-health-list/main/proxies/all/data.json — 5,874 parsed; no additional endpoints after the selected feeds at audit time.
- https://hproxy.com/api/proxy-list?format=txt — 5,823 parsed; no additional endpoints after the selected feeds at audit time.
- https://raw.githubusercontent.com/xyzs996/free-proxy-health-list/main/http.txt — 3,348 parsed; no additional endpoints after the selected feeds at audit time.
- https://raw.githubusercontent.com/xyzs996/free-proxy-health-list/main/proxies/protocols/http/data.json — 3,348 parsed; no additional endpoints after the selected feeds at audit time.
- https://freeproxyhub.net/download/proxies.json — 2,952 parsed; no additional endpoints after the selected feeds at audit time.
- https://freeproxyhub.net/download/proxies.txt — 2,952 parsed; no additional endpoints after the selected feeds at audit time.
- https://raw.githubusercontent.com/hproxy-com/free-proxy-list/main/http.txt — 2,788 parsed; no additional endpoints after the selected feeds at audit time.
- https://raw.githubusercontent.com/xyzs996/free-proxy-health-list/main/https.txt — 2,421 parsed; no additional endpoints after the selected feeds at audit time.
- https://raw.githubusercontent.com/xyzs996/free-proxy-health-list/main/proxies/protocols/https/data.json — 2,421 parsed; no additional endpoints after the selected feeds at audit time.
- https://raw.githubusercontent.com/litportnet/free-proxy-list/live/proxies/all.json — 2,403 parsed; no additional endpoints after the selected feeds at audit time.
- https://raw.githubusercontent.com/xyzs996/free-proxy-health-list/main/proxies/stability/stable/data.json — 2,030 parsed; no additional endpoints after the selected feeds at audit time.
- https://raw.githubusercontent.com/xyzs996/free-proxy-health-list/main/proxies/stability/stable/data.txt — 2,030 parsed; no additional endpoints after the selected feeds at audit time.
- https://raw.githubusercontent.com/litportnet/free-proxy-list/live/proxies/anonymous.json — 2,017 parsed; no additional endpoints after the selected feeds at audit time.
- https://raw.githubusercontent.com/litportnet/free-proxy-list/live/proxies/anonymous.txt — 2,017 parsed; no additional endpoints after the selected feeds at audit time.
- https://api.proxyscrape.com/v4/free-proxy-list/get?request=displayproxies&proxy_format=protocolipport&format=text — 1,941 parsed; no additional endpoints after the selected feeds at audit time.
- https://raw.githubusercontent.com/litportnet/free-proxy-list/live/proxies/http.json — 1,468 parsed; no additional endpoints after the selected feeds at audit time.
- https://raw.githubusercontent.com/litportnet/free-proxy-list/live/proxies/http.txt — 1,468 parsed; no additional endpoints after the selected feeds at audit time.
- https://raw.githubusercontent.com/monosans/proxy-list/main/proxies.json — 919 parsed; no additional endpoints after the selected feeds at audit time.
- https://raw.githubusercontent.com/monosans/proxy-list/main/proxies/all.txt — 919 parsed; no additional endpoints after the selected feeds at audit time.
- https://raw.githubusercontent.com/monosans/proxy-list/main/proxies_pretty.json — 919 parsed; no additional endpoints after the selected feeds at audit time.
- https://raw.githubusercontent.com/maximilianfeix/free-proxy-list/main/all.txt — 908 parsed; no additional endpoints after the selected feeds at audit time.
- https://raw.githubusercontent.com/maximilianfeix/free-proxy-list/main/proxies.json — 908 parsed; no additional endpoints after the selected feeds at audit time.
- https://raw.githubusercontent.com/maximilianfeix/proxy-scraper/proxy-list/all.txt — 908 parsed; no additional endpoints after the selected feeds at audit time.
- https://raw.githubusercontent.com/maximilianfeix/proxy-scraper/proxy-list/proxies.json — 908 parsed; no additional endpoints after the selected feeds at audit time.
- https://raw.githubusercontent.com/xyzs996/free-proxy-health-list/main/proxies/protocols/socks4/data.json — 871 parsed; no additional endpoints after the selected feeds at audit time.
- https://raw.githubusercontent.com/xyzs996/free-proxy-health-list/main/socks4.txt — 871 parsed; no additional endpoints after the selected feeds at audit time.
- https://raw.githubusercontent.com/maximilianfeix/free-proxy-list/main/elite.txt — 786 parsed; no additional endpoints after the selected feeds at audit time.
- https://raw.githubusercontent.com/maximilianfeix/proxy-scraper/proxy-list/elite.txt — 786 parsed; no additional endpoints after the selected feeds at audit time.
- https://raw.githubusercontent.com/litportnet/free-proxy-list/live/proxies/fast.json — 777 parsed; no additional endpoints after the selected feeds at audit time.
- https://raw.githubusercontent.com/litportnet/free-proxy-list/live/proxies/fast.txt — 777 parsed; no additional endpoints after the selected feeds at audit time.
- https://raw.githubusercontent.com/xyzs996/free-proxy-health-list/main/proxies/latency/fast/data.json — 746 parsed; no additional endpoints after the selected feeds at audit time.
- https://raw.githubusercontent.com/xyzs996/free-proxy-health-list/main/proxies/latency/fast/data.txt — 746 parsed; no additional endpoints after the selected feeds at audit time.
- https://raw.githubusercontent.com/litportnet/free-proxy-list/live/proxies/https.json — 694 parsed; no additional endpoints after the selected feeds at audit time.
- https://raw.githubusercontent.com/litportnet/free-proxy-list/live/proxies/https.txt — 694 parsed; no additional endpoints after the selected feeds at audit time.
- https://raw.githubusercontent.com/Moleway/Free-Proxy-List/main/http.txt — 673 parsed; no additional endpoints after the selected feeds at audit time.
- https://raw.githubusercontent.com/litportnet/free-proxy-list/live/proxies/stable.json — 658 parsed; no additional endpoints after the selected feeds at audit time.
- https://raw.githubusercontent.com/litportnet/free-proxy-list/live/proxies/stable.txt — 658 parsed; no additional endpoints after the selected feeds at audit time.
- https://raw.githubusercontent.com/litportnet/free-proxy-list/live/proxies/socks4.json — 592 parsed; no additional endpoints after the selected feeds at audit time.
- https://raw.githubusercontent.com/litportnet/free-proxy-list/live/proxies/socks4.txt — 592 parsed; no additional endpoints after the selected feeds at audit time.
- https://raw.githubusercontent.com/xyzs996/free-proxy-health-list/main/proxies/protocols/socks5/data.json — 531 parsed; no additional endpoints after the selected feeds at audit time.
- https://raw.githubusercontent.com/xyzs996/free-proxy-health-list/main/socks5.txt — 531 parsed; no additional endpoints after the selected feeds at audit time.
- https://raw.githubusercontent.com/litportnet/free-proxy-list/live/proxies/socks5.json — 522 parsed; no additional endpoints after the selected feeds at audit time.
- https://raw.githubusercontent.com/litportnet/free-proxy-list/live/proxies/socks5.txt — 522 parsed; no additional endpoints after the selected feeds at audit time.
- https://raw.githubusercontent.com/maximilianfeix/free-proxy-list/main/https.txt — 476 parsed; no additional endpoints after the selected feeds at audit time.
- https://raw.githubusercontent.com/maximilianfeix/proxy-scraper/proxy-list/https.txt — 476 parsed; no additional endpoints after the selected feeds at audit time.
- https://raw.githubusercontent.com/maximilianfeix/free-proxy-list/main/socks5.txt — 442 parsed; no additional endpoints after the selected feeds at audit time.
- https://raw.githubusercontent.com/Moleway/Free-Proxy-List/main/socks5.txt — 434 parsed; no additional endpoints after the selected feeds at audit time.
- https://raw.githubusercontent.com/relayglass/free-proxy-list/main/all.txt — 397 parsed; no additional endpoints after the selected feeds at audit time.
- https://raw.githubusercontent.com/maximilianfeix/free-proxy-list/main/socks4.txt — 263 parsed; no additional endpoints after the selected feeds at audit time.
- https://raw.githubusercontent.com/maximilianfeix/free-proxy-list/main/http.txt — 259 parsed; no additional endpoints after the selected feeds at audit time.
- https://raw.githubusercontent.com/maximilianfeix/free-proxy-list/main/works-with/discord.txt — 228 parsed; no additional endpoints after the selected feeds at audit time.
- https://raw.githubusercontent.com/maximilianfeix/proxy-scraper/proxy-list/works-with/discord.txt — 228 parsed; no additional endpoints after the selected feeds at audit time.
- https://raw.githubusercontent.com/Moleway/Free-Proxy-List/main/socks4.txt — 212 parsed; no additional endpoints after the selected feeds at audit time.
- https://raw.githubusercontent.com/databay-labs/free-proxy-list/master/by-country/us/http.txt — 183 parsed; no additional endpoints after the selected feeds at audit time.
- https://raw.githubusercontent.com/maximilianfeix/free-proxy-list/main/works-with/reddit.txt — 170 parsed; no additional endpoints after the selected feeds at audit time.
- https://raw.githubusercontent.com/maximilianfeix/proxy-scraper/proxy-list/works-with/reddit.txt — 170 parsed; no additional endpoints after the selected feeds at audit time.
- https://proxybros.com/api/free-proxies/export?&format=txt — 166 parsed; no additional endpoints after the selected feeds at audit time.
- https://raw.githubusercontent.com/relayglass/free-proxy-list/main/protocol/http/http.txt — 152 parsed; no additional endpoints after the selected feeds at audit time.
- https://raw.githubusercontent.com/Moleway/Free-Proxy-List/main/https.txt — 136 parsed; no additional endpoints after the selected feeds at audit time.
- https://raw.githubusercontent.com/maximilianfeix/free-proxy-list/main/works-with/google.txt — 117 parsed; no additional endpoints after the selected feeds at audit time.
- https://raw.githubusercontent.com/maximilianfeix/proxy-scraper/proxy-list/works-with/google.txt — 117 parsed; no additional endpoints after the selected feeds at audit time.
- https://raw.githubusercontent.com/maximilianfeix/free-proxy-list/main/works-with/tiktok.txt — 98 parsed; no additional endpoints after the selected feeds at audit time.
- https://raw.githubusercontent.com/maximilianfeix/proxy-scraper/proxy-list/works-with/tiktok.txt — 98 parsed; no additional endpoints after the selected feeds at audit time.
- https://raw.githubusercontent.com/maximilianfeix/free-proxy-list/main/works-with/instagram.txt — 91 parsed; no additional endpoints after the selected feeds at audit time.
- https://raw.githubusercontent.com/maximilianfeix/proxy-scraper/proxy-list/works-with/instagram.txt — 91 parsed; no additional endpoints after the selected feeds at audit time.
- https://freeproxyhub.net/zh/ — 50 parsed; no additional endpoints after the selected feeds at audit time.
- https://raw.githubusercontent.com/maximilianfeix/free-proxy-list/main/stable.txt — 45 parsed; no additional endpoints after the selected feeds at audit time.
- https://raw.githubusercontent.com/maximilianfeix/proxy-scraper/proxy-list/stable.txt — 45 parsed; no additional endpoints after the selected feeds at audit time.
- https://privasto.com/es/free-proxy-list — 30 parsed; no additional endpoints after the selected feeds at audit time.
- https://www.flashproxy.com/ru/resources/free-proxies — 20 parsed; no additional endpoints after the selected feeds at audit time.
- https://proxywing.com/zh/free-proxy-list — 19 parsed; no additional endpoints after the selected feeds at audit time.
- https://raw.githubusercontent.com/maximilianfeix/free-proxy-list/main/works-with/amazon.txt — 8 parsed; no additional endpoints after the selected feeds at audit time.
- https://raw.githubusercontent.com/maximilianfeix/proxy-scraper/proxy-list/works-with/amazon.txt — 8 parsed; no additional endpoints after the selected feeds at audit time.

## Unavailable or unsupported during this audit

- https://raw.githubusercontent.com/MrMarble/proxy-list/main/all.txt — HTTP Error 404: Not Found; parsed: 0.
- https://raw.githubusercontent.com/ebrasha/abdal-proxy-hub/refs/heads/main/http-proxy-list-by-EbraSha.txt — HTTP Error 404: Not Found; parsed: 0.
- https://raw.githubusercontent.com/ebrasha/abdal-proxy-hub/refs/heads/main/socks4-proxy-list-by-EbraSha.txt — HTTP Error 404: Not Found; parsed: 0.
- https://raw.githubusercontent.com/ebrasha/abdal-proxy-hub/refs/heads/main/socks5-proxy-list-by-EbraSha.txt — HTTP Error 404: Not Found; parsed: 0.
- https://freeproxyhub.net/download/proxies.csv — ok; parsed: 0.
- https://hproxy.com/docs/free-proxy-list — ok; parsed: 0.
- https://proxy5.net/es/free-proxy — ok; parsed: 0.
- https://proxyan.net/zh/free-proxy — ok; parsed: 0.
- https://proxybros.com/api/free-proxies/export?&format=csv — ok; parsed: 0.
- https://proxybros.com/ru/free-proxy-list/ — ok; parsed: 0.
- https://raw.githubusercontent.com/gproxynet/free-proxy-list/main/all.txt — ok; parsed: 0.
- https://raw.githubusercontent.com/wiki/gfpcom/free-proxy-list/lists/anytls.txt — ok; parsed: 0.
- https://raw.githubusercontent.com/wiki/gfpcom/free-proxy-list/lists/hy.txt — ok; parsed: 0.
- https://raw.githubusercontent.com/wiki/gfpcom/free-proxy-list/lists/hy2.txt — ok; parsed: 0.
- https://raw.githubusercontent.com/wiki/gfpcom/free-proxy-list/lists/less.txt — ok; parsed: 0.
- https://raw.githubusercontent.com/wiki/gfpcom/free-proxy-list/lists/socks.txt — ok; parsed: 0.
- https://raw.githubusercontent.com/wiki/gfpcom/free-proxy-list/lists/ss.txt — ok; parsed: 0.
- https://raw.githubusercontent.com/wiki/gfpcom/free-proxy-list/lists/tg.txt — ok; parsed: 0.
- https://raw.githubusercontent.com/wiki/gfpcom/free-proxy-list/lists/tuic.txt — ok; parsed: 0.
- https://www.ipxss.com/free-proxy — HTTP Error 403: Forbidden; parsed: 0.
- https://zj.v.api.aa1.cn/api/proxyip/?type=json — HTTP Error 504: Gateway Time-out; parsed: 0.

## Discovery references

- [Chinese: ProxyWing](https://proxywing.com/zh/free-proxy-list)
- [Chinese: FreeProxyHub](https://freeproxyhub.net/zh/)
- [Russian: HProxy](https://github.com/hproxy-com/free-proxy-list/blob/main/i18n/README.ru.md)
- [Russian: FlashProxy](https://www.flashproxy.com/ru/resources/free-proxies)
- [Spanish: Privasto](https://privasto.com/es/free-proxy-list)
- [Spanish: Proxy5](https://proxy5.net/es/free-proxy)
- [English: Moleway](https://github.com/Moleway/Free-Proxy-List)
- [English: Free Proxy Health List](https://github.com/xyzs996/free-proxy-health-list)

## خلاصهٔ فارسی

جست‌وجو به چهار زبان انجام شد. از 271 منبع بررسی‌شده، 12 منبع جدید با دادهٔ معتبر و سهم تازه به فهرست فعال اضافه شد. آدرس‌های تکراری و زیرمجموعه‌ها برای بزرگ‌نمایی تعداد منابع فعال نشده‌اند. تعداد پروکسی استخراج‌شده، تعداد پروکسی سالم نیست؛ هر پروکسی باید جداگانه بررسی شود. سقف هر نشست برنامه ۵۰۰ هزار آدرس است.
