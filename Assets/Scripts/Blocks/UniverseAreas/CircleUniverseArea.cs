using Newtonsoft.Json;

using UnityEngine;

namespace LunraGames.SubLight
{
	public class CircleUniverseArea : UniverseArea
	{
		public override Types AreaType { get { return Types.Circle; } }

		[JsonProperty] float radius; // In universe units

		[JsonIgnore]
		public float Radius
		{
			set
			{
				radius = Mathf.Max(0f, value);
				MinimumDelta = new UniversePosition(new Vector3(-radius, 0f, -radius));
				MaximumDelta = new UniversePosition(new Vector3(radius, 0f, radius));
				CenterDelta = UniversePosition.Zero;
			}
			get { return radius; }
		}

		#region Events
		protected override bool OnContains(UniversePosition position)
		{
			return UniversePosition.Distance(Origin, position) <= Radius;
		}
		#endregion
	}
}