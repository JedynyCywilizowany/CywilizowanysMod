using System;
using Microsoft.Xna.Framework;

namespace CywilizowanysMod.Common;

partial class CywilsUtils
{
	/// <summary>
	/// a.k.a. Taxicab Distance<br/><br/>
	/// Abs ( x - x2 ) + Abs ( y - y2 )
	/// </summary>
	public static int ManhattanDistance(int fromX,int fromY,int toX,int toY)
	{
		return Math.Abs(toX-fromX)+Math.Abs(toY-fromY);
	}
	/// <inheritdoc cref="ManhattanDistance(int,int,int,int)"/>
	public static int ManhattanDistance(this Point from,Point to)
	{
		return ManhattanDistance(from.X,from.Y,to.X,to.Y);
	}
	/// <inheritdoc cref="ManhattanDistance(int,int,int,int)"/>
	public static float ManhattanDistance(float fromX,float fromY,float toX,float toY)
	{
		return Math.Abs(toX-fromX)+Math.Abs(toY-fromY);
	}
	/// <inheritdoc cref="ManhattanDistance(int,int,int,int)"/>
	public static float ManhattanDistance(this Vector2 from,Vector2 to)
	{
		return ManhattanDistance(from.X,from.Y,to.X,to.Y);
	}

	/// <summary>
	/// Max ( Abs ( x - x2 ), Abs ( y - y2 ) )
	/// </summary>
	public static int ChebyshevDistance(int fromX,int fromY,int toX,int toY)
	{
		return Math.Max(Math.Abs(toX-fromX),Math.Abs(toY-fromY));
	}
	/// <inheritdoc cref="ChebyshevDistance(int,int,int,int)"/>
	public static int ChebyshevDistance(this Point from,Point to)
	{
		return ChebyshevDistance(from.X,from.Y,to.X,to.Y);
	}
	/// <inheritdoc cref="ChebyshevDistance(int,int,int,int)"/>
	public static float ChebyshevDistance(float fromX,float fromY,float toX,float toY)
	{
		return Math.Max(Math.Abs(toX-fromX),Math.Abs(toY-fromY));
	}
	/// <inheritdoc cref="ChebyshevDistance(int,int,int,int)"/>
	public static float ChebyshevDistance(this Vector2 from,Vector2 to)
	{
		return ChebyshevDistance(from.X,from.Y,to.X,to.Y);
	}
}