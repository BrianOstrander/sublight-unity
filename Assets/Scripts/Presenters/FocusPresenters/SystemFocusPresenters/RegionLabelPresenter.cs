using UnityEngine;

using LunraGames.SubLight.Models;
using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class RegionLabelPresenter : SystemFocusPresenter<IRegionLabelView>
	{
		GameModel model;
		UniverseScales scale;
		ListenerProperty<UniverseScaleLabelBlock> labelProperty;
		UniverseScaleModel scaleModel;

		public RegionLabelPresenter(GameModel model, UniverseScales scale)
		{
			this.model = model;
			this.scale = scale;
			scaleModel = model.GetScale(scale);

			scaleModel.Opacity.Changed += OnOpacityStale;
			model.Context.GridScaleOpacity.Changed += OnOpacityStale;

			switch (scale)
			{
				case UniverseScales.System: labelProperty = model.Context.ScaleLabelSystem; break;
				case UniverseScales.Local: labelProperty = model.Context.ScaleLabelLocal; break;
				case UniverseScales.Stellar: labelProperty = model.Context.ScaleLabelStellar; break;
				case UniverseScales.Quadrant: labelProperty = model.Context.ScaleLabelQuadrant; break;
				case UniverseScales.Galactic: labelProperty = model.Context.ScaleLabelGalactic; break;
				case UniverseScales.Cluster: labelProperty = model.Context.ScaleLabelCluster; break;
				default:
					Debug.LogError("Unrecognized scale: " + scale);
					return;
			}

			labelProperty.Changed += OnLabel;
		}

		protected override void OnUnBind()
		{
			base.OnUnBind();

			model.GetScale(scale).Opacity.Changed -= OnOpacityStale;
			model.Context.GridScaleOpacity.Changed -= OnOpacityStale;

			if (labelProperty == null)
			{
				Debug.LogError("A labelProperty was never defined");
				return;
			}

			labelProperty.Changed -= OnLabel;
		}

		protected override void OnUpdateEnabled()
		{
			OnLabel(labelProperty.Value);
			View.PushOpacity(() => scaleModel.Opacity.Value);
			View.PushOpacity(() => model.Context.GridScaleOpacity.Value);
		}

		#region Events
		void OnOpacityStale(float opacity)
		{
			View.SetOpacityStale();
		}

		void OnLabel(UniverseScaleLabelBlock label)
		{
			View.SetRegion(label.Name.Value.Value);
		}
		#endregion
	}
}