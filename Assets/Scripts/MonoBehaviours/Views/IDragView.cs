using UnityEngine;

namespace LunraGames.SpaceFarm.Views
{
	public interface IDragView : IView
	{
		Transform DragRoot { get; }
		Transform DragForward { get; }
	}
}