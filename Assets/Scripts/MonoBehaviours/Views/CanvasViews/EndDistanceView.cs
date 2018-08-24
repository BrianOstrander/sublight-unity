using System;

using UnityEngine;

using TMPro;

namespace LunraGames.SubLight.Views
{
	public class EndDistanceView : CanvasView, IEndDistanceView
	{
		[SerializeField]
		TextMeshProUGUI distanceLabel;

		public Action EncyclopediaClick { set; private get; }
		public float Distance { set { distanceLabel.text = value.ToString("F1") + " Light Years"; } }

		public override void Reset()
		{
			base.Reset();

			EncyclopediaClick = ActionExtensions.Empty;
			Distance = 0f;
		}

		#region Events
		public void OnEncyclopediaClick() { EncyclopediaClick(); }
		#endregion
	}

	public interface IEndDistanceView : ICanvasView
	{
		Action EncyclopediaClick { set; }
		float Distance { set; }
	}
}