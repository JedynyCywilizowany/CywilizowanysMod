using System;
using ColonyLib;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CywilizowanysMod;

partial class CywilsSystem
{
	private static bool Item_Merging(Item item)
	{
		bool netUpdate=false;
		bool isMerging=false;
		if (item.stack<item.maxStack)
		{
			const float maxStackRange=32*16;
			var capProgress=(item.instanced ? itemCapProgressLocal : itemCapProgress);
			var stackRangeSq=maxStackRange*capProgress;
			stackRangeSq*=stackRangeSq;
			for (int i=0;i<Main.maxItems;i++)
			{
				Item item2=Main.item[i];
				if (item2.active&&!item2.beingGrabbed&&item.whoAmI!=item2.whoAmI&&item2.type==item.type&&item2.stack>0&&item2.stack<item2.maxStack&&item.instanced==item2.instanced&&item.playerIndexTheItemIsReservedFor==item2.playerIndexTheItemIsReservedFor&&ItemLoader.CanStack(item,item2)&&ItemLoader.CanStackInWorld(item,item2))
				{
					var center=item.Center;
					var center2=item2.Center;
					var centerDif=center2-center;
					var centerDistSq=centerDif.LengthSquared();
					if (centerDistSq<=stackRangeSq)
					{
						isMerging=true;
						item.Center=center.MoveTowards(center2,1f);
						item.velocity=item.velocity.MoveTowards(centerDif,0.25f);
						if (item.IsReservedHere())
						{
							item.keepTime=Math.Max(15,item.keepTime);
							item.noGrabDelay=Math.Max(15,item.noGrabDelay);
							item2.keepTime=Math.Max(15,item2.keepTime);
							item2.noGrabDelay=Math.Max(15,item2.noGrabDelay);
						}
						if (item.IsReservedHere()&&item.whoAmI<item2.whoAmI&&(centerDistSq<256f||capProgress>0.9f))
						{
							ItemLoader.StackItems(item,item2,out int transferred);
							if (item2.stack<=0)
							{
								var balance=(float)transferred/item.stack;
								item.Center=ColonyUtils.LerpVector2(item.Center,item2.Center,balance);
								item.velocity=ColonyUtils.LerpVector2(item.velocity,item2.velocity,balance);

								item2.SetDefaults();
								item2.active=false;
							}
							if (Main.netMode!=NetmodeID.SinglePlayer&&!item.instanced)
							{
								NetMessage.SendData(MessageID.SyncItem,number:i);
								netUpdate=true;
							}
						}
					}
				}
			}
		}
		else if (item.type>=ItemID.CopperCoin&&item.type<ItemID.PlatinumCoin)
		{
			var reservedIndex=item.playerIndexTheItemIsReservedFor;
			item.SetDefaults(item.type+1);
			item.stack=1;
			item.playerIndexTheItemIsReservedFor=reservedIndex;
		}

		if (netUpdate) NetMessage.SendData(MessageID.SyncItem,number:item.whoAmI);
		return isMerging;
	}
}