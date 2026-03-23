using System;
using System.IO;
using CywilizowanysMod.Common;
using CywilizowanysMod.ContentBases;
using MonoMod.Cil;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace CywilizowanysMod;

partial class CywilizowanysMod : Mod
{
	private static void LoadILEditsAndDetours()
	{
		try
		{
			void NetMessage_SendData(ILContext il)
			{
				ILCursor c=new(il);
				c.GotoNext((ins)=>ins.MatchCall(typeof(ItemIO).GetMethod(nameof(ItemIO.SendModData))!));
				c.GotoPrev(MoveType.After,
					(ins)=>ins.MatchLdsfld(typeof(Main).GetField(nameof(Main.item))!),
					(ins)=>ins.MatchLdarg(4),
					(ins)=>ins.MatchLdelemRef(),
					(ins)=>ins.MatchStloc(out _)
				);
				//The item
				c.Prev!.MatchStloc(out int item3varIndex);

				c.GotoNext(MoveType.After,(ins)=>ins.MatchCallvirt(typeof(BinaryWriter).GetMethod(nameof(BinaryWriter.Write),[typeof(short)])!));
				c.Clone().GotoPrev((ins)=>ins.MatchLdloc(out _)).Next!.MatchLdloc(out int writerVarIndex);
				
				var jumpToMovedLabel=c.DefineLabel();
				c.EmitBr(jumpToMovedLabel);
				var backToBeginningLabel=c.MarkLabel();

				ILCursor c2=new(c);
				c2.GotoNext((ins)=>ins.MatchLdfld(typeof(Entity).GetField(nameof(Entity.active))!));
				c2.GotoPrev((ins)=>
					ins.MatchLdcI4(0)&&
					ins.Next.MatchStloc(out _)
				);
				c2.Next!.Next.MatchStloc(out int value2varIndex);
				var skipMovedLabel=c2.DefineLabel();
				c2.EmitBr(skipMovedLabel);
				c2.MarkLabel(jumpToMovedLabel);

				c2.GotoNext(
					(ins)=>ins.MatchLdcI4(0),
					(ins)=>ins.MatchStloc(value2varIndex)
				);

				c2.GotoNext(MoveType.After,(ins)=>ins.MatchCallvirt(typeof(BinaryWriter).GetMethod(nameof(BinaryWriter.Write),[typeof(short)])!));
				c2.EmitLdloc(value2varIndex);
				c2.EmitLdcI4(0);
				c2.EmitCgt();
				var itemNotExistingLabel=c2.DefineLabel();
				c2.EmitBrfalse(itemNotExistingLabel);
				c2.EmitBr(backToBeginningLabel);
				c2.MarkLabel(skipMovedLabel);

				c2.EmitLdloc(writerVarIndex);
				c2.EmitLdloc(item3varIndex);
				c2.EmitLdfld(typeof(Item).GetField(nameof(Item.timeSinceItemSpawned))!);
				c2.EmitCall(typeof(BinaryWriter).GetMethod(nameof(BinaryWriter.Write7BitEncodedInt))!);

				c2.GotoNext(MoveType.After,(ins)=>ins.MatchCall(typeof(ItemIO).GetMethod(nameof(ItemIO.SendModData))!));
				c2.MarkLabel(itemNotExistingLabel);
			}
			IL_NetMessage.SendData+=NetMessage_SendData;
			try
			{
				IL_MessageBuffer.GetData+=(il)=>
				{
					ILCursor c=new(il);

					c.GotoNext((ins)=>ins.MatchCall(typeof(ItemIO).GetMethod(nameof(ItemIO.ReceiveModData))!));
					c.GotoPrev((ins)=>ins.MatchRet()&&ins.Previous.MatchPop());
					c.GotoNext((ins)=>
					ins.MatchLdsfld(
						typeof(Main).GetField(nameof(Main.item))!)&&
						ins.Next.MatchLdloc(out _)&&
						ins.Next.Next.MatchLdelemRef()
					);
					//Item index
					c.Next!.Next!.MatchLdloc(out int num105varIndex);

					c.GotoPrev(MoveType.After,(ins)=>ins.MatchStloc(num105varIndex));
					var jumpToMovedLabel=c.DefineLabel();
					c.EmitBr(jumpToMovedLabel);
					var backToBeginningLabel=c.MarkLabel();

					c.GotoNext((ins)=>ins.MatchCallvirt(typeof(BinaryReader).GetMethod(nameof(BinaryReader.ReadInt16))!));
					c.Clone().GotoPrev((ins)=>ins.MatchLdfld(out _)).Next!.MatchLdloc(out int readerVarIndex);
					//Item type
					c.Next!.Next!.MatchStloc(out int num107varIndex);
					c.Index-=2;
					var skipMovedLabel=c.DefineLabel();
					c.EmitBr(skipMovedLabel);
					c.MarkLabel(jumpToMovedLabel);
					c.Index+=4;

					//Czy przedmiot istnieje
					c.EmitLdloc(num107varIndex);
					c.EmitBrtrue(backToBeginningLabel);

					//Jeżeli przedmiot nie istnieje
					c.EmitLdloc(num105varIndex);
					static void Insertion(short index)
					{
						Main.item[index].active=false;
						if (Main.dedServ) NetMessage.SendData(MessageID.SyncItem,number:index);
					}
					c.EmitCallFromDelegate(Insertion);
					var returnLabel=c.DefineLabel();
					c.EmitBr(returnLabel);

					c.MarkLabel(skipMovedLabel);

					c.EmitLdsfld(typeof(Main).GetField(nameof(Main.item))!);
					c.EmitLdloc(num105varIndex);
					c.EmitLdelemRef();
					c.EmitLdarg0();
					c.EmitLdfld(typeof(MessageBuffer).GetField(nameof(MessageBuffer.reader))!);
					c.EmitCall(typeof(BinaryReader).GetMethod(nameof(BinaryReader.Read7BitEncodedInt))!);
					c.EmitStfld(typeof(Item).GetField(nameof(Item.timeSinceItemSpawned))!);

					c.GotoNext((ins)=>ins.MatchRet());
					c.MarkLabel(returnLabel);
				};
			}
			catch
			{
				IL_NetMessage.SendData-=NetMessage_SendData;
				throw;
			}
		}
		catch (Exception e)
		{
			Instance.Logger.Error(e.Message);
		}

		try
		{
			IL_PlayerDrawLayers.DrawPlayer_16_ArmorLongCoat+=(il)=>
			{
				ILCursor c=new(il);

				c.GotoNext(MoveType.After,
					(ins)=>ins.MatchLdfld(typeof(Player).GetField(nameof(Player.body))!),
					(ins)=>ins.MatchStloc(out _)
				);
				c.Prev!.MatchStloc(out var bodyInputVar);
				
				c.GotoNext(MoveType.Before,
					(ins)=>ins.MatchLdloc(out _),
					(ins)=>ins.MatchLdcI4(-1),
					(ins)=>ins.MatchBeq(out _)
				);
				c.Next!.MatchLdloc(out var legsOutputVar);
				c.GotoNext(MoveType.Before,(ins)=>ins.MatchBeq(out _));
				c.Next!.MatchBeq(out var endLabel);

				var normalLabel=c.DefineLabel();

				c.Remove();
				c.EmitCeq();
				c.EmitBrfalse(normalLabel);

				c.EmitLdloc(bodyInputVar);
				c.EmitLdcI4(ArmorIDs.Body.Count);
				c.EmitBlt(normalLabel);

				c.EmitLdarg0();
				c.EmitLdfld(typeof(PlayerDrawSet).GetField(nameof(PlayerDrawSet.drawPlayer))!);

				static int Insertion(Player drawPlayer)
				{
					var item=CywilsUtils.DummyItems[Item.bodyType[drawPlayer.body]];
					if (item.ModItem is CywilsItem cywilsItem)
					{
						return cywilsItem.BodyArmorLegsOverlay(drawPlayer.Male);
					}
					return -1;
				}
				c.EmitCallFromDelegate(Insertion);
				
				c.EmitStloc(legsOutputVar);
				c.MarkLabel(normalLabel);
				c.EmitLdloc(legsOutputVar);
				c.EmitLdcI4(0);
				c.EmitBlt(endLabel!);
			};
		}
		catch (Exception e)
		{
			Instance.Logger.Error(e.Message);
		}
		/*
		for (int i=0;i<il.Instrs.Count;i++) il.Instrs[i].Offset=i;
		MonoModHooks.DumpIL(Instance,c.Context);
		*/
	}
}