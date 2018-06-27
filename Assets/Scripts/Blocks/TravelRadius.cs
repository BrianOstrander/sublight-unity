namespace LunraGames.SpaceFarm
{
	/// <summary>
	/// Travel radius, expressed in universe units.
	/// </summary>
	public struct TravelRadius
	{
		public static TravelRadius Zero { get { return new TravelRadius(0f, 0f, 0f); } }

		public readonly float SafeRadius;
		public readonly float DangerRadius;
		public readonly float MaximumRadius;

		public TravelRadius(float safeRadius, float dangerRadius, float maximumRadius)
		{
			SafeRadius = safeRadius;
			DangerRadius = dangerRadius;
			MaximumRadius = maximumRadius;
		}

	}
}