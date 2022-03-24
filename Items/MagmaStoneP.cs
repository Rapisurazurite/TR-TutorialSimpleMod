using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TutorialMod.Items
{
	public class MagmaStoneP : ModItem
	{
		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("TutorialSword"); // By default, capitalization in classnames will add spaces to the display name. You can customize the display name here by uncommenting this line.
			Tooltip.SetDefault("This is a basic modded accessory.");
		}

		public override void SetDefaults()
		{
			Item.CloneDefaults(ItemID.MagmaStone);
		}
		
		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			player.GetModPlayer<MagamaPlayer>().MagmaStoneP = true;
		}
	}
	

	public class MagamaPlayer : ModPlayer
	{
		public bool MagmaStoneP = false;

		public override void ResetEffects()
		{
			MagmaStoneP = false;
		}
	}

	public class MagamaProjectile : GlobalProjectile
	{
		public override void OnHitNPC(Projectile projectile, NPC target, int damage, float knockback, bool crit)
		{
			Player player = Main.player[projectile.owner];
			if (player.GetModPlayer<MagamaPlayer>().MagmaStoneP && projectile.DamageType == DamageClass.Ranged)
			{
				target.AddBuff(BuffID.OnFire, 60);
			}
		}
	}
	
}