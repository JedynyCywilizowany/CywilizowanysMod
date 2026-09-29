using ColonyLib;
using CywilizowanysMod.Items.Placeable;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace CywilizowanysMod.Globals;

public partial class CywilsGlobItem : GlobalItem
{
	public override void ModifyTooltips(Item item,List<TooltipLine> tooltips)
	{
		tooltips.UpdateTooltip(Mod,"Autosell",(Main.LocalPlayer.GetModPlayer<CywilsPlayer>().IsItemAutosold(item.type) ? $"[i:{ModContent.ItemType<Autoseller>()}]{Mod.GetLocalization("Tooltips.ItemAutosold")}" : ""),Color.Gold);
	}
}