using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using ReLogic.Reflection;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace CywilizowanysMod.Common;

partial class CywilsUtils
{
	private static readonly List<byte> byteBuffer=new();
	private static readonly List<bool> boolBuffer=new();
	private static readonly MemoryStream serializationStream=new();
	private static readonly BinaryWriter serStreamWriter=new(serializationStream);
	private static readonly BinaryReader serStreamReader=new(serializationStream);
	/// <summary>
	/// Serializes a collection of bools into bytes.<br/>
	/// Adds a header to more accurately retrieve the original length (otherwise it would always be a multiple of 8), so the result should only be deserialized using <see cref="DeserializeBools"/>.
	/// </summary>
	public static ReadOnlySpan<byte> SerializeBools(ReadOnlySpan<bool> bools)
	{
		byteBuffer.Clear();
		BitsByte bits=(byte)(bools.Length%8);
		int currentBit=3;
		foreach (var entry in bools)
		{
			bits[currentBit]=entry;

			currentBit++;
			if (currentBit==8)
			{
				byteBuffer.Add(bits);
				currentBit=0;
			}
		}
		if (currentBit!=0) byteBuffer.Add(bits);
		byteBuffer.Add((byte)currentBit);
		return CollectionsMarshal.AsSpan(byteBuffer);
	}
	/// <inheritdoc cref="SerializeBools(ReadOnlySpan{bool})"/>
	public static ReadOnlySpan<byte> SerializeBools(this bool[] bools)
	{
		//Bez tego AsSpan() myślałoby że chodzi o tą samą funkcję, powodując nieskończoną rekurencję.
		return SerializeBools(bools.AsSpan());
	}
	/// <inheritdoc cref="SerializeBools(ReadOnlySpan{bool})"/>
	public static ReadOnlySpan<byte> SerializeBools(this List<bool> bools)
	{
		return SerializeBools(CollectionsMarshal.AsSpan(bools));
	}
	/// <summary>
	/// Deserializes a collection of bools from bytes.<br/>
	/// Expects a header to more accurately retrieve the original length (otherwise it would always be a multiple of 8), so it should only be used with the output from <see cref="SerializeBools"/>.
	/// </summary>
	public static ReadOnlySpan<bool> DeserializeBools(ReadOnlySpan<byte> bytes)
	{
		boolBuffer.Clear();
		foreach (byte entry in bytes)
		{
			BitsByte bits=entry;
			boolBuffer.Add(bits[0]);
			boolBuffer.Add(bits[1]);
			boolBuffer.Add(bits[2]);
			boolBuffer.Add(bits[3]);
			boolBuffer.Add(bits[4]);
			boolBuffer.Add(bits[5]);
			boolBuffer.Add(bits[6]);
			boolBuffer.Add(bits[7]);
		}
		int newLength=boolBuffer.Count-3;
		int lengthHeader=bytes[0]&7;
		if (lengthHeader!=0) newLength+=lengthHeader-8;
		return CollectionsMarshal.AsSpan(boolBuffer).Slice(3,newLength);
	}
	/// <inheritdoc cref="DeserializeBools(ReadOnlySpan{byte})"/>
	public static ReadOnlySpan<bool> DeserializeBools(this byte[] bytes)
	{
		//Bez tego AsSpan() myślałoby że chodzi o tą samą funkcję, powodując nieskończoną rekurencję.
		return DeserializeBools(bytes.AsSpan());
	}
	/// <inheritdoc cref="DeserializeBools(ReadOnlySpan{byte})"/>
	public static ReadOnlySpan<bool> DeserializeBools(this List<byte> bytes)
	{
		return DeserializeBools(CollectionsMarshal.AsSpan(bytes));
	}

	/// <summary>
	/// If the <paramref name="collection"/> is not empty and not null, adds it to the <paramref name="tag"/>.
	/// </summary>
	/// <typeparam name="T">Type of the <paramref name="collection"/>'s elements.</typeparam>
	/// <param name="tag">The tag to add to.</param>
	/// <param name="key">The key.</param>
	/// <param name="collection">The collection to add.</param>
	public static void AddIfNotEmpty<T>(this TagCompound tag,string key,T[]? collection)
	{
		if (collection is null) return;
		if (collection.Length!=0) tag.Add(key,collection);
	}
	/// <inheritdoc cref="AddIfNotEmpty{T}(TagCompound,string,T[])"/>
	public static void AddIfNotEmpty<T>(this TagCompound tag,string key,List<T>? collection)
	{
		if (collection is null) return;
		if (collection.Count!=0) tag.Add(key,collection);
	}
	/// <inheritdoc cref="AddIfNotEmpty{T}(TagCompound,string,T[])"/>
	public static void AddIfNotEmpty(this TagCompound tag,string key,TagCompound? collection)
	{
		if (collection is null) return;
		if (collection.Count!=0) tag.Add(key,collection);
	}

