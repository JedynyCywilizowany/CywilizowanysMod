using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CywilizowanysMod.Common;

public static partial class CywilsUtils
{
	/// <summary>
	/// Alphabethically compares the display names of two item types in the current language.
	/// </summary>
	public static readonly Comparer<int> itemSortByNameComparer=Comparer<int>.Create((a,b)=>string.Compare(DummyItems[a].Name,DummyItems[b].Name));
	/// <summary>
	/// Divides an int.<br/>
	/// The remainder is then used to randomly add 1 to the result, so the average is closer to the real quotient.
	/// </summary>
	public static int AveragedDivide(this int x,int divideBy)
	{
		int r=x/divideBy;
		if (Main.rand.NextBool(x%divideBy,divideBy)) r++;
		return r;
	}
	/// <summary>
	/// Casts the given float-point number to int.<br/>
	/// Randomly rounds up or down depending on the fractional part, averages to the original value.
	/// </summary>
	public static int AveragedInt(this float x)
	{
		int r=(int)MathF.Floor(x);
		if (x%1>Main.rand.NextFloat()) r++;
		return r;
	}
	/// <inheritdoc cref="AveragedInt(float)"/>
	public static int AveragedInt(this double x)
	{
		int r=(int)Math.Floor(x);
		if (x%1>Main.rand.NextDouble()) r++;
		return r;
	}
	/// <summary>
	/// Multiplies the value by 100, then rounds it to the specified amount of fractional digits.<br/>
	/// The value is rounded towards 50%, so that a value larger than 0 is never 0%, and a value lesser than 1 is never 100%.
	/// </summary>
	public static float ToPercentage(this float value,int fractionalDigits=0)
	{
		return MathF.Round(value*100,fractionalDigits,(value<0.5f ? MidpointRounding.ToPositiveInfinity : MidpointRounding.ToNegativeInfinity));
	}
	/// <inheritdoc cref="ToPercentage(float,int)"/>
	public static double ToPercentage(this double value,int fractionalDigits=0)
	{
		return Math.Round(value*100,fractionalDigits,(value<0.5 ? MidpointRounding.ToPositiveInfinity : MidpointRounding.ToNegativeInfinity));
	}
	/// <summary>
	/// Creates a span over the entirety of the specified multi-dimensional array of unmanaged structs.<br/>
	/// Make sure <typeparamref name="T"/> matches the type of the array's elements.
	/// </summary>
	public static unsafe Span<T> MultiDimArraySpan<T>(Array array) where T : unmanaged
	{
		return new(Unsafe.AsPointer(ref MemoryMarshal.GetArrayDataReference(array)),array.Length);
	}
	/// <summary>
	/// If <paramref name="value"/> is not empty, ensures the tooltip exists and sets its value,<br/>
	/// otherwise, removes the tooltip.
	/// </summary>
	public static void UpdateTooltip(this List<TooltipLine> tooltips,Mod mod,string name,string value,Color? color=null)
	{
		bool MatchLine(TooltipLine l)
		{
			return l.Name==name&&l.Mod==mod.Name;
		}

		if (value=="")
		{
			var line=tooltips.FindIndex(MatchLine);
			if (line>=0) tooltips.RemoveAt(line);
		}
		else
		{
			var line=tooltips.FindIndex(MatchLine);
			if (line<0) tooltips.Add(new TooltipLine(mod,name,value){OverrideColor=color});
			else tooltips[line].Text=value;
		}
	}
	/// <inheritdoc cref="UpdateTooltip(List{TooltipLine},Mod,string,string,Color?)"/>
	public static void UpdateTooltip(this List<TooltipLine> tooltips,Mod mod,string name,LocalizedText value,Color? color=null)
	{
		UpdateTooltip(tooltips,mod,name,value.Value,color);
	}
	/// <summary>
	/// Creates a string that displays the specified amount of coins using item chat tags.
	/// </summary>
	public static string ValueToCoinsCompact(long value)
	{
		StringBuilder builder=new((2*4+2)+((4+2)*4+2));

		if (value>=Item.platinum)
		{
			builder.Append(value/Item.platinum);
			builder.Append("[i:"+ItemID.PlatinumCoin+"]");
		}
		if (value>=Item.gold)
		{
			builder.Append(value/Item.gold%100);
			builder.Append("[i:"+ItemID.GoldCoin+"]");
		}
		if (value>=Item.silver)
		{
			builder.Append(value/Item.silver%100);
			builder.Append("[i:"+ItemID.SilverCoin+"]");
		}
		builder.Append(value%100);
		builder.Append("[i:"+ItemID.CopperCoin+"]");

		return builder.ToString();
	}
	/// <summary>
	/// Contains item IDs of coins, from copper to platinum.
	/// </summary>
	public static readonly ImmutableArray<int> CoinTypes=[ItemID.CopperCoin,ItemID.SilverCoin,ItemID.GoldCoin,ItemID.PlatinumCoin];
	/// <summary>
	/// Spawns the specified amount of money on the player.<br/>
	/// Can be called on both server and client.
	/// </summary>
	public static void GiveMoney(this Player player,int amount)
	{
		foreach (var coinType in CoinTypes)
		{
			if (amount<=0) break;

			player.QuickSpawnItem(player.GetSource_DropAsItem(),coinType,amount%100);
			amount/=100;
		}
	}
	/// <summary>
	/// Whether this player's <see cref="Entity.whoAmI"/> matches <see cref="Main.myPlayer"/>.
	/// </summary>
	public static bool IsLocal(this Player player)
	{
		return player.whoAmI==Main.myPlayer;
	}
	/// <summary>
	/// Whether this item's <see cref="Item.playerIndexTheItemIsReservedFor"/> matches <see cref="Main.myPlayer"/>.
	/// </summary>
	public static bool IsReservedHere(this Item item)
	{
		return item.playerIndexTheItemIsReservedFor==Main.myPlayer;
	}
	/// <summary>
	/// Emits <see cref="OpCodes.Call"/> to the method this delegate points to.<br/>
	/// Only works for delegates pointing to a single, static, non-lambda function.
	/// </summary>
	public static void EmitCallFromDelegate(this ILCursor c,Delegate func)
	{
		c.EmitCall(func.GetMethodInfo());
	}
}