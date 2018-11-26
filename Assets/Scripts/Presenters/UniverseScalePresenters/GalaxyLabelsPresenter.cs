using System.Collections.Generic;

using UnityEngine;

using LunraGames.SubLight.Models;
using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class GalaxyLabelsPresenter : UniverseScalePresenter<IGalaxyLabelsView>
	{
		UniversePosition scaleInUniverse;
		UniversePosition positionInUniverse;
		UniverseScales[] scaleLabels;

		protected override UniversePosition ScaleInUniverse { get { return scaleInUniverse; } }
		protected override UniversePosition PositionInUniverse { get { return positionInUniverse; } }

		public GalaxyLabelsPresenter(GameModel model, UniverseScales scale, params UniverseScales[] scaleLabels) : base(model, scale)
		{
			scaleInUniverse = model.Galaxy.GalaxySize;
			positionInUniverse = UniversePosition.Zero;
			this.scaleLabels = scaleLabels;

			ScaleModel.Opacity.Changed += OnScaleOpacity;
		}

		protected override void OnUnBind()
		{
			base.OnUnBind();

			ScaleModel.Opacity.Changed -= OnScaleOpacity;
		}

		protected override void OnShowView()
		{
			var labels = new List<GalaxyLabelBlock>();
			var transform = ScaleModel.Transform.Value;

			foreach (var label in Model.Galaxy.GetLabels(scaleLabels))
			{
				var result = new GalaxyLabelBlock();
				result.Text = label.Name;
				result.GroupId = label.GroupId;
				var beginNormalized = UniversePosition.NormalizedSector(label.BeginAnchor, ScaleInUniverse);
				var endNormalized = UniversePosition.NormalizedSector(label.EndAnchor, ScaleInUniverse);
				result.BeginAnchorNormalized = new Vector2(beginNormalized.x, beginNormalized.z);
				result.EndAnchorNormalized = new Vector2(endNormalized.x, endNormalized.z);
				result.CurveInfo = label.CurveInfo;
				result.SliceLayer = label.SliceLayer;
				result.Scale = label.Scale;
				labels.Add(result);
			}
			View.Labels = labels.ToArray();
		}

		#region Events
		void OnScaleOpacity(float value)
		{
			if (!View.Visible) return;

			View.Opacity = value;
		}
		#endregion
	}
}