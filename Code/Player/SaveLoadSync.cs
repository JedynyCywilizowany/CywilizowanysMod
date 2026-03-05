using Terraria;
using Terraria.ModLoader;
using System.Collections.Generic;
using Terraria.ID;
using Terraria.ModLoader.IO;
using CywilizowanysMod.ContentBases;
using System.IO;
using CywilizowanysMod.Common;

namespace CywilizowanysMod;

partial class CywilsPlayer : ModPlayer
{
	private const string SavingTag_AutosellingData="Autoselling";
	private const string SavingTag_Vanilla="Vanilla";
	private const string SavingTag_Modded="Modded";
	public override void SaveData(TagCompound tag)
	{
		List<string>? vanilla=null;
		Dictionary<string,List<string>>? modded=null;
		foreach (var item in autosoldItems)
		{
			if (item>=ItemID.Count)
			{
				if (item<ItemLoader.ItemCount)
				{
					var modItem=ModContent.GetModItem(item);
				
					if (!(modded??=new()).TryGetValue(modItem.Mod.Name,out var itemList)) modded[modItem.Mod.Name]=itemList=new();
					itemList.Add(modItem.Name);
				}
			}
			else
			{
				if (ItemID.Search.TryGetName(item,out var name)) (vanilla??=new()).Add(name);
			}
		}
		foreach (var entry in autosoldItemsUnloaded)
		{
			(modded??=new()).Add(entry.Key,entry.Value);
		}
		TagCompound? autosoldData=null;
		if (vanilla is not null)
		{
			(autosoldData??=new()).Add(SavingTag_Vanilla,vanilla);
		}
		if (modded is not null)
		{
			TagCompound modded2=new();
			foreach (var entry in modded) modded2[entry.Key]=entry.Value;
			(autosoldData??=new()).Add(SavingTag_Modded,modded2);
		}
		if (autosellingEnabled) (autosoldData??=new()).Add(nameof(autosellingEnabled),true);
		if (AutosellerBagFill>0) (autosoldData??=new()).Add(nameof(AutosellerBagFill),AutosellerBagFill);

		tag.AddIfNotEmpty(SavingTag_AutosellingData,autosoldData);
	}
	public override void LoadData(TagCompound tag)
	{
		try
		{
			if (tag.TryGet<TagCompound>(SavingTag_AutosellingData,out var autoSoldData))
			{
				if (autoSoldData.TryGet<List<string>>(SavingTag_Vanilla,out var vList)) foreach (var entry in vList)
				{
					if (ItemID.Search.TryGetId(entry,out var id)) autosoldItems.Add(id);
				}
				if (autoSoldData.TryGet<TagCompound>(SavingTag_Modded,out var mList)) foreach (var modEntry in mList)
				{
					if (ModLoader.TryGetMod(modEntry.Key,out var mod))
					{
						foreach (var item in (modEntry.Value as List<string>)??[])
						{
							if (mod.TryFind<ModItem>(item,out var modItem)) autosoldItems.Add(modItem.Type);
						}
					}
					else autosoldItemsUnloaded.Add(new(modEntry.Key,(List<string>)modEntry.Value));
				}
				autosellingEnabled=autoSoldData.Get<bool>(nameof(autosellingEnabled));
				AutosellerBagFill=autoSoldData.Get<int>(nameof(AutosellerBagFill));
			}
		}
		catch
		{}
	}

	public class SyncAutosoldItems : CywilsPacketType
	{
		public override bool AutoRedistributed=>true;
		internal static CywilsPacket SyncForPlayer(Player player)
		{
			var packet=Get<SyncAutosoldItems>();
			packet.Write((byte)player.whoAmI);
			var list=player.GetModPlayer<CywilsPlayer>().autosoldItems;
			packet.Write((ushort)list.Count);
			foreach (var item in list) packet.Write((ushort)item);
			return packet;
		}
		
		public override void Handle(BinaryReader reader,int whoAmI)
		{
			var modPlayer=Main.player[reader.ReadByte()].GetModPlayer<CywilsPlayer>();
			int length=reader.ReadUInt16();
			var set=modPlayer.autosoldItems;
			lock (set)
			{
				set.Clear();
				for (int i=0;i<length;i++) set.Add(reader.ReadUInt16());
				modPlayer.UpdateAutosell();
			}
		}
	}
	public class SyncAutosellState : CywilsPacketType
	{
		public override bool AutoRedistributed=>true;
		internal static CywilsPacket SyncForPlayer(Player player)
		{
			var packet=Get<SyncAutosellState>();
			packet.Write((byte)player.whoAmI);
			var modPlayer=player.GetModPlayer<CywilsPlayer>();
			packet.Write(modPlayer.AutosellingActive);
			
			return packet;
		}
		
		public override void Handle(BinaryReader reader,int whoAmI)
		{
			var modPlayer=Main.player[reader.ReadByte()].GetModPlayer<CywilsPlayer>();
			modPlayer.autosellingEnabled=reader.ReadBoolean();
		}
	}
	public override void SyncPlayer(int toWho,int fromWho,bool newPlayer)
	{
		if (Main.dedServ)
		{
			SyncAutosoldItems.SyncForPlayer(Player).Send(toWho,fromWho);
			SyncAutosellState.SyncForPlayer(Player).Send(toWho,fromWho);
		}
		else
		{
			if (newPlayer)
			{
				//SyncAutosoldItems.SyncForPlayer(Player).Send();
				SyncAutosellState.SyncForPlayer(Player).Send();
			}
		}
	}
	public override void CopyClientState(ModPlayer targetCopy)
	{
		/*
		var clientCopy=(CywilsPlayer)targetCopy;

		clientCopy.autosellingEnabled=AutosellingActive;
		*/
	}
	public override void SendClientChanges(ModPlayer clientPlayer)
	{
		var clientCopy=(CywilsPlayer)clientPlayer;

		if (clientCopy.autosellingEnabled!=AutosellingActive)
		{
			SyncAutosellState.SyncForPlayer(Player).Send();
			clientCopy.autosellingEnabled=AutosellingActive;
		}
	}
}