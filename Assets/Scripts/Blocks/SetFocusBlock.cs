using System;

using UnityEngine;

namespace LunraGames.SubLight
{
	public struct SetFocusBlock
	{
		public static SetFocusBlock Default<T>()
			where T : SetFocusDetails<T>, new()
		{
			return new SetFocusBlock(new T().SetDefault());
		}

		public readonly SetFocusDetailsBase Details;
		public readonly bool Enabled;
		public readonly int Order;
		public readonly float Weight;

		public SetFocusLayers Layer { get { return Details == null ? SetFocusLayers.Unknown : Details.Layer; } }

		public SetFocusBlock(
			SetFocusDetailsBase details,
			bool enabled = false,
			int order = 0,
			float weight = 0f
		)
		{
			if (details == null) throw new ArgumentNullException("details");

			Details = details;
			Enabled = enabled;
			Order = order;
			Weight = weight;
		}

		public bool HasDelta(SetFocusBlock other)
		{
			if (Layer != other.Layer)
			{
				Debug.LogError("Cannot get delta between Layers " + Layer + " and " + other.Layer);
				return false;
			}

			return Enabled != other.Enabled
								  || Order != other.Order
								  || !Mathf.Approximately(Weight, other.Weight)
								  || Details.HasDelta(other.Details);
		}
	}
}