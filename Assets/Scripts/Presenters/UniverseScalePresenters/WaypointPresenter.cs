﻿using System;

using UnityEngine;

using LunraGames.SubLight.Models;
using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class WaypointPresenter : UniverseScalePresenter<IWaypointView>
	{
		WaypointModel waypoint;
		WaypointLanguageBlock language;

		protected override UniversePosition PositionInUniverse { get { return waypoint.Location.Value.Position; } }

		public WaypointPresenter(
			GameModel model,
			WaypointModel waypoint,
			WaypointLanguageBlock language,
			UniverseScales scale
		) : base(model, scale)
		{
			this.waypoint = waypoint;
			this.language = language;

			ScaleModel.Transform.Changed += OnScaleTransform;
			ScaleModel.Opacity.Changed += OnScaleOpacity;
		}

		protected override void OnUnBind()
		{
			base.OnUnBind();

			ScaleModel.Transform.Changed -= OnScaleTransform;
			ScaleModel.Opacity.Changed -= OnScaleOpacity;
		}

		protected override void OnShowView()
		{
			View.HintVisible = Scale == UniverseScales.Local;

			UpdateDistance();

			View.PushOpacity(() => ScaleModel.Opacity.Value);
		}

		void UpdateDistance()
		{
			string distance;
			string unit;

			var positionOnPlane = waypoint.Location.Value.Position.NewLocal(waypoint.Location.Value.Position.Local.NewY(0f));
			CalculateDistance(UniversePosition.Distance(ScaleModel.Transform.Value.UniverseOrigin, positionOnPlane), out distance, out unit);

			View.SetDetails(waypoint.Name.Value, distance, unit);
		}

		void CalculateDistance(float distance, out string distanceValue, out string unitValue)
		{
			Func<string> getUnitCount;
			LanguageStringModel unitType;
			language.Unit.GetUnitModels(UniversePosition.ToLightYearDistance(distance), out getUnitCount, out unitType);

			distanceValue = getUnitCount();
			unitValue = unitType.Value.Value;
		}

		#region Events
		void OnScaleTransform(UniverseTransform universeTransform)
		{
			if (!View.Visible) return;

			UpdateDistance();
		}

		void OnScaleOpacity(float opacity)
		{
			if (!View.Visible) return;
			View.SetOpacityStale();
		}
		#endregion
	}
}