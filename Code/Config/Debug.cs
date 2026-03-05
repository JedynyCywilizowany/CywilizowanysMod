using Terraria.ModLoader.Config;

namespace CywilizowanysMod.Config;

public class CywilsConfig_Debug : ModConfig
{
	public override ConfigScope Mode=>ConfigScope.ServerSide;

	public bool ReportPackets{get;set;}
}