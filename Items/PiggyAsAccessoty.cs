using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TutorialMod.Items
{
    public class PiggyAsAccessoty : ModItem
    {
        private Chest bank;
        
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            Tooltip.SetDefault("If you equip this, that means you have all the accessories in the piggy bank.");
        }
        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.rare = ItemRarityID.Orange;
            Item.accessory = true;
        }
        
        public override void AddRecipes() {
            CreateRecipe(1).
                AddIngredient(ItemID.PiggyBank, 1)
                .AddIngredient(ItemID.GoldCoin, 99)
                .AddTile(TileID.WorkBenches)
                .Register();
        }

        public void FakeEquipAcc(Player player, Item item, bool hideVisual)
        {
            player.VanillaUpdateEquip(item);
            player.ApplyEquipFunctional(item, hideVisual);
        }
        
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            bank = player.bank;
            
            // loop through all items in the piggy bank
            for (var inventoryIndex = 0; inventoryIndex < 40; inventoryIndex++)
            {
                if (bank.item[inventoryIndex].type == ItemID.None) continue;
                if (bank.item[inventoryIndex].accessory)
                {
                    FakeEquipAcc(player, bank.item[inventoryIndex], hideVisual);
                }
            }
            // Item item = new Item();
            // item.SetDefaults(ItemID.BlizzardinaBottle);
            // FakeEquipAcc(player, item, hideVisual);
        }        
    }
}
