using CywilizowanysMod.ContentBases;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CywilizowanysMod.Items.DevSets;

public abstract class DevSet_Cywil_Base : CywilsItem
{
	public override void SetDefaults()
	{
		Item.value=50;
		Item.rare=ItemRarityID.Expert;
	}
}

[AutoloadEquip(EquipType.Head)]
public class DevSet_Cywil_Helmet : DevSet_Cywil_Base
{
	public override void SetDefaults()
	{
		base.SetDefaults();
		ArmorIDs.Head.Sets.DrawHatHair[Item.headSlot]=true;
	}
}
[AutoloadEquip(EquipType.Body)]
public class DevSet_Cywil_Chestpiece : DevSet_Cywil_Base
{
}
[AutoloadEquip(EquipType.Legs)]
public class DevSet_Cywil_Boots : DevSet_Cywil_Base
{
}
/*
[AutoloadEquip(EquipType.Wings)]
public class DevSet_Cywil_Wings : DevSet_Cywil_Base
{
}
*/