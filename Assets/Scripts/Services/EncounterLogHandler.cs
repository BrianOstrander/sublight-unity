using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	// Must be a class to share the instance with all other handlers.
	public class EncounterLogHandlerConfiguration
	{
		public readonly CallbackService Callbacks;
		public readonly EncounterService EncounterService;
		public readonly KeyValueService KeyValueService;
		public readonly ValueFilterService ValueFilter;
		public readonly Func<PreferencesModel> CurrentPreferences;
		
		public GameModel Model;
		public EncounterInfoModel Encounter;

		public EncounterLogHandlerConfiguration(
			CallbackService callbacks,
			EncounterService encounterService,
			KeyValueService keyValueService,
			ValueFilterService valueFilter,
			Func<PreferencesModel> currentPreferences
		)
		{
			Callbacks = callbacks;
			EncounterService = encounterService;
			KeyValueService = keyValueService;
			ValueFilter = valueFilter;
			CurrentPreferences = currentPreferences;
		}
	}

	public abstract class EncounterLogHandler<T> : IEncounterLogHandler
		where T : EncounterLogModel
	{
		public abstract EncounterLogTypes LogType { get; }

		protected EncounterLogHandlerConfiguration Configuration { get; private set; }

		public EncounterLogHandler(EncounterLogHandlerConfiguration configuration) { Configuration = configuration; }

		public bool Handle(
			EncounterLogModel logModel,
			Action linearDone,
			Action<string> nonLinearDone
		)
		{
			if (logModel.LogType != LogType) return false;

			OnHandle(
				logModel as T,
				linearDone,
				nonLinearDone
			);

			return true;
		}

		protected abstract void OnHandle
		(
			T logModel,
			Action linearDone,
			Action<string> nonLinearDone
		);
	}

	public interface IEncounterLogHandler
	{
		bool Handle(
			EncounterLogModel logModel,
			Action linearDone,
			Action<string> nonLinearDone
		);
	}
}