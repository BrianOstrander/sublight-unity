using UnityEngine;

namespace LunraGames.SubLight.Views
{
	public abstract class UniverseScaleView : View, IUniverseScaleView
	{
		[SerializeField]
		Transform scaleArea;
		[SerializeField]
		Transform positionArea;

		public Vector3 Scale { set { if (scaleArea != null) scaleArea.localScale = value; } } 
		public Vector3 Position { set { if (positionArea != null) positionArea.position = value; } }
	}

	public interface IUniverseScaleView : IView
	{
		Vector3 Scale { set; }
		Vector3 Position { set; }
	}
}