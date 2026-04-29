#!/usr/bin/env python3
"""
Stitch HTML(Tailwind) → Unity UXML/USS 자동 변환.

입력: design/stitch-generated/<name>.html (Tailwind + Material Symbols)
출력: Assets/Boxy.App/UI/Stitch/<Name>.uxml + 별도 USS 보조 파일

매핑 룰:
- HTML 태그 → UXML 태그 (div→VisualElement, button→Button, h1-h6→Label, p→Label)
- Tailwind 색상 → 인라인 USS color
- bg-primary=#FFD60A / bg-warmBeige=#FFF8F0 / text-onAccent=#1A1A1A 등
- flex/items-center/justify-center → display:flex + align-items + justify-content
- padding/margin/gap → padding/margin (UI Toolkit는 gap 미지원 → 컴포넌트별 margin 처리)
- rounded-xl(12px) / rounded-2xl(20px) / rounded-full(9999px)
- font-bold → -unity-font-style: bold

자동 변환 어려운 부분 → 변환 후 주석 // [STITCH-MANUAL] 로 마킹:
- Material Symbols (아이콘 PNG로 교체 필요)
- <img src="https://...> (로컬 자산으로 교체)
- box-shadow (USS 미지원, 별도 element 시뮬레이션)
- group/hover/active/transition 일부

사용:
  python3 scripts/stitch_to_uxml.py main-menu
  python3 scripts/stitch_to_uxml.py --all
"""
import re
import sys
import os
from pathlib import Path
from html.parser import HTMLParser

PROJECT_ROOT = Path(__file__).resolve().parent.parent
INPUT_DIR = PROJECT_ROOT / "design" / "stitch-generated"
OUTPUT_DIR = PROJECT_ROOT / "Assets" / "Boxy.App" / "UI" / "Stitch"

# Tailwind 색상 매핑 (Boxy design system)
COLOR_MAP = {
    "primary": "#FFD60A",
    "onAccent": "#1A1A1A",
    "bgLight": "#F8F9FA",
    "bgSurface": "#FFFFFF",
    "border": "#E5E7EB",
    "textPrimary": "#1A1A1A",
    "textSecondary": "#6B7280",
    "success": "#4ADE80",
    "warning": "#FBBF24",
    "error": "#EF4444",
    "warmBeige": "#FFF8F0",
    "white": "#FFFFFF",
    "black": "#000000",
    "gray-50": "#F9FAFB",
    "gray-100": "#F3F4F6",
    "gray-200": "#E5E7EB",
    "gray-300": "#D1D5DB",
    "gray-400": "#9CA3AF",
    "gray-500": "#6B7280",
    "gray-600": "#4B5563",
    "gray-700": "#374151",
    "gray-800": "#1F2937",
    "gray-900": "#111827",
    "yellow-400": "#FACC15",
    "yellow-500": "#EAB308",
    "red-500": "#EF4444",
    "red-600": "#DC2626",
    "green-500": "#22C55E",
    "blue-500": "#3B82F6",
}

# Tailwind 사이즈 (rem 기준 1rem=16px)
SIZE_MAP = {
    "0": "0",
    "0.5": "2px", "1": "4px", "1.5": "6px",
    "2": "8px", "2.5": "10px", "3": "12px",
    "4": "16px", "5": "20px", "6": "24px",
    "7": "28px", "8": "32px", "10": "40px",
    "12": "48px", "14": "56px", "16": "64px",
    "20": "80px", "24": "96px", "32": "128px",
    "40": "160px", "48": "192px", "64": "256px",
    "80": "320px", "96": "384px",
}

# Border radius
RADIUS_MAP = {
    "rounded": "8px",
    "rounded-sm": "4px",
    "rounded-md": "8px",
    "rounded-lg": "12px",
    "rounded-xl": "12px",
    "rounded-2xl": "20px",
    "rounded-3xl": "24px",
    "rounded-full": "9999px",
    "rounded-t-xl": "12px",  # top-only — UXML에선 4 corners 별도로 처리 X (단순화)
    "rounded-t-[20px]": "20px",
}

