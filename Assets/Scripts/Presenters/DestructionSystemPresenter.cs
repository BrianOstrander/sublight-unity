﻿using LunraGames.SpaceFarm.Models;
using LunraGames.SpaceFarm.Views;

namespace LunraGames.SpaceFarm.Presenters
{
	public class DestructionSystemPresenter : Presenter<IDestructionSystemView>
	{
		GameModel model;

		public DestructionSystemPresenter(GameModel model)
		{
			this.model = model;

			model.DestructionRadius.Changed += OnDestructionRadius;
			App.Callbacks.VoidRenderTexture += OnVoidRenderTexture;
			App.Callbacks.DayTimeDelta += OnDayTimeDelta;
		}

		protected override void UnBind()
		{
			base.UnBind();

			model.DestructionRadius.Changed -= OnDestructionRadius;
			App.Callbacks.VoidRenderTexture -= OnVoidRenderTexture;
			App.Callbacks.DayTimeDelta -= OnDayTimeDelta;
		}

		public void Show()
		{
			if (View.Visible) return;
			View.Reset();
			View.UniversePosition = UniversePosition.Zero;
			View.Radius = model.DestructionRadius;
			View.Highlight = OnHighlight;
			View.Click = OnClick;
			View.VoidTexture = App.Callbacks.LastVoidRenderTexture.Texture;
			ShowView(instant: true);
		}

		#region Events
		void OnDayTimeDelta(DayTimeDelta delta)
		{
			model.DestructionRadius.Value += delta.Delta.TotalTime * model.DestructionSpeed;
		}

		void OnVoidRenderTexture(VoidRenderTexture voidRenderTexture)
		{
			if (!View.Visible) return;
			View.VoidTexture = voidRenderTexture.Texture;
		}

		void OnDestructionRadius(float radius)
		{
			if (!View.Visible) return;
			View.Radius = model.DestructionRadius;
		}

		void OnHighlight(bool highlighted)
		{
			App.Log("Highlight", LogTypes.ToDo);
		}

		void OnClick()
		{
			App.Log("Click", LogTypes.ToDo);
		}
		#endregion
	}
}