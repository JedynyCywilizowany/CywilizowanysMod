using Terraria.ModLoader;

namespace CywilizowanysMod.ContentBases;

public abstract class CywilsNPC : ModNPC,ICywilsContent
{
	string ICywilsContent.AssetCategory=>"NPCs";
	public override string Texture=>this.DefaultTexturePath();
}