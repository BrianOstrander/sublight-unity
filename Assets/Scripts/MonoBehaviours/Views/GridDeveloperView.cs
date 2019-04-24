using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

using TMPro;

using CurvedUI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LunraGames.SubLight.Views
{
	public struct DeveloperMessageEntry
	{
		public Action<string> Message;
	}

	public class GridDeveloperView : View, ISystemScratchView
	{
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
		[SerializeField]
		RectTransform messageArea;
		[SerializeField]
		CurvedUISettings canvas;

		[SerializeField]
		GridDeveloperMessageLeaf messagePrefab;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null

		List<GridDeveloperMessageLeaf> entries = new List<GridDeveloperMessageLeaf>();
		bool wasChange;

		public DeveloperMessageEntry CreateEntry(
			Action<string> clickLink,
			float offset = 0f,
			string message = null
		)
		{
			var instance = messageArea.gameObject.InstantiateChild(messagePrefab, setActive: true);
			entries.Add(instance);

			message = message ?? string.Empty;

			var result = new DeveloperMessageEntry
			{
				Message = value =>
				{
					instance.MessageLabel.text = value;
					wasChange = true;
				}
			};

			result.Message(message);

			instance.Button.OnClick.AddListener(new UnityAction(() => OnClickMessage(instance.MessageLabel, clickLink)));

			var areaHalf = messageArea.rect.width * 0.5f;

			instance.Anchor.anchoredPosition = new Vector2(areaHalf * offset, 0f);

			return result;
		}

		protected override void OnOpacityStack(float opacity)
		{
			foreach (var entry in entries)
			{
				entry.Group.alpha = opacity;
				entry.Group.blocksRaycasts = !Mathf.Approximately(0f, opacity);
			}
		}

		public override void Reset()
		{
			base.Reset();

			wasChange = false;

			messagePrefab.gameObject.SetActive(false);

			messageArea.transform.ClearChildren();
			entries.Clear();
		}

		#region Events
		void OnClickMessage(TextMeshProUGUI label, Action<string> clickLink)
		{
			if (clickLink == null)
			{
				Debug.LogError("A null click link was given...");
				return;
			}

			if (App.V.Camera == null)
			{
				Debug.LogError("Unable to process click while camera is null");
				return;
			}

			// Required when inside a curved canvas, otherwise Input.mousePosition would suffice.
			if (!canvas.RaycastToCanvasSpace(App.V.Camera.ScreenPointToRay(Input.mousePosition), out var positionOnCanvas)) return;

			var screenPosition = App.V.Camera.WorldToScreenPoint(
				canvas.GetComponent<RectTransform>().TransformPoint(
					positionOnCanvas
				)
			);

			var linkIndex = TMP_TextUtilities.FindIntersectingLink(
				label,
				screenPosition,
				App.V.Camera
			);

			if (-1 < linkIndex) clickLink(label.textInfo.linkInfo[linkIndex].GetLinkID());
		}
		#endregion

		protected override void OnIdle(float delta)
		{
			base.OnIdle(delta);

			// This is a hack to fix something about CurveUI not updating sub meshes properly...
			if (!wasChange || !Visible) return;

			wasChange = false;

			foreach (var entry in entries)
			{
				entry.MessageLabel.gameObject.SetActive(false);
				entry.MessageLabel.gameObject.SetActive(true);
			}
		}

		void OnDrawGizmos()
		{
#if UNITY_EDITOR
			//Handles.color = Color.red;
			//Handles.DrawWireDisc(transform.position, Vector3.up, 3.5f);
#endif
		}
	}

	public interface ISystemScratchView : IView
	{
		DeveloperMessageEntry CreateEntry(
			Action<string> clickLink,
			float angle = 0f,
			string message = null
		);
	}
}