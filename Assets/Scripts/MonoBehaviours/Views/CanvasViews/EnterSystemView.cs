using System;

using UnityEngine;

using TMPro;

namespace LunraGames.SpaceFarm.Views
{
	public class EnterSystemView : CanvasView, IEnterSystemView
	{
		[SerializeField]
		TextMeshProUGUI titleLabel;
		[SerializeField]
		TextMeshProUGUI detailsLabel;

		public string Title { set { titleLabel.text = value ?? string.Empty; } }
		public string Details { set { detailsLabel.text = value ?? string.Empty; } }
		public Action OkayClick { set; private get; }

		public override void Reset()
		{
			base.Reset();

			Title = string.Empty;
			Details = string.Empty;
			OkayClick = ActionExtensions.Empty;
		}

		#region Events
		public void OnOkayClick() { OkayClick(); }
		#endregion
	}

	public interface IEnterSystemView : ICanvasView
	{
		string Title { set; }
		string Details { set; }
		Action OkayClick { set; }
	}
}