# Font sizes
FONT_SIZE_MAP = {
    "text-xs": "12px",
    "text-sm": "14px",
    "text-base": "16px",
    "text-lg": "18px",
    "text-xl": "20px",
    "text-2xl": "24px",
    "text-3xl": "30px",
    "text-4xl": "36px",
    "text-5xl": "48px",
}


def parse_size(token, prefix):
    """Tailwind 사이즈 토큰 → px. e.g. p-4, w-12, h-[56px]"""
    # 임의 값 [56px]
    m = re.match(rf"{prefix}\[(.+?)\]", token)
    if m:
        return m.group(1)
    # 표준 값 p-4 등
    m = re.match(rf"{prefix}(.+)", token)
    if m and m.group(1) in SIZE_MAP:
        return SIZE_MAP[m.group(1)]
    return None


def class_to_style(classes):
    """Tailwind 클래스 → USS 인라인 스타일 dict + 변환 못 한 토큰 리스트."""
    style = {}
    unmapped = []

    for cls in classes:
        # bg-* (color)
        m = re.match(r"bg-([\w\-]+)$", cls)
        if m and m.group(1) in COLOR_MAP:
            style["background-color"] = COLOR_MAP[m.group(1)]
            continue
        # bg-[#hex]
        m = re.match(r"bg-\[(#[0-9A-Fa-f]{3,8})\]$", cls)
        if m:
            style["background-color"] = m.group(1)
            continue
        # bg-color/opacity (e.g. bg-primary/20)
        m = re.match(r"bg-([\w\-]+)/(\d+)$", cls)
        if m and m.group(1) in COLOR_MAP:
            hexc = COLOR_MAP[m.group(1)]
            opacity = int(m.group(2)) / 100.0
            style["background-color"] = hex_to_rgba(hexc, opacity)
            continue
        # text-* (color or size)
        m = re.match(r"text-([\w\-]+)$", cls)
        if m:
            v = m.group(1)
            if v in COLOR_MAP:
                style["color"] = COLOR_MAP[v]
                continue
            if cls in FONT_SIZE_MAP:
                style["font-size"] = FONT_SIZE_MAP[cls]
                continue
        # text-[#hex]
        m = re.match(r"text-\[(#[0-9A-Fa-f]{3,8})\]$", cls)
        if m:
            style["color"] = m.group(1)
            continue
        # font-bold/extrabold/semibold
        if cls in ("font-bold", "font-extrabold", "font-black"):
            style["-unity-font-style"] = "bold"
            continue
        # padding p-N / px-N / py-N / pt-N etc.
        for prefix, props in [
            ("p-", ["padding"]),
            ("px-", ["padding-left", "padding-right"]),
            ("py-", ["padding-top", "padding-bottom"]),
            ("pt-", ["padding-top"]),
            ("pb-", ["padding-bottom"]),
            ("pl-", ["padding-left"]),
            ("pr-", ["padding-right"]),
        ]:
            if cls.startswith(prefix):
                v = parse_size(cls, prefix)
                if v:
                    for p in props:
                        style[p] = v
                    break
        # margin
        for prefix, props in [
            ("m-", ["margin"]),
            ("mx-", ["margin-left", "margin-right"]),
            ("my-", ["margin-top", "margin-bottom"]),
            ("mt-", ["margin-top"]),
            ("mb-", ["margin-bottom"]),
            ("ml-", ["margin-left"]),
            ("mr-", ["margin-right"]),
        ]:
            if cls.startswith(prefix):
                v = parse_size(cls, prefix)
                if v:
                    for p in props:
                        style[p] = v
                    break
        # width/height
        for prefix, prop in [("w-", "width"), ("h-", "height"),
                             ("max-w-", "max-width"), ("min-w-", "min-width"),
                             ("max-h-", "max-height"), ("min-h-", "min-height")]:
            if cls.startswith(prefix):
                v = parse_size(cls, prefix)
                if v:
                    style[prop] = v
                    break
                # w-full
                if cls == prefix + "full":
                    style[prop] = "100%"
                # w-screen
                elif cls == prefix + "screen":
                    style[prop] = "100%"
                # w-[80%]
                m = re.match(rf"{prefix}\[(.+?)\]$", cls)
                if m:
                    style[prop] = m.group(1)
                    break
        # rounded
        if cls in RADIUS_MAP:
            r = RADIUS_MAP[cls]
            style["border-top-left-radius"] = r
            style["border-top-right-radius"] = r
            style["border-bottom-left-radius"] = r
            style["border-bottom-right-radius"] = r
            continue
        # rounded-[Npx]
        m = re.match(r"rounded-\[(.+?)\]$", cls)
        if m:
            r = m.group(1)
            style["border-top-left-radius"] = r
            style["border-top-right-radius"] = r
            style["border-bottom-left-radius"] = r
            style["border-bottom-right-radius"] = r
            continue
        # flex / flex-col / flex-row
        if cls == "flex":
            style["display"] = "flex"
            continue
        if cls == "flex-col":
            style["flex-direction"] = "column"
            continue
        if cls == "flex-row":
            style["flex-direction"] = "row"
            continue
        if cls == "flex-1":
            style["flex-grow"] = "1"
            continue
        if cls == "flex-grow":
            style["flex-grow"] = "1"
            continue
        if cls == "flex-shrink-0":
            style["flex-shrink"] = "0"
            continue
        # items-center / justify-center
        if cls == "items-center":
            style["align-items"] = "center"
            continue
        if cls == "items-start":
            style["align-items"] = "flex-start"
            continue
        if cls == "items-end":
            style["align-items"] = "flex-end"
            continue
        if cls == "items-stretch":
            style["align-items"] = "stretch"
            continue
        if cls == "justify-center":
            style["justify-content"] = "center"
            continue
        if cls == "justify-between":
            style["justify-content"] = "space-between"
            continue
        if cls == "justify-around":
            style["justify-content"] = "space-around"
            continue
        if cls == "justify-end":
            style["justify-content"] = "flex-end"
            continue
        # gap-N (UXML에서 gap 직접 미지원 — 자식 element margin으로 처리. 여기선 표시만)
        if cls.startswith("gap-"):
            unmapped.append(cls + " /* gap → 자식 margin으로 처리 필요 */")
            continue
        # space-y-N (자식 사이 vertical 간격)
        if cls.startswith("space-y-") or cls.startswith("space-x-"):
            unmapped.append(cls + " /* 자식 element 사이 간격 — 수동 처리 */")
            continue
        # opacity-N
        m = re.match(r"opacity-(\d+)$", cls)
        if m:
            style["opacity"] = str(int(m.group(1)) / 100)
            continue
        # absolute / relative / fixed
        if cls == "absolute" or cls == "fixed":
            style["position"] = "absolute"
            continue
        if cls == "relative":
            style["position"] = "relative"
            continue
        # top/bottom/left/right
        for prefix, prop in [("top-", "top"), ("bottom-", "bottom"),
                             ("left-", "left"), ("right-", "right")]:
            if cls.startswith(prefix):
                v = parse_size(cls, prefix)
                if v:
                    style[prop] = v
                    break
                if cls == prefix + "0":
                    style[prop] = "0"
        # border / border-N
        if cls == "border":
            style["border-top-width"] = "1px"
            style["border-bottom-width"] = "1px"
            style["border-left-width"] = "1px"
            style["border-right-width"] = "1px"
            continue
        m = re.match(r"border-(\d+)$", cls)
        if m:
            w = m.group(1) + "px"
            style["border-top-width"] = w
            style["border-bottom-width"] = w
            style["border-left-width"] = w
            style["border-right-width"] = w
            continue
        m = re.match(r"border-\[(\d+px)\]$", cls)
        if m:
            w = m.group(1)
            style["border-top-width"] = w
            style["border-bottom-width"] = w
            style["border-left-width"] = w
            style["border-right-width"] = w
            continue
        # border-color (border-onAccent)
        m = re.match(r"border-([\w\-]+)$", cls)
        if m and m.group(1) in COLOR_MAP:
            c = COLOR_MAP[m.group(1)]
            style["border-top-color"] = c
            style["border-bottom-color"] = c
            style["border-left-color"] = c
            style["border-right-color"] = c
            continue
        # text-center / text-left
        if cls == "text-center":
            style["-unity-text-align"] = "middle-center"
            continue
        if cls == "text-left":
            style["-unity-text-align"] = "middle-left"
            continue
        if cls == "text-right":
            style["-unity-text-align"] = "middle-right"
            continue
        # 변환 못 한 토큰
        unmapped.append(cls)
    return style, unmapped


