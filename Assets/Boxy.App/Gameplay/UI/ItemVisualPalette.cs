using UnityEngine;

namespace Boxy.App.Gameplay.UI
{
    // 아이템 시각화 — itemKey → 컬러 + 이모지.
    // LevelGenerator는 "book_0", "notebook_1" 형식 (suffix _N) → prefix만 사용.
    // mound-design-system §2 카테고리별 톤: 학용품(따뜻), 이사박스(차분), 여행(시원).
    public static class ItemVisualPalette
    {
        // Cozy 톤 (#FFD60A 액센트와 어울리는 채도) — boxy-final-checklist §디자인.
        static readonly Color Apricot = new Color32(0xFF, 0xB3, 0x88, 0xFF);
        static readonly Color Coral = new Color32(0xFF, 0x8E, 0x7A, 0xFF);
        static readonly Color Honey = new Color32(0xFF, 0xD6, 0x0A, 0xFF);
        static readonly Color Mint = new Color32(0x9D, 0xE0, 0xC1, 0xFF);
        static readonly Color Sage = new Color32(0xB8, 0xD0, 0xA8, 0xFF);
        static readonly Color Sky = new Color32(0xA8, 0xD0, 0xE8, 0xFF);
        static readonly Color Lavender = new Color32(0xC4, 0xB5, 0xE0, 0xFF);
        static readonly Color Rose = new Color32(0xE8, 0xB0, 0xC0, 0xFF);
        static readonly Color Cream = new Color32(0xF5, 0xE6, 0xC8, 0xFF);
        static readonly Color Terracotta = new Color32(0xC8, 0x88, 0x6B, 0xFF);
        static readonly Color Olive = new Color32(0xA8, 0xA8, 0x70, 0xFF);
        static readonly Color Plum = new Color32(0xA0, 0x70, 0x90, 0xFF);

        public static Color GetColor(string itemKey)
        {
            string prefix = StripSuffix(itemKey);
            switch (prefix)
            {
                // 학용품
                case "book": return Coral;
                case "notebook": return Sky;
                case "pencil_case": return Honey;
                case "lunchbox": return Mint;
                case "water_bottle": return Sky;
                case "apple": return Coral;
                case "snack": return Apricot;
                case "ruler": return Honey;
                case "calculator": return Lavender;
                case "headphones": return Plum;
                case "socks_pe": return Rose;
                case "boxy_keychain": return Honey;

                // 이사박스
                case "frame": return Terracotta;
                case "lamp": return Cream;
                case "vase": return Sage;
                case "books_stack": return Coral;
                case "plates": return Cream;
                case "blanket": return Lavender;
                case "cushion": return Rose;
                case "speaker": return Plum;
                case "candle": return Apricot;
                case "plant": return Sage;
                case "magazines": return Sky;
                case "boxy_figure": return Honey;

                // 여행 트렁크
                case "tshirt": return Sky;
                case "jeans": return Lavender;
                case "sweater": return Coral;
                case "hat": return Honey;
                case "passport": return Terracotta;
                case "camera": return Plum;
                case "shoes": return Olive;
                case "toiletries": return Mint;
                case "sunglasses": return Lavender;
                case "guidebook": return Apricot;
                case "umbrella": return Sky;
                case "boxy_souvenir": return Honey;

                default: return Honey;
            }
        }

        public static string GetEmoji(string itemKey)
        {
            string prefix = StripSuffix(itemKey);
            switch (prefix)
            {
                // 학용품
                case "book": return "📖";
                case "notebook": return "📓";
                case "pencil_case": return "✏️";
                case "lunchbox": return "🍱";
                case "water_bottle": return "💧";
                case "apple": return "🍎";
                case "snack": return "🍪";
                case "ruler": return "📏";
                case "calculator": return "🧮";
                case "headphones": return "🎧";
                case "socks_pe": return "🧦";
                case "boxy_keychain": return "🧸";

                // 이사박스
                case "frame": return "🖼";
                case "lamp": return "💡";
                case "vase": return "🏺";
                case "books_stack": return "📚";
                case "plates": return "🍽";
                case "blanket": return "🛏";
                case "cushion": return "🪑";
                case "speaker": return "🔊";
                case "candle": return "🕯";
                case "plant": return "🌱";
                case "magazines": return "📰";
                case "boxy_figure": return "🎨";

                // 여행 트렁크
                case "tshirt": return "👕";
                case "jeans": return "👖";
                case "sweater": return "🧥";
                case "hat": return "🧢";
                case "passport": return "📕";
                case "camera": return "📷";
                case "shoes": return "👟";
                case "toiletries": return "🧴";
                case "sunglasses": return "🕶";
                case "guidebook": return "📔";
                case "umbrella": return "☂";
                case "boxy_souvenir": return "🎁";

                default: return "📦";
            }
        }

        // "book_0" → "book", "boxy_keychain_3" → "boxy_keychain"
        // 끝의 "_N"만 제거 (중간 underscore는 유지)
        static string StripSuffix(string itemKey)
        {
            if (string.IsNullOrEmpty(itemKey)) return itemKey;
            int last = itemKey.LastIndexOf('_');
            if (last < 0) return itemKey;
            string after = itemKey.Substring(last + 1);
            // 마지막 "_뒤"가 모두 숫자면 잘라냄
            for (int i = 0; i < after.Length; i++)
            {
                if (after[i] < '0' || after[i] > '9') return itemKey;
            }
            return itemKey.Substring(0, last);
        }
    }
}
