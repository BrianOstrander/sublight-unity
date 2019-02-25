using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;

using TMPro;

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
		GameObject messageArea;

		[SerializeField]
		GridDeveloperMessageLeaf messagePrefab;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null

		List<GridDeveloperMessageLeaf> entries = new List<GridDeveloperMessageLeaf>();

		public DeveloperMessageEntry CreateEntry(
			Action<string> clickLink,
			float angle = 0f,
			string message = null
		)
		{
			var instance = messageArea.InstantiateChild(messagePrefab, setActive: true);
			entries.Add(instance);

			message = message ?? string.Empty;
			instance.MessageLabel.text = message;

			instance.Button.OnClick.AddListener(new UnityAction(() => OnClickMessage(instance.MessageLabel, clickLink)));

			instance.Canvas.SetParent(instance.CanvasAnchor, false);
			instance.transform.Rotate(Vector3.up, angle);

			return new DeveloperMessageEntry
			{
				Message = value => instance.MessageLabel.text = value
			};
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

			var linkIndex = TMP_TextUtilities.FindIntersectingLink(
				label,
				Input.mousePosition, // TODO: Should this be here???
				App.V.Camera
			);

			if (-1 < linkIndex) clickLink(label.textInfo.linkInfo[linkIndex].GetLinkID());
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
		DeveloperMessageEntry CreateEntry(
			Action<string> clickLink,
			float angle = 0f,
			string message = null
		);
	}
}