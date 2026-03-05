using CywilizowanysMod.ContentBases;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CywilizowanysMod.Items.Placeable;

public class ItemCompressor : CywilsItem
{
	public override string Texture=>ModContent.GetInstance<Tiles.ItemCompressor>().DefaultTexturePath();
	public override void SetStaticDefaults()
	{
		Item.ResearchUnlockCount=100;
	}
	public override void SetDefaults()
	{
		Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.ItemCompressor>());
	}
	public override void AddRecipes()
	{
		CreateRecipe()
		.AddIngredient(ItemID.ConveyorBeltLeft,5)
		.AddIngredient(ItemID.ConveyorBeltRight,5)
		.AddIngredient(ItemID.Wire,5)
		.AddTile(TileID.Anvils)
		.Register();
	}
}