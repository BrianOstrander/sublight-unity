using System;

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

			//waypoint.Distance.Changed += OnWaypointDistance;

			ScaleModel.Transform.Changed += OnScaleTransform;
			ScaleModel.Opacity.Changed += OnScaleOpacity;
		}

		protected override void OnUnBind()
		{
			base.OnUnBind();

			//waypoint.Distance.Changed -= OnWaypointDistance;

			ScaleModel.Transform.Changed -= OnScaleTransform;
			ScaleModel.Opacity.Changed -= OnScaleOpacity;
		}

		protected override void OnShowView()
		{
			View.HintVisible = Scale == UniverseScales.Local;

			//string distance;
			//string unit;

			//CalculateDistance(waypoint.Distance.Value, out distance, out unit);

			//View.SetDetails(waypoint.Name.Value, distance, unit);

			View.PushOpacity(() => ScaleModel.Opacity.Value);
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
		}

		void OnScaleOpacity(float opacity)
		{
			if (!View.Visible) return;
			View.SetOpacityStale();
		}

		//void OnWaypointDistance(float distance)
		//{
		//	if (!View.Visible) return;

		//	string distanceValue;
		//	string unitValue;

		//	CalculateDistance(waypoint.Distance.Value, out distanceValue, out unitValue);

		//	View.SetDetails(waypoint.Name.Value, distanceValue, unitValue);
		//}
		#endregion
	}
}