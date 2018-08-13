﻿using UnityEngine;

using LunraGames.SubLight.Views;

namespace LunraGames.SubLight
{
	public class ExpandAreaAnimation : ViewAnimation
	{
		public override void OnPrepare(IView view)
		{
			if (!(view is ICanvasView)) return;
			var canvasView = view as ICanvasView;
			canvasView.CanvasTransform.sizeDelta = Vector2.one;
			//canvasView.CanvasTransform.anchorMax = Vector2.zero;
			//canvasView.CanvasTransform.anchorMin = Vector2.zero;
		}
	}
}