using System;

using UnityEngine;

namespace LunraGames.SubLight
{
	public struct DeliverFocusBlock
	{
		public readonly SetFocusLayers Layer;
		public readonly RenderTexture Texture;
		public readonly Action<DeliverFocusBlock> Done;

		public DeliverFocusBlock(SetFocusLayers layer, RenderTexture texture) : this(layer, texture, null) {}
		public DeliverFocusBlock(SetFocusLayers layer, Action<DeliverFocusBlock> done) : this(layer, null, done) {}

		DeliverFocusBlock(SetFocusLayers layer, RenderTexture texture, Action<DeliverFocusBlock> done)
		{
			Layer = layer;
			Texture = texture;
			Done = done;
		}

		public DeliverFocusBlock Duplicate(RenderTexture texture)
		{
			return new DeliverFocusBlock(
				Layer,
				texture,
				Done
			);
		}
	}
}