using Terraria.ModLoader;

namespace CywilizowanysMod.ContentBases;

public abstract class CywilsItem : ModItem,ICywilsContent
{
	string ICywilsContent.AssetCategory=>"Items";
	public override string Texture=>this.DefaultTexturePath();
}