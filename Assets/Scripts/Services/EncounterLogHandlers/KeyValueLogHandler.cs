using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using LunraGames.SubLight.Models;

using BooleanBlock = LunraGames.SubLight.Models.KeyValueEntryModel.BooleanBlock;
using IntegerBlock = LunraGames.SubLight.Models.KeyValueEntryModel.IntegerBlock;
using StringBlock = LunraGames.SubLight.Models.KeyValueEntryModel.StringBlock;
using FloatBlock = LunraGames.SubLight.Models.KeyValueEntryModel.FloatBlock;

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
			var operations = logModel.Edges.Where(e => !e.Ignore.Value).OrderBy(e => e.Index.Value).Select(e => e.Entry).ToList();

			OnHandleNextOperation(operations, linearDone);
		}

		void OnHandleNextOperation(
			List<KeyValueEntryModel> remaining,
			Action done
		)
		{
			if (remaining.None())
			{
				done();
				return;
			}

			var next = remaining.First();
			remaining.RemoveAt(0);

			Action onNext = () => OnHandleNextOperation(remaining, done);

			switch (next.KeyValueType.Value)
			{
				case KeyValueTypes.Boolean: OnHandleInputs(next.BooleanValue.Value, onNext); break;
				case KeyValueTypes.Integer: OnHandleInputs(next.IntegerValue.Value, onNext); break;
				case KeyValueTypes.String: OnHandleInputs(next.StringValue.Value, onNext); break;
				case KeyValueTypes.Float: OnHandleInputs(next.FloatValue.Value, onNext); break;
				default:
					Debug.LogError("Unrecognized KeyValueType: " + next.KeyValueType.Value);
					onNext();
					break;
			}
		}

		#region Inputs
		void OnHandleInputs(
			BooleanBlock block,
			Action next
		)
		{
			block.Input0.Get(
				Configuration.Callbacks.KeyValueRequest,
				input0 => block.Input1.Get(
					Configuration.Callbacks.KeyValueRequest,
					input1 => OnHandleOperation(
						next,
						block,
						block.Output,
						input0,
						input1
					)
				)
			);
		}

		void OnHandleInputs(
			IntegerBlock block,
			Action next
		)
		{
			block.MinimumClamping.Get(
				Configuration.Callbacks.KeyValueRequest,
				minimumClamping => block.MaximumClamping.Get(
					Configuration.Callbacks.KeyValueRequest,
					maximumClamping => block.Input0.Get(
						Configuration.Callbacks.KeyValueRequest,
						input0 => block.Input1.Get(
							Configuration.Callbacks.KeyValueRequest,
							input1 => OnHandleOperation(
								next,
								block,
								block.Output,
								minimumClamping,
								maximumClamping,
								input0,
								input1
							)
						)
					)
				)
			);
		}

		void OnHandleInputs(
			StringBlock block,
			Action next
		)
		{
			block.Input0.Get(
				Configuration.Callbacks.KeyValueRequest,
				input0 => OnHandleOperation(
					next,
					block,
					block.Output,
					input0
				)
			);
		}

		void OnHandleInputs(
			FloatBlock block,
			Action next
		)
		{
			block.MinimumClamping.Get(
				Configuration.Callbacks.KeyValueRequest,
				minimumClamping => block.MaximumClamping.Get(
					Configuration.Callbacks.KeyValueRequest,
					maximumClamping => block.Input0.Get(
						Configuration.Callbacks.KeyValueRequest,
						input0 => block.Input1.Get(
							Configuration.Callbacks.KeyValueRequest,
							input1 => OnHandleOperation(
								next,
								block,
								block.Output,
								minimumClamping,
								maximumClamping,
								input0,
								input1
							)
						)
					)
				)
			);
		}
		#endregion

		bool CheckForErrors(
			Action next,
			IKeyValueAddress output,
			params IKeyValueResult[] results
		)
		{
			try
			{
				switch (output.Source)
				{
					case KeyValueSources.LocalValue:
						throw new Exception("Cannot have a local KeyValueAddress as output, skipping...");
					case KeyValueSources.KeyValue:
						if (string.IsNullOrEmpty(output.ForeignKey))
						{
							throw new Exception("Cannot have null or empty ForeignKey with Source." + output.Source + ", skipping...");
						}
						if (output.ForeignTarget == KeyValueTargets.Unknown)
						{
							throw new Exception("Cannot have ForeignTarget." + output.ForeignTarget + " with Source." + output.Source + ", skipping...");
						}
						break;
					default:
						throw new Exception("Unrecognized Source: " + output.Source + ", skipping...");
				}

				foreach (var result in results)
				{
					if (result.GenericResult.IsNotSuccess)
					{
						throw new Exception("KeyValue request resulted in error, skipping. Error: " + result.GenericResult.Message);
					}
				}
			}
			catch (Exception e)
			{
				Debug.LogException(e);
				next();
				return true;
			}
			return false;
		}

		#region Operations
		void OnHandleOperation(
			Action next,
			BooleanBlock block,
			KeyValueAddress<bool> output,
			KeyValueResult<bool> input0,
			KeyValueResult<bool> input1
		)
		{
			if (CheckForErrors(next, output, input0, input1)) return;

			var outputValue = false;

			switch (block.Operation)
			{
				case BooleanBlock.Operations.Set:
					outputValue = input0.Value;
					break;
				case BooleanBlock.Operations.And:
					outputValue = input0.Value && input1.Value;
					break;
				case BooleanBlock.Operations.Or:
					outputValue = input0.Value || input1.Value;
					break;
				case BooleanBlock.Operations.Xor:
					outputValue = input0.Value ^ input1.Value;
					break;
				default:
					Debug.LogError("Unrecognized Operation: " + block.Operation.GetType().FullName + "." + block.Operation + ", skipping...");
					next();
					return;
			}

			output.Set(Configuration.Callbacks.KeyValueRequest, result => OnHandleSetOutput(next, result.GenericResult), outputValue);
		}

		void OnHandleOperation(
			Action next,
			IntegerBlock block,
			KeyValueAddress<int> output,
			KeyValueResult<int> minimumClamping,
			KeyValueResult<int> maximumClamping,
			KeyValueResult<int> input0,
			KeyValueResult<int> input1
		)
		{
			if (CheckForErrors(next, output, minimumClamping, maximumClamping, input0, input1)) return;

			var outputValue = 0;

			switch (block.Operation)
			{
				case IntegerBlock.Operations.Set:
					outputValue = input0.Value;
					break;
				case IntegerBlock.Operations.Add:
					outputValue = input0.Value + input1.Value;
					break;
				case IntegerBlock.Operations.Subtract:
					outputValue = input0.Value - input1.Value;
					break;
				case IntegerBlock.Operations.Multiply:
					outputValue = input0.Value * input1.Value;
					break;
				case IntegerBlock.Operations.Divide:
					outputValue = input0.Value / input1.Value;
					break;
				case IntegerBlock.Operations.Modulo:
					outputValue = input0.Value % input1.Value;
					break;
				case IntegerBlock.Operations.Clamp:
					outputValue = input0.Value;
					break;
				default:
					Debug.LogError("Unrecognized Operation: " + block.Operation.GetType().FullName + "." + block.Operation + ", skipping...");
					next();
					return;
			}

			if (block.MinimumClampingEnabled && block.MaximumClampingEnabled)
			{
				outputValue = Mathf.Clamp(outputValue, Mathf.Min(minimumClamping.Value, maximumClamping.Value), Mathf.Max(minimumClamping.Value, maximumClamping.Value));
			}
			else if (block.MinimumClampingEnabled) outputValue = Mathf.Max(outputValue, minimumClamping.Value);
			else if (block.MaximumClampingEnabled) outputValue = Mathf.Min(outputValue, maximumClamping.Value);

			output.Set(Configuration.Callbacks.KeyValueRequest, result => OnHandleSetOutput(next, result.GenericResult), outputValue);
		}

		void OnHandleOperation(
			Action next,
			StringBlock block,
			KeyValueAddress<string> output,
			KeyValueResult<string> input0
		)
		{
			if (CheckForErrors(next, output, input0)) return;

			var outputValue = string.Empty;

			switch (block.Operation)
			{
				case StringBlock.Operations.Set:
					outputValue = input0.Value;
					break;
				default:
					Debug.LogError("Unrecognized Operation: " + block.Operation.GetType().FullName + "." + block.Operation + ", skipping...");
					next();
					return;
			}

			output.Set(Configuration.Callbacks.KeyValueRequest, result => OnHandleSetOutput(next, result.GenericResult), outputValue);
		}

		void OnHandleOperation(
			Action next,
			FloatBlock block,
			KeyValueAddress<float> output,
			KeyValueResult<float> minimumClamping,
			KeyValueResult<float> maximumClamping,
			KeyValueResult<float> input0,
			KeyValueResult<float> input1
		)
		{
			if (CheckForErrors(next, output, minimumClamping, maximumClamping, input0, input1)) return;

			var outputValue = 0f;

			switch (block.Operation)
			{
				case FloatBlock.Operations.Set:
					outputValue = input0.Value;
					break;
				case FloatBlock.Operations.Add:
					outputValue = input0.Value + input1.Value;
					break;
				case FloatBlock.Operations.Subtract:
					outputValue = input0.Value - input1.Value;
					break;
				case FloatBlock.Operations.Multiply:
					outputValue = input0.Value * input1.Value;
					break;
				case FloatBlock.Operations.Divide:
					outputValue = input0.Value / input1.Value;
					break;
				case FloatBlock.Operations.Modulo:
					outputValue = input0.Value % input1.Value;
					break;
				case FloatBlock.Operations.Clamp:
					outputValue = input0.Value;
					break;
				case FloatBlock.Operations.Round:
					outputValue = Mathf.Round(input0.Value);
					break;
				case FloatBlock.Operations.Floor:
					outputValue = Mathf.Floor(input0.Value);
					break;
				case FloatBlock.Operations.Ceiling:
					outputValue = Mathf.Ceil(input0.Value);
					break;
				default:
					Debug.LogError("Unrecognized Operation: " + block.Operation.GetType().FullName + "." + block.Operation + ", skipping...");
					next();
					return;
			}

			if (block.MinimumClampingEnabled && block.MaximumClampingEnabled)
			{
				outputValue = Mathf.Clamp(outputValue, Mathf.Min(minimumClamping.Value, maximumClamping.Value), Mathf.Max(minimumClamping.Value, maximumClamping.Value));
			}
			else if (block.MinimumClampingEnabled) outputValue = Mathf.Max(outputValue, minimumClamping.Value);
			else if (block.MaximumClampingEnabled) outputValue = Mathf.Min(outputValue, maximumClamping.Value);

			output.Set(Configuration.Callbacks.KeyValueRequest, result => OnHandleSetOutput(next, result.GenericResult), outputValue);
		}
		#endregion

		void OnHandleSetOutput(
			Action next,
			RequestResult result
		)
		{
			if (result.IsNotSuccess) Debug.LogError("Setting KeyValue output returned error: " + result.Message);
			next();
		}

		/*
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
		*/
	}
}