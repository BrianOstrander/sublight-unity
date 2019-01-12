using UnityEngine;

using TMPro;

namespace LunraGames.SubLight.Views
{
	public class TextEncounterLogView : EntryEncounterLogView, ITextEncounterLogView
	{
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
		[SerializeField]
		TextMeshProUGUI headerLabel;
		[SerializeField]
		TextMeshProUGUI messageLabel;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null

		public Color HeaderColor { set { headerLabel.color = value; } }
		public string Header { set { headerLabel.text = value ?? string.Empty; } }
		public string Message { set { messageLabel.text = value ?? string.Empty; } }

		public override void Reset()
		{
			base.Reset();

			HeaderColor = Color.white;
			Header = string.Empty;
			Message = string.Empty;
		}
	}

	public interface ITextEncounterLogView : IEntryEncounterLogView
	{
		Color HeaderColor { set; }
		string Header { set; }
		string Message { set; }
	}
}