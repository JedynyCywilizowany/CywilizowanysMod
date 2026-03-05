using System;
using CywilizowanysMod.Common;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace CywilizowanysMod;

partial class CywilsSystem : ModSystem
{
	public override void SaveWorldData(TagCompound tag)
	{
		tag.AddIfNotDefault(nameof(DaysSinceStart),DaysSinceStart);
	}
	public override void LoadWorldData(TagCompound tag)
	{
		DaysSinceStart=tag.Get<int>(nameof(DaysSinceStart));
	}
	public override void ClearWorld()
	{
		itemCapProgress=0;
		forceItemStackTime=0;
		Array.Clear(itemDespawnTracker);
		
		DaysSinceStart=0;
		lastMoonPhase=-1;
	}
}