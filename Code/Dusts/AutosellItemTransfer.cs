using Microsoft.Xna.Framework;
using Terraria;
using System;
using Terraria.UI;
using CywilizowanysMod.ContentBases;
using Terraria.ID;
using CywilizowanysMod.Common;

namespace CywilizowanysMod.Dusts;

public class AutosellItemTransfer : CywilsDust
{
	internal class AutosellTansferData(Vector2 stationPos,int owner,int buyer,int itemType,int coins)
	{
		public Vector2 stationPos=stationPos;
		public byte owner=(byte)owner;
		public int buyer=buyer;
		public int itemType=itemType;
		public int coins=coins;
	}

	public override string Texture=>CywilsContentUtils.NoTexture;
	public override void OnSpawn(Dust dust)
	{
		dust.scale=1f;
		dust.fadeIn=0;
		dust.alpha=0;
	}
	public override bool PreDraw(Dust dust)
	{
		if (dust.customData is AutosellTansferData data)
		{
			ItemSlot.DrawItemIcon(CywilsUtils.DummyItems[data.itemType],ItemSlot.Context.InWorld,Main.spriteBatch,dust.position-Main.screenPosition,dust.scale,32f,Lighting.GetColor(dust.position.ToTileCoordinates()));
		}
		return false;
	}
	public override bool Update(Dust dust)
	{
		const int MaxTimeToReachDestination=180;
		if (dust.customData is AutosellTansferData data)
		{
			dust.fadeIn++;

			if (dust.alpha==1&&data.buyer>=0&&(data.buyer>=Main.maxNPCs||!Main.npc[data.buyer].active)) data.buyer=-1;
			Vector2 destination=dust.alpha switch
			{
				0=>data.stationPos,
				1=>(data.buyer<0 ? data.stationPos : Main.npc[data.buyer].Center),
				2=>data.stationPos,
				3=>Main.player[data.owner].Center,
				_=>default,
			};
			
			dust.position+=(destination-dust.position)*Math.Min(1f,dust.fadeIn/MaxTimeToReachDestination);
			if (dust.position.DistanceSQ(destination)<16)
			{
				if (dust.alpha>=3) goto killDust;
				else
				{
					if (dust.alpha==1)
					{
						if (data.coins>0)
						{
							if (data.coins>=Item.platinum) data.itemType=ItemID.PlatinumCoin;
							else if (data.coins>=Item.gold) data.itemType=ItemID.GoldCoin;
							else if (data.coins>=Item.silver) data.itemType=ItemID.SilverCoin;
							else data.itemType=ItemID.CopperCoin;
						}
						else goto killDust;
					}

					dust.alpha++;
					dust.fadeIn=0;
				}
			}
		}
		
		goto endFunction;
		killDust:
		dust.active=false;
		endFunction:
		return false;
	}
}