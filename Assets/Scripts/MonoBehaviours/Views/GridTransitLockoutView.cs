using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using TMPro;

namespace LunraGames.SubLight.Views
{
	public class GridTransitLockoutView : View, IGridTransitLockoutView
	{
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null

#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null

		public override void Reset()
		{
			base.Reset();

		}

		#region Events

		#endregion
	}

	public interface IGridTransitLockoutView : IView
	{

	}
}