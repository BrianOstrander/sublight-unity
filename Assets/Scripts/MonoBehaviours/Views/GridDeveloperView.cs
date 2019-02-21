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
		public Action<string> ClickLink { set; private get; }

		protected override void OnOpacityStack(float opacity)
		{
			group.alpha = opacity;
			group.blocksRaycasts = !Mathf.Approximately(0f, opacity);
		}

		public override void Reset()
		{
			base.Reset();

			canvas.SetParent(canvasAnchor, false);

			Message = string.Empty;
			ClickLink = ActionExtensions.GetEmpty<string>();
		}

		void GetSelectedLink(Action<string> selection)
		{
			if (selection == null) throw new ArgumentNullException("selection");
			if (App.V.Camera == null)
			{
				Debug.LogError("Unable to process click while camera is null");
				return;
			}

			var linkIndex = TMP_TextUtilities.FindIntersectingLink(
				messageLabel,
				Input.mousePosition, // TODO: Should this be here???
				App.V.Camera
			);

			if (-1 < linkIndex) selection(messageLabel.textInfo.linkInfo[linkIndex].GetLinkID());
		}

		#region Events
		public void OnClickMessage()
		{
			if (ClickLink != null) GetSelectedLink(ClickLink);
		}
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
		Action<string> ClickLink { set; }
	}
}