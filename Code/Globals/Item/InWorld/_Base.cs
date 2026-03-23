using CywilizowanysMod.Common;
using CywilizowanysMod.Config;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CywilizowanysMod.Globals;

partial class CywilsGlobItem : GlobalItem
{
	internal const int ItemDespawnTime_Valuable=60*(60*60);
	internal const int ItemDespawnTime_Cheap=30*(60*60);
	internal const int ItemDespawnTime_Pickups=60*60;

	public static int FindItemIndex(Item item)
	{
		return Array.FindIndex(Main.item,(r)=>ReferenceEquals(r,item));
	}

	public bool merging;
	public bool lastMerging;
	public override void Update(Item item,ref float gravity,ref float maxFallSpeed)
	{
		if (item.stack<item.maxStack&&CywilsSystem.itemCapProgress>=0.25f)
		{
			const float maxStackRange=32*16;
			var stackRangeSq=maxStackRange*CywilsSystem.itemCapProgress;
			stackRangeSq*=stackRangeSq;
			for (int i=0;i<Main.maxItems;i++)
			{
				Item item2=Main.item[i];
				if (item2.active&&!item2.beingGrabbed&&!ReferenceEquals(item,item2)&&item2.type==item.type&&item2.stack<item2.maxStack&&item.instanced==item2.instanced&&ItemLoader.CanStack(item,item2))
				{
					var center=item.Center;
					var center2=item2.Center;
					var centerDif=center2-center;
					var centerDistSq=centerDif.LengthSquared();
					if (centerDistSq<=stackRangeSq)
					{
						merging=true;
						item.noGrabDelay=15;
						item2.noGrabDelay=15;
						if (item.IsReservedHere()&&(centerDistSq<256f||CywilsSystem.itemCapProgress>0.9f))
						{
							ItemLoader.StackItems(item,item2,out int transferred);
							if (item2.stack<=0)
							{
								item.Center=((item.Center*(item.stack-transferred))+(item2.Center*transferred))/item.stack;
								item.velocity=((item.velocity*(item.stack-transferred))+(item2.velocity*transferred))/item.stack;
								item2.active=false;
							}
							if (Main.netMode!=NetmodeID.SinglePlayer&&!item2.instanced) NetMessage.SendData(MessageID.SyncItem,number:i);
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
		if ((item.type==ItemID.CopperCoin||item.type==ItemID.SilverCoin||item.type==ItemID.GoldCoin)&&item.stack==item.maxStack)
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

		if (!merging)
		{
			if (ModContent.GetInstance<CywilsConfig_World>().UnstuckItems&&!AvailableSpace(item.position+Vector2.One,item.BottomRight-Vector2.One)) GetUnstuck(item);
			else
			{
				unstuckingRadius=0;

				if (item.wet&&(!item.shimmerWet||!item.CanShimmer())&&Collision.WetCollision(item.position,item.width,item.height/2))
				{
					if (item.velocity.Y>-0.2f) item.velocity.Y-=0.15f;
				}
				else if (ItemID.Sets.ItemNoGravity[item.type]&&(item.position.Y+item.height)/16<=Main.worldSurface)
				{	
					if (item.velocity.Y<0.2) item.velocity.Y+=0.15f;
				}
			}

			if (ModContent.GetInstance<CywilsConfig_World>().DespawnAbandonedItems&&(!Main.dedServ||item.IsReservedHere()))
			{
				int despawnTime;
				if (ItemID.Sets.IsAPickup[item.type]) despawnTime=ItemDespawnTime_Pickups;
				else if (item.rare==ItemRarityID.White&&item.type!=ItemID.GoldCoin&&item.type!=ItemID.PlatinumCoin) despawnTime=ItemDespawnTime_Cheap;
				else despawnTime=ItemDespawnTime_Valuable;

				var timeLeft=despawnTime-(item.timeSinceItemSpawned-ItemID.Sets.OverflowProtectionTimeOffset[item.type])/ItemID.Sets.ItemSpawnDecaySpeed[item.type];
			
				if (!Main.dedServ&&timeLeft<=60*60&&item.position.Between(Main.Camera.ScaledPosition,Main.Camera.ScaledPosition+Main.Camera.ScaledSize))
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
				
				if (item.IsReservedHere()&&timeLeft<0)
				{
					item.active=false;
					if (Main.netMode!=NetmodeID.SinglePlayer&&!item.instanced) NetMessage.SendData(MessageID.SyncItem,number:FindItemIndex(item));
				}
			}
		}
	}
	public override void PostUpdate(Item item)
	{
		if (Main.netMode!=NetmodeID.SinglePlayer&&!item.instanced)
		{
			if (!merging&&lastMerging) NetMessage.SendData(MessageID.SyncItem,number:FindItemIndex(item));
			lastMerging=merging;
		}
		merging=false;

		CywilsSystem.itemCounter++;
	}
	public override bool CanPickup(Item item,Player player)
	{
		return !merging;
	}
	public override bool CanStackInWorld(Item destination,Item source)
	{
		return CywilsSystem.itemCapProgress<0.25f;
	}
	public override void OnStack(Item destination,Item source,int numToTransfer)
	{
		destination.timeSinceItemSpawned=Math.Min(destination.timeSinceItemSpawned,source.timeSinceItemSpawned);
	}
	public override bool OnPickup(Item item,Player player)
	{
		var modPlayer=player.GetModPlayer<CywilsPlayer>();
		if (modPlayer.AutosellingActive&&modPlayer.IsItemAutosold(item.type))
		{
			SoundEngine.PlaySound((item.value>0 ? SoundID.Coins : SoundID.Grab),player.Center);
			
			int totalValue=CywilsUtils.AveragedInt(0.15d/modPlayer.AutosellingPriceMultiplier*item.value*item.stack);
			
			if (modPlayer.autosellerBagAvailable&&!modPlayer.AutosellerAvailable)
			{
				modPlayer.AutosellerBagFill+=totalValue;
			}
			else
			{
				player.GiveMoney(totalValue);
				modPlayer.AutosellTransferAnimation(item.type,totalValue);
			}
			
			return false;
		}

		return true;
	}
	public override bool ItemSpace(Item item,Player player)
	{
		var modPlayer=player.GetModPlayer<CywilsPlayer>();
		return modPlayer.AutosellingActive&&modPlayer.IsItemAutosold(item.type);
	}
	public override void OnSpawn(Item item,IEntitySource source)
	{
		unstuckingRadius=0;
		merging=false;
		lastMerging=false;
	}
}