using CywilizowanysMod.ContentBases;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CywilizowanysMod.Items.Placeable;

public class Autoseller : CywilsItem
{
	public override void SetDefaults()
	{
		Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Autoseller>());
		Item.width=36;
		Item.height=48;
		Item.rare=ItemRarityID.Blue;
	}
	public override void AddRecipes()
	{
		CreateRecipe()
		.AddIngredient(ItemID.PiggyBank,10)
		.AddIngredient(ItemID.BlackLens)
		.AddRecipeGroup(RecipeGroupID.IronBar,15)
		.AddTile(TileID.Anvils)
		.Register();
	}
}