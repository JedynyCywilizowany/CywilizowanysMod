using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace CywilizowanysMod.Globals;

partial class CywilsGlobItem : GlobalItem
{
	private short unstuckingRadius;
	private static bool AvailableSpace(int startX,int startY,int endX,int endY)
	{
		if (startX<0||startY<0||endX>=Main.maxTilesX||endY>=Main.maxTilesY) return false;
		for (int x=startX;x<=endX;x++) for (int y=startY;y<=endY;y++)
		{
			if (WorldGen.SolidTile(x,y)) return false;
		}
		return true;
	}
	private static bool AvailableSpace(Vector2 start,Vector2 end)
	{
		var startTile=start.ToTileCoordinates();
		var endTile=end.ToTileCoordinates();
		int startX=startTile.X;
		int startY=startTile.Y;
		int endX=endTile.X;
		int endY=endTile.Y;
		return AvailableSpace(startX,startY,endX,endY);
	}
	private static bool AvailableSpace(int startX,int startY,Point size)
	{
		return AvailableSpace(startX,startY,startX+size.X,startY+size.Y);
	}
	private void GetUnstuck(Item item)
	{
		item.Bottom=item.Bottom.ToTileCoordinates().ToWorldCoordinates(8,14);
		item.velocity=Vector2.Zero;

		const int RadiusGrowDelay=15;
		if (unstuckingRadius%RadiusGrowDelay==0)
		{
			if (unstuckingRadius<RadiusGrowDelay) unstuckingRadius=RadiusGrowDelay;

			var center=item.position.ToTileCoordinates();
			var size=(item.BottomRight.ToTileCoordinates()-item.position.ToTileCoordinates())+new Point(1,1);
			int radius=unstuckingRadius/RadiusGrowDelay;
			int leftX=center.X-radius;
			int topY=center.Y-radius;
			int rightX=center.X+radius;
			int bottomY=center.Y+radius;
			int maxI=radius/2;

			Point move=Point.Zero;

			for (int i=0;i<=maxI;i++)
			{
				if (AvailableSpace(center.X+i,topY+i,size)||AvailableSpace(center.X-i,topY+i,size))
				{
					move.Y=-1;
					goto foundTarget;
				}
				if (AvailableSpace(center.X+i,bottomY-i,size)||AvailableSpace(center.X-i,bottomY-i,size))
				{
					move.Y=1;
					goto foundTarget;
				}
			}
			var (x1,x2,dir)=(center.X<Main.maxTilesX/2 ? (leftX,rightX,1) : (rightX,leftX,-1));
			for (int i=0;i<=maxI;i++)
			{
				var iDir=(i*dir);
				if (AvailableSpace(x1+iDir,center.Y-i,size)||AvailableSpace(x1+iDir,center.Y+i,size))
				{
					move.X=-dir;
					goto foundTarget;
				}
				if (AvailableSpace(x2-iDir,center.Y-i,size)||AvailableSpace(x2-iDir,center.Y+i,size))
				{
					move.X=dir;
					goto foundTarget;
				}
			}

			goto skipFoundTarget;
			foundTarget:
			item.position+=move.ToVector2()*16;
			item.velocity=move.ToVector2();

			unstuckingRadius-=RadiusGrowDelay*2;
		}
		skipFoundTarget:
		unstuckingRadius++;
	}
}