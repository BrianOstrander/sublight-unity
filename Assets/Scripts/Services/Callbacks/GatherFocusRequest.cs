using System;
using System.Linq;

using UnityEngine;

namespace LunraGames.SubLight
{
	public struct GatherFocusResult
	{
		public static GatherFocusResult Empty { get { return new GatherFocusResult(new DeliverFocusBlock[0]); } }

		public readonly DeliverFocusBlock[] DeliveredTextures;

		public GatherFocusResult(DeliverFocusBlock[] deliveredTextures)
		{
			DeliveredTextures = deliveredTextures;
		}
	}

	public struct GatherFocusRequest
	{
		public static GatherFocusRequest Request(Action<GatherFocusResult> done, DeliverFocusBlock[] textureRequests)
		{
			return new GatherFocusRequest(done, textureRequests);
		}

		public readonly Action<GatherFocusResult> Done;
		public readonly DeliverFocusBlock[] TextureRequests;
		public readonly bool NoRequests;

		GatherFocusRequest(Action<GatherFocusResult> done, DeliverFocusBlock[] textureRequests)
		{
			Done = done;
			TextureRequests = textureRequests;
			NoRequests = textureRequests.Length == 0;
		}

		public bool GetGather(SetFocusLayers layer, out DeliverFocusBlock result)
		{
			if (layer == SetFocusLayers.Unknown)
			{
				Debug.Log("Layer " + layer + " is not supported.");
				result = default(DeliverFocusBlock);
				return false;
			}
			result = TextureRequests.FirstOrDefault(t => t.Layer == layer);
			return !result.NotGathered;
		}
	}
}