using System;
using System.Reflection;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;

namespace CywilizowanysMod;

partial class CywilizowanysMod
{
	internal static void OptimizeTextureLoad()
	{
		static void LoadMethodModificationCommon<T>(ILContext il,bool asyncAllowed,Action<ILCursor> uniquePart) where T : ModType
		{
			new ILCursor(il).FindNext(out var cursors,(ins)=>ins.MatchLdsfld(typeof(Main).GetField(nameof(Main.Assets))!));
			foreach (var c in cursors)
			{
				var isModdedLabel=c.DefineLabel();
				var elseVanillaLabel=c.DefineLabel();
				var postVanillaLabel=c.DefineLabel();
				
				c.Remove();

				uniquePart(c);
				c.EmitDup();

				c.EmitBrtrue(isModdedLabel);

				c.EmitPop();
				c.EmitBr(elseVanillaLabel);

				c.MarkLabel(isModdedLabel);
				c.EmitCallvirt(typeof(T).GetProperty("Texture")!.GetMethod!);
				c.EmitBr(postVanillaLabel);

				c.MarkLabel(elseVanillaLabel);
				c.EmitLdstr("Terraria/");
				
				c.GotoNext(MoveType.Before,(ins)=>ins.MatchCallvirt(typeof(IAssetRepository).GetMethod(nameof(IAssetRepository.Request),BindingFlags.Instance|BindingFlags.NonPublic,[typeof(string)])!.MakeGenericMethod(typeof(Texture2D))));
				c.Remove();
				c.EmitCall(typeof(string).GetMethod(nameof(string.Concat),[typeof(string),typeof(string)])!);
				c.MarkLabel(postVanillaLabel);
				
				//
				//c.EmitDup();//
				//c.EmitCall(typeof(Console).GetMethod(nameof(Console.WriteLine),[typeof(string)])!);//
				//
				c.EmitLdcI4((int)(asyncAllowed ? AssetRequestMode.AsyncLoad : AssetRequestMode.ImmediateLoad));
				c.EmitCall(typeof(ModContent).GetMethod(nameof(ModContent.Request))!.MakeGenericMethod(typeof(Texture2D)));
			}

			//for (int i=0;i<il.Instrs.Count;i++) il.Instrs[i].Offset=i;
			//Console.WriteLine(il.ToString());
		}
		
		static void OptimizeType<T>(string loadMethodName,MethodInfo? autoLoadMethod,string getInstanceMethodName,bool asyncAllowed=true) where T : ModType
		{
			try
			{
				MonoModHooks.Modify(typeof(Main).GetMethod(loadMethodName)!,(il)=>
				{
					LoadMethodModificationCommon<T>(il,asyncAllowed,(c)=>
					{
						c.EmitLdarg1();
						c.EmitCall(typeof(ModContent).GetMethod(getInstanceMethodName)!);
					});
				});
				MonoModHooks.Modify(autoLoadMethod!,(il)=>
				{
					ILCursor c=new(il);
					c.GotoNext(MoveType.Before,(ins)=>ins.MatchCallvirt(typeof(T).GetProperty("Texture")!.GetMethod!));
					c.Index++;
					c.Remove();
					c.EmitLdcI4((int)AssetRequestMode.DoNotLoad);
				});
			}
			catch //(Exception e)
			{
				//Console.WriteLine("ni ma; "+e);
			}
		}

		OptimizeType<ModItem>(
			nameof(Main.LoadItem),
			typeof(ModItem).GetMethod(nameof(ModItem.AutoStaticDefaults)),
			nameof(ModContent.GetModItem)
		);
		OptimizeType<ModProjectile>(
			nameof(Main.LoadProjectile),
			typeof(ModProjectile).GetMethod(nameof(ModProjectile.AutoStaticDefaults)),
			nameof(ModContent.GetModProjectile)
		);
		OptimizeType<ModTile>(
			nameof(Main.LoadTiles),
			typeof(ModTile).GetMethod(nameof(ModTile.SetupContent)),
			nameof(ModContent.GetModTile),
			asyncAllowed:false
		);
		OptimizeType<ModWall>(
			nameof(Main.LoadWall),
			typeof(ModWall).GetMethod(nameof(ModWall.SetupContent)),
			nameof(ModContent.GetModWall),
			asyncAllowed:false
		);
		OptimizeType<ModNPC>(
			nameof(Main.LoadNPC),
			typeof(ModNPC).GetMethod(nameof(ModNPC.AutoStaticDefaults)),
			nameof(ModContent.GetModNPC),
			asyncAllowed:false
		);

		/*
		static void OptimizeEquipType(string loadMethodName,EquipType equipType)
		{
			try
			{
				var loadMethod=typeof(Main).GetMethod(loadMethodName);
				if (loadMethod is not null) MonoModHooks.Modify(loadMethod,(il)=>
				{
					LoadMethodModificationCommon(il,true,(c)=>
					{
						c.EmitLdcI4((int)equipType);

						c.EmitLdarg1();
						c.EmitCall(typeof(EquipLoader).GetMethod(nameof(EquipLoader.GetEquipTexture),[typeof(EquipType),typeof(int)])!);

						c.EmitDup();
						var isNullLabel=c.DefineLabel();
						c.EmitBrfalse(isNullLabel);

						c.EmitCall(typeof(EquipTexture).GetProperty(nameof(EquipTexture.Item))!.GetMethod!);
						c.MarkLabel(isNullLabel);
					});
				});
			}
			catch
			{
			}
		}

		OptimizeEquipType(
			nameof(Main.LoadArmorHead),
			EquipType.Head
		);
		OptimizeEquipType(
			nameof(Main.LoadArmorBody),
			EquipType.Body
		);
		OptimizeEquipType(
			nameof(Main.LoadArmorLegs),
			EquipType.Legs
		);
		OptimizeEquipType(
			nameof(Main.LoadWings),
			EquipType.Wings
		);
		foreach (var slot in Enum.GetValues<EquipType>())
		{
			OptimizeEquipType(
				"LoadAcc"+Enum.GetName(slot),
				slot
			);
		}try
		{
			foreach (var method in typeof(PlayerDrawSet).GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static))
			{
				if (method.GetMethodBody() is not null) MonoModHooks.Modify(method,(il)=>
				{
					foreach (var type in typeof(ArmorIDs).GetNestedTypes())
					{
						var countField=type.GetField("Count");
						if (countField is null) continue;

						var ta=typeof(TextureAssets);
						var correspondingArrField=ta.GetField(type.Name)
						??ta.GetField(type.Name+"s")
						??ta.GetField(type.Name.Replace("Hand","Hands"))
						??ta.GetField(type.Name[..(type.Name.Length-1)]);

						if (correspondingArrField is null) continue;

						if (new ILCursor(il).TryFindNext(out var cursors,(ins)=>ins.MatchLdsfld(countField)))
						{
							foreach (var c in cursors)
							{
								c.Remove();
								c.EmitLdsfld(correspondingArrField);
								c.EmitCall(typeof(Asset<Texture2D>[]).GetProperty("Length")!.GetMethod!);
							}
						}
					}
				});
			}
			
			static void PreventPreloading(ILContext il)
			{
				new ILCursor(il).FindNext(out var cursors,(ins)=>ins.MatchCall(typeof(ModContent).GetMethod(nameof(ModContent.Request),[typeof(string),typeof(AssetRequestMode)])!.MakeGenericMethod(typeof(Texture2D))));
				foreach (var c in cursors)
				{
					c.EmitPop();
					c.EmitLdcI4((int)AssetRequestMode.DoNotLoad);
				}
			}
			MonoModHooks.Modify(typeof(EquipLoader).GetMethod(nameof(EquipLoader.AddEquipTexture))!,PreventPreloading);
			MonoModHooks.Modify(typeof(EquipLoader).GetMethod("ResizeAndFillArrays",BindingFlags.Static|BindingFlags.NonPublic|BindingFlags.Public)!,PreventPreloading);
		}
		catch
		{
		}*/
	}
}