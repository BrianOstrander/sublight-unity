using System;

using UnityEngine;

using TMPro;

namespace LunraGames.SubLight.Views
{
	public class GameLostView : CanvasView, IGameLostView
	{
		[SerializeField]
		TextMeshProUGUI reasonLabel;

		public string Reason { set { reasonLabel.text = value ?? string.Empty; } }
		public Action MainMenuClick { set; private get; }

		public override void Reset()
		{
			base.Reset();

			Reason = string.Empty;
			MainMenuClick = ActionExtensions.Empty;
		}

		#region Events
		public void OnMainMenuClick() { MainMenuClick(); }
		#endregion
	}

	public interface IGameLostView : ICanvasView
	{
		string Reason { set; }
		Action MainMenuClick { set; }
	}
}