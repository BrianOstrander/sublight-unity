using Newtonsoft.Json;

using UnityEngine;

namespace LunraGames.SubLight
{
	public class RectangleUniverseArea : UniverseArea
	{
		public override Types AreaType { get { return Types.Rectangle; } }

		[JsonProperty] float width; // In universe units
		[JsonProperty] float height; // In universe units

		[JsonIgnore]
		public float Width
		{
			set { width = value; OnWidthHeight(); }
			get { return width; }
		}

		[JsonIgnore]
		public float Height
		{
			set { height = value; OnWidthHeight(); }
			get { return height; }
		}

		#region Events
		void OnWidthHeight()
		{
			var halfWidth = width * 0.5f;
			var halfHeight = height * 0.5f;
			MinimumDelta = new UniversePosition(new Vector3(-halfWidth, 0f, -halfHeight));
			MaximumDelta = new UniversePosition(new Vector3(halfWidth, 0f, halfHeight));
			CenterDelta = UniversePosition.Zero;
		}
		#endregion
	}
}