using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework.Input;
using Terraria.ModLoader;

namespace CywilizowanysMod;

internal static class Keybinds
{
	public static ModKeybind autosellAddKey=null!;
	
	[MethodImpl(MethodImplOptions.NoInlining)]
	public static void Setup(CywilizowanysMod modInstance)
	{
		autosellAddKey=KeybindLoader.RegisterKeybind(modInstance,"ToggleAutosellForItem",Keys.U);
	}
}