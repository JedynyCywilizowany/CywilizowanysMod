using Terraria.ModLoader.Config;

namespace CywilizowanysMod.Config;

public class CywilsConfig_World : ModConfig
{
	public override ConfigScope Mode=>ConfigScope.ServerSide;

	public bool ShowDayCounter{get;set;}
}