using CywilizowanysMod.Tiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CywilizowanysMod.Globals;

public class CywilsGlobTile : GlobalTile
{
	public override void RandomUpdate(int x,int y,int type)
	{	
		if (TileID.Sets.Conversion.Grass[type])
		{
			if (!WorldGen.TileIsExposedToAir(x,y)) WorldGen.ConvertTile(x,y,TileID.Dirt);
			else
			{
				if (Main.rand.NextBool(3))
				{
					int x2=x+Main.rand.Next(-1,2);
					int y2=y+Main.rand.Next(-1,2);
					if (Framing.GetTileSafely(x2,y2).TileType==TileID.ClayBlock) WorldGen.ConvertTile(x2,y2,TileID.Dirt);
				}
			}
		}
		else if ((TileID.Sets.Conversion.JungleGrass[type]||TileID.Sets.Conversion.MushroomGrass[type])&&!WorldGen.TileIsExposedToAir(x,y)) WorldGen.ConvertTile(x,y,TileID.Mud);
		else if (TileID.Sets.Conversion.Moss[type]&&!WorldGen.TileIsExposedToAir(x,y)) WorldGen.ConvertTile(x,y,TileID.Stone);
		else if (TileID.Sets.Conversion.MossBrick[type]&&!WorldGen.TileIsExposedToAir(x,y)) WorldGen.ConvertTile(x,y,TileID.GrayBrick);
		else if (TileID.Sets.Leaves[type])
		{
			if (WorldGen.InWorld(x,y,1))
			{
				var tile=Main.tile[x+Main.rand.Next(-1,2),y+Main.rand.Next(-1,2)];
				if (tile.LiquidAmount>0&&tile.LiquidType==LiquidID.Lava)
				{
					WorldGen.KillTile(x,y);
					return;
				}
			}
		}
		else if (type==TileID.Meteorite)
		{
			if (Main.hardMode&&WorldGen.InWorld(x,y,1)&&Main.rand.NextBool(100))
			{
				var tile=Main.tile[x+Main.rand.Next(-1,2),y+Main.rand.Next(-1,2)];
				if (!tile.HasTile||tile.TileType!=TileID.Meteorite) WorldGen.ConvertTile(x,y,ModContent.TileType<ColdMeteorite>());
			}
		}
	}
}