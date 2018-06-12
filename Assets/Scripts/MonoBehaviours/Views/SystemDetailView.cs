using UnityEngine;

using TMPro;

namespace LunraGames.SpaceFarm.Views
{
	public class SystemDetailView : View, ISystemDetailView
	{
		[SerializeField]
		TextMeshProUGUI nameLabel;

		public string Name { set { nameLabel.text = value ?? string.Empty; } }

		public RectTransform CanvasTransform { get { return transform as RectTransform; } }

		public override void Reset()
		{
			base.Reset();

			Name = string.Empty;
		}

		#region Events
		#endregion
	}

	public interface ISystemDetailView : ICanvasView
	{
		string Name { set; }
	}
}