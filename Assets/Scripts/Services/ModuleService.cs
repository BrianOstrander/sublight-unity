using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using LunraGames.NumberDemon;
using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public abstract class ModuleService : IModuleService
	{
		protected readonly IModelMediator ModelMediator;
		protected readonly GameModel Model;

		public ModuleService(
			IModelMediator modelMediator,
			GameModel model
		)
		{
			ModelMediator = modelMediator ?? throw new ArgumentNullException(nameof(modelMediator));
			Model = model ?? throw new ArgumentNullException(nameof(model));
		}
		
		#region Initialization
		public void Initialize(
			Action<RequestStatus> done
		)
		{
			// ModelMediator.List<ModuleTraitModel>(result => OnListEncounters(result, done));
		}
		
		// void OnInitializeListTraits(
		// 	SaveLoadArrayRequest<SaveModel> 
		// 	
		// )
		#endregion
	}

	public interface IModuleService
	{
		
	}
}