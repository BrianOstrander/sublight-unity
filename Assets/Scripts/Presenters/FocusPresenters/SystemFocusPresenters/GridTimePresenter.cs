﻿using UnityEngine;

using LunraGames.SubLight.Models;
using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class GridTimePresenter : SystemFocusPresenter<IGridTimeView>
	{
		GameModel model;
		GridTimeLanguageBlock chronometerLanguage;
		GridTimeLanguageBlock transitLanguage;

		float targetOpacitySpeed = 1f / 0.3f;
		float currentOpacity;
		float? targetOpacity;

		public GridTimePresenter(
			GameModel model,
			GridTimeLanguageBlock chronometerLanguage,
			GridTimeLanguageBlock transitLanguage
		)
		{
			this.model = model;
			this.chronometerLanguage = chronometerLanguage;
			this.transitLanguage = transitLanguage;

			App.Heartbeat.Update += OnUpdate;

			model.RelativeDayTime.Changed += OnDayTime;
			model.Context.CelestialSystemStateLastSelected.Changed += OnSelectedSystem;
			//model.Ship.Velocity.Changed += OnVelocity;
			model.Context.TransitState.Changed += OnTransitState;
		}

		protected override void OnUnBind()
		{
			base.OnUnBind();

			App.Heartbeat.Update -= OnUpdate;

			model.RelativeDayTime.Changed -= OnDayTime;
			model.Context.CelestialSystemStateLastSelected.Changed -= OnSelectedSystem;
			//model.Ship.Velocity.Changed -= OnVelocity;
			model.Context.TransitState.Changed -= OnTransitState;
		}

		protected override void OnUpdateEnabled()
		{
			View.PushOpacity(() => currentOpacity);
			View.PushOpacity(() => model.Context.TransitState.Value.State == TransitState.States.Active ? 0f : 1f);

			View.ReferenceFrame = ReferenceFrames.Ship;
			View.Configuration = GetConfiguration(IsTransit);

			View.TimeStamp = GetTimeStamp(model.Context.CelestialSystemStateLastSelected.Value);

			targetOpacity = 1f;
		}

		GridTimeBlock GetConfiguration(bool isTransit)
		{
			var result = GridTimeBlock.Default;

			var language = isTransit ? transitLanguage : chronometerLanguage;

			result.Title = language.Title.Value.Value;
			result.SubTitle = language.SubTitle.Value.Value;
			result.Tooltip = language.Tooltip.Value.Value;
			foreach (var kv in language.ReferenceFrameNames) result.ReferenceFrameNames[kv.Key] = kv.Value.Value.Value; // Lol...
			result.IsTransit = isTransit;

			return result;
		}

		GridTimeStampBlock GetTimeStamp(CelestialSystemStateBlock block)
		{
			var result = GridTimeStampBlock.Default;

			if (block.State == CelestialSystemStateBlock.States.Selected)
			{
				/*
				var distance = UniversePosition.ToLightYearDistance(UniversePosition.Distance(block.Position, model.Ship.Position.Value));
				var transitDelta = RelativityUtility.TransitTime(model.Ship.Velocity.Value.Current.RelativisticLightYears, distance);

				result.DeltaTimes[ReferenceFrames.Ship] = transitDelta.ShipTime;
				result.DeltaTimes[ReferenceFrames.Galactic] = transitDelta.GalacticTime;

				result.AbsoluteTimes[ReferenceFrames.Ship] = model.RelativeDayTime.Value.ShipTime + result.DeltaTimes[ReferenceFrames.Ship];
				result.AbsoluteTimes[ReferenceFrames.Galactic] = model.RelativeDayTime.Value.GalacticTime + result.DeltaTimes[ReferenceFrames.Galactic];
*/
				return result;
			}

			result.AbsoluteTimes[ReferenceFrames.Ship] = model.RelativeDayTime.Value.ShipTime;
			result.AbsoluteTimes[ReferenceFrames.Galactic] = model.RelativeDayTime.Value.GalacticTime;

			return result;
		}

		bool IsTransit { get { return model.Context.CelestialSystemStateLastSelected.Value.State == CelestialSystemStateBlock.States.Selected; } }

		#region Events
		void OnUpdate(float delta)
		{
			if (!View.Visible) return;
			if (!targetOpacity.HasValue) return;

			var dir = Mathf.Sign(targetOpacity.Value);

			currentOpacity = Mathf.Max(0f, Mathf.Min(1f, currentOpacity + (delta * targetOpacitySpeed * dir)));
			if (Mathf.Approximately(currentOpacity, targetOpacity.Value)) targetOpacity = null;
			View.SetOpacityStale();
		}

		void OnDayTime(RelativeDayTime relativeDayTime)
		{
			if (!View.Visible) return;

			View.TimeStamp = GetTimeStamp(model.Context.CelestialSystemStateLastSelected.Value);
		}

		void OnSelectedSystem(CelestialSystemStateBlock block)
		{
			if (!View.Visible) return;

			View.Configuration = GetConfiguration(block.State == CelestialSystemStateBlock.States.Selected);
			View.TimeStamp = GetTimeStamp(block);
			View.TimeStampTransition();
		}

		//void OnVelocity(VelocityProfileState velocity)
		//{
		//	if (!View.Visible) return;

		//	View.TimeStamp = GetTimeStamp(model.Context.CelestialSystemStateLastSelected.Value);
		//	if (model.Context.CelestialSystemStateLastSelected.Value.State == CelestialSystemStateBlock.States.Selected) View.ReferenceFrameTransition();
		//}

		void OnReferenceFrameSelection(ReferenceFrames referenceFrame)
		{

		}

		void OnTitleClick()
		{

		}

		void OnTransitState(TransitState transitState)
		{
			model.RelativeDayTime.Value = transitState.RelativeTimeCurrent;

			if (!View.Visible) return;

			switch (transitState.State)
			{
				case TransitState.States.Request:
				case TransitState.States.Complete:
					View.SetOpacityStale();
					break;
			}
		}
		#endregion
	}
}