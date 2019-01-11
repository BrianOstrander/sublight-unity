using UnityEngine;
using UnityEngine.Events;

namespace LunraGames.SubLight.Views
{
	public class ButtonEncounterLogView : EntryEncounterLogView, IButtonEncounterLogView
	{
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
		[SerializeField]
		CanvasGroup group;
		[SerializeField]
		XButtonStyleObject normalButtonStyle;
		[SerializeField]
		XButtonStyleObject usedButtonStyle;
		[SerializeField]
		GameObject buttonArea;
		[SerializeField]
		EncounterButtonLeaf buttonEntryPrefab;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null

		public ButtonLogBlock[] Buttons 
		{
			set
			{
				buttonArea.transform.ClearChildren<LabelButtonLeaf>();
				if (value == null) return;
				foreach (var entry in value)
				{
					var instance = buttonArea.InstantiateChild(buttonEntryPrefab, setActive: true);
					instance.Button.interactable = entry.Interactable;
					var usedStyle = entry.Used ? usedButtonStyle : normalButtonStyle;
					foreach (var leaf in instance.UsedLeafs) leaf.GlobalStyle = usedStyle;
					instance.ButtonLabel.text = entry.Message ?? string.Empty;
					instance.Button.OnClick.AddListener(new UnityAction(entry.Click ?? ActionExtensions.Empty));
				}
			}
		}

		public override bool Interactable
		{
			get { return base.Interactable; }

			set
			{
				group.interactable = value;
				base.Interactable = value;
			}
		}

		public override void Reset()
		{
			base.Reset();

			buttonEntryPrefab.gameObject.SetActive(false);
			Buttons = null;
		}
	}

	public interface IButtonEncounterLogView : IEntryEncounterLogView
	{
		ButtonLogBlock[] Buttons { set; }
	}
}