using UnityEngine;

using LunraGames.SubLight.Models;
using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class RegionLabelPresenter : FocusPresenter<IRegionLabelView, SystemFocusDetails>
	{
		GameModel model;
		UniverseScales scale;
		ListenerProperty<UniverseScaleLabelBlock> labelProperty;

		public RegionLabelPresenter(GameModel model, UniverseScales scale)
		{
			this.model = model;
			this.scale = scale;

			switch (scale)
			{
				case UniverseScales.System: labelProperty = model.ScaleLabelSystem; break;
				case UniverseScales.Local: labelProperty = model.ScaleLabelLocal; break;
				case UniverseScales.Stellar: labelProperty = model.ScaleLabelStellar; break;
				case UniverseScales.Quadrant: labelProperty = model.ScaleLabelQuadrant; break;
				case UniverseScales.Galactic: labelProperty = model.ScaleLabelGalactic; break;
				case UniverseScales.Cluster: labelProperty = model.ScaleLabelCluster; break;
				default:
					Debug.LogError("Unrecognized scale: " + scale);
					return;
			}

			labelProperty.Changed += OnLabel;
		}

		protected override void OnUnBind()
		{
			if (labelProperty == null)
			{
				Debug.LogError("A labelProperty was never defined");
				return;
			}

			labelProperty.Changed -= OnLabel;
		}

		protected override void OnUpdateEnabled()
		{

		}

		#region
		void OnLabel(UniverseScaleLabelBlock label)
		{

		}
		#endregion
	}
}