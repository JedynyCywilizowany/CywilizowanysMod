using System.ComponentModel;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace CywilizowanysMod.Config;

[Autoload(Side=ModSide.Client)]
public class CywilsConfig_Client : ModConfig
{
	public override ConfigScope Mode=>ConfigScope.ClientSide;

	[ReloadRequired]
	[DefaultValue(true)]
	public bool TextureLoadOptimization{get;set;}
	public bool AutosellWhenBagIsFull{get;set;}

	public override void OnLoaded()
	{
		//To jest tu głównie dla tego że to jedyny hak tModLoader'a wywoływany dość wcześnie
		if (!Main.dedServ&&TextureLoadOptimization) CywilizowanysMod.OptimizeTextureLoad();
	}
}