	/// <summary>
	/// If the <paramref name="value"/> is not the default for <typeparamref name="T"/>, adds it to the <paramref name="tag"/>.
	/// </summary>
	/// <typeparam name="T">Type of the <paramref name="value"/>.</typeparam>
	/// <param name="tag">The tag to add to.</param>
	/// <param name="key">The key.</param>
	/// <param name="value">The value to add.</param>
	public static void AddIfNotDefault<T>(this TagCompound tag,string key,T value) where T : struct
	{
		if (!EqualityComparer<T>.Default.Equals(value,default)) tag.Add(key,value);
	}

	/// <summary>
	/// Efficiently serializes the player's gender, body type, hair style, colors and hair dye.
	/// </summary>
	/// <param name="forSaving">Whether persistent IDs should be used.</param>
	public static ReadOnlySpan<byte> SerializePlayerAppearance(this Player player,bool forSaving=false)
	{
		serializationStream.SetLength(0);
		serializationStream.Position=0;

		void SaveShortID(int id,IdDictionary search)
		{
			if (forSaving)
			{
				var success=search.TryGetName(id,out var name);
				serStreamWriter.Write(success);
				if (success) serStreamWriter.Write(name);
				else serStreamWriter.Write((ushort)id);
			}
			else serStreamWriter.Write((ushort)id);
		}

		SaveShortID(player.skinVariant,PlayerVariantID.Search);
		SaveShortID(player.hair,HairID.Search);
		serStreamWriter.Write((short)player.hairDye);
		
		serStreamWriter.WriteRGB(player.hairColor);
		serStreamWriter.WriteRGB(player.skinColor);
		serStreamWriter.WriteRGB(player.eyeColor);
		serStreamWriter.WriteRGB(player.shirtColor);
		serStreamWriter.WriteRGB(player.underShirtColor);
		serStreamWriter.WriteRGB(player.pantsColor);
		serStreamWriter.WriteRGB(player.shoeColor);
		
		CollectionsMarshal.SetCount(byteBuffer,(int)serializationStream.Length);
		var span=CollectionsMarshal.AsSpan(byteBuffer);
		serializationStream.Position=0;
		serializationStream.ReadExactly(span);
		return span;
	}
	/// <summary>
	/// Deserializes the result of <see cref="SerializePlayerAppearance"/> onto the player.
	/// </summary>
	/// <param name="fromSaving">Whether persistent IDs were used.</param>
	public static void DeserializePlayerAppearance(this Player player,ReadOnlySpan<byte> bytes,bool fromSaving=false)
	{
		serializationStream.SetLength(0);
		serializationStream.Position=0;
		serializationStream.Write(bytes);
		serializationStream.Position=0;

		int? LoadShortID(IdDictionary search,int? count=null)
		{
			int id;
			if (fromSaving&&serStreamReader.ReadBoolean())
			{
				if (search.TryGetId(serStreamReader.ReadString(),out id)) return id;
				else return null;
			}
			id=serStreamReader.ReadUInt16();
			if (id>=(count??search.Count)) return null;
			else return id;
		}

		player.skinVariant=LoadShortID(PlayerVariantID.Search,PlayerVariantID.Count-2)??Main.rand.Next(PlayerVariantID.Count-2);
		player.hair=LoadShortID(HairID.Search)??Main.rand.Next(HairLoader.Count);
		player.hairDye=serStreamReader.ReadInt16();
		
		player.hairColor=serStreamReader.ReadRGB();
		player.skinColor=serStreamReader.ReadRGB();
		player.eyeColor=serStreamReader.ReadRGB();
		player.shirtColor=serStreamReader.ReadRGB();
		player.underShirtColor=serStreamReader.ReadRGB();
		player.pantsColor=serStreamReader.ReadRGB();
		player.shoeColor=serStreamReader.ReadRGB();
	}
}