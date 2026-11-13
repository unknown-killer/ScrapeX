# ScrapeX server & API

[English](#quick-start) · [فارسی](#persian)

## Quick start

ScrapeX 1.2.0 supports desktop hosting and an independent Windows service. The steps below describe desktop mode, which stops when the app closes. For automatic startup after reboot and unattended collection/checking, use the bilingual [Windows service guide](WINDOWS-SERVICE.md).

1. Configure sources, HTTP/HTTPS targets, parallelism and stage timeout in the desktop workspace.
2. Open **Server API**. Default port: `8088`; default binding: `127.0.0.1` only.
3. Leave **Public list** unchecked to require an API key. **Copy API key** copies the masked key for your client. Keys are encrypted on disk with Windows DPAPI for the current Windows account.
4. Optionally enable automatic collection and checking. The first cycle starts immediately; subsequent cycles start the configured interval after the previous cycle finishes. Range: 0.25–168 hours.
5. Set maximum result age (0.25–720 hours, default 24). A proxy older than that is excluded from responses even if no new run has succeeded.
6. Click **Save & start API**. Use **Refresh now** for a manual cycle, **Cancel refresh** to stop a cycle, or **Publish desktop results** to publish previously checked working rows.
7. Use **Build your link** to choose type, country, HTTPS status, format and page size. **Monitoring** shows the published inventory, current cycle, country flags, protocol totals and API traffic.

Stop and restart the server to apply connection, schedule, source or checker-setting changes. Link filters can be edited while it is running. A refresh waits for a manual desktop collection/check to finish; network check workloads do not overlap. Country resolution continues in the background, with up to two minutes to drain remaining lookups at the end. Unknown countries remain `ZZ` and are still included when no country filter is used.

## Windows Server deployment

For a local reverse proxy, keep the default loopback binding. If Windows reports **Access denied**, an administrator can reserve the exact listener URL for the Windows account running ScrapeX:

```powershell
# Replace COMPUTER\User with the actual account. Run in an elevated terminal once.
netsh http add urlacl url=http://127.0.0.1:8088/ user="COMPUTER\User"
```

For direct remote connections, select **Accept remote connections**. Reserve the matching wildcard URL instead:

```powershell
netsh http add urlacl url=http://+:8088/ user="COMPUTER\User"
```

Allow inbound TCP on the selected port in Windows Firewall and any provider firewall, limited to the intended clients. ScrapeX does not change firewall rules or URL reservations itself. Set **Public address / domain** to the actual external origin, for example `http://SERVER_IP:8088` or `https://proxies.example.com`. This field builds links; it does not configure DNS, certificates, bindings or NAT.

For remote API-key clients, terminate HTTPS at a reverse proxy such as IIS and forward to the loopback listener. Forward `X-Api-Key` or `Authorization` unchanged. The built-in listener serves HTTP; the proxy verification setting **HTTPS** describes tested proxy capability, not the listener's transport. **Public list** deliberately allows anyone who can reach the listener to read the published list and statistics. Keep it off for private feeds.

## Routes

All routes accept GET and HEAD. No remote endpoint changes settings or starts checks.

| Route | Authentication | Response |
| --- | --- | --- |
| `/` | Public | Bilingual landing page and feed form; no proxy data or secrets |
| `/health` | Public | Minimal listener health |
| `/api/v1/proxies` | API key, unless public mode | JSON by default; supports `format=txt` |
| `/proxies.txt` | API key, unless public mode | TXT by default; accepts the same filters |
| `/api/v1/stats` | API key, unless public mode | Inventory, country/protocol totals, expired count, traffic and refresh history |

Send `X-Api-Key: YOUR_KEY` or `Authorization: Bearer YOUR_KEY`. Keys in URLs are not supported. In protected mode, a plain browser link to a data route returns 401; use a client that can send headers. The landing-page feed form opens data directly in public mode.

## Filters

| Parameter | Values / default |
| --- | --- |
| `format` | `json` or `txt` |
| `type` | `http`, `socks4`, `socks5`, comma-separated combinations, or `all` (default) |
| `country` | ISO two-letter codes such as `US,DE`, `ZZ` for unknown, or `all` (default) |
| `https` | `true` (default): HTTPS verified; `false`: HTTP-only; `all`: all working |
| `limit` | 1–10,000; default 1,000 |
| `offset` | 0–500,000; default 0 |
| `max_ping` | Maximum measured TCP connection time, 0–3,600,000 ms; omitted means no limit. Unknown timings are excluded when this filter is set. |
| `max_age_hours` | Positive number no greater than the configured server maximum; default is that maximum |
| `sort` | `endpoint` (default) or `ping`; endpoint breaks ties, unknown pings sort last |

Unknown or repeated parameters, invalid codes/formats and out-of-range values return 400. Country is the location of the endpoint IP, not a guaranteed exit location. Each endpoint appears once, under its winning HTTPS protocol when HTTPS succeeds, otherwise its winning HTTP protocol; this is not an enumeration of every protocol it may support.

```bash
# Detailed, HTTPS-verified SOCKS5 proxies in Germany or the US
curl -H "X-Api-Key: YOUR_KEY" "http://127.0.0.1:8088/api/v1/proxies?type=socks5&country=DE,US&format=json&https=true&limit=100&sort=ping"

# Plain HTTP-only list, any country, measured TCP connection <= 500 ms
curl -H "Authorization: Bearer YOUR_KEY" "http://127.0.0.1:8088/proxies.txt?https=false&max_ping=500&limit=1000"

# Monitoring
curl -H "X-Api-Key: YOUR_KEY" "http://127.0.0.1:8088/api/v1/stats"
```

PowerShell:

```powershell
$headers = @{ 'X-Api-Key' = 'YOUR_KEY' }
$page = Invoke-RestMethod 'http://127.0.0.1:8088/api/v1/proxies?https=true&limit=1000' -Headers $headers
$page.proxies | Select-Object endpoint, protocol, countryCode, pingMs, https
```

JSON envelopes contain `revision`, `publishedUtc`, `total`, `count`, `offset`, `limit`, `nextOffset` and `proxies`. Records contain `endpoint`, `protocol`, `countryCode`, `country`, `https`, `http`, `pingMs`, `httpMs`, `httpsMs` and `checkedUtc`. A negative timing means unavailable. The HTTP and HTTPS successes may use different protocols; `protocol` follows the winning route described above. `http=true` reports that an HTTP probe succeeded at that endpoint.

TXT returns one `IP:port` per line. Both formats include `X-Total-Count` and `X-Snapshot-Revision`. Follow `nextOffset` in JSON, or increment the offset for TXT until empty. Compare revisions between pages and restart paging if a publication changed. Age filtering can also remove records while paging. The API does not pin a multi-request snapshot.

## Publication and monitoring

A complete cycle writes a new snapshot to disk and then swaps it into the API. During checking, readers continue to receive the previous snapshot, subject to its age limit. Cancellation, source failure with zero candidates, or a pipeline exception retains the prior publication. A completed check with zero working results publishes an empty list. Partial success from some sources is allowed and logged.

Settings, published snapshot and country cache are under `%LOCALAPPDATA%\ScrapeX`. Automatic-cycle text outputs are in `Server results\<UTC timestamp>`, including separate HTTPS/HTTP_ONLY folders and `Full Detail.txt`. The on-screen recent history contains the last 50 cycles in the current server session; request counters and history reset on server restart. The snapshot and settings persist.

The API admits up to 32 simultaneous requests and 120 requests/second globally. Excess traffic returns 429 with `Retry-After: 1`. Responses have a 15-second deadline. Other statuses include 401 (missing/wrong key), 404 (unknown route), 405 (unsupported method), 414 (query too long) and 500 (request failure). `/health` confirms the listener is running, not that proxies are fresh or working now.

---

<a name="persian"></a>
<div dir="rtl">

## راهنمای فارسی سرور و API

نسخهٔ ۱.۲.۰ هم اجرای داخل برنامه و هم سرویس مستقل ویندوز را پشتیبانی می‌کند. مراحل زیر مربوط به حالت دسکتاپ است که با بستن برنامه متوقف می‌شود. برای اجرای خودکار بعد از ری‌استارت و اسکرپ و چک بدون ورود کاربر، [راهنمای سرویس ویندوز](WINDOWS-SERVICE.md) را دنبال کنید.

### شروع کار

۱. منابع، مقصد HTTP و HTTPS، تعداد بررسی‌های موازی و زمان انتظار را در بخش بررسی پروکسی تنظیم کنید.

۲. به **Server API** بروید. پورت پیش‌فرض ۸۰۸۸ و اتصال پیش‌فرض فقط روی همین کامپیوتر است.

۳. برای فهرست خصوصی، **Public list** را خاموش نگه دارید و کلید را با **Copy API key** بردارید. کلید روی دیسک با DPAPI ویندوز برای همان حساب کاربری رمزگذاری می‌شود. در حالت عمومی، هر کسی که به سرور دسترسی شبکه دارد می‌تواند فهرست و آمار را بخواند.

۴. جمع‌آوری خودکار را فعال و فاصلهٔ اجرا را از ۰٫۲۵ تا ۱۶۸ ساعت تعیین کنید. دور اول بلافاصله شروع می‌شود و فاصلهٔ دور بعد از پایان دور قبلی محاسبه می‌شود. حداکثر عمر نتیجه بین ۰٫۲۵ تا ۷۲۰ ساعت است؛ پیش‌فرض ۲۴ ساعت. نتایج قدیمی‌تر از این زمان از پاسخ API کنار گذاشته می‌شوند.

۵. **Save & start API** را بزنید. **Refresh now** یک دور تازه شروع می‌کند، **Cancel refresh** دور فعلی را لغو می‌کند و **Publish desktop results** نتایج فعالِ بررسی‌شدهٔ دسکتاپ را منتشر می‌کند.

۶. در **Build your link**، پروتکل، کد کشورها، وضعیت HTTPS، قالب و اندازهٔ صفحه را انتخاب کنید. تب **Monitoring** تعدادها، پرچم کشورها، پروتکل‌ها، ترافیک API و پیشرفت دور فعلی را نشان می‌دهد.

برای اعمال تغییر تنظیمات سرور، زمان‌بندی، منابع یا بررسی‌کننده، سرور را متوقف و دوباره شروع کنید. فیلتر لینک هنگام اجرا قابل تغییر است. بررسی خودکار منتظر پایان عملیات دستی می‌ماند و دو بار بررسی شبکه هم‌زمان اجرا نمی‌شود. شناسایی کشور در پس‌زمینه انجام می‌شود و در پایان تا دو دقیقه برای تکمیل صف فرصت دارد. موارد نامشخص با کد ZZ نگه داشته می‌شوند.

### راه‌اندازی روی سرور ویندوز

برای reverse proxy محلی، اتصال پیش‌فرض loopback را نگه دارید. اگر ویندوز خطای Access denied داد، مدیر سرور باید URL دقیق را برای حساب اجراکننده رزرو کند. دستورهای netsh در بخش انگلیسی همین فایل آمده‌اند؛ COMPUTER\User را با نام حساب واقعی عوض کنید.

برای دسترسی مستقیم از بیرون، **Accept remote connections** را فعال کنید و رزرو URL با علامت + و پورت انتخاب‌شده را انجام دهید. در فایروال ویندوز و فایروال شرکت میزبان نیز دسترسی TCP همان پورت را برای کاربران موردنظر باز کنید. برنامه این تنظیمات سیستم را خودکار تغییر نمی‌دهد.

در **Public address / domain**، آدرس واقعی سرور یا دامنه را بنویسید. این فیلد فقط لینک می‌سازد و DNS، گواهی، NAT یا اتصال شبکه را تنظیم نمی‌کند. برای ارتباط خصوصی از راه دور، HTTPS را روی reverse proxy مانند IIS فعال و درخواست‌ها را همراه هدر کلید به listener محلی منتقل کنید. listener داخلی HTTP است؛ گزینهٔ HTTPS در فیلترها به قابلیت پروکسی بررسی‌شده اشاره دارد.

### مسیرها، احراز هویت و فیلترها

مسیر / صفحهٔ راهنما و فرم ساخت لینک است؛ /health فقط وضعیت listener را نشان می‌دهد. مسیرهای /api/v1/proxies، /proxies.txt و /api/v1/stats به‌ترتیب خروجی JSON، فهرست متنی و آمار را می‌دهند. فقط GET و HEAD پشتیبانی می‌شود و API قابلیت تغییر تنظیمات یا شروع بررسی از راه دور ندارد.

کلید را با هدر **X-Api-Key** یا **Authorization: Bearer** بفرستید. کلید داخل URL پذیرفته نمی‌شود. در حالت خصوصی، لینک سادهٔ مرورگر بدون هدر کلید خطای ۴۰۱ می‌دهد؛ لینک مستقیم و فرم صفحهٔ راهنما برای حالت عمومی مناسب‌اند. نمونه‌های curl و PowerShell در همین فایل آمده‌اند.

- **format:** مقدار json یا txt.
- **type:** مقدار http، socks4، socks5، ترکیب آن‌ها با ویرگول یا all.
- **country:** کد دوحرفی مانند US,DE؛ مقدار all برای همه و ZZ برای کشور نامشخص.
- **https:** مقدار true برای HTTPS تأییدشده که پیش‌فرض است؛ false برای HTTP-only؛ all برای تمام نتایج فعال.
- **limit:** از ۱ تا ۱۰ هزار، پیش‌فرض ۱۰۰۰؛ **offset:** از صفر تا ۵۰۰ هزار برای صفحه‌بندی.
- **max_ping:** سقف زمان اتصال TCP بر حسب میلی‌ثانیه، از صفر تا ۳۶۰۰۰۰۰؛ با این فیلتر، پینگ نامشخص حذف می‌شود.
- **max_age_hours:** عدد مثبت و حداکثر برابر عمر مجاز تنظیم‌شدهٔ سرور.
- **sort:** مرتب‌سازی بر اساس endpoint یا ping؛ پینگ نامشخص در انتها قرار می‌گیرد.

پارامتر ناشناخته، تکراری یا نامعتبر خطای ۴۰۰ می‌دهد. کشور به IP سرور پروکسی مربوط است و موقعیت خروجی را تضمین نمی‌کند. هر آدرس یک بار با پروتکل موفق HTTPS، یا در صورت نبود آن با پروتکل موفق HTTP، برگردانده می‌شود؛ تمام پروتکل‌های احتمالی یک آدرس فهرست نمی‌شوند.

خروجی JSON شامل شمار کل نتایج، صفحهٔ فعلی، nextOffset، revision و زمان انتشار است. هر رکورد آدرس، پروتکل، کشور، وضعیت HTTP و HTTPS، زمان‌های TCP/HTTP/HTTPS و زمان بررسی را دارد. عدد منفی یعنی زمان اندازه‌گیری در دسترس نیست. موفقیت HTTP و HTTPS ممکن است با پروتکل‌های متفاوت باشد؛ فیلد http موفقیت HTTP روی آن آدرس را نشان می‌دهد.

در خروجی متنی هر خط یک IP:port است. هدرهای X-Total-Count و X-Snapshot-Revision در هر دو قالب وجود دارند. برای صفحهٔ بعد از nextOffset یا افزایش offset استفاده کنید. اگر revision تغییر کرد، دریافت صفحه‌ها را از ابتدا شروع کنید؛ انقضای نتایج نیز ممکن است هنگام صفحه‌بندی رخ دهد. نسخهٔ ثابت چنددرخواستی رزرو نمی‌شود.

### نگهداری نتایج و مانیتورینگ

تا پایان بررسی جدید، انتشار قبلی با رعایت زمان انقضا در دسترس می‌ماند. لغو، خطای دور یا نبود هیچ کاندیدی انتشار قبلی را حفظ می‌کند؛ دور کامل با صفر نتیجهٔ فعال، فهرست خالی منتشر می‌کند. خرابی بخشی از منابع مانع استفاده از منابع موفق نمی‌شود و در گزارش ثبت می‌شود.

تنظیمات و انتشار در پوشهٔ LocalAppData برنامه ذخیره می‌شوند. خروجی دورهای خودکار در **Server results** با زمان UTC، پوشه‌های جداگانهٔ HTTPS و HTTP_ONLY و فایل **Full Detail.txt** قرار می‌گیرد. تاریخچهٔ ۵۰ دور اخیر و شمارندهٔ درخواست‌ها مربوط به نشست فعلی سرور است و با شروع دوباره صفر می‌شود؛ انتشار و تنظیمات باقی می‌مانند.

حد سرور ۳۲ درخواست هم‌زمان و ۱۲۰ درخواست در ثانیه به‌صورت مجموع است. ترافیک اضافه پاسخ ۴۲۹ با Retry-After: 1 می‌گیرد و مهلت هر پاسخ ۱۵ ثانیه است. وضعیت‌های ۴۰۱، ۴۰۴، ۴۰۵، ۴۱۴ و ۵۰۰ به‌ترتیب مربوط به کلید، مسیر، متد، طول درخواست و خطای پاسخ هستند. سلامت listener به معنی سالم یا تازه بودن تمام پروکسی‌ها نیست.

</div>
