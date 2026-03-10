using CywilizowanysMod.ContentBases;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CywilizowanysMod.Items.DevSets;

public abstract class DevSet_Cywil_Base : CywilsItem
{
	public override void SetDefaults()
	{
		Item.value=500;
		Item.rare=ItemRarityID.LightPurple;
		Item.vanity=true;
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
	private static int legsOverlaySlot;
	private static int legsOverlaySlotFemale;
	public override void Load()
	{
		legsOverlaySlot=EquipLoader.AddEquipTexture(Mod,$"{Texture}_{EquipType.Legs}",EquipType.Legs,name:$"{Name}_{EquipType.Legs}");
		legsOverlaySlotFemale=EquipLoader.AddEquipTexture(Mod,$"{Texture}_{EquipType.Legs}_Female",EquipType.Legs,name:$"{Name}_{EquipType.Legs}_Female");
	}
	public override int BodyArmorLegsOverlay(bool isMale)
	{
		return (isMale ? legsOverlaySlot : legsOverlaySlotFemale);
	}
}
[AutoloadEquip(EquipType.Legs)]
public class DevSet_Cywil_Boots : DevSet_Cywil_Base
{
	private static int femaleVariant;
	public override void Load()
	{
		femaleVariant=EquipLoader.AddEquipTexture(Mod,$"{Texture}_{EquipType.Legs}_Female",EquipType.Legs,name:$"{Name}_{EquipType.Legs}_Female");
	}
	public override void SetMatch(bool male,ref int equipSlot,ref bool robes)
	{
		if (!male) equipSlot=femaleVariant;
	}
}
/*
[AutoloadEquip(EquipType.Wings)]
public class DevSet_Cywil_Wings : DevSet_Cywil_Base
{
}
*/