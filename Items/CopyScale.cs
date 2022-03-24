using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TutorialMod.Items
{
	public class CopyScale : ModItem
	{
		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("TutorialSword"); // By default, capitalization in classnames will add spaces to the display name. You can customize the display name here by uncommenting this line.
			Tooltip.SetDefault("This is a basic modded accessory.");
		}

		public override void SetDefaults()
		{
			Item.accessory = true;
			
		}
		
		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			player.GetModPlayer<CopyScalePlayer>().CopyScale = true;
		}
	}
	

	public class CopyScalePlayer : ModPlayer
	{
		public bool CopyScale = false;
		public override void ResetEffects()
		{
			CopyScale = false;
		}
	}
	
	public class CopyScaleProjectile : GlobalProjectile
	{
		// Finding the closest NPC to attack within maxDetectDistance range
		// If not found then returns null
		public NPC FindClosestEnemyNpc(float maxDetectDistance, Projectile projectile) {
			NPC closestNPC = null;

			// Using squared values in distance checks will let us skip square root calculations, drastically improving this method's speed.
			float sqrMaxDetectDistance = maxDetectDistance * maxDetectDistance;

			// Loop through all NPCs(max always 200)
			for (int k = 0; k < Main.maxNPCs; k++) {
				NPC target = Main.npc[k];
				// Check if NPC able to be targeted. It means that NPC is
				// 1. active (alive)
				// 2. chaseable (e.g. not a cultist archer)
				// 3. max life bigger than 5 (e.g. not a critter)
				// 4. can take damage (e.g. moonlord core after all it's parts are downed)
				// 5. hostile (!friendly)
				// 6. not immortal (e.g. not a target dummy)
				if (target.CanBeChasedBy()) {
					// The DistanceSquared function returns a squared distance between 2 points, skipping relatively expensive square root calculations
					float sqrDistanceToTarget = Vector2.DistanceSquared(target.Center, projectile.Center);

					// Check if it is within the radius
					if (sqrDistanceToTarget < sqrMaxDetectDistance) {
						sqrMaxDetectDistance = sqrDistanceToTarget;
						closestNPC = target;
					}
				}
			}
			return closestNPC;
		}
		public override void OnHitNPC(Projectile projectile, NPC target, int damage, float knockback, bool crit)
		{	
			float maxDetectRadius = 400f;
			Player player = Main.player[projectile.owner];
			if (player.GetModPlayer<CopyScalePlayer>().CopyScale && projectile.DamageType == DamageClass.Ranged)
			{
				NPC closestNPC = FindClosestEnemyNpc(maxDetectRadius, projectile);
				if (closestNPC != null && target.life <= 0)
				{
					float projectileSpeed = projectile.velocity.Length();
					Vector2 newVelocity = (closestNPC.Center - projectile.Center).SafeNormalize(Vector2.Zero) * projectileSpeed;// new projectile velocity
					Projectile.NewProjectile(Projectile.InheritSource(projectile), projectile.Center, newVelocity, projectile.type, damage, knockback, projectile.owner);
				}
			}
		}
	}
}