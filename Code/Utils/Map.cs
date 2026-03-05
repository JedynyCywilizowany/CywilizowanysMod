using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Map;

namespace CywilizowanysMod.Common;

partial class CywilsUtils
{
	/// <summary>
	/// Gets the color of this tile type on the map.
	/// </summary>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public static Color GetMapColorTile(int type)
	{
		if (Main.dedServ) return default;
		var mapTile=MapTile.Create(MapHelper.tileLookup[type],byte.MaxValue,0);
		return MapHelper.GetMapTileXnaColor(ref mapTile);
	}
	/// <summary>
	/// Gets the color of this wall type on the map.
	/// </summary>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public static Color GetMapColorWall(int type)
	{
		if (Main.dedServ) return default;
		var mapTile=MapTile.Create(MapHelper.wallLookup[type],byte.MaxValue,0);
		return MapHelper.GetMapTileXnaColor(ref mapTile);
	}
}