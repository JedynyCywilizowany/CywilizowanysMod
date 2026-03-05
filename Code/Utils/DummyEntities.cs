using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using Terraria;
using Terraria.ID;

namespace CywilizowanysMod.Common;

partial class CywilsUtils
{
	public static ImmutableArray<T> CreateDummyEntityList<T>(Dictionary<int,T> lookup) where T : Entity
	{
		List<T> list=new(lookup.Count);
		foreach (var entry in lookup) if (entry.Key>=0)
		{
			if (entry.Key>=list.Count) CollectionsMarshal.SetCount(list,entry.Key+1);
			list[entry.Key]=entry.Value;
		}
		return list.ToImmutableArray();
	}
	public static ImmutableArray<Item> DummyItems{get;private set;}
	public static ImmutableArray<NPC> DummyNPCs{get;private set;}
	internal static void SetupDummyEntities()
	{
		DummyItems=CreateDummyEntityList(ContentSamples.ItemsByType);
		DummyNPCs=CreateDummyEntityList(ContentSamples.NpcsByNetId);
	}
}