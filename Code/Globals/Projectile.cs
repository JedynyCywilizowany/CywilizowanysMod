using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CywilizowanysMod.Globals;

public class CywilsGlobProjectile : GlobalProjectile
{
	public override void SetDefaults(Projectile Projectile)
	{
		if (Projectile.type==ProjectileID.FallingStar)
		{
			Projectile.friendly=true;
			Projectile.hostile=true;
		}
	}
}