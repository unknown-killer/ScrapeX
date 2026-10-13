<div align="center">
  <img src="ScrapeX/Resources/scrapex-logo.png" alt="ScrapeX" width="420" />
  <h1>ScrapeX — Proxy Intelligence Workspace</h1>
  <p><strong>Collect. Verify. Organize. Export.</strong></p>
  <p>A modern Windows desktop workspace for public proxy discovery and HTTP / HTTPS verification.</p>
  <p>
    <img alt="Platform: Windows" src="https://img.shields.io/badge/platform-Windows-0078D4?style=flat-square" />
    <img alt="Framework: .NET 4.8" src="https://img.shields.io/badge/.NET-Framework%204.8-512BD4?style=flat-square" />
    <img alt="Language: C Sharp" src="https://img.shields.io/badge/language-C%23-239120?style=flat-square" />
    <img alt="Bundled sources: 168" src="https://img.shields.io/badge/bundled%20sources-168-36D6B5?style=flat-square" />
  </p>
  <p><a href="https://github.com/unknown-killer/ScrapeX/releases/latest"><strong>Download for Windows</strong></a> · <a href="#features">Features</a> · <a href="#build-from-source">Build from source</a> · <a href="#راهنمای-فارسی">فارسی</a></p>
</div>

![ScrapeX desktop interface](docs/screenshot.png)
*Interface preview with synthetic demonstration rows. Live proxy availability varies.*

## Features

| Capability | What ScrapeX does |
| --- | --- |
| **168 bundled sources** | Collect from public TXT feeds, JSON APIs and supported websites. Defaults are embedded, so a clean installation does not need a source file. |
| **HTTP · SOCKS4 · SOCKS5** | Detect protocol support with concurrent probes and display separate working counts. |
| **Independent HTTPS verification** | Test a real TLS connection through the proxy with certificate and hostname validation. HTTP-only results stay separate. |
| **Adaptive concurrency** | Separate TCP admission and protocol queues, bounded sockets, cancellation and automatic retry when local socket capacity is exhausted. |
| **Country intelligence** | ISO country codes, compact local flags and country names; multiple providers with direct/proxy fallback and a persistent positive cache. |
| **Measured latency** | TCP connection time plus separate HTTP and HTTPS probe timings. |
| **Flexible exports** | Export by country and protocol, split countries into separate files, or save a detailed report. |
| **Full Detail.txt** | Automatic working-result report with address, country, protocol, TLS verdict, timings and diagnostic details. |
| **Modern desktop UI** | Dark theme, a resizable results table, live counters and an editable Visual Studio WinForms Designer. |
| **Editable source catalog** | Add custom sources, disable entries, restore defaults and deduplicate endpoints across feeds. |

## Download & run

1. Download **ScrapeX-v1.0.0-windows-portable.zip** from [Releases](https://github.com/unknown-killer/ScrapeX/releases).
2. Extract the entire archive to a writable folder on Windows with **.NET Framework 4.8** installed.
3. Run `ScrapeX.exe`, keeping its DLL and configuration file alongside it.
4. Choose **Collect proxies**, import an endpoint list, or click **Check HTTP + TLS** to collect automatically when the list is empty.
5. Use **Working Proxy**, **Export HTTPS**, **Export HTTP only** and **Results folder** to inspect and save results.

Saved sessions and country cache live under `%LOCALAPPDATA%\ScrapeX`. A source returning addresses is not evidence that those proxies work; each endpoint must pass its own checks.

## Understanding the results

- **HTTPS VERIFIED:** a real HTTPS request passed through a certificate-validated TLS tunnel.
- **HTTP ONLY / TLS FAILED:** HTTP worked but HTTPS did not verify; these results never enter the HTTPS output.
- **Ping (TCP):** TCP connection setup time, not ICMP ping. **TLS ms** includes the successful HTTPS probe through its response headers.
- **Country:** location associated with the proxy server IP, not a guaranteed exit-IP location. Unavailable data stays unknown.
- **valid / new:** addresses parsed from a source versus addresses newly added to the current collection. Different sources often contain the same endpoints.

Supported proxy endpoints are public IPv4 addresses without authentication. HTTPS testing currently uses TLS 1.2. Network conditions, provider limits and changing public lists affect results; no checker can guarantee every healthy endpoint on every run.

## Build from source

Use Visual Studio with **.NET desktop development** and the **.NET Framework 4.8 targeting pack**.

1. Open `ScrapeX.sln`.
2. Restore NuGet packages (Newtonsoft.Json 13.0.3).
3. Build **Release / Any CPU**.
4. Find the application in `ScrapeX/bin/Release`.

Alternatively, from a Visual Studio Developer PowerShell:

```powershell
msbuild ScrapeX.sln /t:Restore /p:RestorePackagesConfig=true
msbuild ScrapeX.sln /p:Configuration=Release
.\tools\build-and-test.ps1
```

Use **View Designer** on `Form1.cs` and `ExportForm.cs` to edit controls visually. The default client size is 1440 × 1000; the result table expands with the window.

## Validation

The included suite contains **130 assertions** covering protocol handshakes, TLS rejection, timeout/cancellation, source parsing, exports, country resolution and UI behavior. The resilience suite processes **50,000 simulated endpoints** with injected TCP/protocol capacity errors. That is a deterministic stress test, not a claim of 50,000 working internet proxies.

The bundled catalog was audited on **2026-10-07**. Detailed audit documents ship with the portable release; source freshness can change after that date. Technical implementation notes are in [docs/TECHNICAL.md](docs/TECHNICAL.md).

## راهنمای فارسی

اسکرپ ایکس برای جمع‌آوری، بررسی و دسته‌بندی پروکسی‌های عمومی ساخته شده است. از بخش **Releases** نسخهٔ Portable را دانلود و همهٔ فایل‌ها را استخراج کنید. برای اجرا ویندوز و دات‌نت فریم‌ورک ۴٫۸ لازم است.

- تست HTTPS/SSL جداگانه انجام می‌شود و خروجی پروکسی‌های بدون HTTPS از بقیه جداست.
- کشور، کد کشور، پرچم محلی و زمان اتصال در جدول دیده می‌شوند.
- خروجی بر اساس کشور و نوع پروکسی و فایل **Full Detail.txt** در دسترس است.
- کمبود سوکت باعث ثبت نتیجهٔ خراب برای پروکسی نمی‌شود؛ فشار اتصال کاهش یافته و همان مورد دوباره امتحان می‌شود.
- کنترل‌های رابط کاربری در Designer ویژوال استودیو قابل ویرایش‌اند.

## Third-party components

See [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md) for the bundled dependency notice. No project-wide license is declared in this initial release.
