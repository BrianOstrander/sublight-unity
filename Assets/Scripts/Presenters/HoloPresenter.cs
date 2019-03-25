using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using LunraGames.SubLight.Views;
using LunraGames.SubLight.Models;

namespace LunraGames.SubLight.Presenters
{
	public class HoloPresenter : Presenter<IHoloView>
	{
		GameModel model;
		Dictionary<SetFocusLayers, int> layerToOrder = new Dictionary<SetFocusLayers, int>();

		UniverseScaleModel lastActiveScale;

		public HoloPresenter(GameModel model = null)
		{
			this.model = model;
			App.Callbacks.HoloColorRequest += OnHoloColorRequest;
			App.Callbacks.TransitionFocusRequest += OnTransitionFocusRequest;
			App.Callbacks.CameraTransformRequest += OnCameraTransformRequest;

			if (model == null) return;

			model.Context.ActiveScale.Changed += OnActiveScale;
			OnActiveScale(model.Context.ActiveScale.Value);
			model.Context.TransitState.Changed += OnTransitState;
		}

		protected override void OnUnBind()
		{
			App.Callbacks.HoloColorRequest -= OnHoloColorRequest;
			App.Callbacks.TransitionFocusRequest -= OnTransitionFocusRequest;
			App.Callbacks.CameraTransformRequest -= OnCameraTransformRequest;

			if (model == null) return;

			if (lastActiveScale != null) lastActiveScale.Transform.Changed -= OnActiveScaleTransform;
			model.Context.ActiveScale.Changed -= OnActiveScale;
			model.Context.TransitState.Changed -= OnTransitState;
		}

		#region Events
		void OnCameraTransformRequest(CameraTransformRequest request)
		{
			if (!View.Visible) return;

			View.CameraPitch = App.V.CameraTransform.PitchValue();
		}

		void OnActiveScale(UniverseScaleModel scale)
		{
			if (lastActiveScale != null) lastActiveScale.Transform.Changed -= OnActiveScaleTransform;
			lastActiveScale = scale;
			lastActiveScale.Transform.Changed += OnActiveScaleTransform;
		}

		void OnActiveScaleTransform(UniverseTransform transform)
		{
			var offset = transform.GetGridOffset(transform.UnityToUniverse.x); // Lol don't know what i'm doing...
			if (float.IsNaN(offset.x)) offset = offset.NewX(0f);
			if (float.IsNaN(offset.z)) offset = offset.NewZ(0f);
			View.GridOffset = new Vector2(offset.x, offset.z);
		}

		void OnHoloColorRequest(HoloColorRequest request)
		{
			View.HoloColor = request.Color;
		}

		void OnTransitionFocusRequest(TransitionFocusRequest request)
		{
			switch (request.State)
			{
				case TransitionFocusRequest.States.Active:
					if (request.FirstActive) OnTransitionFocusActiveInitialize(request);
					else OnTransitionFocusActiveUpdate(request);
					break;
			}
		}

		void OnTransitionFocusActiveInitialize(TransitionFocusRequest request)
		{
			// Only if we're going from some other transition to Active is this run.

			var orderedTransitions = request.Transitions.OrderBy(t => t.Order);

			layerToOrder.Clear();
			var absoluteOrder = 0;

			var textures = new List<RenderLayerTextureBlock>();
			var properties = new List<RenderLayerPropertyBlock>();

			foreach (var transition in orderedTransitions)
			{
				DeliverFocusBlock delivery;
				if(request.GatherResult.GetDelivery(transition.Layer, out delivery)) 
				{
					if (delivery.Ignore) continue;
					textures.Add(new RenderLayerTextureBlock(absoluteOrder, delivery.Texture));
					var weight = request.LastActive ? transition.End.Weight : transition.Start.Weight;
					properties.Add(new RenderLayerPropertyBlock(absoluteOrder, weight));
				}

				layerToOrder.Add(transition.Layer, absoluteOrder);
				absoluteOrder++;
			}

			if ((ShaderConstants.RoomProjectionShared.LayerCount - absoluteOrder) != 0)
			{
				// Fill in the others with blanks.
				for (var i = absoluteOrder; i < ShaderConstants.RoomProjectionShared.MaxLayer; i++)
				{
					//textures.Add(new RenderLayerTextureBlock(i, Texture2D.blackTexture));
					properties.Add(new RenderLayerPropertyBlock(i, 0f));
				}
			}

			var wasClosed = View.TransitionState == TransitionStates.Closed;

			if (wasClosed) View.Reset();

			View.HoloColor = App.Callbacks.LastHoloColorRequest.Color;
			View.CameraPitch = App.V.CameraTransform.PitchValue();
			View.LayerTextures = textures.ToArray();
			View.LayerProperties = properties.ToArray();

			if (model != null) View.TimeScalar = model.Context.TransitState.Value.RelativeTimeScalar;

			if (wasClosed) ShowView(instant: true);
		}

		void OnTransitionFocusActiveUpdate(TransitionFocusRequest request)
		{
			var properties = new List<RenderLayerPropertyBlock>();
			foreach (var transition in request.Transitions)
			{
				int order;
				if (!layerToOrder.TryGetValue(transition.Layer, out order)) continue;
				var weight = ((transition.End.Weight - transition.Start.Weight) * request.Progress) + transition.Start.Weight;
				properties.Add(new RenderLayerPropertyBlock(order, weight));
			}
			View.LayerProperties = properties.ToArray();
		}

		void OnTransitState(TransitState transitState)
		{
			if (!View.Visible) return;
			View.TimeScalar = model.Context.TransitState.Value.RelativeTimeScalar;
		}
		#endregion
	}
}