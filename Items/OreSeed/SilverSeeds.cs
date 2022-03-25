using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;
using Terraria.ObjectData;

namespace TutorialMod.Items.OreSeed
{
    public class SilverSeeds : ModItem
    {
        public override void SetDefaults() {
            Item.CloneDefaults(ModContent.ItemType<IronSeeds>());
            Item.createTile = ModContent.TileType<SilverHerb>();
        }
        
        public override void AddRecipes() {
            CreateRecipe(1).
                AddIngredient(ItemID.SilverBar, 1)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }

    public class SilverHerb : IronHerb
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
            };
            TileObjectData.newTile.AnchorAlternateTiles = new int[]
            {
                TileID.SilverBrick,
                TileID.ClayPot,
                TileID.PlanterBox
            };
            TileObjectData.addTile(Type);

            SoundType = SoundID.Grass;
            SoundStyle = 0;
            DustType = DustID.Ambient_DarkBrown;
            herbItemType = ItemID.SilverOre;
            seedItemType = ModContent.ItemType<SilverSeeds>();
        }
    }
}

