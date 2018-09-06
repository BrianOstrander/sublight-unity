using UnityEngine;

namespace LunraGames.SubLight
{
	public class FollowTagAnimation : ViewAnimation
	{
		[SerializeField]
		string tag;

		bool hasInitialized;
		Vector3? tagPosition;

		void SetPosition(IView view)
		{
			if (!tagPosition.HasValue) Debug.LogError("Unable to find a position for tag " + tag);
			view.Root.position = tagPosition.HasValue ? tagPosition.Value : Vector3.zero;
		}

		public override void OnPrepare(IView view)
		{
			if (!hasInitialized)
			{
				hasInitialized = true;
				
				var tagObject = GameObjectExtensions.FindWithTagOrHandleMissingTag(tag);
				if (tagObject != null) tagPosition = tagObject.transform.position;
			}

			SetPosition(view);

		}
	}
}