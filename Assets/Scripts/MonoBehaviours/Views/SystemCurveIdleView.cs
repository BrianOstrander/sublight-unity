using UnityEngine;

using CurvedUI;

namespace LunraGames.SubLight.Views
{
	public class SystemCurveIdleView : View, ISystemCurveIdleView
	{
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
		[SerializeField]
		RectTransform canvasAnchor;
		[SerializeField]
		CurvedUIRaycaster raycaster;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null


		public override void Reset()
		{
			base.Reset();

			InputHack();
		}

		public void InputHack()
		{
			// Idk but this fixes weird stuff.
			raycaster.enabled = false;
			raycaster.enabled = true;
		}
	}

	public interface ISystemCurveIdleView : IView
	{
		void InputHack();
	}
}