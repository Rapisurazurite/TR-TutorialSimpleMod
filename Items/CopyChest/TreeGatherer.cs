using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace TutorialMod.Items.CopyChest;

public class TreeGatherer : ModItem
{
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        DisplayName.SetDefault("Tree Gatherer");
        Tooltip.SetDefault("Gathers wood. Can convert basic wood into other types.");
    }

    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.CloneDefaults(ItemID.Chest);
        Item.createTile = ModContent.TileType<TreeGathererTile>();
    }
}

public class TreeGathererTile : ModTile
{
    private bool haveAlreadyGathered;
    public string displayName;
    public int chestType;
    public List<int> itemList;
    public List<int> itemAmount;

    public override void SetStaticDefaults()
    {   
        // chest configuration
        displayName = "Tree Gatherer";
        chestType = ModContent.ItemType<TreeGatherer>();
        itemList = new List<int>(){ItemID.Wood, ItemID.PalmWood};
        itemAmount = new List<int>(){8, 8};

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
    
    public void AddToChest(Chest chest, int itemId, int amount)
    {
        if (amount <= 0) return;
        Item item = new Item();
        item.SetDefaults(itemId);
        int currentItemStackMax = item.maxStack;
        for (var inventoryIndex = 0; inventoryIndex < 40 && amount > 0; inventoryIndex++)
        {
            // If this slot is empty
            if (chest.item[inventoryIndex].type == ItemID.None)
            {
                chest.item[inventoryIndex].SetDefaults(itemId);
                chest.item[inventoryIndex].stack = Math.Min(item.maxStack, amount);
                amount -= chest.item[inventoryIndex].stack;
                continue;
            }
            // If this slot have be already occupied by the other item
            else if (chest.item[inventoryIndex].type != itemId) continue;
            // If this slot is already full
            else if (chest.item[inventoryIndex].stack >= currentItemStackMax) continue;
            // If this slot is stackable but not enough
            else if (chest.item[inventoryIndex].stack + amount > currentItemStackMax)
            {
                amount -= currentItemStackMax - chest.item[inventoryIndex].stack;
                chest.item[inventoryIndex].stack = currentItemStackMax;
                continue;
            }
            // If this slot is stackable and enough
            else
            {
                chest.item[inventoryIndex].stack += amount;
                amount = 0;
                break;
            }
        }
    }
    
    public override void RandomUpdate(int i, int j)
    {   
        bool dayTime = Main.dayTime;
        // Reset the haveAlreadyGathered flag when it's night time
        if (!dayTime)
        {
            haveAlreadyGathered = false;
            return;
        }
        // If it's day time and have not already gathered
        else if (dayTime && !haveAlreadyGathered)
        {
            var tile = Main.tile[i, j];
            var left = i;
            var top = j;
            if (tile.TileFrameX % 36 != 0) left--;
            if (tile.TileFrameY != 0) top--;

            var chest = Chest.FindChest(left, top);
            if (chest < 0) return;
        
            // Loop the items to add
            for (var itemIndex = 0; itemIndex < itemList.Count; itemIndex++)
            {
                AddToChest(Main.chest[chest], itemList[itemIndex], itemAmount[itemIndex]);;
            }
            haveAlreadyGathered = true;
            return;
        }
    }
}