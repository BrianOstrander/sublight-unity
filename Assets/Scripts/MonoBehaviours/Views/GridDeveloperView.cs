using System;

using UnityEngine;

using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LunraGames.SubLight.Views
{
	public class GridDeveloperView : View, ISystemScratchView
	{
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
		[SerializeField]
		CanvasGroup group;
		[SerializeField]
		RectTransform canvas;
		[SerializeField]
		Transform canvasAnchor;

		[SerializeField]
		TextMeshProUGUI messageLabel;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null

		public string Message { set { messageLabel.text = value ?? string.Empty; } }

		protected override void OnOpacityStack(float opacity)
		{
			group.alpha = opacity;
			group.blocksRaycasts = Mathf.Approximately(0f, opacity);
		}

		public override void Reset()
		{
			base.Reset();

			canvas.SetParent(canvasAnchor, false);

			Message = string.Empty;
		}

		#region Events
		#endregion

		void OnDrawGizmos()
		{
#if UNITY_EDITOR
			Handles.color = Color.red;
			Handles.DrawWireDisc(transform.position, Vector3.up, 3.5f);
#endif
		}
	}

	public interface ISystemScratchView : IView
	{
		string Message { set; }
	}
}