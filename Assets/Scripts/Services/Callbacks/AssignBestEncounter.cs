using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	// TODO: Remove "Request"
	public struct AssignBestEncounter
	{
		public static AssignBestEncounter ErrorResult(string error)
		{
			return new AssignBestEncounter(RequestStatus.Failure, error, false, null);
		}

		public static AssignBestEncounter NoEncounterResult()
		{
			return new AssignBestEncounter(RequestStatus.Success, null, false, null);
		}

		public static AssignBestEncounter EncounterResult(EncounterInfoModel encounter)
		{
			return new AssignBestEncounter(RequestStatus.Success, null, true, encounter);
		}

		public readonly RequestStatus Status;
		public readonly string Error;

		public readonly bool EncounterAssigned;
		public readonly EncounterInfoModel Encounter;

		public AssignBestEncounter(
			RequestStatus status,
			string error,
			bool encounterAssigned,
			EncounterInfoModel encounter
		)
		{
			Status = status;
			Error = error;

			EncounterAssigned = encounterAssigned;
			Encounter = encounter;
		}
	}
}
