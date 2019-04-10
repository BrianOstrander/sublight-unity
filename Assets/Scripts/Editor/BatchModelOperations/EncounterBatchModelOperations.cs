using System;

using UnityEngine;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public static class EncounterBatchModelOperations
	{
		[BatchModelOperation(typeof(EncounterInfoModel))]
		static void UpdateVersions(
			EncounterInfoModel model,
			Action<EncounterInfoModel, RequestResult> done
		)
		{
			var message = string.Empty;

			if (model.Version.Value != BuildPreferences.Instance.Info.Version)
			{
				message = "Updated version of " + model.Name.Value + " from " + model.Version.Value + " to " + BuildPreferences.Instance.Info.Version;
			}

			done(model, RequestResult.Success(message));
		}
	}
}