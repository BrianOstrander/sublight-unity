using UnityEngine;

namespace LunraGames.SubLight
{
	/// <summary>
	/// Name of each layer.
	/// </summary>
	public static class LayerConstants
	{
		public const string Void = "Void";
		public const string HoloRoom = "HoloRoom";
		public const string HoloPriority = "HoloPriority";
		public const string HoloHome = "HoloHome";
		public const string HoloToolbar = "HoloToolbar";
		public const string HoloSystem = "HoloSystem";
		public const string HoloCommunications = "HoloCommunications";
		public const string HoloShip = "HoloShip";
		public const string HoloEncyclopedia = "HoloEncyclopedia";

		public static string Get(SetFocusLayers layer)
		{
			switch (layer)
			{
				case SetFocusLayers.Room: return HoloRoom;
				case SetFocusLayers.Priority: return HoloPriority;
				case SetFocusLayers.Home: return HoloHome;
				case SetFocusLayers.Toolbar: return HoloToolbar;
				case SetFocusLayers.System: return HoloSystem;
				case SetFocusLayers.Communications: return HoloCommunications;
				case SetFocusLayers.Ship: return HoloShip;
				case SetFocusLayers.Encyclopedia: return HoloEncyclopedia;
				default:
					Debug.LogError("Unrecognized layer: " + layer);
					return null;
			}
		}
	}
}