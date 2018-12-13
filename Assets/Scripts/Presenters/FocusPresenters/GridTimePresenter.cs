using System.Collections.Generic;

using UnityEngine;

using LunraGames.SubLight.Models;
using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class GridTimePresenter : FocusPresenter<IGridTimeView, SystemFocusDetails>
	{
		GameModel model;
		bool isDelta;
		GridTimeLanguageBlock language;

		float targetOpacitySpeed = 1f / 0.3f;
		float beginOpacity;
		float currentOpacity;
		float? targetOpacity;

		public GridTimePresenter(
			GameModel model,
			bool isDelta,
			GridTimeLanguageBlock language
		)
		{
			this.model = model;
			this.isDelta = isDelta;
			this.language = language;

			model.DayTime.Changed += OnDayTime;
			model.CelestialSystemStateLastSelected.Changed += OnSelectedSystem;
		}

		protected override void OnUnBind()
		{
			model.DayTime.Changed -= OnDayTime;
			model.CelestialSystemStateLastSelected.Changed -= OnSelectedSystem;
		}

		protected override void OnUpdateEnabled()
		{
			OnSelectedSystem(model.CelestialSystemStateLastSelected.Value);
			View.PushOpacity(() => currentOpacity);

			if (isDelta)
			{
				Debug.Log("logic here");
			}
			else
			{
				View.ReferenceFrame = ReferenceFrames.Ship;
				View.Configuration = new GridTimeBlock
				{
					Title = language.Title.Value.Value,
					SubTitle = language.SubTitle.Value.Value,
					Tooltip = language.Tooltip.Value.Value,
					ReferenceFrames = new Dictionary<ReferenceFrames, string> {
						{ ReferenceFrames.Ship, language.Ship.Value.Value },
						{ ReferenceFrames.Galactic, language.Galactic.Value.Value },
					},
					ReferenceFrameSelection = OnReferenceFrameSelection,
					TitleClick = OnTitleClick,
					IsDelta = false
				};

				View.TimeStamp = new GridTimeStampBlock
				{
					AbsoluteTimes = new Dictionary<ReferenceFrames, DayTime> {
						{ ReferenceFrames.Ship, model.DayTime.Value.ShipTime },
						{ ReferenceFrames.Galactic, model.DayTime.Value.GalacticTime }
					},
					DeltaTimes = new Dictionary<ReferenceFrames, DayTime> {
						{ ReferenceFrames.Ship, model.DayTime.Value.ShipTime },
						{ ReferenceFrames.Galactic, model.DayTime.Value.GalacticTime }
					}
				};
			}
		}

		#region Events
		void OnUpdate(float delta)
		{
			if (!View.Visible) return;
			if (!targetOpacity.HasValue) return;

			var dir = Mathf.Sign(targetOpacity.Value - beginOpacity);

			currentOpacity = Mathf.Max(0f, Mathf.Min(1f, currentOpacity + (delta * targetOpacitySpeed * dir)));
			if (Mathf.Approximately(currentOpacity, targetOpacity.Value)) targetOpacity = null;
			View.SetOpacityStale();
		}

		void OnDayTime(DayTimeBlock dayTime)
		{
			if (!View.Visible) return;

			View.TimeStamp = new GridTimeStampBlock
			{
				AbsoluteTimes = new Dictionary<ReferenceFrames, DayTime> {
					{ ReferenceFrames.Ship, dayTime.ShipTime },
					{ ReferenceFrames.Galactic, dayTime.GalacticTime }
				},
				DeltaTimes = new Dictionary<ReferenceFrames, DayTime> {
					{ ReferenceFrames.Ship, dayTime.ShipTime },
					{ ReferenceFrames.Galactic, dayTime.GalacticTime }
				}
			};
		}

		void OnSelectedSystem(CelestialSystemStateBlock block)
		{
			beginOpacity = currentOpacity;
			switch (block.State)
			{
				case CelestialSystemStateBlock.States.Selected:
					targetOpacity = isDelta ? 1f : 0f;
					if (isDelta) Debug.Log("todo calculate time stuff herer");
					break;
				case CelestialSystemStateBlock.States.UnSelected:
					targetOpacity = isDelta ? 0f : 1f;
					break;
			}
		}

		void OnReferenceFrameSelection(ReferenceFrames referenceFrame)
		{

		}

		void OnTitleClick()
		{

		}
		#endregion
	}
}