using CywilizowanysMod.Config;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Chat;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CywilizowanysMod;

public partial class CywilsSystem : ModSystem
{
	public static int DaysSinceStart{get;private set;}
	private int lastMoonPhase;
	public static void TryShowDayCounter()
	{
		if (ModContent.GetInstance<CywilsConfig_World>().ShowDayCounter) ChatHelper.BroadcastChatMessage(NetworkText.FromKey(CywilizowanysMod.Instance.GetLocalizationKey("Messages.DayCounter"),(DaysSinceStart+1)),Color.Yellow);
	} 
	public override void PreUpdateWorld()
	{
		if (lastMoonPhase!=Main.moonPhase)
		{
			if (lastMoonPhase>=0)
			{
				DaysSinceStart++;
				TryShowDayCounter();
			}
			lastMoonPhase=Main.moonPhase;
		}
	}
}