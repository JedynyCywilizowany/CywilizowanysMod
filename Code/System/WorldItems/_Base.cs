using ColonyLib;
using CywilizowanysMod.Config;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CywilizowanysMod;

partial class CywilsSystem
{
	private static int itemCounter=0;
	private static int itemCounterLocal=0;
	internal static float itemCapProgress=0f;
	internal static float itemCapProgressLocal=0f;
	internal struct WorldItemUpdateData
	{
		public ushort unstuckingRadius;
	}
	internal static readonly WorldItemUpdateData[] worldItemUpdateDataArr=new WorldItemUpdateData[Main.maxItems];
	public override void PreUpdateItems()
	{
		itemCapProgress=((float)itemCounter)/Main.maxItems;
		itemCapProgressLocal=((float)itemCounterLocal)/Main.maxItems;
		itemCounter=0;
		itemCounterLocal=0;
		for (int i=0;i<Main.maxItems;i++)
		{
			var item=Main.item[i];
			ref var updateDataRef=ref worldItemUpdateDataArr[i];
			if (item.active&&item.type!=ItemID.None&&item.stack>0&&item.whoAmI==i)
			{
				UpdateItem(item,ref updateDataRef);
				if (!item.instanced) itemCounter++;
				itemCounterLocal++;
			}
			else updateDataRef=default;
		}
	}
	private static void UpdateItem(Item item,ref WorldItemUpdateData updateData)
	{
		var config=ModContent.GetInstance<CywilsConfig_World>();

		if (!item.beingGrabbed&&!(((item.instanced ? itemCapProgressLocal : itemCapProgress)>config.ThresholdForItemMerging)&&Item_Merging(item)))
		{
			if (config.UnstuckItems&&!Item_Unstucking_AvailableSpace(item.position+Vector2.One,item.BottomRight-Vector2.One)) Item_Unstucking(item,ref updateData.unstuckingRadius);
			else
			{
				updateData.unstuckingRadius=0;

				if (item.wet&&(!item.shimmerWet||!item.CanShimmer())&&Collision.WetCollision(item.position,item.width,item.height/2))
				{
					if (item.velocity.Y>-0.2f) item.velocity.Y-=0.15f;
				}
				else if (ItemID.Sets.ItemNoGravity[item.type]&&(item.position.Y+item.height)/16<=Main.worldSurface)
				{	
					if (item.velocity.Y<0.2) item.velocity.Y+=0.15f;
				}
			}
		}
		else updateData=default;

		if ((!Main.dedServ||item.IsReservedHere())) Item_Despawning(item);
	}
}