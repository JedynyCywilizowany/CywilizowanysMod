using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ObjectData;
using Terraria.ModLoader;
using Terraria.GameContent.ObjectInteractions;
using CywilizowanysMod.Buffs;
using CywilizowanysMod.ContentBases;

namespace CywilizowanysMod.Tiles;

public class Autoseller : CywilsTile
{
	public override void SetStaticDefaults()
	{
		Main.tileFrameImportant[Type]=true;
		Main.tileNoAttach[Type]=true;

		DustType=DustID.Torch;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
		TileObjectData.newTile.Height=3;
		TileObjectData.newTile.CoordinateHeights=[16,16,16];
		TileObjectData.addTile(Type);

		AddMapEntry(new Color(200,200,200),ModContent.GetInstance<Items.Placeable.Autoseller>().DisplayName);
	}
	public override void SetDrawPositions(int x,int y,ref int width,ref int offsetY,ref int height,ref short tileFrameX,ref short tileFrameY)
	{
		offsetY=2;
		var modPlayer=Main.LocalPlayer.GetModPlayer<CywilsPlayer>();
		var tile=Main.tile[x,y];
		if (modPlayer.autosellingNear&&x-(tile.TileFrameX/16)==modPlayer.autosellingTileX&&y-(tile.TileFrameY/16)==modPlayer.autosellingTileY) tileFrameY+=52;
	}
	public override bool RightClick(int x,int y)
	{
		var modPlayer=Main.LocalPlayer.GetModPlayer<CywilsPlayer>();
		modPlayer.autosellingEnabled=!modPlayer.autosellingEnabled;
		SoundEngine.PlaySound(SoundID.MenuTick);
		return true;
	}
	public override void NearbyEffects(int x,int y,bool closer)
	{
		if (!closer&&!Main.LocalPlayer.DeadOrGhost)
		{
			var tile=Main.tile[x,y];
			if (tile.TileFrameX==0&&tile.TileFrameY==0)
			{
				var player=Main.LocalPlayer;
				var modPlayer=player.GetModPlayer<CywilsPlayer>();

				if (modPlayer.autosellingEnabled)
				{
					player.AddBuff(ModContent.BuffType<Autoselling>(),20);
					var oldTile=Framing.GetTileSafely(modPlayer.autosellingTileX,modPlayer.autosellingTileY);
					if (!oldTile.HasTile||oldTile.TileType!=ModContent.TileType<Autoseller>()||(Math.Abs(player.Center.X-x*16)+Math.Abs(player.Center.Y-y*16)<Math.Abs(modPlayer.AutosellingX-player.Center.X)+Math.Abs(modPlayer.AutosellingY-player.Center.Y)))
					{
						modPlayer.autosellingTileX=x;
						modPlayer.autosellingTileY=y;
					}
				}
			}
		}
	}
	public override bool HasSmartInteract(int x,int y,SmartInteractScanSettings settings)
	{
		return true;
	}
	public override void MouseOver(int x, int y)
	{
		Player player=Main.LocalPlayer;
		player.noThrow=2;
		player.cursorItemIconEnabled=true;
		player.cursorItemIconID=TileLoader.GetItemDropFromTypeAndStyle(Type,TileObjectData.GetTileStyle(Main.tile[x,y]));
	}
}