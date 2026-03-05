using Terraria.ModLoader;

namespace CywilizowanysMod.ContentBases;

public abstract class CywilsProjectile : ModProjectile,ICywilsContent
{
	string ICywilsContent.AssetCategory=>"Projectiles";
	public override string Texture=>this.DefaultTexturePath();
}