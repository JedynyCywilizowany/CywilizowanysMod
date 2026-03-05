using Terraria.ModLoader;

namespace CywilizowanysMod.ContentBases;

public abstract class CywilsWall : ModWall,ICywilsContent
{
	string ICywilsContent.AssetCategory=>"Walls";
	public override string Texture=>this.DefaultTexturePath();
}