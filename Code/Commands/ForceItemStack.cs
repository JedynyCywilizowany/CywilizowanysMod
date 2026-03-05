using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CywilizowanysMod.Commands;

public class ForceItemStack : ModCommand
{
	public override CommandType Type=>CommandType.World;
	public override string Command=>"forceItemStack";
	//public override string Usage
	//	=>"/tileUpdateRate num\n num — non-negative floatpoint number. sets the multiplier."+
	//	"\n/tileUpdateRate get\n shows the current value.";
	public override string Description=>"Force item stacking";
	public override void Action(CommandCaller caller,string input,string[] args)
	{
		if (args.Length==0) args=["10"];

		if (!int.TryParse(args[0],out int time)) throw new UsageException(args[0]+" is not a correct integer value.");
		if (time<0f) throw new UsageException("Value cannot be negative.");
		
		CywilsSystem.forceItemStackTime=(uint)time*60;
		if (Main.dedServ) NetMessage.SendData(MessageID.WorldData);
	}
}