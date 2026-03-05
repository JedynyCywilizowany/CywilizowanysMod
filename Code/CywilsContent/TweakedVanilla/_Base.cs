using Terraria.ModLoader;

namespace CywilizowanysMod.ContentBases;

public interface ICywilsContent
{
	public string AssetCategory{get;}
}
public static class CywilsContentUtils
{
	private static string AssetPath<T>(T type,string assetType,string name) where T : ModType,ICywilsContent
	{
		return $"{type.Mod.Name}/Assets/{assetType}/{type.AssetCategory}/{name}";
	}
	public static string TexturePath<T>(this T type,string name) where T : ModType,ICywilsContent
	{
		return AssetPath(type,"Textures",name);
	}
	public static string OwnTexturePath<T>(this T type,string name) where T : ModType,ICywilsContent
	{
		return TexturePath(type,$"{type.Name}/{name}");
	}
	public static string DefaultTexturePath<T>(this T type) where T : ModType,ICywilsContent
	{
		return TexturePath(type,type.Name);
	}
	public static readonly string NoTexture="CywilizowanysMod/Assets/Textures/None";
}