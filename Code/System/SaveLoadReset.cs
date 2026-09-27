using System;
using ColonyLib;
using Terraria.ModLoader.IO;

namespace CywilizowanysMod;

partial class CywilsSystem
{
	public override void ClearWorld()
	{
		itemCapProgress=0;
		Array.Clear(worldItemUpdateDataArr);
		
		DaysSinceStart=0;
		lastMoonPhase=-1;
	}
	public override void SaveWorldData(TagCompound tag)
	{
		tag.AddIfNotDefault(nameof(DaysSinceStart),DaysSinceStart);
	}
	public override void LoadWorldData(TagCompound tag)
	{
		DaysSinceStart=tag.Get<int>(nameof(DaysSinceStart));
	}
}