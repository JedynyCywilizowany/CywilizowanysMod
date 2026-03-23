using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace CywilizowanysMod.Config;

public class CywilsConfig_World : ModConfig
{
	public override ConfigScope Mode=>ConfigScope.ServerSide;

	[DefaultValue(true)]
	public bool ShowDayCounter{get;set;}
	
	[Header("WorldItems")]
	[DefaultValue(true)]
	public bool UnstuckItems{get;set;}
	[DefaultValue(true)]
	public bool DespawnAbandonedItems{get;set;}
}