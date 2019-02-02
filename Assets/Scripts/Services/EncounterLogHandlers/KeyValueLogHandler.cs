using System;
using System.Linq;

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
			var operations = logModel.Edges.Where(e => !e.Ignore.Value).OrderBy(e => e.Index.Value).Select(e => e.Entry).ToArray();

			var total = operations.Length;

			if (total == 0)
			{
				linearDone();
				return;
			}

			var progress = 0;

			foreach (var operation in operations)
			{
				switch (operation.Operation.Value)
				{
					case KeyValueOperations.SetString:
						Configuration.Callbacks.KeyValueRequest(
							KeyValueRequest.Set(
								operation.Target.Value,
								operation.Key.Value,
								operation.StringValue.Value,
								result => OnDone(result, total, ref progress, linearDone)
							)
						);
						break;
					case KeyValueOperations.SetBoolean:
						Configuration.Callbacks.KeyValueRequest(
							KeyValueRequest.Set(
								operation.Target.Value,
								operation.Key.Value,
								operation.BoolValue.Value,
								result => OnDone(result, total, ref progress, linearDone)
							)
						);
						break;
					default:
						Debug.LogError("Unrecognized KeyValueType: " + operation.Operation);
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