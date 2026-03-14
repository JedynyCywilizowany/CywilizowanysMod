using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CywilizowanysMod.Globals;

partial class CywilsGlobItem : GlobalItem
{
	public override void ModifyShootStats(Item item,Player player,ref Vector2 position,ref Vector2 velocity,ref int type,ref int damage,ref float knockback)
	{
		if (item.type==ItemID.SniperRifle) velocity*=1.75f;
	}
	public override void ModifyWeaponDamage(Item item,Player player,ref StatModifier damage)
	{
		if (item.type==ItemID.SniperRifle) damage*=5;
		
		int crit=player.GetWeaponCrit(item);
		if (crit>100) damage*=crit/100f;
	}
	public override float UseTimeMultiplier(Item item,Player player)
	{
		if (item.type==ItemID.SniperRifle) return 5f;
		return 1f;
	}
	public override float UseAnimationMultiplier(Item item,Player player)
	{
		if (item.type==ItemID.SniperRifle) return 5f;
		return 1f;
	}
}