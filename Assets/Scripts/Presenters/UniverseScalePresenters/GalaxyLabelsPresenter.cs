﻿using System.Collections.Generic;

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
			scaleInUniverse = model.Context.Galaxy.GalaxySize;
			positionInUniverse = UniversePosition.Zero;
			this.scaleLabels = scaleLabels;
		}

		protected override void OnUnBind()
		{
			base.OnUnBind();
		}

		protected override void OnShowView()
		{
			var labels = new List<GalaxyLabelBlock>();
			var transform = ScaleModel.Transform.Value;

			foreach (var label in Model.Context.Galaxy.GetLabels(scaleLabels))
			{
				var result = new GalaxyLabelBlock();
				result.Text = label.Name;
				result.GroupId = label.GroupId;
				result.BeginAnchorNormalized = new Vector2(label.BeginAnchorNormal.Value.x, label.BeginAnchorNormal.Value.z);
				result.EndAnchorNormalized = new Vector2(label.EndAnchorNormal.Value.x, label.EndAnchorNormal.Value.z);
				result.CurveInfo = label.CurveInfo;
				result.SliceLayer = label.SliceLayer;
				result.Scale = label.Scale;
				labels.Add(result);
			}
			View.Labels = labels.ToArray();
		}

		#region Events
		#endregion
	}
}