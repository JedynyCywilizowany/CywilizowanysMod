using CywilizowanysMod.Common;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CywilizowanysMod;

public partial class CywilsSystem : ModSystem
{
	internal static int itemCounter=0;
	internal static float itemCapProgress=0f;
	internal static uint forceItemStackTime=0;
	public override void PreUpdateItems()
	{
		itemCapProgress=((float)itemCounter)/Main.maxItems;
		itemCounter=0;
		
		if (forceItemStackTime!=0)
		{
			if (itemCapProgress<0.75) itemCapProgress=0.75f;
			forceItemStackTime--;
		}
	}
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
		CywilsUtils.SetupDummyEntities();
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