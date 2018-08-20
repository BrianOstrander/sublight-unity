using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	// TODO: Remove "Request"
	public struct AssignBestEncounterRequest
	{
		public static AssignBestEncounterRequest ErrorResult(string error)
		{
			return new AssignBestEncounterRequest(RequestStatus.Failure, error, false, null, null, null);
		}

		public static AssignBestEncounterRequest NoEncounterResult(SystemModel system)
		{
			return new AssignBestEncounterRequest(RequestStatus.Success, null, false, null, system, null);
		}

		public static AssignBestEncounterRequest EncounterResult(SystemModel system, EncounterInfoModel encounter, BodyModel body)
		{
			return new AssignBestEncounterRequest(RequestStatus.Success, null, true, encounter, system, body);
		}

		public readonly RequestStatus Status;
		public readonly string Error;

		public readonly bool EncounterAssigned;
		public readonly EncounterInfoModel Encounter;
		public readonly SystemModel System;
		public readonly BodyModel Body;

		public AssignBestEncounterRequest(
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
