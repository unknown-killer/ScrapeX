"""Package a built application or a documentation-only update, with matching bilingual guides.

README.md is the single source for both languages. Run from the repository root:
  python tools/package-release.py --version 1.0.1 --portable-base PATH_TO_EXISTING_ZIP
For an application release, use --app-dir ScrapeX/bin/Release instead of --portable-base.
"""
import argparse
import hashlib
import re
import subprocess
from pathlib import Path
from zipfile import ZipFile, ZIP_DEFLATED


def plain_guide(markdown):
    text = re.sub(r"<img\b[^>]*>", "", markdown)
    text = re.sub(r'<a\s+href="([^"]+)"[^>]*>(.*?)</a>',
                  lambda m: m[2] + " (" + m[1] + ")", text)
    text = re.sub(r"</?(?:div|p|h1|strong|a|details|summary)\b[^>]*>", "", text)
    text = re.sub(r"!\[([^]]*)\]\([^)]+\)", r"\1", text)
    text = re.sub(r"\[([^]]+)\]\(([^)]+)\)",
                  lambda m: m[1] + " (" + m[2] + ")", text)
    text = re.sub(r"(?m)^#{1,6} ", "", text)
    text = re.sub(r"(?m)^```[^\n]*\n?", "", text)
    text = text.replace("**", "").replace("`", "")
    text = re.sub(r"\n[ \t]*\n(?:[ \t]*\n)+", "\n\n", text)
    return text.strip() + "\n"


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--version", required=True)
    mode = parser.add_mutually_exclusive_group(required=True)
    mode.add_argument("--portable-base", type=Path)
    mode.add_argument("--app-dir", type=Path)
    args = parser.parse_args()
    if not re.fullmatch(r"\d+\.\d+\.\d+", args.version):
        parser.error("Version must have the form X.Y.Z")
    root = Path(__file__).resolve().parents[1]
    readme = (root / "README.md").read_bytes()
    guide = plain_guide(readme.decode("utf-8-sig"))
    if "فارسی — اسکرپ ایکس" not in guide or "Built-in proxy discovery" not in guide:
        raise ValueError("Both language sections are required")
    guide_path = root / "docs/USER-GUIDE.txt"
    guide_path.write_text(guide, encoding="utf-8-sig", newline="\n")
    guide_bytes = guide_path.read_bytes()
    output = root / "outputs" / ("release-v" + args.version)
    output.mkdir(parents=True, exist_ok=True)
    portable = output / ("ScrapeX-v" + args.version + "-windows-portable.zip")
    source = output / ("ScrapeX-v" + args.version + "-source.zip")
    if args.portable_base and args.portable_base.resolve() in (portable.resolve(), source.resolve()):
        raise ValueError("Use a separate baseline archive")
    supplemental = [str(p.relative_to(root)).replace("\\", "/") for p in (root / "docs").rglob("*") if p.is_file()]
    supplemental += ["THIRD-PARTY-NOTICES.md", "ScrapeX/Resources/scrapex-logo.png"]
    replacements = {"ScrapeX/README.md": readme, "ScrapeX/راهنما.txt": guide_bytes}
    replacements.update({"ScrapeX/" + p: (root / p).read_bytes() for p in supplemental})
    payload = {}
    if args.app_dir:
        for name in ("ScrapeX.exe", "ScrapeX.exe.config", "Newtonsoft.Json.dll", "sources.txt"):
            payload["ScrapeX/" + name] = (args.app_dir / name).read_bytes()
        assert payload["ScrapeX/sources.txt"] == (root / "ScrapeX/sources.txt").read_bytes()
    else:
        with ZipFile(args.portable_base) as old:
            payload = {n: old.read(n) for n in old.namelist() if n not in replacements}
    with ZipFile(portable, "w", ZIP_DEFLATED) as new:
        for name, data in {**payload, **replacements}.items():
            new.writestr(name, data)
    files = subprocess.check_output(["git", "ls-files", "-z"], cwd=root).decode().split("\0")
    files = set(filter(None, files)) | {"docs/USER-GUIDE.txt", "tools/package-release.py"}
    prefix = "ScrapeX-" + args.version + "/"
    with ZipFile(source, "w", ZIP_DEFLATED) as package:
        for name in sorted(files):
            package.write(root / name, prefix + name)
        package.writestr(prefix + "راهنما.txt", guide_bytes)
    # Validate package contents, both language copies, and unchanged payloads.
    with ZipFile(portable) as new, ZipFile(source) as src:
        assert new.testzip() is None and src.testzip() is None
        assert len(new.namelist()) == len(set(new.namelist()))
        assert len(src.namelist()) == len(set(src.namelist()))
        for name, data in payload.items():
            assert data == new.read(name), name
        assert new.read("ScrapeX/README.md") == src.read(prefix + "README.md") == readme
        assert new.read("ScrapeX/راهنما.txt") == src.read(prefix + "راهنما.txt") == guide_bytes
        assert src.read(prefix + "docs/USER-GUIDE.txt") == guide_bytes
    sums = "".join(hashlib.sha256(p.read_bytes()).hexdigest() + "  " + p.name + "\n"
                   for p in (source, portable))
    (output / "SHA256SUMS.txt").write_text(sums, encoding="ascii", newline="\n")
    print("Verified: bilingual guides match, archive integrity passes, application payload matches selected build/baseline.")
    print(output)
    print(sums)


if __name__ == "__main__":
    main()
