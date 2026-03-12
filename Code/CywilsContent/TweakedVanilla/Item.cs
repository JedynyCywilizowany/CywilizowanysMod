using Terraria.ModLoader;
using Terraria.ID;
using CywilizowanysMod.Items.DevSets;

namespace CywilizowanysMod.ContentBases;

public abstract class CywilsItem : ModItem,ICywilsContent
{
	string ICywilsContent.AssetCategory=>"Items";
	public virtual bool ReplaceDefaultTexturePath=>true;
	public override string Texture=>(ReplaceDefaultTexturePath ? this.DefaultTexturePath() : base.Texture);

	/// <summary>
	/// If the item is body armor, return a <see cref="EquipType.Legs"/> slot to draw it over the legs, without replacing them like robes do.<br/>
	/// This effect is hard-coded into some vanilla vanity items (ex. the skirt from <see cref="ItemID.PrettyPinkDress"/>), but there is no official mod support for it.<br/>
	/// Used in <see cref="DevSet_Cywil_Chestpiece"/>.
	/// </summary>
	/// <returns></returns>
	public virtual int BodyArmorLegsOverlay(bool isMale)
	{
		return -1;
	}
}