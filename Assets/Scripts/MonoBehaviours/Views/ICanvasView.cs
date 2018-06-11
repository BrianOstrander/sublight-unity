using UnityEngine;

namespace LunraGames.SpaceFarm.Views
{
	public interface ICanvasView : IView
	{
		RectTransform CanvasTransform { get; }
	}
}