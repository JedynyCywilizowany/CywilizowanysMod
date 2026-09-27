using CywilizowanysMod.Common;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CywilizowanysMod;

public partial class CywilsSystem : ModSystem
{
	public override void Load()
	{
		if (!Main.dedServ)
		{
			ui=new();
			autoSellerUI=new();
		}
	}
	public override void PostSetupContent()
	{
		autoSellerUI?.Activate();
	}
	public override void PostAddRecipes()
	{
		CywilsSets.SetupSets();

		foreach (var recipe in Main.recipe)
		{
			if (recipe.HasResult(ItemID.ActiveStoneBlock)||recipe.HasResult(ItemID.InactiveStoneBlock))
			{
				if (recipe.RemoveIngredient(ItemID.Wire)) recipe.AddIngredient(ItemID.Actuator);
			}
		}
	}
}