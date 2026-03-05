using Terraria.ModLoader;

namespace CywilizowanysMod.ContentBases;

public abstract class CywilsBuff : ModBuff,ICywilsContent
{
	string ICywilsContent.AssetCategory=>"Buffs";
	public override string Texture=>this.DefaultTexturePath();
}