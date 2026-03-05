using System;
using System.Collections.Generic;
using CywilizowanysMod.Common;
using CywilizowanysMod.Config;
using CywilizowanysMod.ContentBases;
using CywilizowanysMod.Items.Placeable;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Default;

namespace CywilizowanysMod.Items;

public class AutosellerBag : CywilsItem
{
	public static readonly int capacity=100*Item.gold;
	internal const int emptyingTime=60*10;
	internal const int emptyingDelay=20;
	public override void SetDefaults()
	{
		Item.rare=ItemRarityID.Orange;
	}
	public override void UpdateInventory(Player player)
	{
		if (player.whoAmI==Main.myPlayer)
		{
			var modPlayer=player.GetModPlayer<CywilsPlayer>();
		
			if (modPlayer.autosellingEnabled&&(modPlayer.AutosellerBagFill<capacity||ModContent.GetInstance<CywilsConfig_Client>().AutosellWhenBagIsFull)) modPlayer.autosellerBagAvailable=true;

			if (modPlayer.AutosellerBagFill!=0&&modPlayer.AutosellerAvailable)
			{
				if (Main.rand.NextBool(emptyingDelay))
				{
					var toDump=Math.Min(modPlayer.AutosellerBagFill,CywilsUtils.AveragedDivide(capacity,emptyingTime/emptyingDelay));
					if (player.whoAmI==Main.myPlayer)
					{
						var toGain=(toDump/modPlayer.AutosellingPriceMultiplier).AveragedInt();
						player.GiveMoney(toGain);
						modPlayer.AutosellTransferAnimation(ModContent.ItemType<StartBag>(),toGain);
					}
					modPlayer.AutosellerBagFill-=toDump;
				}
			}
		}
	}
	public override void ModifyTooltips(List<TooltipLine> tooltips)
	{
		var modPlayer=Main.LocalPlayer.GetModPlayer<CywilsPlayer>();
		
		tooltips.UpdateTooltip(Mod,"AutosellerBagIsEnabled",Mod.GetLocalization(modPlayer.autosellingEnabled ? "Tooltips.AutosellingEnabled.True" : "Tooltips.AutosellingEnabled.False"));

		int fill=modPlayer.AutosellerBagFill;
		tooltips.UpdateTooltip(Mod,"AutosellerBagFill",$"{CywilsUtils.ValueToCoinsCompact(fill)} ({((float)fill/capacity).ToPercentage()}%)",(fill>=capacity ? Color.Red : null));
	}

	public override bool CanRightClick()
	{
		return true;
	}
	public override bool ConsumeItem(Player player)
	{
		return false;
	}
	public override void RightClick(Player player)
	{
		var modPlayer=player.GetModPlayer<CywilsPlayer>();
		modPlayer.autosellingEnabled=!modPlayer.autosellingEnabled;
	}

	public override void AddRecipes()
	{
		CreateRecipe()
		.AddIngredient(ItemID.VoidLens)
		.AddIngredient(ModContent.ItemType<Autoseller>())
		.AddTile(TileID.Anvils)
		.Register();
	}
}