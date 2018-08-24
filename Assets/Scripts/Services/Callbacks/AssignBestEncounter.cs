using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	// TODO: Remove "Request"
	public struct AssignBestEncounter
	{
		public static AssignBestEncounter ErrorResult(string error)
		{
			return new AssignBestEncounter(RequestStatus.Failure, error, false, null, null, null);
		}

		public static AssignBestEncounter NoEncounterResult(SystemModel system)
		{
			return new AssignBestEncounter(RequestStatus.Success, null, false, null, system, null);
		}

		public static AssignBestEncounter EncounterResult(SystemModel system, EncounterInfoModel encounter, BodyModel body)
		{
			return new AssignBestEncounter(RequestStatus.Success, null, true, encounter, system, body);
		}

		public readonly RequestStatus Status;
		public readonly string Error;

		public readonly bool EncounterAssigned;
		public readonly EncounterInfoModel Encounter;
		public readonly SystemModel System;
		public readonly BodyModel Body;

		public AssignBestEncounter(
			RequestStatus status,
			string error,
			bool encounterAssigned,
			EncounterInfoModel encounter,
			SystemModel system,
			BodyModel body
		)
		{
			Status = status;
			Error = error;

			EncounterAssigned = encounterAssigned;
			Encounter = encounter;
			System = system;
			Body = body;
		}
	}
}
