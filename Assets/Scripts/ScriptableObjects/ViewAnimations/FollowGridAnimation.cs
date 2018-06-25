using UnityEngine;
using LunraGames.SpaceFarm.Views;

namespace LunraGames.SpaceFarm
{
	public class FollowGridAnimation : ViewAnimation
	{
		void SetPosition(IView view)
		{
			if (!(view is IGridTransform)) return;
			var gridView = view as IGridTransform;
			gridView.Root.position = UniversePosition.ToUnity(gridView.UniversePosition);
		}

		public override void OnIdle(IView view) { SetPosition(view); }

	}
}