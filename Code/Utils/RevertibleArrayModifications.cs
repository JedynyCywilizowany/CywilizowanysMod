using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CywilizowanysMod.Common;

partial class CywilsUtils
{
	private interface IRevertibleModifyEntry
	{
		public void Revert();
	}
	private class RevertibleModifyEntry<T>(T[] array) : IRevertibleModifyEntry
	{
		readonly T[] target=array;
		readonly Dictionary<int,T> originals=new();
		public void Revert()
		{
			foreach (var subEntry in originals)
			{
				target[subEntry.Key]=subEntry.Value;
			}
		}
		public void AddIfNotPresent(int index,T value)
		{
			originals.TryAdd(index,value);
		}
	}
	private static Dictionary<Array,IRevertibleModifyEntry> valuesToRevertTo=new(ReferenceEqualityComparer.Instance);

	/// <summary>
	/// Modifies an entry of an array, the change will be automatically reverted during mod unloading.
	/// </summary>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public static void RevertibleModify<T>(this T[] array,int index,T newValue)
	{
		RevertibleModifyEntry<T> entry;
		if (valuesToRevertTo.TryGetValue(array,out var entry2)) entry=(RevertibleModifyEntry<T>)entry2;
		else valuesToRevertTo[array]=(entry=new RevertibleModifyEntry<T>(array));
		
		ref T arrayEntry=ref array[index];
		if (!EqualityComparer<T>.Default.Equals(newValue,arrayEntry))
		{
			entry.AddIfNotPresent(index,arrayEntry);
			arrayEntry=newValue;
		}
	}
	[MethodImpl(MethodImplOptions.NoInlining)]
	internal static void RevertArrayModifications()
	{
		if (valuesToRevertTo is not null)
		{
			foreach (var entry in valuesToRevertTo.Values) entry.Revert();
			valuesToRevertTo=null!;
		}
	}
}