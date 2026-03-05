using System;
using CywilizowanysMod.Common;
using CywilizowanysMod.ContentBases;
using Microsoft.Xna.Framework;
using Terraria;

namespace CywilizowanysMod.Buffs;

public class Autoselling : CywilsBuff
{
	public override void SetStaticDefaults()
	{
		Main.buffNoTimeDisplay[Type]=true;
		Main.buffNoSave[Type]=true;
	}
	public override void Update(Player player,ref int buffIndex)
	{
		var modPlayer=player.GetModPlayer<CywilsPlayer>();
		modPlayer.autosellingNear=true;

		if (player.IsLocal())
		{
			Vector2 searchCenter=new(modPlayer.AutosellingX,modPlayer.AutosellingY);
			int bestIndex=-1;
			double bestPrice=double.MaxValue;
			foreach (var npc in Main.ActiveNPCs)
			{
				if (CywilsSets.npcCanBeAutosoldTo[npc.type])
				{
					var distance=npc.Center-searchCenter;
					if (Math.Abs(distance.X)<(169*16)&&Math.Abs(distance.Y)<(124*16))
					{
						double price=Main.ShopHelper.GetShoppingSettings(player,npc).PriceAdjustment;
						if (price<bestPrice)
						{
							bestPrice=price;
							bestIndex=npc.whoAmI;
						}
					}
				}
			}
			modPlayer.autosellingBuyer=bestIndex;
		}
	}
	public override bool RightClick(int buffIndex)
	{
		Main.LocalPlayer.GetModPlayer<CywilsPlayer>().autosellingEnabled=false;
		return true;
	}
}