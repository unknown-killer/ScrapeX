"""Download public discovery candidates for validation with ScrapeX's production parser.
No credentials, browser state, or access-control bypasses. Outputs stay under outputs/.
"""
import concurrent.futures, datetime, hashlib, html, json, re, urllib.parse, urllib.request
from pathlib import Path

OUT=Path('outputs/source-audit-2026-11-13'); OUT.mkdir(parents=True,exist_ok=True)
SITES=[
 'https://proxywing.com/zh/free-proxy-list', 'https://freeproxyhub.net/zh/',
 'https://www.ipxss.com/free-proxy', 'https://remproxy.com/cn/freeproxy',
 'https://proxyan.net/zh/free-proxy', 'https://privasto.com/es/free-proxy-list',
 'https://proxy5.net/es/free-proxy', 'https://www.flashproxy.com/ru/resources/free-proxies',
 'https://proxybros.com/ru/free-proxy-list/', 'https://proxylister.com/free-proxies',
 'https://zj.v.api.aa1.cn/api/proxyip/?type=json',
 'https://hproxy.com/docs/free-proxy-list', 'https://api.proxyscrape.com/v4/free-proxy-list/get?request=displayproxies&proxy_format=protocolipport&format=text',
]
REPOS=['Moleway/Free-Proxy-List','hproxy-com/free-proxy-list','gfpcom/free-proxy-list',
 'xyzs996/free-proxy-health-list','maximilianfeix/free-proxy-list','gproxynet/free-proxy-list',
 'monosans/proxy-list','databay-labs/free-proxy-list','relayglass/free-proxy-list',
 'litportnet/free-proxy-list','maximilianfeix/proxy-scraper','ProxyScrape/free-proxy-list']
def get(url,limit=16*1024*1024):
    req=urllib.request.Request(url,headers={'User-Agent':'ScrapeX-source-audit/1.1 (+https://github.com/unknown-killer/ScrapeX)','Accept':'*/*'})
    with urllib.request.urlopen(req,timeout=20) as response:
        data=response.read(limit+1)
        if len(data)>limit: raise ValueError('Response exceeds 16 MiB')
        return data,response.geturl(),dict(response.headers)
def download(url):
    record={'url':url,'checked_utc':datetime.datetime.now(datetime.timezone.utc).isoformat()}
    try:
        data,final,headers=get(url)
        name=hashlib.sha256(url.encode()).hexdigest()[:24]+'.body'
        (OUT/name).write_bytes(data)
        record.update(status=200,final_url=final,file=name,bytes=len(data),sha256=hashlib.sha256(data).hexdigest())
    except Exception as ex: record.update(status='unavailable',error=str(ex))
    return record
def canonical(url): return url.replace('/refs/heads/','/')
def main():
    existing=[s.strip() for s in Path('ScrapeX/sources.txt').read_text(encoding='utf-8-sig').splitlines() if s.startswith('http')]
    candidates=set(existing+SITES); discoveries=[]
    pages=SITES+['https://raw.githubusercontent.com/'+repo+'/HEAD/README.md' for repo in REPOS]
    with concurrent.futures.ThreadPoolExecutor(max_workers=6) as pool:
        for result in pool.map(download,pages):
            discoveries.append(result)
            if result.get('status')!=200: continue
            body=(OUT/result['file']).read_text(encoding='utf-8',errors='replace')
            # Read links/data literals only; never run a site's JavaScript.
            for raw in re.findall(r'https?://[^\s<>"\x27`\\)\]}]+',body):
                url=html.unescape(raw).rstrip('.,;')
                if 'raw.githubusercontent.com/' in url and re.search(r'\.(txt|json)(?:\?|$)',url,re.I) and not any(s in url.lower() for s in ['countries/','anonymity/','ssr','vmess','trojan','vless','wireguard','hysteria','mtproto','sitemap','package.json']): candidates.add(url)
            for raw in re.findall(r'(?:href|action)\s*=\s*["\x27]([^"\x27]+)',body,re.I):
                url=urllib.parse.urljoin(result.get('final_url',result['url']),html.unescape(raw))
                if urllib.parse.urlparse(url).hostname==urllib.parse.urlparse(result['url']).hostname and re.search(r'(?:download|export|/api/)',url,re.I) and any(s in url.lower() for s in ['txt','json','proxy','proxies']): candidates.add(url)
    ordered=[];seen=set()
    for url in existing+sorted(candidates):
        if canonical(url) not in seen and '{' not in url and '$' not in url and len(url)<1000:
            seen.add(canonical(url));ordered.append(url)
    (OUT/'discovery.json').write_text(json.dumps(discoveries,ensure_ascii=False,indent=2),encoding='utf-8')
    print('Existing:',len(existing),'Candidates:',len(ordered),flush=True)
    records=[]
    with concurrent.futures.ThreadPoolExecutor(max_workers=6) as pool:
        for i,result in enumerate(pool.map(download,ordered)):
            result['new']=canonical(result['url']) not in {canonical(u) for u in existing};records.append(result)
            if (i+1)%25==0: print('Downloaded',i+1,'/',len(ordered),flush=True)
    (OUT/'downloads.json').write_text(json.dumps(records,ensure_ascii=False,indent=2),encoding='utf-8')
    print('Saved',len(records),'audits to',OUT,flush=True)
if __name__=='__main__': main()
