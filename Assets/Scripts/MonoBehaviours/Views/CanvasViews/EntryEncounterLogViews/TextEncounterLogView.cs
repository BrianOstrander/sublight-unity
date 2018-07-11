using UnityEngine;

using TMPro;

namespace LunraGames.SpaceFarm.Views
{
	public class TextEncounterLogView : EntryEncounterLogView, ITextEncounterLogView
	{
		[SerializeField]
		TextMeshProUGUI headerLabel;
		[SerializeField]
		TextMeshProUGUI messageLabel;

		public string Header { set { headerLabel.text = value ?? string.Empty; } }
		public string Message { set { messageLabel.text = value ?? string.Empty; } }

		public override void Reset()
		{
			base.Reset();

			Header = string.Empty;
			Message = string.Empty;
		}
	}

	public interface ITextEncounterLogView : IEntryEncounterLogView
	{
		string Header { set; }
		string Message { set; }
	}
}