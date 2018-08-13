using UnityEngine;

namespace LunraGames.SubLight.Views
{
	public interface IGridTransform : IView
	{
		Vector3 UnityPosition { set; get; }
		UniversePosition UniversePosition { set; get; }
	}
}