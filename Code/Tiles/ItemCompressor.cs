using CywilizowanysMod.ContentBases;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace CywilizowanysMod.Tiles;

public class ItemCompressor : CywilsTile
{
	public override void SetStaticDefaults()
	{
		Main.tileSolid[Type]=false;
		Main.tileBlockLight[Type]=true;
		Main.tileNoFail[Type]=true;
		Main.tileNoAttach[Type]=true;
		Main.tileLighted[Type]=true;
		Main.tileFrameImportant[Type]=true;
		
		DustType=DustID.Stone;

		AddMapEntry(new Color(255,0,0));
	}
}