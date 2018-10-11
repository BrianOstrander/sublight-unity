using System;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;

using TMPro;

namespace LunraGames.SubLight.Views
{
	public class DialogView : View, IDialogView
	{
		[Serializable]
		struct DialogEntry
		{
			public DialogTypes DialogType;
			public GameObject Root;
		}

		[SerializeField]
		ColorStyleBlock neutralPrimaryColor = ColorStyleBlock.Default;
		[SerializeField]
		ColorStyleBlock neutralSecondaryColor = ColorStyleBlock.Default;
		[SerializeField]
		ColorStyleBlock warningPrimaryColor = ColorStyleBlock.Default;
		[SerializeField]
		ColorStyleBlock warningSecondaryColor = ColorStyleBlock.Default;
		[SerializeField]
		ColorStyleBlock errorPrimaryColor = ColorStyleBlock.Default;
		[SerializeField]
		ColorStyleBlock errorSecondaryColor = ColorStyleBlock.Default;

		[SerializeField]
		XButtonStyleObject neutralPrimaryButtonStyle;
		[SerializeField]
		XButtonStyleObject neutralSecondaryButtonStyle;
		[SerializeField]
		XButtonStyleObject warningPrimaryButtonStyle;
		[SerializeField]
		XButtonStyleObject warningSecondaryButtonStyle;
		[SerializeField]
		XButtonStyleObject errorPrimaryButtonStyle;
		[SerializeField]
		XButtonStyleObject errorSecondaryButtonStyle;

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

		[SerializeField]
		GameObject[] neutralIcons;
		[SerializeField]
		GameObject[] warningIcons;
		[SerializeField]
		GameObject[] errorIcons;

		[SerializeField]
		Graphic[] primaryColors;
		[SerializeField]
		Graphic[] secondaryColors;

		[SerializeField]
		XButtonLeaf[] primaryButtons;
		[SerializeField]
		XButtonLeaf[] secondaryButtons;

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

		public DialogStyles Style
		{
			set
			{
				var primaryColor = Color.white;
				var secondaryColor = Color.white;

				var primaryButtonStyle = neutralPrimaryButtonStyle;
				var secondaryButtonStyle = neutralSecondaryButtonStyle;

				switch (value)
				{
					case DialogStyles.Neutral:
						primaryColor = neutralPrimaryColor;
						secondaryColor = neutralSecondaryColor;
						primaryButtonStyle = neutralPrimaryButtonStyle;
						secondaryButtonStyle = neutralSecondaryButtonStyle;
						break;
					case DialogStyles.Warning:
						primaryColor = warningPrimaryColor;
						secondaryColor = warningSecondaryColor;
						primaryButtonStyle = warningPrimaryButtonStyle;
						secondaryButtonStyle = warningSecondaryButtonStyle;
						break;
					case DialogStyles.Error:
						primaryColor = errorPrimaryColor;
						secondaryColor = errorSecondaryColor;
						primaryButtonStyle = errorPrimaryButtonStyle;
						secondaryButtonStyle = errorSecondaryButtonStyle;
						break;
					case DialogStyles.Unknown: break;
					default:
						Debug.LogError("Unrecognized style: " + value);
						break;
				}

				foreach (var icon in neutralIcons) icon.SetActive(value == DialogStyles.Neutral);
				foreach (var icon in warningIcons) icon.SetActive(value == DialogStyles.Warning);
				foreach (var icon in errorIcons) icon.SetActive(value == DialogStyles.Error);

				foreach (var graphic in primaryColors) graphic.color = primaryColor;
				foreach (var graphic in secondaryColors) graphic.color = secondaryColor;

				foreach (var button in primaryButtons) button.GlobalStyle = primaryButtonStyle;
				foreach (var button in secondaryButtons) button.GlobalStyle = secondaryButtonStyle;
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
			Style = DialogStyles.Unknown;

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

	public interface IDialogView : IView
	{
		DialogTypes DialogType { set; }
		DialogStyles Style { set; }
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