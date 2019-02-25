namespace LunraGames.SubLight
{
	public struct RenderLayerPropertyBlock
	{
		public int Order;
		public float Weight;

		public string WeightKey { get { return ShaderConstants.RoomProjectionShared.GetWeight(Order); } }

		public RenderLayerPropertyBlock(
			int order,
			float weight
		)
		{
			Order = order;
			Weight = weight;
		}
	}
}