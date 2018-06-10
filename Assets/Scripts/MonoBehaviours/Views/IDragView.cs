using UnityEngine;

namespace LunraGames.SpaceFarm.Views
{
	public interface IDragView : IView
	{
		Transform DragRoot { get; }
		Transform DragForward { get; }
		Transform DragAxisRoot { get; }
		Vector3 DragAxis { get; set; }
	}
}