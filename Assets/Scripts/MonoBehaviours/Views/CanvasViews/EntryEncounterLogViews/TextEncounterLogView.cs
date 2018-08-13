using UnityEngine;

using TMPro;

namespace LunraGames.SubLight.Views
{
	public class TextEncounterLogView : EntryEncounterLogView, ITextEncounterLogView
	{
		[SerializeField]
		TextMeshProUGUI headerLabel;
		[SerializeField]
		TextMeshProUGUI messageLabel;

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