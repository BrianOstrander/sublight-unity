using UnityEngine;

namespace LunraGames.SpaceFarm.Views
{
	public interface IGridTransform : IView
	{
		UniversePosition UniversePosition { set; get; }
	}
}