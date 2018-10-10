using UnityEngine;

using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public abstract class FocusCameraPresenter<V, D> : FocusPresenter<V, D>
		where V : class, IFocusCameraView
		where D : SetFocusDetails<D>, new()
	{
		protected virtual bool IsGatherable { get { return true; } }

		RenderTexture renderTexture;

		public FocusCameraPresenter(Transform viewParent, string overrideName = null, float? fieldOfView = null) : base(viewParent, overrideName)
		{
			App.Focus.RegisterLayer(FocusLayer);
			App.Callbacks.GatherFocusRequest += OnGatherFocusRequest;

			if (fieldOfView.HasValue) View.FieldOfView = fieldOfView.Value;
		}

		protected override void OnUnBind()
		{
			base.OnUnBind(); // There's logic above this class in FocusPresenter.

			App.Focus.UnRegisterLayer(FocusLayer);
			App.Callbacks.GatherFocusRequest -= OnGatherFocusRequest;
		}

		#region Events
		void OnGatherFocusRequest(GatherFocusRequest request)
		{
			DeliverFocusBlock gather;
			if (!request.GetGather(FocusLayer, out gather)) return;
			if (!IsGatherable)
			{
				gather.Done(gather.Duplicate(null));
				return;
			}

			View.Texture = renderTexture = new RenderTexture(Screen.width, Screen.height, 16);
			gather.Done(gather.Duplicate(renderTexture));
		}

		protected override void OnUpdateEnabled()
		{
			// No need to call base, FocusPresenter OnUpdateEnabled has no logic.

			View.Texture = IsGatherable ? renderTexture : null;
		}

		protected override void OnUpdateDisabled()
		{
			// No need to call base, FocusPresenter OnUpdateDisabled has no logic.

			renderTexture = null;
			View.Texture = null;
		}
		#endregion
	}
}