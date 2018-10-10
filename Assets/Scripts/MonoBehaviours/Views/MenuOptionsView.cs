using UnityEngine;

namespace LunraGames.SubLight.Views
{
	public class MenuOptionsView : View, IMenuOptionsView
	{
		[SerializeField]
		Transform optionsAnchor;
		
		[SerializeField]
		AnimationCurve optionsRadius;
		[SerializeField]
		AnimationCurve optionsIconAlpha;
		[SerializeField]
		AnimationCurve optionsLabelAlpha;

		[SerializeField]
		float optionsTopDeadSpace;
		[SerializeField]
		float optionsSpacing;
	}

	public interface IMenuOptionsView : IView
	{

	}
}