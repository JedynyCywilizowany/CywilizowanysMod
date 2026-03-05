using System;
using System.IO;
using CywilizowanysMod.Common;
using MonoMod.Cil;
using Terraria;
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
				var jumpToMovedLabel=c.DefineLabel();
				c.EmitBr(jumpToMovedLabel);
				var backToBeginningLabel=c.MarkLabel();

				ILCursor c2=new(c);
				c2.GotoNext((ins)=>ins.MatchLdfld(typeof(Entity).GetField(nameof(Entity.active))!));
				c2.GotoPrev((ins)=>
					ins.MatchLdcI4(0)&&
					ins.Next.MatchStloc(out _)
				);
				c2.Next!.Next!.MatchStloc(out int value2varIndex);
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
		/*
		for (int i=0;i<il.Instrs.Count;i++) il.Instrs[i].Offset=i;
		MonoModHooks.DumpIL(Instance,c.Context);
		*/
	}
}