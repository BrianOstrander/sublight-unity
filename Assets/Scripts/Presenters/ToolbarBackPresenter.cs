﻿using UnityEngine;

using LunraGames.SubLight.Models;
using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class ToolbarBackPresenter : Presenter<IToolbarBackView>, IPresenterCloseShowOptions
	{
		GameModel model;
		LanguageStringModel back;
		bool waitingForAnimation;

		public ToolbarBackPresenter(
			GameModel model,
			LanguageStringModel back
		)
		{
			this.model = model;
			this.back = back;

			model.Context.ToolbarSelectionRequest.Changed += OnToolbarSelectionRequest;
		}

		protected override void OnUnBind()
		{
			model.Context.ToolbarSelectionRequest.Changed -= OnToolbarSelectionRequest;
		}

		public void Show(Transform parent = null, bool instant = false)
		{
			if (View.Visible) return;

			View.Reset();

			View.BackText = back.Value.Value;
			View.Click = OnClick;

			ShowView(parent, instant);
		}

		public void Close(bool instant = false)
		{
			if (!View.Visible) return;

			CloseView(instant);
		}

		#region Events
		void OnToolbarSelectionRequest(ToolbarSelectionRequest request)
		{
			switch (model.ToolbarSelection.Value)
			{
				case ToolbarSelections.Ship: break;
				default:
					OnClick();
					break;
			}
		}

		void OnClick()
		{
			if (waitingForAnimation) return;
			waitingForAnimation = true;
			App.Callbacks.CameraTransformRequest(
				CameraTransformRequest.Animation(
					pitch: 0f,
					duration: View.AnimationTime,
					done: OnAnimationDone)
			);
		}

		void OnAnimationDone()
		{
			waitingForAnimation = false;
		}
		#endregion
	}
}