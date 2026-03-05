using System;
using System.Buffers;
using System.IO;
using CywilizowanysMod.Config;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Chat;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;

namespace CywilizowanysMod.ContentBases;

/// <summary>
/// Designed to help with managing mod packets
/// </summary>
public abstract class CywilsPacketType : ModType
{
	private static CywilsPacketType[] all=[];
	internal static CywilsPacketType ByID(int id)
	{
		return all[id];
	}
	public ushort ID{get;private set;}
	internal static bool longerIDs;
	protected sealed override void Register()
	{
		ID=(ushort)all.Length;
		Array.Resize(ref all,ID+1);
		all[ID]=this;
		ModTypeLookup<CywilsPacketType>.Register(this);

		longerIDs=(ID>byte.MaxValue);
	}
	protected sealed override void ValidateType()
	{
		if (!(LoaderUtils.HasOverride(this,(t)=>t.HandleServer)||LoaderUtils.HasOverride(this,(t)=>t.HandleClient)||LoaderUtils.HasOverride(this,(t)=>t.Handle))) throw new Exception($"{FullName} must override at least one of: {nameof(HandleServer)}, {nameof(HandleClient)}, {nameof(Handle)}");
	}

	public sealed override void SetupContent()
	{
		SetStaticDefaults();
	}
	
	/// <summary>
	/// Used to get a packet of this type.<br/>
	/// </summary>
	public CywilsPacket Get()
	{
		var p=new CywilsPacket(this);
		//if (AutoRedistributed) p.Write((byte)Main.myPlayer);
		OnCreated(p);
		return p;
	}
	/// <summary>
	/// Used to get a packet of the given type.<br/>
	/// </summary>
	public static CywilsPacket Get<T>() where T : CywilsPacketType
	{
		return ModContent.GetInstance<T>().Get();
	}
	private void Redistribute(BinaryReader reader,int sender,int length)
	{
		var startIndex=reader.BaseStream.Position;
		var p=new CywilsPacket(this)
		{
			origSender=(byte)sender
		};

		var buffer=ArrayPool<byte>.Shared.Rent(length);
		try
		{
			reader.Read(buffer,0,length);
			p.Write(buffer,0,length);
		}
		finally
		{
			ArrayPool<byte>.Shared.Return(buffer);
		}

		p.Send(ignoreClient:sender);
		reader.BaseStream.Position=startIndex;
	}
	/// <summary>
	/// Calls <see cref="Get"/> and immediately sends its result.<br/>
	/// Meant to be used with packets that need no further data (ex. informing of an event having taken place) or already set all thier data in <see cref="OnCreated"/>.
	/// </summary>
	public static void SendNew<T>(int toClient=-1,int ignoreClient=-1) where T : CywilsPacketType
	{
		Get<T>().Send(toClient,ignoreClient);
	}
	
	internal static void Receive(BinaryReader reader,int whoAmI)
	{
		var packetType=ByID(longerIDs ? reader.ReadUInt16() : reader.ReadByte());
		if (Main.dedServ)
		{
			if (packetType.AutoRedistributed)
			{
				int length=reader.ReadByte();
				if (length==byte.MaxValue) length=reader.ReadUInt16();

				packetType.Redistribute(reader,whoAmI,length);
			}
			packetType.HandleServer(reader,whoAmI);
		}
		else
		{
			int origSender=(packetType.AutoRedistributed ? reader.ReadByte() : byte.MaxValue);
			packetType.HandleClient(reader,origSender);
		}
	}

	/// <summary>
	/// Called whenever a packet of this typeID is created, before it's returned.<br/>
	/// Could be used to instantly fill its data for use with <see cref="SendNew"/>.
	/// </summary>
	public virtual void OnCreated(BinaryWriter writer)
	{
	}
	/// <summary>
	/// If true, whenever the server receives a packet of this type it will automatically send copies to other clients.
	/// </summary>
	public virtual bool AutoRedistributed=>false;
	/// <summary>
	/// Called when the server receives this packet.<br/>
	/// See <see cref="Mod.HandlePacket"/> for more information.<br/>
	/// <br/>
	/// <b>NOTE:</b> a few leading bytes representing the packet's internal data have already been read.
	/// </summary>
	/// <param name="whoAmI">Index of the client that sent this packet.</param>
	public virtual void HandleServer(BinaryReader reader,int whoAmI)
	{
		Handle(reader,whoAmI);
	}
	/// <summary>
	/// Called when the client receives this packet.<br/>
	/// See <see cref="Mod.HandlePacket"/> for more information.<br/>
	/// <br/>
	/// <b>NOTE:</b> a few leading bytes representing the packet's internal data have already been read.
	/// </summary>
	/// <param name="whoAmI">If <see cref="AutoRedistributed"/> is true and the packet is redistributed, this is the index of the client that originally sent the packet.<br/>Otherwise it 255 (the server's index)</param>
	public virtual void HandleClient(BinaryReader reader,int whoAmI)
	{
		Handle(reader,whoAmI);
	}
	/// <summary>
	/// Called by default by <see cref="HandleServer"/> and <see cref="HandleClient"/>, making it useful if both do the same thing.
	/// </summary>
	public virtual void Handle(BinaryReader reader,int whoAmI)
	{
		if (Main.dedServ&&AutoRedistributed) return;
		throw new NotImplementedException($"{FullName} does not support handling on the {(Main.dedServ ? "server" : "client")}.");
	}
}
public class CywilsPacket : BinaryWriter
{
	private readonly ushort typeID;
	internal byte origSender=byte.MaxValue;
	private ModPacket? underlyingPacket;
	internal CywilsPacket(CywilsPacketType packetType) : base(new MemoryStream(16))
	{
		typeID=packetType.ID;
	}
	/// <summary>
	/// Similar to <see cref="ModPacket.Send"/>
	/// </summary>
	public void Send(int toClient=-1,int ignoreClient=-1)
	{
		if (underlyingPacket is null)
		{
			underlyingPacket=CywilizowanysMod.Instance.GetPacket();

			var type=CywilsPacketType.ByID(typeID);

			if (CywilsPacketType.longerIDs) underlyingPacket.Write(typeID);
			else underlyingPacket.Write((byte)typeID);

			if (type.AutoRedistributed)
			{
				if (Main.dedServ) underlyingPacket.Write(origSender);
				else
				{
					if (OutStream.Length<byte.MaxValue) underlyingPacket.Write((byte)OutStream.Length);
					else
					{
						underlyingPacket.Write(byte.MaxValue);
						underlyingPacket.Write((ushort)OutStream.Length);
					}
				}
			}

			if (!Main.gameMenu&&ModContent.GetInstance<CywilsConfig_Debug>().ReportPackets)
			{
				static string NameId(int id)
				{
					if (id<0||id>byte.MaxValue) return "(Error)";
					return $"({id}: {(id==byte.MaxValue ? "Server" : Main.player[id].name)})";
				}
				var message=$"{NameId(Main.myPlayer)} sent {type.FullName} of size: {OutStream.Length}{(Main.dedServ ? (toClient>=0 ? ", to: "+NameId(toClient) : (ignoreClient>=0 ? ", ignoring: "+NameId(ignoreClient) : "")) : "")}";
				if (Main.dedServ) ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(message),Color.Cyan);
				else ChatHelper.SendChatMessageFromClient(new ChatMessage($"[c/{Color.Cyan.Hex3()}:{message}]"));
			}
			
			OutStream.Position=0;
			OutStream.CopyTo(underlyingPacket.BaseStream);

			Close();
		}
		underlyingPacket.Send(toClient,ignoreClient);
	}
}