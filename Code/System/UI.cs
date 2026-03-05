using System;
using System.Collections.Generic;
using System.Linq;
using CywilizowanysMod.Common;
using CywilizowanysMod.Globals;
using CywilizowanysMod.Items;
using CywilizowanysMod.Items.Placeable;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.ModLoader;
using Terraria.UI;

namespace CywilizowanysMod;

public class AutoSellerUI : UIState
{
	private static UIPanel toggleButton=null!;
	private static UIPanel list=null!;
	private static UIScrollbar scrollbar=null!;
	private static UIPanel dropAutosoldButton=null!;
	internal static bool showList=false;
	private static int listStartIndex;
	private const int slotSize=38;
	private const float halfSlotSize=slotSize/2f;
	private const int listCols=5;
	private static int listRows=1;
	
	public override void OnInitialize()
	{
		toggleButton=new();
		toggleButton.Width.Set(95,0);
		toggleButton.Height.Set(45,0);
		toggleButton.OnLeftClick+=(a,b)=>
		{
			showList=!showList;
		};
		Append(toggleButton);
		
		list=new();
		list.Width.Set((slotSize*listCols)+24f,0);
		list.PaddingLeft=0;
		Append(list);

		scrollbar=new();
		scrollbar.Left.Set((slotSize*listCols)+2f,0);
		scrollbar.Top.Set(0,0);
		scrollbar.Width.Set(20f,0);
		scrollbar.Height.Set(0,100f);
		scrollbar.MarginLeft=0;
		list.Append(scrollbar);
		
		dropAutosoldButton=new();
		dropAutosoldButton.Width.Set(45,0);
		dropAutosoldButton.Height.Set(45,0);
		dropAutosoldButton.OnLeftClick+=(a,b)=>
		{
			Main.LocalPlayer.GetModPlayer<CywilsPlayer>().DropAutosoldItems();
		};
		Append(dropAutosoldButton);
	}
	public override void Draw(SpriteBatch spriteBatch)
	{
		toggleButton.Left.Set((540-(56f*Main.inventoryScale))*Main.inventoryScale,0);
		toggleButton.Top.Set(Main.instance.invBottom,0);

		dropAutosoldButton.Top=toggleButton.Top;
		dropAutosoldButton.Left.Set(toggleButton.Left.Pixels+toggleButton.Width.Pixels,0);

		var player=Main.LocalPlayer;

		if (toggleButton.IsMouseHovering)
		{
			player.mouseInterface=true;
			Main.hoverItemName=CywilizowanysMod.Instance.GetLocalization("UI.AutosoldItemList").Value;
		}
		toggleButton.Draw(spriteBatch);
		ItemSlot.DrawItemIcon(CywilsUtils.DummyItems[ModContent.ItemType<Autoseller>()],ItemSlot.Context.ChatItem,spriteBatch,new(toggleButton.Left.Pixels+(toggleButton.Width.Pixels/4f),toggleButton.Top.Pixels+(toggleButton.Height.Pixels/2f)),1f,32f,Color.White);
		
		if (dropAutosoldButton.IsMouseHovering)
		{
			player.mouseInterface=true;
			Main.hoverItemName=CywilizowanysMod.Instance.GetLocalization("UI.DropAutosold").Value;
		}
		dropAutosoldButton.Draw(spriteBatch);
		ItemSlot.DrawItemIcon(CywilsUtils.DummyItems[ModContent.ItemType<AutosellerBag>()],ItemSlot.Context.ChatItem,spriteBatch,new(dropAutosoldButton.Left.Pixels+(dropAutosoldButton.Width.Pixels/2f),dropAutosoldButton.Top.Pixels+(dropAutosoldButton.Height.Pixels/2f)),1f,32f,Color.White);

		if (showList)
		{
			list.Left.Set(toggleButton.Left.Pixels-50f,0);
			list.Top.Set(toggleButton.Top.Pixels+toggleButton.Height.Pixels+4f,0);
			listRows=Math.Min(10,(Main.screenHeight-(int)list.Top.Pixels)/slotSize);
			list.Height.Set((slotSize*listRows)+4f,0);

			var autosoldItems=player.GetModPlayer<CywilsPlayer>().autosoldItems;
			scrollbar.SetView(1,Math.Max(0,(autosoldItems.Count-1)/listCols-(listRows-2)));
			scrollbar.ViewPosition=MathF.Round(scrollbar.ViewPosition);
			listStartIndex=(int)scrollbar.ViewPosition*listCols;
			if (list.IsMouseHovering)
			{
				player.mouseInterface=true;
				PlayerInput.LockVanillaMouseScroll("AutosellList");
			}
			list.Draw(spriteBatch);
			
			int mouseIndex=MouseOverEntryIndex();
			int endIndex=Math.Min(listCols*listRows,autosoldItems.Count-listStartIndex);
			for (int i=0;i<endIndex;i++)
			{
				float x=i%listCols*slotSize+2+list.Left.Pixels;
				float y=i/listCols*slotSize+2+list.Top.Pixels;
				Rectangle rect=new((int)x,(int)y,slotSize,slotSize);

				int index=i+listStartIndex;
				var item=CywilsUtils.DummyItems[autosoldItems.ElementAt(index)];

				spriteBatch.Draw(TextureAssets.InventoryBack.Value,rect,Color.White);
				ItemSlot.DrawItemIcon(item,ItemSlot.Context.CraftingMaterial,spriteBatch,new(x+halfSlotSize,y+halfSlotSize),1f,slotSize,Color.White);
				if (mouseIndex==index)
				{
					spriteBatch.Draw(TextureAssets.MapDeath.Value,rect,Color.White);

					ItemSlot.MouseHover(ref item,ItemSlot.Context.CraftingMaterial);
					Main.HoverItem.GetGlobalItem<CywilsGlobItem>().isAutosellListDummy=true;
				}
			}
		}
	}
	public override void LeftClick(UIMouseEvent evt)
	{
		if (showList)
		{
			int index=MouseOverEntryIndex();
			var player=Main.LocalPlayer.GetModPlayer<CywilsPlayer>();
			if (index!=-1)
			{
				player.autosoldItems.Remove(player.autosoldItems.ElementAt(index));
				player.UpdateAutosell();
			}
		}
	}
	public override void ScrollWheel(UIScrollWheelEvent evt)
	{
		if (showList&&list.ContainsPoint(evt.MousePosition)) scrollbar.ViewPosition-=evt.ScrollWheelValue/100f;
	}
	public static int MouseOverEntryIndex()
	{
		if (showList)
		{
			float x=Main.mouseX-2.5f-list.Left.Pixels;
			float y=Main.mouseY-2.5f-list.Top.Pixels;
			if (x>4f&&y>4f&&x<(slotSize*listCols)-4f&&y<(slotSize*listRows)-4f)
			{
				int index=(int)(MathF.Floor(x/slotSize)+(MathF.Floor(y/slotSize)*listCols))+listStartIndex;
				if (index>=0&&index<Main.LocalPlayer.GetModPlayer<CywilsPlayer>().autosoldItems.Count) return index;
			}
		}
		return -1;
	}
}
partial class CywilsSystem : ModSystem
{
	internal UserInterface ui=null!;
	internal AutoSellerUI autoSellerUI=null!;
	private GameTime lastUpdateUiGameTime=null!;
	public override void UpdateUI(GameTime gameTime)
	{
		void SetVisible(bool state)
		{
			if ((ui.CurrentState!=null)!=state) ui.SetState(state ? autoSellerUI : null);
		}

		lastUpdateUiGameTime=gameTime;
		if (Main.playerInventory&&Main.LocalPlayer.chest==-1&&!Main.recBigList&&Main.npcShop<=0&&Main.LocalPlayer.GetModPlayer<CywilsPlayer>().AutosellingActive)
		{
			SetVisible(true);
			ui.Update(gameTime);
		}
		else
		{
			SetVisible(false);
			AutoSellerUI.showList=false;
		}
	}
	public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
	{
		int inventoryIndex=layers.FindIndex(layer=>layer.Name.Equals("Vanilla: Inventory"));
		if (inventoryIndex>=0)
		{
			layers.Insert(inventoryIndex+1,new LegacyGameInterfaceLayer(
				"CywilizowanysMod: AutosellerInterface",
				()=>
				{
					if (lastUpdateUiGameTime!=null&&ui?.CurrentState!=null) ui.Draw(Main.spriteBatch,lastUpdateUiGameTime);
					return true;
				},
				InterfaceScaleType.UI)
			);
		}
	}
}