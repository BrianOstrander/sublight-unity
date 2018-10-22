using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class HoloPresenter : Presenter<IHoloView>
	{
		TransitionFocusRequest.States lastTransition;
		Dictionary<SetFocusLayers, int> layerToOrder = new Dictionary<SetFocusLayers, int>();

		public HoloPresenter()
		{
			App.Callbacks.HoloColorRequest += OnHoloColorRequest;
			App.Callbacks.TransitionFocusRequest += OnTransitionFocusRequest;
		}

		protected override void OnUnBind()
		{
			App.Callbacks.HoloColorRequest -= OnHoloColorRequest;
			App.Callbacks.TransitionFocusRequest -= OnTransitionFocusRequest;
		}

		#region Events
		void OnHoloColorRequest(HoloColorRequest request)
		{
			View.HoloColor = request.Color;
		}

		void OnTransitionFocusRequest(TransitionFocusRequest request)
		{
			switch (request.State)
			{
				case TransitionFocusRequest.States.Active:
					if (lastTransition != request.State) OnTransitionFocusActiveInitialize(request);
					else OnTransitionFocusActiveUpdate(request);
					break;
			}
			lastTransition = request.State;
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

			View.LayerTextures = textures.ToArray();
			View.LayerProperties = properties.ToArray();

			//OnTransitionFocusActiveUpdate(request);

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
		#endregion
	}
}