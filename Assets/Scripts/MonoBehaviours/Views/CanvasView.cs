﻿using UnityEngine;

namespace LunraGames.SubLight.Views
{
	public abstract class CanvasView : View, ICanvasView
	{
		public RectTransform CanvasTransform { get { return transform as RectTransform; } }
	}

	public interface ICanvasView : IView
	{
		RectTransform CanvasTransform { get; }
	}
}