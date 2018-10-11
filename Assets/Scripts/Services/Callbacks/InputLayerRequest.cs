using System.Collections.Generic;

namespace LunraGames.SubLight
{
	public struct InputLayerRequest
	{
		public static InputLayerRequest Set(Dictionary<string, bool> layerDeltas)
		{
			return new InputLayerRequest(layerDeltas, null);
		}

		public static InputLayerRequest SetAll(bool value)
		{
			return new InputLayerRequest(null, value);
		}

		public readonly Dictionary<string, bool> LayerDeltas;
		public readonly bool? SetAllLayers;

		InputLayerRequest(Dictionary<string, bool> layerDeltas, bool? setAllLayers)
		{
			LayerDeltas = layerDeltas;
			SetAllLayers = setAllLayers;
		}
	}
}