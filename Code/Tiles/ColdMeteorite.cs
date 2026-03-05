using CywilizowanysMod.ContentBases;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CywilizowanysMod.Tiles;

public class ColdMeteorite : CywilsTile
{
	public override string Texture=>"Terraria/Images/Tiles_"+TileID.Meteorite;
	public override void SetStaticDefaults()
	{
		Main.tileSolid[Type]=true;
		Main.tileBrick[Type]=true;
		Main.tileBlockLight[Type]=true;
		Main.tileOreFinderPriority[Type]=Main.tileOreFinderPriority[TileID.Meteorite];
		Main.tileSpelunker[Type]=true;
		Main.tileMergeDirt[Type]=true;
		TileID.Sets.Ore[Type]=true;
		
		DustType=DustID.Meteorite;
		MinPick=50;
		HitSound=SoundID.Tink;

		RegisterItemDrop(ItemID.Meteorite);

		AddMapEntry(new Color(80,70,60),Lang.GetItemName(ItemID.Meteorite));
	}
	public override void PostSetupTileMerge()
	{
		for (int i=0;i<TileLoader.TileCount;i++)
		{
			Main.tileMerge[Type][i]=Main.tileMerge[TileID.Meteorite][i];
			Main.tileMerge[i][Type]=Main.tileMerge[i][TileID.Meteorite];
		}
		Main.tileMerge[Type][TileID.Meteorite]=true;
		Main.tileMerge[TileID.Meteorite][Type]=true;
	}
	public override bool CanExplode(int x,int y)
	{
		return Main.hardMode;
	}
}