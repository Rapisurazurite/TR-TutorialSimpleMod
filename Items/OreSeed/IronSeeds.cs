using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;
using Terraria.ObjectData;

namespace TutorialMod.Items.OreSeed
{
    public class IronSeeds : ModItem
    {
        public override void SetDefaults() {
            Item.autoReuse = true;
            Item.useTurn = true;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.maxStack = 999;
            Item.consumable = true;
            Item.placeStyle = 0;
            Item.width = 12;
            Item.height = 14;
            Item.value = 80;
            Item.createTile = ModContent.TileType<IronHerb>();
        }
        
        public override void AddRecipes() {
            CreateRecipe(1).
                AddIngredient(ItemID.IronBar, 1)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }

    public class IronHerb : ModTile
    {
        public const int FrameWidth = 18;
        public int herbItemType;
        public int seedItemType;
        
        public override void SetStaticDefaults() {
            Main.tileFrameImportant[Type] = true;
            Main.tileObsidianKill[Type] = true;
            Main.tileCut[Type] = true;
            Main.tileNoFail[Type] = true;
            TileID.Sets.ReplaceTileBreakUp[Type] = true;
            TileID.Sets.IgnoredInHouseScore[Type] = true;

            // We do not use this because our tile should only be spelunkable when it's fully grown. That's why we use the IsTileSpelunkable hook instead
            //Main.tileSpelunker[Type] = true;

            // Do NOT use this, it causes many unintended side effects
            //Main.tileAlch[Type] = true;

            AddMapEntry(new Color(128, 128, 128));

            TileObjectData.newTile.CopyFrom(TileObjectData.StyleAlch);
            // TileObjectData.newTile.AnchorValidTiles = new int[] {
            //     TileID.Grass,
            //     TileID.HallowedGrass,
            //     ModContent.TileType<ExampleBlock>()
            // };
            TileObjectData.newTile.AnchorValidTiles = new int[]
            {
            };
            TileObjectData.newTile.AnchorAlternateTiles = new int[]            {
                TileID.IronBrick,
                TileID.ClayPot,
                TileID.PlanterBox
            };
            
            TileObjectData.addTile(Type);

            SoundType = SoundID.Grass;
            SoundStyle = 0;
            DustType = DustID.Ambient_DarkBrown;
            herbItemType = ItemID.IronOre;
            seedItemType = ModContent.ItemType<IronSeeds>();
        }
        
        // A helper method to quickly get the current stage of the herb (assuming the tile at the coordinates is our herb)
        public static OrePlantStage GetStage(int i, int j) {
            Tile tile = Framing.GetTileSafely(i, j);
            return (OrePlantStage)(tile.TileFrameX / FrameWidth);
        }
        
        public override bool CanPlace(int i, int j) {
            Tile tile = Framing.GetTileSafely(i, j); // Safe way of getting a tile instance

            if (tile.HasTile) {
                int tileType = tile.TileType;
                if (tileType == Type) {
                    OrePlantStage stage = GetStage(i, j); // The current stage of the herb

                    // Can only place on the same herb again if it's grown already
                    return stage == OrePlantStage.Grown;
                }
                else {
                    // Support for vanilla herbs/grasses:
                    if (Main.tileCut[tileType] || TileID.Sets.BreakableWhenPlacing[tileType] || tileType == TileID.WaterDrip || tileType == TileID.LavaDrip || tileType == TileID.HoneyDrip || tileType == TileID.SandDrip) {
                        bool foliageGrass = tileType == TileID.Plants || tileType == TileID.Plants2;
                        bool moddedFoliage = tileType >= TileID.Count && (Main.tileCut[tileType] || TileID.Sets.BreakableWhenPlacing[tileType]);
                        bool harvestableVanillaHerb = Main.tileAlch[tileType] && WorldGen.IsHarvestableHerbWithSeed(tileType, tile.TileFrameX / 18);

                        if (foliageGrass || moddedFoliage || harvestableVanillaHerb) {
                            WorldGen.KillTile(i, j);
                            if (!tile.HasTile && Main.netMode == NetmodeID.MultiplayerClient) {
                                NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 0, i, j);
                            }

                            return true;
                        }
                    }

                    return false;
                }
            }

            return true;
        }
        
        public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects) {
            if (i % 2 == 1) {
                spriteEffects = SpriteEffects.FlipHorizontally;
            }
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
            }

            if (seedItemType > 0 && seedItemStack > 0) {
                Item.NewItem(source, worldPosition, seedItemType, seedItemStack);
            }

            // Custom drop code, so return false
            return false;
        }

        public override bool IsTileSpelunkable(int i, int j) {
            OrePlantStage stage = GetStage(i, j);

            // Only glow if the herb is grown
            return stage == OrePlantStage.Grown;
        }

        public override void RandomUpdate(int i, int j) {
            Tile tile = Framing.GetTileSafely(i, j);
            OrePlantStage stage = GetStage(i, j);

            // Only grow to the next stage if there is a next stage. We don't want our tile turning pink!
            if (stage != OrePlantStage.Grown) {
                // Increase the x frame to change the stage
                tile.TileFrameX += FrameWidth;

                // If in multiplayer, sync the frame change
                if (Main.netMode != NetmodeID.SinglePlayer) {
                    NetMessage.SendTileSquare(-1, i, j, 1);
                }
            }
        }
    }
}