def hex_to_rgba(hexc, opacity):
    h = hexc.lstrip("#")
    if len(h) == 3:
        h = "".join(c * 2 for c in h)
    r, g, b = int(h[0:2], 16), int(h[2:4], 16), int(h[4:6], 16)
    return f"rgba({r}, {g}, {b}, {opacity})"


def style_to_uxml_string(style_dict):
    """USS dict → 인라인 style 문자열."""
    return "; ".join(f"{k}: {v}" for k, v in style_dict.items())


# HTML → UXML 태그 매핑
TAG_MAP = {
    "div": "ui:VisualElement",
    "main": "ui:VisualElement",
    "header": "ui:VisualElement",
    "footer": "ui:VisualElement",
    "nav": "ui:VisualElement",
    "section": "ui:VisualElement",
    "article": "ui:VisualElement",
    "aside": "ui:VisualElement",
    "button": "ui:Button",
    "h1": "ui:Label",
    "h2": "ui:Label",
    "h3": "ui:Label",
    "h4": "ui:Label",
    "h5": "ui:Label",
    "h6": "ui:Label",
    "p": "ui:Label",
    "span": "ui:Label",
    "label": "ui:Label",
    "img": "ui:VisualElement",  # background-image로 처리
    "ul": "ui:VisualElement",
    "ol": "ui:VisualElement",
    "li": "ui:VisualElement",
}

