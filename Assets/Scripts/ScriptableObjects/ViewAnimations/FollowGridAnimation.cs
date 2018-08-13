using LunraGames.SubLight.Views;

namespace LunraGames.SubLight
{
	public class FollowGridAnimation : ViewAnimation
	{
		void SetPosition(IView view)
		{
			if (!(view is IGridTransform)) return;
			var gridView = view as IGridTransform;
			gridView.UnityPosition = UniversePosition.ToUnity(gridView.UniversePosition);
		}

		public override void OnLateIdle(IView view) { SetPosition(view); }
	}
}