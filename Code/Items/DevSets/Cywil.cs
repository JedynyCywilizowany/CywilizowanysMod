using CywilizowanysMod.Common;
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
	protected internal Recipe CommonRecipe()
	{
		return CreateRecipe()
		.AddIngredient(ItemID.CopperBar,10)
		.AddIngredient(ItemID.TinBar,10)
		.AddIngredient(ItemID.Amethyst,5)
		.AddIngredient(ItemID.FlinxFur,2)
		.AddIngredient(ItemID.Shiverthorn,5)
		.AddIngredient(ItemID.Deathweed,5)
		.AddTile(TileID.Anvils)
		.AddTile(TileID.Loom);
	}
	public override void AddRecipes()
	{
		CommonRecipe().Register();
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
	public override void ArmorSetShadows(Player player)
	{
		player.armorEffectDrawShadow=true;
	}
	public override bool IsVanitySet(int head,int body,int legs)
	{
		return head==CywilsUtils.DummyItems[ModContent.ItemType<DevSet_Cywil_Helmet>()].headSlot&&
		body==Item.bodySlot&&
		(legs==CywilsUtils.DummyItems[ModContent.ItemType<DevSet_Cywil_Boots>()].legSlot||legs==DevSet_Cywil_Boots.femaleVariant);
	}
}
[AutoloadEquip(EquipType.Legs)]
public class DevSet_Cywil_Boots : DevSet_Cywil_Base
{
	internal static int femaleVariant;
	public override void Load()
	{
		femaleVariant=EquipLoader.AddEquipTexture(Mod,$"{Texture}_{EquipType.Legs}_Female",EquipType.Legs,this,$"{Name}_{EquipType.Legs}_Female");
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
	public override void SetStaticDefaults()
	{
		ArmorIDs.Wing.Sets.Stats[Item.wingSlot]=ArmorIDs.Wing.Sets.Stats[ArmorIDs.Wing.CreativeWings];
		//ArmorIDs.Front.Sets.DrawsInNeckLayer[Item.frontSlot]=true;
	}
	public override void SetDefaults()
	{
		base.SetDefaults();
		Item.vanity=false;
		Item.accessory=true;
	}
	public override void AddRecipes()
	{
		CommonRecipe()
		.AddIngredient(ItemID.CreativeWings)
		.Register();
		CommonRecipe()
		.AddIngredient(ItemID.SoulofFlight,20)
		.Register();
	}
}
*/