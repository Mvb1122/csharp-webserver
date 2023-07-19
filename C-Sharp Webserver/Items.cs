namespace Main
{
    class ItemsListResponse
    {
        public Item[] Items { get; set; }

        public ItemsListResponse()
        {
            // Items = Array.Empty<Item>();
            List<Item> items = new()
            {
                new Item { Name = "Coal", Description = "Typical coal, all burnin' and stuff.", CraftingRecipe = null, Type = "Resource", PrefabURL = "Assets/Prefabs/Coal.prefab" },
                new Item { Name = "Salvage", Description = "A hunk of steel, use it to build what you need.", CraftingRecipe = null, Type = "Resource", PrefabURL = "Assets/Prefabs/Salvage.prefab" },
                new Item { Name = "Conveyor", Description = "Do you really need me to explain what a goddamn conveyor is?", CraftingRecipe = new ItemCount[] { new ItemCount() { Item = "Salvage", Count = 2 } }, Type = "Buildable", PrefabURL = "Assets/Prefabs/Conveyor.prefab" },
                new Item { Name = "Grating", Description = "A grating. You can stand on it. You can build on it. What else? Oh yeah, and it's made of metal.", CraftingRecipe = new ItemCount[] { new ItemCount() { Item = "Salvage", Count = 3 } }, Type = "Buildable", PrefabURL = "Assets/Prefabs/Grating.prefab" },
                new Item { Name = "Deck Plating", Description = "Use it as armor. Use it as flooring. Use it as roofing. Use it as whatever, I don't care.", CraftingRecipe = new ItemCount[] { new ItemCount() { Item = "Salvage", Count = 8 } }, Type = "Buildable", PrefabURL = "Assets/Prefabs/Deck_Plate_1.prefab" },
                new Item { Name = "Construction Gun", Description = "This beut was slapped together out of some old tech and a spare revolver; it lets you construct stuff, provided that you have the materials it needs in your bag.", CraftingRecipe = null, Type = "Weapon", PrefabURL = "Assets/Prefabs/Construction Gun.prefab" },
                new Item { Name = "Pristine Construction Gun", Description = "Much like its modern equivalent, this lets you construct devices, but the pristine old-tech lets you build much more precise devices.", CraftingRecipe = null, Type = "Weapon", PrefabURL = "Assets/Prefabs/Pristine Construction Gun.prefab" },
                new Item { Name = "Default Cube", Description = "The ancients apparently worshiped this shape using a Shrine-Program called Blender.", CraftingRecipe = new ItemCount[] { new ItemCount() { Item = "Salvage", Count = 3 } }, Type = "Buildable", PrefabURL = "Assets/Prefabs/Cube.prefab" }
            };
            Items = items.ToArray();
        }
    }

    public class Item
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string PrefabURL { get; set; }

        public ItemCount[]? CraftingRecipe { get; set; }
    }

    public class ItemCount
    {
        public int Count { get; set; }
        public string Item { get; set; }
    }

    public class Inventory
    {
        public ItemCount[] Items { get; set; }
    }

    public class InventoryHelpers
    {
        public static ItemCount[] DefaultInventory = new ItemCount[] {
            new ItemCount() { Item = "Salvage", Count = 999 },
            new ItemCount() { Item = "Coal", Count = 999 },
            new ItemCount() { Item = "Pristine Construction Gun", Count = 1 },
        };
    }
}