# UXML 출력에서 무시할 HTML 요소 (런타임 효과 없음)
SKIP_TAGS = {"script", "style", "link", "meta", "head", "title"}


class StitchHTMLParser(HTMLParser):
    def __init__(self):
        super().__init__(convert_charrefs=True)
        self.root_children = []
        self.stack = [self.root_children]  # 현재 자식 리스트 가리킴
        self.skip_depth = 0
        self.unmapped_total = []
        self.manual_warnings = []

    def handle_starttag(self, tag, attrs):
        if tag in SKIP_TAGS or self.skip_depth > 0:
            self.skip_depth += 1
            return
        attrs_dict = dict(attrs)
        classes = attrs_dict.get("class", "").split()
        style, unmapped = class_to_style(classes)
        self.unmapped_total.extend(unmapped)

        # 인라인 style 속성도 합치기
        inline_style = attrs_dict.get("style", "")
        if inline_style:
            for decl in inline_style.split(";"):
                if ":" in decl:
                    k, v = decl.split(":", 1)
                    style[k.strip()] = v.strip()

        # img 태그 → background-image
        if tag == "img":
            src = attrs_dict.get("src", "")
            alt = attrs_dict.get("alt", "")
            if src.startswith("https://"):
                self.manual_warnings.append(
                    f"<img src=\"{src[:50]}...\" alt=\"{alt}\"> → 로컬 PNG로 교체 필요")
                style["-unity-background-scale-mode"] = "scale-to-fit"

        # button text는 자식에서 수집
        node = {
            "tag": tag,
            "uxml_tag": TAG_MAP.get(tag, "ui:VisualElement"),
            "style": style,
            "classes": classes,
            "text": "",
            "children": [],
            "attrs": attrs_dict,
        }
        self.stack[-1].append(node)
        self.stack.append(node["children"])

    def handle_endtag(self, tag):
        if tag in SKIP_TAGS or self.skip_depth > 0:
            if self.skip_depth > 0:
                self.skip_depth -= 1
            return
        if len(self.stack) > 1:
            self.stack.pop()

    def handle_data(self, data):
        if self.skip_depth > 0:
            return
        text = data.strip()
        if not text:
            return
        # 현재 열린 노드 = stack[-2]의 마지막 자식 (handle_starttag에서 append + stack.append children)
        if len(self.stack) >= 2 and self.stack[-2]:
            cur = self.stack[-2][-1]
            if isinstance(cur, dict):
                cur["text"] = (cur["text"] + " " + text).strip() if cur["text"] else text

    def handle_startendtag(self, tag, attrs):
        # <br/> 같은 self-closing tag는 <br> 텍스트 줄바꿈으로 처리
        if tag == "br" and self.skip_depth == 0:
            if len(self.stack) >= 2 and self.stack[-2]:
                cur = self.stack[-2][-1]
                if isinstance(cur, dict):
                    cur["text"] = (cur["text"] + "\\n").strip() if cur["text"] else "\\n"
            return
        # img는 정상 start+end 처리
        self.handle_starttag(tag, attrs)
        self.handle_endtag(tag)


