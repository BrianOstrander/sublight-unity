using System;

namespace LunraGames.SubLight
{
	public struct GatherFocusResult
	{
		public readonly DeliverFocusBlock[] DeliveredTextures;
	}

	public struct GatherFocusRequest
	{
		public static GatherFocusRequest Request(Action<GatherFocusResult> done, DeliverFocusBlock[] textureRequests)
		{
			return new GatherFocusRequest(done, textureRequests);
		}

		public readonly Action<GatherFocusResult> Done;
		public readonly DeliverFocusBlock[] TextureRequests;

		GatherFocusRequest(Action<GatherFocusResult> done, DeliverFocusBlock[] textureRequests)
		{
			Done = done;
			TextureRequests = textureRequests;
		}
	}
}