using LunraGames.SubLight.Models;
using LunraGames.SubLight.Views;

using UnityEngine;

namespace LunraGames.SubLight.Presenters
{
	public class ClusterPresenter : UniverseScalePresenter<IClusterView>
	{
		GalaxyInfoModel galaxy;
		LanguageStringModel detailText;

		UniversePosition scaleInUniverse;
		UniversePosition positionInUniverse;

		protected override UniversePosition ScaleInUniverse { get { return scaleInUniverse; } }
		protected override UniversePosition PositionInUniverse { get { return positionInUniverse; } }

		public ClusterPresenter(
			GameModel model,
			GalaxyInfoModel galaxy,
			LanguageStringModel detailText = null
		) : base(model, UniverseScales.Cluster)
		{
			this.galaxy = galaxy;
			this.detailText = detailText;

			scaleInUniverse = galaxy.GalaxySize;
			positionInUniverse = galaxy.GalaxyOrigin;

			ScaleModel.Opacity.Changed += OnScaleOpacity;
		}

		protected override void OnUnBind()
		{
			base.OnUnBind();

			ScaleModel.Opacity.Changed -= OnScaleOpacity;
		}

		#region Events
		protected override void OnShowView()
		{
			var transform = Model.ActiveScale.Value.Transform.Value;
			SetGrid(transform.UnityOrigin, transform.UnityRadius);
			View.SetGalaxy(galaxy.FullPreview);
			View.GalaxyName = galaxy.Name;
			if (detailText == null)
			{
				View.Interactable = false;
			}
			else
			{
				View.Interactable = true;
				View.DetailText = detailText.Value;
				View.Click = OnClick;
			}

			View.GalaxyNormal = galaxy.UniverseNormal;
			View.AlertHeightMultiplier = galaxy.AlertHeightMultiplier;
		}

		void OnScaleOpacity(float value)
		{
			if (!View.Visible) return;

			View.Opacity = value;
		}

		void OnClick()
		{
			if (string.IsNullOrEmpty(galaxy.EncyclopediaEntryId.Value))
			{
				Debug.LogWarning("No provided encyclopedia entry to view...");
				return;
			}
			Debug.LogWarning("Go to encyclopedia entry logic here...");
		}
		#endregion
	}
}