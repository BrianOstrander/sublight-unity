using System;

using UnityEngine;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public class KeyValueLogHandler : EncounterLogHandler<KeyValueEncounterLogModel>
	{
		public override EncounterLogTypes LogType { get { return EncounterLogTypes.KeyValue; } }

		public KeyValueLogHandler(EncounterLogHandlerConfiguration configuration) : base(configuration) { }

		protected override void OnHandle(
			KeyValueEncounterLogModel logModel,
			Action linearDone,
			Action<string> nonLinearDone
		)
		{
			var total = logModel.Operations.Value.Length;

			if (total == 0)
			{
				linearDone();
				return;
			}

			var progress = 0;

			foreach (var entry in logModel.Operations.Value)
			{
				switch (entry.Operation)
				{
					case KeyValueOperations.SetString:
						var setString = entry as SetStringOperationModel;
						Configuration.Callbacks.KeyValueRequest(
							KeyValueRequest.Set(
								entry.Target.Value,
								entry.Key.Value,
								setString.Value.Value,
								result => OnDone(result, total, ref progress, linearDone)
							)
						);
						break;
					case KeyValueOperations.SetBoolean:
						var setBoolean = entry as SetBooleanOperationModel;
						Configuration.Callbacks.KeyValueRequest(
							KeyValueRequest.Set(
								entry.Target.Value,
								entry.Key.Value,
								setBoolean.Value.Value,
								result => OnDone(result, total, ref progress, linearDone)
							)
						);
						break;
					default:
						Debug.LogError("Unrecognized KeyValueType: " + entry.Operation);
						linearDone();
						return;
				}
			}
		}

		void OnDone<T>(KeyValueResult<T> result, int total, ref int progress, Action done) where T : IConvertible
		{
			if (result.Status != RequestStatus.Success)
			{
				Debug.LogError("Setting " + result.TargetKey + " = " + result.Value + " returned with status: " + result.Status + " and error:\n" + result.Error);
				Debug.LogWarning("Continuing after this failure may result in unpredictable behaviour.");
			}
			progress++;
			if (total == progress) done();
		}
	}
}