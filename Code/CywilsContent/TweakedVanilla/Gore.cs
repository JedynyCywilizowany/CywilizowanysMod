using Terraria.ModLoader;

namespace CywilizowanysMod.ContentBases;

public abstract class CywilsGore : ModGore,ICywilsContent
{
	string ICywilsContent.AssetCategory=>"Gores";
	public override string Texture=>this.DefaultTexturePath();
}