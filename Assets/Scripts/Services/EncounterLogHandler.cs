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

		protected void FilterFirst<E>(
			Action<RequestStatus, E> done,
			Func<E, ValueFilterModel> getFilter,
			params E[] remaining
		)
		{
			if (done == null) throw new ArgumentNullException("done");
			if (getFilter == null) throw new ArgumentNullException("getFilter");

			if (remaining == null || remaining.Length == 0)
			{
				done(RequestStatus.Failure, default(E));
				return;
			}

			OnFilterFirst(
				done,
				getFilter,
				remaining.ToList()
			);
		}

		void OnFilterFirst<E>(
			Action<RequestStatus, E> done,
			Func<E, ValueFilterModel> getFilter,
			List<E> remaining,
			E target = default(E),
			bool? result = null
		)
		{
			if (result.HasValue && result.Value)
			{
				done(RequestStatus.Success, target);
				return;
			}

			if (remaining.None())
			{
				done(RequestStatus.Failure, default(E));
				return;
			}

			target = remaining.First();
			remaining.RemoveAt(0);

			Configuration.ValueFilter.Filter(
				filterResult => OnFilterFirst(done, getFilter, remaining, target, filterResult),
				getFilter(target),
				Configuration.Model,
				Configuration.Encounter
			);
		}

		protected void FilterAll<E>(
			Action<RequestStatus, E[]> done,
			Func<E, ValueFilterModel> getFilter,
			params E[] remaining
		)
		{
			if (done == null) throw new ArgumentNullException("done");
			if (getFilter == null) throw new ArgumentNullException("getFilter");

			if (remaining == null || remaining.Length == 0)
			{
				done(RequestStatus.Failure, new E[0]);
				return;
			}

			OnFilterAll(
				done,
				getFilter,
				remaining.ToList(),
				new List<E>()
			);
		}

		void OnFilterAll<E>(
			Action<RequestStatus, E[]> done,
			Func<E, ValueFilterModel> getFilter,
			List<E> remaining,
			List<E> passed,
			E target = default(E),
			bool? result = null
		)
		{
			if (result.HasValue && result.Value)
			{
				passed.Add(target);
			}

			if (remaining.None())
			{
				if (0 < passed.Count) done(RequestStatus.Success, passed.ToArray());
				else done(RequestStatus.Failure, new E[0]);
				return;
			}

			target = remaining.First();
			remaining.RemoveAt(0);

			Configuration.ValueFilter.Filter(
				filterResult => OnFilterAll(done, getFilter, remaining, passed, target, filterResult),
				getFilter(target),
				Configuration.Model,
				Configuration.Encounter
			);
		}
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