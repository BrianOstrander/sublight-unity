namespace LunraGames.SpaceFarm
{
	public struct TravelRadius
	{
		public static TravelRadius Zero { get { return new TravelRadius(0f, 0f, 0f); } }

		public TravelRadius(float safeRadius, float dangerRadius, float maximumRadius)
		{
			SafeRadius = safeRadius;
			DangerRadius = dangerRadius;
			MaximumRadius = maximumRadius;
		}

		public float SafeRadius;
		public float DangerRadius;
		public float MaximumRadius;
	}
}