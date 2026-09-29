using ColonyLib;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace CywilizowanysMod.Globals;

partial class CywilsGlobItem
{
	public override bool OnPickup(Item item,Player player)
	{
		var modPlayer=player.GetModPlayer<CywilsPlayer>();
		if (modPlayer.AutosellingActive&&modPlayer.IsItemAutosold(item.type))
		{
			SoundEngine.PlaySound((item.value>0 ? SoundID.Coins : SoundID.Grab),player.Center);
			
			int totalValue=(0.15d/modPlayer.AutosellingPriceMultiplier*item.value*item.stack).AveragedInt();
			
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

	public override void OnStack(Item destination,Item source,int numToTransfer)
	{
		destination.timeSinceItemSpawned=Math.Min(destination.timeSinceItemSpawned,source.timeSinceItemSpawned);
	}
	public override void NetSend(Item item,BinaryWriter writer)
	{
		writer.Write7BitEncodedInt(item.timeSinceItemSpawned-ItemID.Sets.OverflowProtectionTimeOffset[item.type]);
	}
	public override void NetReceive(Item item, BinaryReader reader)
	{
		item.timeSinceItemSpawned=reader.Read7BitEncodedInt()+ItemID.Sets.OverflowProtectionTimeOffset[item.type];
	}
}