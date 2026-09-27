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
			var stackRangeSq=maxStackRange*itemCapProgress;
			stackRangeSq*=stackRangeSq;
			for (int i=0;i<Main.maxItems;i++)
			{
				Item item2=Main.item[i];
				if (item2.active&&!item2.beingGrabbed&&!ReferenceEquals(item,item2)&&item2.type==item.type&&item2.stack<item2.maxStack&&item.instanced==item2.instanced&&item.playerIndexTheItemIsReservedFor==item2.playerIndexTheItemIsReservedFor&&ItemLoader.CanStack(item,item2))
				{
					var center=item.Center;
					var center2=item2.Center;
					var centerDif=center2-center;
					var centerDistSq=centerDif.LengthSquared();
					if (centerDistSq<=stackRangeSq)
					{
						isMerging=true;
						item.noGrabDelay=15;
						item2.noGrabDelay=15;
						if (item.IsReservedHere()&&(centerDistSq<256f||itemCapProgress>0.9f))
						{
							ItemLoader.StackItems(item,item2,out int transferred);
							if (item2.stack<=0)
							{
								item.Center=((item.Center*(item.stack-transferred))+(item2.Center*transferred))/item.stack;
								item.velocity=((item.velocity*(item.stack-transferred))+(item2.velocity*transferred))/item.stack;
								item2.active=false;
							}
							if (Main.netMode!=NetmodeID.SinglePlayer&&!item.instanced)
							{
								NetMessage.SendData(MessageID.SyncItem,number:i);
								netUpdate=true;
							}
						}
						else
						{
							item.Center=center.MoveTowards(center2,1f);
							item.velocity=item.velocity.MoveTowards(centerDif,0.25f);
						}
					}
				}
			}
		}
		else if (item.type==ItemID.CopperCoin||item.type==ItemID.SilverCoin||item.type==ItemID.GoldCoin)
		{
			var reservedIndex=item.playerIndexTheItemIsReservedFor;
			item.SetDefaults(item.type switch
			{
				ItemID.CopperCoin=>ItemID.SilverCoin,
				ItemID.SilverCoin=>ItemID.GoldCoin,
				ItemID.GoldCoin=>ItemID.PlatinumCoin,
				_=>ItemID.CopperCoin,
			});
			item.stack=1;
			item.playerIndexTheItemIsReservedFor=reservedIndex;
		}

		if (netUpdate) NetMessage.SendData(MessageID.SyncItem,number:item.whoAmI);
		return isMerging;
	}
}