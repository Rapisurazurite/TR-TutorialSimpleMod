using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;
using Terraria.Utilities;
namespace TutorialMod.Items.ComposeChest;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;


public class ComposeChest : ModItem
{
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        DisplayName.SetDefault("Compose Chest");
        Tooltip.SetDefault("Compose two weapons to create a stronger one");
    }

    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.CloneDefaults(ItemID.Chest);
        Item.createTile = ModContent.TileType<ComposeChestTile>();
    }
}

public class ComposeChestTile : ModTile
{
    public int chestType;
    public string displayName;
    private bool haveAlreadyGathered;

    public override void SetStaticDefaults()
    {
        // chest configuration
        displayName = "Compose Chest";
        chestType = ModContent.ItemType<ComposeChest>();

        haveAlreadyGathered = true;
        // Properties
        Main.tileSpelunker[Type] = true;
        Main.tileContainer[Type] = true;
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileOreFinderPriority[Type] = 500;
        TileID.Sets.HasOutlines[Type] = true;
        TileID.Sets.BasicChest[Type] = true;
        TileID.Sets.DisableSmartCursor[Type] = true;

        AdjTiles = new int[] {TileID.Containers};
        ChestDrop = chestType;

        ContainerName.SetDefault(displayName);
        var name = CreateMapEntryName();
        name.SetDefault(displayName);
        AddMapEntry(new Color(160, 195, 60), name, MapChestName);


        // Placement
        TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
        TileObjectData.newTile.Origin = new Point16(0, 1);
        TileObjectData.newTile.CoordinateHeights = new[] {16, 18};
        TileObjectData.newTile.HookCheckIfCanPlace = new PlacementHook(Chest.FindEmptyChest, -1, 0, true);
        TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(Chest.AfterPlacement_Hook, -1, 0, false);
        TileObjectData.newTile.AnchorInvalidTiles = new int[] {TileID.MagicalIceBlock};
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.LavaDeath = false;
        TileObjectData.newTile.AnchorBottom = new AnchorData(
            AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
        TileObjectData.addTile(Type);
    }

    public override ushort GetMapOption(int i, int j)
    {
        return (ushort) (Main.tile[i, j].TileFrameX / 36);
    }

    public override bool HasSmartInteract()
    {
        return true;
    }

    public static string MapChestName(string name, int i, int j)
    {
        var left = i;
        var top = j;
        var tile = Main.tile[i, j];
        if (tile.TileFrameX % 36 != 0) left--;

        if (tile.TileFrameY != 0) top--;

        var chest = Chest.FindChest(left, top);
        if (chest < 0) return Language.GetTextValue("LegacyChestType.0");

        if (Main.chest[chest].name == "") return name;

        return name + ": " + Main.chest[chest].name;
    }

    public override void NumDust(int i, int j, bool fail, ref int num)
    {
        num = 1;
    }

    public override void KillMultiTile(int i, int j, int frameX, int frameY)
    {
        Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 32, 32, ChestDrop);
        Chest.DestroyChest(i, j);
    }

    public override bool RightClick(int i, int j)
    {
        var player = Main.LocalPlayer;
        var tile = Main.tile[i, j];
        Main.mouseRightRelease = false;
        var left = i;
        var top = j;
        if (tile.TileFrameX % 36 != 0) left--;

        if (tile.TileFrameY != 0) top--;

        if (player.sign >= 0)
        {
            SoundEngine.PlaySound(SoundID.MenuClose);
            player.sign = -1;
            Main.editSign = false;
            Main.npcChatText = "";
        }

        if (Main.editChest)
        {
            SoundEngine.PlaySound(SoundID.MenuTick);
            Main.editChest = false;
            Main.npcChatText = "";
        }

        if (player.editedChestName)
        {
            NetMessage.SendData(MessageID.SyncPlayerChest, -1, -1,
                NetworkText.FromLiteral(Main.chest[player.chest].name), player.chest, 1f);
            player.editedChestName = false;
        }

        if (Main.netMode == NetmodeID.MultiplayerClient)
        {
            if (left == player.chestX && top == player.chestY && player.chest >= 0)
            {
                player.chest = -1;
                Recipe.FindRecipes();
                SoundEngine.PlaySound(SoundID.MenuClose);
            }
            else
            {
                NetMessage.SendData(MessageID.RequestChestOpen, -1, -1, null, left, top);
                Main.stackSplit = 600;
            }
        }
        else
        {
            var chest = Chest.FindChest(left, top);
            if (chest >= 0)
            {
                Main.stackSplit = 600;
                if (chest == player.chest)
                {
                    player.chest = -1;
                    SoundEngine.PlaySound(SoundID.MenuClose);
                }
                else
                {
                    player.chest = chest;
                    Main.playerInventory = true;
                    Main.recBigList = false;
                    player.chestX = left;
                    player.chestY = top;
                    SoundEngine.PlaySound(player.chest < 0 ? SoundID.MenuOpen : SoundID.MenuTick);
                }

                Recipe.FindRecipes();
            }
        }

        return true;
    }

