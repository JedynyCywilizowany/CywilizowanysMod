using Terraria;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.GameInput;
using CywilizowanysMod.Common;

namespace CywilizowanysMod;

public partial class CywilsPlayer : ModPlayer
{
	public bool cheatImmortal;
	public override void ResetEffects()
	{
		cheatImmortal=false;
		
		autosellingNear=false;
		autosellerBagAvailable=false;
		autosellingBuyer=-1;
	}
	public override bool PreKill(double damage,int hitDirection,bool pvp,ref bool playSound,ref bool genDust,ref PlayerDeathReason damageSource)
	{
		if (cheatImmortal)
		{
			Player.statLife=1;
			return false;
		}
		return true;
	}
	public override void Kill(double damage,int hitDirection,bool pvp,PlayerDeathReason damageSource)
	{
		if (Player.IsLocal())
		{
			Player.GiveMoney(AutosellerBagFill/2);
			AutosellerBagFill=0;
		}
	}
	public override void OnEnterWorld()
	{
		UpdateAutosell();
	}
	
	public override void ProcessTriggers(TriggersSet triggersSet)
	{
		if (Keybinds.autosellAddKey.JustPressed&&!Main.HoverItem.IsAir) ToggleAutosell(Main.HoverItem.type);
	}
}