def material_symbol_warning(node):
    """Material Symbols span 감지."""
    cls = node.get("classes", [])
    if "material-symbols-outlined" in cls:
        return f"Material Symbols 아이콘 \"{node.get('text', '')}\" → 로컬 PNG 자산으로 교체 필요"
    return None


def escape_xml(s):
    """XML 속성 값 이스케이프."""
    return (s.replace("&", "&amp;")
             .replace("<", "&lt;")
             .replace(">", "&gt;")
             .replace('"', "&quot;"))


def render_uxml(node, indent=0, manual_warnings=None):
    """노드 트리 → UXML 문자열."""
    if manual_warnings is None:
        manual_warnings = []
    pad = "    " * indent
    tag = node["uxml_tag"]
    style_str = style_to_uxml_string(node["style"])

    # Material Symbols 경고 + 텍스트는 placeholder 이름으로 보존 (정수가 아이콘 매핑할 때 참고)
    msw = material_symbol_warning(node)
    if msw:
        manual_warnings.append(msw)
        icon_name = node.get("text", "").strip()
        # Material Symbols span은 빈 ui:Label로 두지 않고 name 속성으로 마킹
        attrs = []
        if style_str:
            attrs.append(f'style="{style_str}"')
        if icon_name:
            attrs.append(f'name="icon-{icon_name}"')
            attrs.append(f'tooltip="[STITCH-MANUAL] icon: {icon_name} → PNG 교체 필요"')
        attr_str = " " + " ".join(attrs) if attrs else ""
        return f'{pad}<ui:VisualElement{attr_str} />'

    attrs = []
    if style_str:
        attrs.append(f'style="{style_str}"')

    # 텍스트 있고 Label/Button인 경우 text 속성으로 (단, 자식이 텍스트 노드만 있을 때)
    has_only_text = node["text"] and not node["children"]
    if (tag == "ui:Button" or tag == "ui:Label") and node["text"]:
        attrs.append(f'text="{escape_xml(node["text"])}"')
    attr_str = " " + " ".join(attrs) if attrs else ""

    if not node["children"]:
        if node["text"] and tag not in ("ui:Button", "ui:Label"):
            # 텍스트 있는 div는 Label로 변환
            new_attrs = ([f'style="{style_str}"'] if style_str else []) + [f'text="{escape_xml(node["text"])}"']
            return f'{pad}<ui:Label {" ".join(new_attrs)} />'
        return f'{pad}<{tag}{attr_str} />'

    # 자식이 있지만 텍스트도 있는 경우 — 텍스트가 우선이면 Label로 (Button은 자식 X 권장)
    if node["text"] and tag in ("ui:Button", "ui:Label"):
        # text 속성 + 자식 element 둘 다 가능
        inner = "\n".join(render_uxml(c, indent + 1, manual_warnings) for c in node["children"])
        return f'{pad}<{tag}{attr_str}>\n{inner}\n{pad}</{tag}>'

    inner = "\n".join(render_uxml(c, indent + 1, manual_warnings) for c in node["children"])
    return f'{pad}<{tag}{attr_str}>\n{inner}\n{pad}</{tag}>'


