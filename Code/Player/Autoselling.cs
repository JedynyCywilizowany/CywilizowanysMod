using Terraria;
using Terraria.ModLoader;
using System.Collections.Generic;
using Terraria.ID;
using Terraria.ModLoader.Default;
using System;
using CywilizowanysMod.Items;
using Microsoft.Xna.Framework;
using CywilizowanysMod.Dusts;
using CywilizowanysMod.ContentBases;
using System.IO;
using CywilizowanysMod.Common;
using System.Linq;

namespace CywilizowanysMod;

partial class CywilsPlayer : ModPlayer
{
	public bool autosellingEnabled;
	public bool autosellingNear;
	public bool autosellerBagAvailable;
	public bool AutosellerAvailable=>autosellingNear&&autosellingBuyer>=0;
	public bool AutosellingActive=>autosellingEnabled&&(!Player.IsLocal()||(autosellerBagAvailable||AutosellerAvailable));
	public int autosellingTileX=-1;
	public int autosellingTileY=-1;
	public int AutosellingX=>autosellingTileX*16+16;
	public int AutosellingY=>autosellingTileY*16+16;
	public int autosellingBuyer;
	public double AutosellingPriceMultiplier=>(AutosellerAvailable ? Main.ShopHelper.GetShoppingSettings(Player,Main.npc[autosellingBuyer]).PriceAdjustment : 1);
	private int _autosellerBagFill;
	public int AutosellerBagFill
	{
		get=>_autosellerBagFill;
		set
		{
			_autosellerBagFill=Math.Clamp(value,0,AutosellerBag.capacity);
		}
	}

	public SortedSet<int> autosoldItems=(Main.dedServ ? new() : new(CywilsUtils.itemSortByNameComparer));
	public List<KeyValuePair<string,List<string>>> autosoldItemsUnloaded=[];
	public bool IsItemAutosold(int itemType)
	{
		return autosoldItems.Contains(itemType);
	}
	
	private static bool InvalidForAutosell(int itemType)
	{
		return itemType==ModContent.ItemType<UnloadedItem>()||
		itemType<=0||itemType>=ItemLoader.ItemCount||
		ItemID.Sets.ItemsThatShouldNotBeInInventory[itemType]||
		CywilsUtils.CoinTypes.Contains(itemType)||
		itemType==ModContent.ItemType<AutosellerBag>();
	}
	internal void UpdateAutosell()
	{
		autosoldItems.RemoveWhere(InvalidForAutosell);
		if (Main.netMode==NetmodeID.MultiplayerClient&&Player.IsLocal()) SyncAutosoldItems.SyncForPlayer(Player).Send();
	}
	public void AddToAutosell(int itemType)
	{
		autosoldItems.Add(itemType);
		UpdateAutosell();
	}
	public void RemoveFromAutosell(int itemType)
	{
		autosoldItems.Remove(itemType);
		UpdateAutosell();
	}
	public void ToggleAutosell(int itemType)
	{
		if (autosoldItems.Contains(itemType)) RemoveFromAutosell(itemType);
		else AddToAutosell(itemType);
	}

	public class AutosellTransferAnimationBroadcast : CywilsPacketType
	{
		public override bool AutoRedistributed=>true;
		public override void HandleClient(BinaryReader reader,int whoAmI)
		{
			int playerId=reader.ReadByte();
			Vector2 stationPos=new(reader.ReadUInt16()*16+16,reader.ReadUInt16()*16+16);
			var buyer=reader.ReadUInt16();
			var itemType=reader.ReadUInt16();
			var returnedCoins=reader.ReadInt32();
			AutosellTransferAnimation_SpawnDust(Main.player[playerId].Center,stationPos,playerId,buyer,itemType,returnedCoins);
		}
	}
	private static void AutosellTransferAnimation_SpawnDust(Vector2 playerPos,Vector2 stationPos,int playerId,int buyer,int itemType,int returnedCoins)
	{
		Dust.NewDustPerfect(playerPos,ModContent.DustType<AutosellItemTransfer>()).customData=new AutosellItemTransfer.AutosellTansferData(stationPos,playerId,buyer,itemType,returnedCoins);
	}
	internal void AutosellTransferAnimation(int itemType,int returnedCoins)
	{
		if (Main.netMode!=NetmodeID.SinglePlayer)
		{
			var packet=CywilsPacketType.Get<AutosellTransferAnimationBroadcast>();
			packet.Write((byte)Player.whoAmI);
			packet.Write((ushort)autosellingTileX);
			packet.Write((ushort)autosellingTileY);
			packet.Write((ushort)autosellingBuyer);
			packet.Write((ushort)itemType);
			packet.Write(returnedCoins);
			packet.Send();
		}
		if (!Main.dedServ) AutosellTransferAnimation_SpawnDust(Player.Center,new Vector2(AutosellingX,AutosellingY),Player.whoAmI,autosellingBuyer,itemType,returnedCoins);
	}

	internal void DropAutosoldItems()
	{
		if (Player.DeadOrGhost) return;

		List<Item> items=new();
		items.AddRange(Player.inventory.AsSpan(0,50));
		if (Player.HasItem(ItemID.VoidLens))
		{
			items.AddRange(Player.bank4.item);
		}
		
		foreach (var item in items)
		{
			if (!item.IsAir&&!item.favorited&&IsItemAutosold(item.type))
			{
				Player.QuickSpawnItem(Player.GetSource_DropAsItem(),item,item.stack);
				item.TurnToAir();
			}
		}
	}
}