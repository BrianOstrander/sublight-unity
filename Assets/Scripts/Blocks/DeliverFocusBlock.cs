using System;

using UnityEngine;

namespace LunraGames.SubLight
{
	public struct DeliverFocusBlock
	{
		public readonly SetFocusLayers Layer;
		public readonly Texture Texture;
		public readonly Action<DeliverFocusBlock> Done;

		public DeliverFocusBlock(SetFocusLayers layer, Texture texture) : this(layer, texture, null) {}
		public DeliverFocusBlock(SetFocusLayers layer, Action<DeliverFocusBlock> done) : this(layer, null, done) {}

		DeliverFocusBlock(SetFocusLayers layer, Texture texture, Action<DeliverFocusBlock> done)
		{
			Layer = layer;
			Texture = texture;
			Done = done;
		}

		public DeliverFocusBlock Duplicate(Texture texture)
		{
			return new DeliverFocusBlock(
				Layer,
				texture,
				Done
			);
		}

		public bool NotGathered { get { return Layer == SetFocusLayers.Unknown; } }
	}
}