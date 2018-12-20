﻿using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using TMPro;

namespace LunraGames.SubLight.Views
{
	public class GridTransitLockoutView : View, IGridTransitLockoutView
	{
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
		[SerializeField]
		float prepareDuration;
		[SerializeField]
		Vector2 transitDuration;
		[SerializeField]
		float finalizeDuration;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null

		public float PrepareDuration { get { return prepareDuration; } }
		public Vector2 TransitDuration { get { return transitDuration; } }
		public float FinalizeDuration { get { return finalizeDuration; } }

		public override void Reset()
		{
			base.Reset();

		}

		#region Events

		#endregion
	}

	public interface IGridTransitLockoutView : IView
	{
		float PrepareDuration { get; }
		Vector2 TransitDuration { get; }
		float FinalizeDuration { get; }
	}
}