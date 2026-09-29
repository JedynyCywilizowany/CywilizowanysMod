using System;
using ColonyLib;
using CywilizowanysMod.Config;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CywilizowanysMod;

partial class CywilsSystem
{
	private static void Item_Despawning(Item item)
	{
		const int ItemDespawnType_Default=0;
		const int ItemDespawnType_Cheap=1;
		const int ItemDespawnType_Pickups=2;

		var config=ModContent.GetInstance<CywilsConfig_World>();

		int despawnType;
		if (ItemID.Sets.IsAPickup[item.type]) despawnType=ItemDespawnType_Pickups;
		else if (item.rare==ItemRarityID.White&&item.type!=ItemID.GoldCoin&&item.type!=ItemID.PlatinumCoin) despawnType=ItemDespawnType_Cheap;
		else despawnType=ItemDespawnType_Default;

		int despawnTime=(despawnType==ItemDespawnType_Pickups ? config.PickupsDespawnTime : config.ItemsDespawnTime);
		if (despawnTime==0) return;
		despawnTime*=(60*60);
		if (despawnType==ItemDespawnType_Cheap) despawnTime/=2;

		var timeLeft=despawnTime-(item.timeSinceItemSpawned-ItemID.Sets.OverflowProtectionTimeOffset[item.type])/ItemID.Sets.ItemSpawnDecaySpeed[item.type];
	
		if (!Main.dedServ&&timeLeft<=(60*60)&&item.position.Between(Main.Camera.ScaledPosition,Main.Camera.ScaledPosition+Main.Camera.ScaledSize))
		{
			if (timeLeft%60==0&&despawnType==ItemDespawnType_Default)
			{
				CombatText.NewText(new Rectangle((int)item.Top.X,(int)item.Top.Y,0,0),Color.Yellow,timeLeft/60,dot:true);
			}
			if ((despawnType!=ItemDespawnType_Pickups||timeLeft<=5*60)&&Main.rand.NextBool(Math.Max(1,timeLeft/(despawnType==ItemDespawnType_Default ? 20 : 10))))
			{
				Dust.NewDustDirect(item.position,item.width,item.height,(despawnType==ItemDespawnType_Pickups ? DustID.TreasureSparkle : DustID.Smoke),item.velocity.X,item.velocity.Y,Scale:Main.rand.NextFloat(1f,2.5f)).velocity/=2f;
			}
		}

		if (item.IsReservedHere()&&timeLeft<0&&!item.beingGrabbed)
		{
			item.SetDefaults();
			item.active=false;
			if (Main.netMode!=NetmodeID.SinglePlayer&&!item.instanced) NetMessage.SendData(MessageID.SyncItem,number:item.whoAmI);
		}
	}
}