using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;
using Terraria.ObjectData;

namespace TutorialMod.Items.OreSeed
{
    public class HellstoneSeeds : ModItem
    {
        public override void SetDefaults() {
            Item.CloneDefaults(ModContent.ItemType<IronSeeds>());
            Item.createTile = ModContent.TileType<HellstoneHerb>();
        }
        
        public override void AddRecipes() {
            CreateRecipe(1).
                AddIngredient(ItemID.HellstoneBar, 1)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }

    public class HellstoneHerb : IronHerb
    {
        public override void SetStaticDefaults() {
            Main.tileFrameImportant[Type] = true;
            Main.tileObsidianKill[Type] = true;
            Main.tileCut[Type] = true;
            Main.tileNoFail[Type] = true;
            TileID.Sets.ReplaceTileBreakUp[Type] = true;
            TileID.Sets.IgnoredInHouseScore[Type] = true;
            
            AddMapEntry(new Color(128, 128, 128));

            TileObjectData.newTile.CopyFrom(TileObjectData.StyleAlch);
            TileObjectData.newTile.AnchorValidTiles = new int[]
            {
                TileID.HellstoneBrick,
                TileID.ClayPot,
                TileID.PlanterBox
            };
            TileObjectData.addTile(Type);

            SoundType = SoundID.Grass;
            SoundStyle = 0;
            DustType = DustID.Ambient_DarkBrown;
            herbItemType = ItemID.Hellstone;
            seedItemType = ModContent.ItemType<HellstoneSeeds>();
        }
        
        public override bool Drop(int i, int j) {
            OrePlantStage stage = GetStage(i, j);

            if (stage == OrePlantStage.Planted) {
                // Do not drop anything when just planted
                return false;
            }

            Vector2 worldPosition = new Vector2(i, j).ToWorldCoordinates();
            Player nearestPlayer = Main.player[Player.FindClosest(worldPosition, 16, 16)];

            // int herbItemType = ModContent.ItemType<ExampleItem>();
            int herbItemStack = 1;

            // int seedItemType = ModContent.ItemType<IronSeeds>();
            int seedItemStack = 1;

            if (nearestPlayer.active && nearestPlayer.HeldItem.type == ItemID.StaffofRegrowth) {
                // Increased yields with Staff of Regrowth, even when not fully grown
                herbItemStack = Main.rand.Next(5, 10);
                seedItemStack = Main.rand.Next(1, 2);
            }
            else if (stage == OrePlantStage.Grown) {
                // Default yields, only when fully grown
                herbItemStack = Main.rand.Next(2, 8);
                seedItemStack = 1;
            }

            var source = new EntitySource_TileBreak(i, j);

            if (herbItemType > 0 && herbItemStack > 0) {
                Item.NewItem(source, worldPosition, herbItemType, herbItemStack);
                Item.NewItem(source, worldPosition, ItemID.Obsidian, herbItemStack);
            }

            if (seedItemType > 0 && seedItemStack > 0) {
                Item.NewItem(source, worldPosition, seedItemType, seedItemStack);
            }

            // Custom drop code, so return false
            return false;
        }
    }
}

