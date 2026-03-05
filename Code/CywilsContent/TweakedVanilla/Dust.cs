using Terraria.ModLoader;

namespace CywilizowanysMod.ContentBases;

public abstract class CywilsDust : ModDust,ICywilsContent
{
	string ICywilsContent.AssetCategory=>"Dusts";
	public override string Texture=>this.DefaultTexturePath();
}