using System.Collections.Generic;

namespace Boxy.App.Levels.Generation
{
    public static class ItemKeyPool
    {
        public static IReadOnlyList<string> SchoolBag { get; } = new[]
        {
            "book", "notebook", "pencil_case", "lunchbox", "water_bottle", "apple", "snack",
            "ruler", "calculator", "headphones", "socks_pe", "boxy_keychain"
        };

        public static IReadOnlyList<string> MovingBox { get; } = new[]
        {
            "frame", "lamp", "vase", "books_stack", "plates", "blanket", "cushion",
            "speaker", "candle", "plant", "magazines", "boxy_figure"
        };

        public static IReadOnlyList<string> TravelTrunk { get; } = new[]
        {
            "tshirt", "jeans", "sweater", "hat", "passport", "camera", "shoes",
            "toiletries", "sunglasses", "guidebook", "umbrella", "boxy_souvenir"
        };

        public static IReadOnlyList<string> ForTheme(string themeKey) => themeKey switch
        {
            "school_bag" => SchoolBag,
            "moving_box" => MovingBox,
            "travel_trunk" => TravelTrunk,
            _ => SchoolBag
        };
    }
}
