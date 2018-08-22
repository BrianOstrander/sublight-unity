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
			Active = 20,
			Complete = 30
		}

		public static EncounterRequest Request(GameModel gameModel, string encounterId, UniversePosition systemPosition)
		{
			return new EncounterRequest(States.Request, gameModel, encounterId, systemPosition);
		}

		public static EncounterRequest Active<T>(
			GameModel gameModel,
			string encounterId,
			UniversePosition systemPosition,
			T model
		) where T : Model
		{
			return new EncounterRequest(States.Active, gameModel, encounterId, systemPosition, typeof(T), model);
		}

		public static EncounterRequest Complete(GameModel gameModel, string encounterId, UniversePosition systemPosition)
		{
			return new EncounterRequest(States.Complete, gameModel, encounterId, systemPosition);
		}

		public readonly States State;
		public readonly GameModel GameModel;
		public readonly string EncounterId;
		public readonly UniversePosition SystemPosition;
		public readonly Type ModelType;
		public readonly Model Model;

		public EncounterRequest(
			States state,
			GameModel gameModel,
			string encounterId,
			UniversePosition systemPosition,
			Type modelType = null,
			Model model = null
		)
		{
			State = state;
			GameModel = gameModel;
			EncounterId = encounterId;
			SystemPosition = systemPosition;
			ModelType = modelType;
			Model = model;
		}

		/// <summary>
		/// Gets a correctly typed instance of the model if the provided type
		/// matches this request's model. Returns true if it was succesful.
		/// </summary>
		/// <returns><c>true</c>, if model was gotten, <c>false</c> otherwise.</returns>
		/// <param name="model">Model.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public bool GetModel<T>(out T model) where T : Model
		{
			model = default(T);
			if (ModelType != typeof(T)) return false;
			model = Model as T;
			return true;
		}
	}
}