using System;
using System.Linq;

using UnityEngine;

using TMPro;

namespace LunraGames.SpaceFarm.Views
{
	public class DialogView : CanvasView, IDialogView
	{
		[Serializable]
		struct DialogEntry
		{
			public DialogTypes DialogType;
			public GameObject Root;
		}

		[SerializeField]
		DialogEntry[] entries;
		[SerializeField]
		TextMeshProUGUI[] titleLabels;
		[SerializeField]
		TextMeshProUGUI[] messageLabels;
		[SerializeField]
		TextMeshProUGUI[] cancelLabels;
		[SerializeField]
		TextMeshProUGUI[] failureLabels;
		[SerializeField]
		TextMeshProUGUI[] successLabels;

		public DialogTypes DialogType
		{
			set
			{
				foreach (var entry in entries.Where(e => e.Root != null))
				{
					entry.Root.SetActive(entry.DialogType == value);
				}
			}
		}
					
		public string Title { set { SetText(value, titleLabels); } }
		public string Message { set { SetText(value, messageLabels); } }
		public string CancelText { set { SetText(value, cancelLabels); } }
		public string FailureText { set { SetText(value, failureLabels); } }
		public string SuccessText { set { SetText(value, successLabels); } }

		public Action CancelClick { set; private get; }
		public Action FailureClick { set; private get; }
		public Action SuccessClick { set; private get; }

		void SetText(string text, params TextMeshProUGUI[] labels)
		{
			text = text ?? string.Empty;
			foreach (var label in labels.Where(l => l != null)) label.text = text;
		}

		public override void Reset()
		{
			base.Reset();

			DialogType = DialogTypes.Unknown;

			Title = string.Empty;
			CancelText = string.Empty;
			FailureText = string.Empty;
			SuccessText = string.Empty;

			CancelClick = ActionExtensions.Empty;
			FailureClick = ActionExtensions.Empty;
			SuccessClick = ActionExtensions.Empty;
		}

		#region Events
		public void OnCancelClick()
		{
			CancelClick();
		}

		public void OnFailureClick()
		{
			FailureClick();
		}

		public void OnSuccessClick()
		{
			SuccessClick();
		}
		#endregion
	}

	public interface IDialogView : ICanvasView
	{
		DialogTypes DialogType { set; }
		string Title { set; }
		string Message { set; }
		string CancelText { set; }
		string FailureText { set; }
		string SuccessText { set; }
		Action CancelClick { set; }
		Action FailureClick { set; }
		Action SuccessClick { set; }
	}
}