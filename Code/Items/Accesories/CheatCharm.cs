using CywilizowanysMod.ContentBases;
using Terraria;
using Terraria.ID;

namespace CywilizowanysMod.Items.Accesories;

public class CheatCharm : CywilsItem
{
	public override void SetDefaults()
	{
		Item.DefaultToAccessory();
		Item.value=99999999;
		Item.maxStack=1;
		Item.rare=ItemRarityID.Master;
	}
	public override void UpdateAccessory(Player Player,bool hideVisual)
	{
		if (!hideVisual) Player.manaCost=0f;
		Player.GetModPlayer<CywilsPlayer>().cheatImmortal=true;
	}
}