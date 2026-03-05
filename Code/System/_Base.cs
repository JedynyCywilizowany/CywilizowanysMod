using CywilizowanysMod.Common;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CywilizowanysMod;

public partial class CywilsSystem : ModSystem
{
	public static readonly int ItemDespawnTime=60*60*60;
	
	internal static int itemCounter=0;
	internal static float itemCapProgress=0f;
	internal static uint forceItemStackTime=0;
	private static int itemDespawnTrackingCooldown;
	private static readonly (bool lastActive,int timeLeft,int lastType,int lastStack)[] itemDespawnTracker=new (bool,int,int,int)[Main.maxItems];
	public override void PreUpdateItems()
	{
		itemCapProgress=((float)itemCounter)/Main.maxItems;
		itemCounter=0;
		
		if (forceItemStackTime>0)
		{
			if (itemCapProgress<0.75) itemCapProgress=0.75f;
			forceItemStackTime--;
		}

		if (itemDespawnTrackingCooldown<=0) for (int i=0;i<Main.maxItems;i++)
		{
			var item=Main.item[i];
			ref var despawnEntry=ref itemDespawnTracker[i];
			if (item.active&&(Main.netMode!=NetmodeID.MultiplayerClient||item.instanced))
			{
				despawnEntry.lastActive=true;
				if (despawnEntry.lastActive&&item.stack==despawnEntry.lastStack&&item.type==despawnEntry.lastType)
				{
					if (--despawnEntry.timeLeft<0) 
					{
						item.active=false;
						if (Main.dedServ) NetMessage.SendData(MessageID.SyncItem,-1,-1,null,i);
					}
				}
				else
				{
					despawnEntry.lastType=item.type;
					despawnEntry.lastStack=item.stack;
					despawnEntry.timeLeft=ItemDespawnTime;
				}
			}
			else despawnEntry.lastActive=false;

			itemDespawnTrackingCooldown=600;
		}
		else itemDespawnTrackingCooldown--;
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