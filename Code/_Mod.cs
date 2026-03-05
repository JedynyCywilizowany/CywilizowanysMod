using System.IO;
using CywilizowanysMod.Common;
using CywilizowanysMod.ContentBases;
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
	public override void Unload()
	{
		CywilsUtils.RevertArrayModifications();
		this.AutoUnload();
	}
	public override void HandlePacket(BinaryReader reader,int whoAmI)
	{
		CywilsPacketType.Receive(reader,whoAmI);
	}
}