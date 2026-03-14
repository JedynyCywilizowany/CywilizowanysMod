using CywilizowanysMod.Common;
using CywilizowanysMod.Items.Placeable;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace CywilizowanysMod.Globals;

public partial class CywilsGlobItem : GlobalItem
{
	public override bool InstancePerEntity=>true;
	
	public bool isAutosellListDummy;
	public override void ModifyTooltips(Item item,List<TooltipLine> tooltips)
	{
		tooltips.UpdateTooltip(Mod,"Autosell",(Main.LocalPlayer.GetModPlayer<CywilsPlayer>().IsItemAutosold(item.type) ? $"[i:{ModContent.ItemType<Autoseller>()}]{Mod.GetLocalization(isAutosellListDummy ? "Tooltips.ItemAutosold.List" : "Tooltips.ItemAutosold.Inventory")}" : ""),Color.Gold);
	}
}