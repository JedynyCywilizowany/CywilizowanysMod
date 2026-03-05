using System.Runtime.CompilerServices;
using Terraria.ID;
using Terraria.ModLoader;

namespace CywilizowanysMod.Common;

[ReinitializeDuringResizeArrays]
public static class CywilsSets
{
	public static readonly bool[] npcCanBeAutosoldTo=NPCID.Sets.Factory.CreateBoolSet();

	[MethodImpl(MethodImplOptions.NoInlining)]
	internal static void SetupSets()
	{
		foreach (var type in CywilsUtils.CoinTypes) ItemID.Sets.IsLavaImmuneRegardlessOfRarity.RevertibleModify(type,true);

		foreach (var npc in CywilsUtils.DummyNPCs)
		{
			if (npc.isLikeATownNPC&&NPCShopDatabase.TryGetNPCShop(NPCShopDatabase.GetShopName(npc.type),out _))
			{
				npcCanBeAutosoldTo[npc.type]=true;
			}
		}
	}
}