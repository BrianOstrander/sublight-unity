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

		public FocusCameraPresenter(Transform viewParent, string overrideName = null) : base(viewParent, overrideName)
		{
			App.Focus.RegisterLayer(FocusLayer);
			App.Callbacks.GatherFocusRequest += OnGatherFocusRequest;
		}

		protected override void OnUnBind()
		{
			base.OnUnBind();

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
				gather.Done(gather.Duplicate(Texture2D.blackTexture));
				return;
			}
			gather.Done(gather.Duplicate(renderTexture = new RenderTexture(Screen.width, Screen.height, 16)));
		}

		protected override void OnShowInstant()
		{
			// No need to call base, FocusPresenter OnShowInstant has no logic.

			View.Texture = IsGatherable ? renderTexture : null;
		}

		protected override void OnCloseInstant()
		{
			// No need to call base, FocusPresenter OnCloseInstant has no logic.

			renderTexture = null;
			View.Texture = null;
		}
		#endregion
	}
}