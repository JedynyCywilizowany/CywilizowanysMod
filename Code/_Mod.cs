using Terraria.ModLoader;

namespace CywilizowanysMod;

public partial class CywilizowanysMod : Mod
{
	public override string Name=>nameof(CywilizowanysMod);
	public static CywilizowanysMod Instance=>ModContent.GetInstance<CywilizowanysMod>();
	public override void Load()
	{
		LoadILEditsAndDetours();
		Keybinds.Setup(this);
	}
}