def convert(name):
    """단일 화면 변환."""
    in_path = INPUT_DIR / f"{name}.html"
    out_path = OUTPUT_DIR / f"{capitalize(name)}.uxml"

    if not in_path.exists():
        print(f"❌ 입력 파일 없음: {in_path}")
        return False

    OUTPUT_DIR.mkdir(parents=True, exist_ok=True)

    html = in_path.read_text(encoding="utf-8")
    parser = StitchHTMLParser()
    parser.feed(html)

    # 가장 위 body 자식 노드들이 root_children에 있음
    # body 노드 자체가 들어 있을 수도 있고, 그냥 children일 수도 — 양쪽 처리
    roots = parser.root_children
    # body 안의 자식만 사용
    body_children = []
    for r in roots:
        if r.get("tag") in ("html", "body"):
            body_children.extend(r["children"])
        elif r.get("tag") in ("body",):
            body_children.extend(r["children"])
        else:
            body_children.append(r)

    # 한번 더 body 추출 시도
    if any(c.get("tag") == "body" for c in body_children):
        new_children = []
        for c in body_children:
            if c.get("tag") == "body":
                new_children.extend(c["children"])
            else:
                new_children.append(c)
        body_children = new_children

    manual_warnings = []
    rendered = "\n".join(render_uxml(c, indent=2, manual_warnings=manual_warnings)
                         for c in body_children)

    unmapped_uniq = sorted(set(parser.unmapped_total))
    unmapped_block = ""
    if unmapped_uniq:
        unmapped_block = "\n         " + "\n         ".join(
            f"// 변환 안 됨 Tailwind 토큰: {u}" for u in unmapped_uniq[:30])
    manual_block = ""
    if manual_warnings:
        manual_uniq = sorted(set(manual_warnings))
        manual_block = "\n         " + "\n         ".join(
            f"// [STITCH-MANUAL] {w}" for w in manual_uniq[:20])

    output = f"""<ui:UXML xmlns:ui="UnityEngine.UIElements" xmlns:uie="UnityEditor.UIElements" editor-extension-mode="False">
    <Style src="project://database/Assets/Mound/UI/Styles/tokens.uss" />
    <Style src="project://database/Assets/Mound/UI/Styles/components.uss" />
    <Style src="project://database/Assets/Boxy.App/UI/accent-boxy.uss" />

    <!-- 자동 변환 (scripts/stitch_to_uxml.py): {name}.html → {capitalize(name)}.uxml{unmapped_block}{manual_block}
         원본 Stitch screenshot: design/stitch-generated/{name}.png -->

    <ui:VisualElement name="root" style="flex-grow: 1; flex-direction: column; background-color: #FFF8F0;">
{rendered}
    </ui:VisualElement>
</ui:UXML>
"""
    out_path.write_text(output, encoding="utf-8")
    print(f"✅ {name} → {out_path.relative_to(PROJECT_ROOT)}")
    print(f"   변환 안 됨 토큰 {len(unmapped_uniq)}개, 수동 보정 {len(manual_warnings)}개")
    return True


def capitalize(name):
    """main-menu → MainMenu, level-select → LevelSelect"""
    return "".join(p.capitalize() for p in name.split("-"))


def main():
    if len(sys.argv) < 2:
        print(__doc__)
        sys.exit(1)

    if sys.argv[1] == "--all":
        names = [p.stem for p in INPUT_DIR.glob("*.html")]
        for n in sorted(names):
            convert(n)
    else:
        for n in sys.argv[1:]:
            convert(n)


if __name__ == "__main__":
    main()
