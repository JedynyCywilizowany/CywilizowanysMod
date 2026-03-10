using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;

namespace CywilizowanysMod.Common;

partial class CywilsUtils
{
	/// <summary>
	/// Sets all static fields in all types from the given mod's assembly to null or default.<br/>
	/// Intended to be used with <c>this</c> at the end of <see cref="Mod.Unload"/>.<br/>
	/// Does it help in any way? I don't know ¯\_(ツ)_/¯
	/// </summary>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public static void AutoUnload(this Mod mod)
	{
		var start=DateTime.UtcNow;
		int unloadedFields=0;
		int unloadedTypes=0;
		StringBuilder logged=new($"-----( {nameof(AutoUnload)} for {mod.Name}... )-----");
		Parallel.ForEach(AssemblyManager.GetLoadableTypes(mod.Code),(type)=>
		{
			if (!type.ContainsGenericParameters)
			{
				int unloadedFieldsInType=0;
				StringBuilder? subLogged=null;
				foreach (var field in type.GetFields(BindingFlags.Static|BindingFlags.DeclaredOnly|BindingFlags.Public|BindingFlags.NonPublic))
				{
					if (!field.IsLiteral&&!field.IsInitOnly)
					{
						field.SetValue(null,null);

						unloadedFieldsInType++;
						subLogged??=new($"\n<{type.Name}>");
						subLogged.AppendJoin("","\n    ",field.Name);
					}
				}
				if (subLogged is not null) lock (logged)
				{
					unloadedFields+=unloadedFieldsInType;
					unloadedTypes++;
					logged.Append(subLogged);
				}
			}
		});
		logged.Append($"\n-----( {nameof(AutoUnload)} for {mod.Name} unloaded {unloadedFields} fields from {unloadedTypes} types in {(int)(DateTime.UtcNow - start).TotalMilliseconds} ms )-----");
		mod.Logger.Debug(logged);
	}
}