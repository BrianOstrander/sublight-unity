using UnityEngine;

using LunraGames.SubLight.Models;
using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class ModuleSwapPresenter : ShipFocusPresenter<IModuleSwapView>
	{
		GameModel model;
		ModuleSwapLanguageBlock language;
		
		public ModuleSwapPresenter(
			GameModel model,
			ModuleSwapLanguageBlock language
		)
		{
			this.model = model;
			this.language = language;

			App.Callbacks.EncounterRequest += OnEncounterRequest;
		}

		protected override void OnUnBind()
		{
			base.OnUnBind();
			
			App.Callbacks.EncounterRequest -= OnEncounterRequest;
		}
		
		protected override void OnUpdateEnabled()
		{
			
		}
		
		#region Callback Events
		void OnEncounterRequest(EncounterRequest request)
		{
			if (request.State != EncounterRequest.States.Handle || request.LogType != EncounterLogTypes.ModuleSwap) return;
			if (!request.TryHandle<ModuleSwapHandlerModel>(OnEncounterModuleSwapHandle)) Debug.LogError("Unable to handle specified model");
		}

		void OnEncounterModuleSwapHandle(ModuleSwapHandlerModel handler)
		{
			switch (handler.Log.Value.Style.Value)
			{
				case ModuleSwapEncounterLogModel.Styles.Derelict: break;
				case ModuleSwapEncounterLogModel.Styles.Instant:
					Debug.LogError("Instant Styles for module swaps should have been handled in the module handler. Skipping...");
					handler.Done.Value();
					return;
				default:
					Debug.LogError("Unrecognized Style for module swaps: " + handler.Log.Value.Style.Value + ", skipping...");
					handler.Done.Value();
					return;
			}

			View.Reset();

			Debug.Log("handle stuff here");

			// if (!View.Visible)
			// {
			// 	View.Shown += () => OnEncounterModuleSwapHandleViewShown(handler);
			// 	
			// }

		}

		void OnEncounterModuleSwapHandleViewShown(ModuleSwapHandlerModel handler)
		{
			
		}
		#endregion
		
		#region Model Events
		
		#endregion

		#region View Events
		
		#endregion
	}
}