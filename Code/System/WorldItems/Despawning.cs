using System;
using ColonyLib;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace CywilizowanysMod;

partial class CywilsSystem
{
	private static void Item_Despawning(Item item)
	{
		const int ItemDespawnTime_Valuable=60*(60*60);
		const int ItemDespawnTime_Cheap=30*(60*60);
		const int ItemDespawnTime_Pickups=(60*60);

		int despawnTime;
		if (ItemID.Sets.IsAPickup[item.type]) despawnTime=ItemDespawnTime_Pickups;
		else if (item.rare==ItemRarityID.White&&item.type!=ItemID.GoldCoin&&item.type!=ItemID.PlatinumCoin) despawnTime=ItemDespawnTime_Cheap;
		else despawnTime=ItemDespawnTime_Valuable;

		var timeLeft=despawnTime-(item.timeSinceItemSpawned-ItemID.Sets.OverflowProtectionTimeOffset[item.type])/ItemID.Sets.ItemSpawnDecaySpeed[item.type];
	
		if (!Main.dedServ&&timeLeft<=(60*60)&&item.position.Between(Main.Camera.ScaledPosition,Main.Camera.ScaledPosition+Main.Camera.ScaledSize))
		{
			if (timeLeft%60==0&&despawnTime==ItemDespawnTime_Valuable)
			{
				CombatText.NewText(new Rectangle((int)item.Top.X,(int)item.Top.Y,0,0),Color.Yellow,timeLeft/60,dot:true);
			}
			if ((despawnTime!=ItemDespawnTime_Pickups||timeLeft<=5*60)&&Main.rand.NextBool(Math.Max(1,timeLeft/(despawnTime==ItemDespawnTime_Valuable ? 20 : 10))))
			{
				Dust.NewDustDirect(item.position,item.width,item.height,(despawnTime==ItemDespawnTime_Pickups ? DustID.TreasureSparkle : DustID.Smoke),item.velocity.X,item.velocity.Y,Scale:Main.rand.NextFloat(1f,2.5f)).velocity/=2f;
			}
		}
		
		if (item.IsReservedHere()&&timeLeft<0&&!item.beingGrabbed)
		{
			item.active=false;
			if (Main.netMode!=NetmodeID.SinglePlayer&&!item.instanced) NetMessage.SendData(MessageID.SyncItem,number:item.whoAmI);
		}
	}
}