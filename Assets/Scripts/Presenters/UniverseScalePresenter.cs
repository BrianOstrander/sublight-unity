using System;

using UnityEngine;

using LunraGames.SubLight.Models;
using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public abstract class UniverseScalePresenter<V> : Presenter<V>
		where V : class, IUniverseScaleView
	{
		const float MinimumScale = 0.001f;

		protected abstract UniversePosition ScaleInUniverse { get; }
		protected abstract UniversePosition PositionInUniverse { get; }

		protected GameModel Model;

		UniverseScales scale;
		UniverseScaleModel scaleModel;

		public UniverseScalePresenter(GameModel model, UniverseScales scale)
		{
			Model = model;
			this.scale = scale;
			scaleModel = model.GetScale(scale);

			scaleModel.Transform.Changed += OnScaleTransform;
			scaleModel.Opacity.Changed += OnOpacity;
		}

		protected override void OnUnBind()
		{
			scaleModel.Transform.Changed -= OnScaleTransform;
			scaleModel.Opacity.Changed -= OnOpacity;
		}

		void ApplyScaleTransform(UniverseTransform transform)
		{
			var result = scaleModel.Transform.Value.GetUnityScale(ScaleInUniverse);
			result = new Vector3(
				Mathf.Max(MinimumScale, result.x),
				Mathf.Max(MinimumScale, result.y),
				Mathf.Max(MinimumScale, result.z)
			);
			View.Scale = result;
			
			View.Position = scaleModel.Transform.Value.GetUnityPosition(PositionInUniverse);
		}

		void ShowViewInstant()
		{
			View.Reset();

			OnShowView();

			ShowView(instant: true);
		}

		void CloseViewInstant()
		{
			OnCloseView();

			CloseView(true);
		}

		#region Events
		void OnScaleTransform(UniverseTransform transform)
		{
			if (!View.Visible) return;
			ApplyScaleTransform(transform);
		}

		void OnOpacity(float opacity)
		{
			var isOpacityZero = Mathf.Approximately(0f, opacity);

			switch (View.TransitionState)
			{
				case TransitionStates.Closed:
					if (!isOpacityZero) ShowViewInstant();
					else return;
					break;
				case TransitionStates.Shown:
					if (isOpacityZero) CloseViewInstant();
					break;
			}
			OnSetOpacity(opacity);
		}

		protected virtual void OnShowView() {}
		protected virtual void OnCloseView() {}
		protected virtual void OnSetOpacity(float opacity) {}
		#endregion
	}
}