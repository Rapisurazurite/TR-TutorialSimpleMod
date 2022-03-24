using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using System;

namespace TutorialMod.Items
{
	public class Wisp : ModProjectile
	{
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Example Bullet");     //The English name of the Projectile
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;    //The length of old position to be recorded
			ProjectileID.Sets.TrailingMode[Projectile.type] = 0;        //The recording mode
		}

		public override void SetDefaults() {
			Projectile.width = 8;               //The width of Projectile hitbox
			Projectile.height = 8;              //The height of Projectile hitbox
			Projectile.aiStyle = 1;             //The ai style of the Projectile, please reference the source code of Terraria
			Projectile.friendly = true;         //Can the Projectile deal damage to enemies?
			Projectile.hostile = false;         //Can the Projectile deal damage to the player?
			Projectile.penetrate = 5;           //How many monsters the Projectile can penetrate. (OnTileCollide below also decrements penetrate for bounces as well)
			Projectile.timeLeft = 600;          //The live time for the Projectile (60 = 1 second, so 600 is 10 seconds)
			Projectile.alpha = 255;             //The transparency of the Projectile, 255 for completely transparent. (aiStyle 1 quickly fades the Projectile in) Make sure to delete this if you aren't using an aiStyle that fades in. You'll wonder why your Projectile is invisible.
			Projectile.light = 0.5f;            //How much light emit around the Projectile
			Projectile.ignoreWater = true;          //Does the Projectile's speed be influenced by water?
			Projectile.tileCollide = true;          //Can the Projectile collide with tiles?
			Projectile.extraUpdates = 1;            //Set to above 0 if you want the Projectile to update multiple time in a frame
			Projectile.DamageType = DamageClass.Melee;
			AIType = ProjectileID.Bullet;           //Act exactly like default Bullet
		}

		public override bool OnTileCollide(Vector2 oldVelocity) {
			//If collide with tile, reduce the penetrate.
			//So the Projectile can reflect at most 5 times
			Projectile.penetrate--;
			if (Projectile.penetrate <= 0) {
				Projectile.Kill();
			}
			else {
				Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
				SoundEngine.PlaySound(SoundID.Item10, Projectile.position);

				// If the projectile hits the left or right side of the tile, reverse the X velocity
				if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon) {
					Projectile.velocity.X = -oldVelocity.X;
				}

				// If the projectile hits the top or bottom side of the tile, reverse the Y velocity
				if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon) {
					Projectile.velocity.Y = -oldVelocity.Y;
				}
			}
			return false;
		}
		
		// when this projectile kill the enemy, it will spawn a new one
		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
			float maxDetectRadius = 400f; // The maximum radius at which a projectile can detect a target
			NPC closestNPC = FindClosestNPC(maxDetectRadius);
			if (closestNPC == null)
				;
			else
			{
				float projectileSpeed = Projectile.velocity.Length();//current projectile speed
				Vector2 newVelocity = (closestNPC.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * projectileSpeed;// new projectile velocity
				// if killed the enemy, spawn a new one
				if (target.life <= 0)
				{
					// spawn a new projectile
					Projectile.NewProjectile(Projectile.InheritSource(Projectile), Projectile.Center, newVelocity, ModContent.ProjectileType<Wisp>(), damage, knockback, Projectile.owner);
				}
			}
			Projectile.Kill();
		}
		
		public override void Kill(int timeLeft)
		{
			// This code and the similar code above in OnTileCollide spawn dust from the tiles collided with. SoundID.Item10 is the bounce sound you hear.
			Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);
			SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
		}
		
		// Finding the closest NPC to attack within maxDetectDistance range
		// If not found then returns null
		public NPC FindClosestNPC(float maxDetectDistance) {
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
					float sqrDistanceToTarget = Vector2.DistanceSquared(target.Center, Projectile.Center);

					// Check if it is within the radius
					if (sqrDistanceToTarget < sqrMaxDetectDistance) {
						sqrMaxDetectDistance = sqrDistanceToTarget;
						closestNPC = target;
					}
				}
			}

			return closestNPC;
		}
	}
}