    public override void MouseOver(int i, int j)
    {
        var player = Main.LocalPlayer;
        var tile = Main.tile[i, j];
        var left = i;
        var top = j;
        if (tile.TileFrameX % 36 != 0) left--;

        if (tile.TileFrameY != 0) top--;

        var chest = Chest.FindChest(left, top);
        if (chest < 0)
        {
            player.cursorItemIconText = Language.GetTextValue("LegacyChestType.0");
        }
        else
        {
            player.cursorItemIconText = Main.chest[chest].name.Length > 0 ? Main.chest[chest].name : displayName;
            if (player.cursorItemIconText == displayName)
            {
                player.cursorItemIconID = chestType;
                player.cursorItemIconText = "";
            }
        }

        player.noThrow = 2;
        player.cursorItemIconEnabled = true;
    }

    public override void MouseOverFar(int i, int j)
    {
        MouseOver(i, j);
        var player = Main.LocalPlayer;
        if (player.cursorItemIconText == "")
        {
            player.cursorItemIconEnabled = false;
            player.cursorItemIconID = 0;
        }
    }


    public override void RandomUpdate(int i, int j)
    {
        var dayTime = Main.dayTime;
        // Reset the haveAlreadyGathered flag when it's night time
        if (!dayTime)
        {
            haveAlreadyGathered = false;
            return;
        }
        // If it's day time and have not already gathered

        if (dayTime && !haveAlreadyGathered)
        {
            var tile = Main.tile[i, j];
            var left = i;
            var top = j;
            if (tile.TileFrameX % 36 != 0) left--;
            if (tile.TileFrameY != 0) top--;

            var chest = Chest.FindChest(left, top);
            if (chest < 0) return;

            // Loop the items to add
            /*
             *
             * Do this here
             * 
             */
            var chestEntity = Main.chest[chest];
            Item firstItem = chestEntity.item[0];
            int firstItemId = firstItem.type;
            if (firstItem.type != ItemID.None && firstItem.damage > 0 && !firstItem.accessory && firstItem.maxStack == 1)
            {
                // calculate the same item in the chest
                var totalCompose = 0;
                for (var inventoryIndex=1; inventoryIndex<40; inventoryIndex++)
                {
                    if (chestEntity.item[inventoryIndex].type == firstItemId)
                    {
                        totalCompose += chestEntity.item[inventoryIndex].GetGlobalItem<ComposeItem>().composed + 1;
                        chestEntity.item[inventoryIndex] = new Item(ItemID.None);
                    }
                }
                firstItem.GetGlobalItem<ComposeItem>().SetCompose(firstItem, totalCompose);
            }
            haveAlreadyGathered = true;
        }
    }
}

public class ComposeItem : GlobalItem
{
    public int composed;
    public double damageBonus;

    
    public ComposeItem()
    {
        composed = 0;
    }
    
    public override bool InstancePerEntity => true;
    
    private void UpdateItemProperties(Item item)
    {
        damageBonus = 1 + composed / 10.0;
        
        item.damage = (int)Math.Ceiling((1+composed/10.0)*item.damage);
        item.useTime = (int)Math.Ceiling(Math.Max(item.useTime/2, item.useTime/(1+composed/10.0)));
        if (item.mana > 0)
        {
            item.mana = (int) Math.Ceiling(Math.Max(item.mana / 2, item.mana / (1 + composed / 10.0)));
        }
    }
    

    public override GlobalItem Clone(Item item, Item itemClone)
    {
        ComposeItem myClone = (ComposeItem) base.Clone(item, itemClone);
        myClone.composed = composed;
        return myClone;
    }

    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
    {
        if (composed != 0) {
            string bonusText = String.Format("bonus {0}: damage multiple {1:F2}", composed, damageBonus);
            tooltips.Add(new TooltipLine(Mod, "Composed", bonusText));
        }
    }

    public override void LoadData(Item item, TagCompound tag)
    {
        base.LoadData(item, tag);
        composed = tag.GetInt("composed");
        UpdateItemProperties(item);
    }
    
    public override void SaveData(Item item, TagCompound tag)
    {
        base.SaveData(item, tag);
        tag["composed"] = composed;
    }
    
    public void SetCompose(Item item, int i)
    {
        composed += i;
        UpdateItemProperties(item);
    }
}