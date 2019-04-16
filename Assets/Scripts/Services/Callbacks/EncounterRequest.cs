using System;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public struct EncounterRequest
	{
		public enum States
		{
			Unknown = 0,
			Request = 10,
			Handle = 20,
			Controls = 30,
			Next = 40,
			PrepareComplete = 50,
			Complete = 60
		}

		public static EncounterRequest Request(
			GameModel gameModel,
			EncounterInfoModel encounter,
			EncounterTriggers trigger
		)
		{
			return new EncounterRequest(
				States.Request,
				gameModel,
				encounter,
				trigger: trigger
			);
		}

		public static EncounterRequest Handle<T>(T model) where T : IEncounterHandlerModel
		{
			return new EncounterRequest(States.Handle, modelType: typeof(T), model: model);
		}

		public static EncounterRequest Controls(bool next, bool prepareCompleteControl)
		{
			return new EncounterRequest(States.Controls, next: next, prepareCompleteControl: prepareCompleteControl);
		}

		public static EncounterRequest Next()
		{
			return new EncounterRequest(States.Next);
		}

		public static EncounterRequest PrepareComplete(string synchronizedId)
		{
			return new EncounterRequest(States.PrepareComplete, synchronizedId: synchronizedId);
		}

		public static EncounterRequest Complete()
		{
			return new EncounterRequest(States.Complete);
		}

		public readonly States State;
		public readonly GameModel GameModel;
		public readonly EncounterInfoModel Encounter;
		public readonly Type ModelType;
		public readonly IEncounterHandlerModel Model;
		public readonly bool NextControl;
		public readonly bool PrepareCompleteControl;
		public readonly string SynchronizedId;
		public readonly EncounterTriggers Trigger;

		public EncounterLogTypes LogType { get { return Model == null ? EncounterLogTypes.Unknown : Model.LogType; } }

		public EncounterRequest(
			States state,
			GameModel gameModel = null,
			EncounterInfoModel encounter = null,
			Type modelType = null,
			IEncounterHandlerModel model = null,
			bool next = false,
			bool prepareCompleteControl = false,
			string synchronizedId = null,
			EncounterTriggers trigger = EncounterTriggers.Unknown
		)
		{
			State = state;
			GameModel = gameModel;
			Encounter = encounter;
			ModelType = modelType;
			Model = model;
			NextControl = next;
			PrepareCompleteControl = prepareCompleteControl;
			SynchronizedId = synchronizedId;
			Trigger = trigger;
		}

		/// <summary>
		/// Gets a correctly typed instance of the model if the provided type
		/// matches this request's model, and feeds it to the provided callback.
		/// Returns true if it was succesful.
		/// </summary>
		/// <returns><c>true</c>, if model was gotten, <c>false</c> otherwise.</returns>
		/// <param name="done">Done.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public bool TryHandle<T>(Action<T> done)
			where T : class, IEncounterHandlerModel
		{
			if (ModelType != typeof(T)) return false;
			done(Model as T);
			return true;
		}
	}
}