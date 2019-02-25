using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using TMPro;

namespace LunraGames.SubLight.Views
{
	public class RingTransitView : UniverseScaleView, IRingTransitView
	{
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null

#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null

		public float AnimationProgress
		{
			set
			{

			}
		}
	}

	public interface IRingTransitView : IUniverseScaleView
	{
		/// <summary>
		/// Sets the animation progress from 0 to 3.
		/// </summary>
		/// <value>The animation progress.</value>
		float AnimationProgress { set; }
	}
}