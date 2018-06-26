using UnityEngine;

namespace LunraGames.SpaceFarm.Views
{
	public interface IGridTransform : IView
	{
		Vector3 UnityPosition { set; get; }
		UniversePosition UniversePosition { set; get; }
	}
}