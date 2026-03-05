using Terraria.ModLoader;

namespace CywilizowanysMod.ContentBases;

public abstract class CywilsTile : ModTile,ICywilsContent
{
	string ICywilsContent.AssetCategory=>"Tiles";
	public override string Texture=>this.DefaultTexturePath();
}