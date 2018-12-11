using UnityEngine;

using TMPro;

namespace LunraGames.SubLight.Views
{
	public class BodiesDisabledView : UniverseScaleView, IBodiesDisabledView
	{
		[SerializeField]
		TextMeshProUGUI titleLabel;
		[SerializeField]
		TextMeshProUGUI descriptionLabel;
		[SerializeField]
		CanvasGroup group;

		public void SetText(string title, string description)
		{
			title = title ?? string.Empty;
			description = description ?? string.Empty;

			titleLabel.text = title;
			descriptionLabel.text = description;
		}

		protected override void OnOpacityStack(float opacity)
		{
			group.alpha = opacity;
		}

		public override void Reset()
		{
			base.Reset();

			SetText(string.Empty, string.Empty);
		}
	}

	public interface IBodiesDisabledView : IUniverseScaleView
	{
		void SetText(string title, string description);
	}
}