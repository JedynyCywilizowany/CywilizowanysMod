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
				if (item2.active&&!item2.beingGrabbed&&!ReferenceEquals(item,item2)&&item2.type==item.type&&item2.stack<item2.maxStack&&item.instanced==item2.instanced)
				{
					var center=item.Center;
					var center2=item2.Center;
					var centerDif=center2-center;
					var centerDistSq=centerDif.LengthSquared();
					if (centerDistSq<=stackRangeSq&&ItemLoader.CanStack(item,item2))
					{
						merging=true;
						item.noGrabDelay=15;
						item2.noGrabDelay=15;
						if ((Main.netMode==NetmodeID.SinglePlayer||item.playerIndexTheItemIsReservedFor==Main.myPlayer)&&(centerDistSq<256f||CywilsSystem.itemCapProgress>0.9f))
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
		if ((item.type==ItemID.CopperCoin||item.type==ItemID.SilverCoin||item.type==ItemID.GoldCoin)&&item.stack==item.maxStack/*&&(Main.netMode==NetmodeID.SinglePlayer||item.playerIndexTheItemIsReservedFor==Main.myPlayer)*/)
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
			//if (Main.netMode!=NetmodeID.SinglePlayer&&!item.instanced) NetMessage.SendData(MessageID.SyncItem,number:Array.FindIndex(Main.item,(r)=>ReferenceEquals(r,item)));
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
		}
	}
	public override void PostUpdate(Item item)
	{
		if (Main.netMode!=NetmodeID.SinglePlayer&&!item.instanced)
		{
			if (!merging&&lastMerging) NetMessage.SendData(MessageID.SyncItem,number:Array.FindIndex(Main.item,(r)=>ReferenceEquals(r,item)));
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