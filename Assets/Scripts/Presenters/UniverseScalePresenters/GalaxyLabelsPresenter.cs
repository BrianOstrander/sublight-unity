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

		protected override UniversePosition ScaleInUniverse { get { return scaleInUniverse; } }
		protected override UniversePosition PositionInUniverse { get { return positionInUniverse; } }

		public GalaxyLabelsPresenter(GameModel model, UniverseScales scale) : base(model, scale)
		{
			scaleInUniverse = model.Galaxy.GalaxySize;
			positionInUniverse = UniversePosition.Zero;

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

			foreach (var label in Model.Galaxy.GetLabels(Scale))
			{
				var result = new GalaxyLabelBlock();
				result.Text = label.Name;
				result.GroupId = label.GroupId;
				var beginNormalized = UniversePosition.NormalizedSector(label.BeginAnchor, ScaleInUniverse);
				var endNormalized = UniversePosition.NormalizedSector(label.EndAnchor, ScaleInUniverse);
				result.BeginAnchorNormalized = new Vector2(beginNormalized.x, beginNormalized.z);
				result.EndAnchorNormalized = new Vector2(endNormalized.x, endNormalized.z);
				Debug.Log("begin normal: " + result.BeginAnchorNormalized);
				Debug.Log("end normal: " + result.EndAnchorNormalized);
				result.CurveInfo = label.CurveInfo;
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