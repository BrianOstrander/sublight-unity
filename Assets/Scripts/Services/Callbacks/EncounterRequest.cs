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
			Done = 50,
			Complete = 60
		}

		public static EncounterRequest Request(GameModel gameModel, string encounterId, UniversePosition sectorPosition, int systemIndex)
		{
			return new EncounterRequest(States.Request, gameModel, encounterId, sectorPosition, systemIndex);
		}

		public static EncounterRequest Handle<T>(T model) where T : IEncounterHandlerModel
		{
			return new EncounterRequest(States.Handle, modelType: typeof(T), model: model);
		}

		public static EncounterRequest Controls(bool next, bool done)
		{
			return new EncounterRequest(States.Controls, next: next, done: done);
		}

		public static EncounterRequest Next()
		{
			return new EncounterRequest(States.Next);
		}

		public static EncounterRequest Done()
		{
			return new EncounterRequest(States.Done);
		}

		public static EncounterRequest Complete()
		{
			return new EncounterRequest(States.Complete);
		}

		public readonly States State;
		public readonly GameModel GameModel;
		public readonly string EncounterId;
		public readonly UniversePosition SectorPosition;
		public readonly int SystemIndex;
		public readonly Type ModelType;
		public readonly IEncounterHandlerModel Model;
		public readonly bool NextControl;
		public readonly bool DoneControl;

		public EncounterRequest(
			States state,
			GameModel gameModel = null,
			string encounterId = null,
			UniversePosition sectorPosition = default(UniversePosition),
			int systemIndex = -1,
			Type modelType = null,
			IEncounterHandlerModel model = null,
			bool next = false,
			bool done = false
		)
		{
			State = state;
			GameModel = gameModel;
			EncounterId = encounterId;
			SectorPosition = sectorPosition;
			SystemIndex = systemIndex;
			ModelType = modelType;
			Model = model;
			NextControl = next;
			DoneControl = done;
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