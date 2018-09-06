using UnityEngine;

namespace LunraGames.SubLight
{
	public class FollowTagAnimation : ViewAnimation
	{
		[SerializeField]
		string tag;

		public override void OnPrepare(IView view)
		{
			var tagObject = GameObjectExtensions.FindWithTagOrHandleMissingTag(tag, true);
			view.Root.position = tagObject == null ? Vector3.zero : tagObject.transform.position;
		}
	}
}