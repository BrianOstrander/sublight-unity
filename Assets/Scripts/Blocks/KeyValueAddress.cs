using System;

using UnityEngine;

using Newtonsoft.Json;

namespace LunraGames.SubLight
{
	public interface IKeyValueAddress
	{
		string AddressId { get; set; }
		KeyValueSources Source { get; set; }
		KeyValueTargets ForeignTarget { get; set; }
		string ForeignKey { get; set; }
		object LocalValueRaw { get; set; }
	}

	[Serializable]
	public struct KeyValueAddress<T> : IKeyValueAddress
		where T : IConvertible
	{
		public static KeyValueAddress<T> Default { get { return Local(); } }

		public static KeyValueAddress<T> Local(T value = default(T))
		{
			return new KeyValueAddress<T>(
				KeyValueSources.LocalValue,
				KeyValueTargets.Unknown,
				null,

				value
			);
		}

		public static KeyValueAddress<T> Foreign(
			KeyValueTargets target = KeyValueTargets.Unknown,
			string key = null
		)
		{
			return new KeyValueAddress<T>(
				KeyValueSources.KeyValue,
				target,
				key,

				default(T)
			);
		}

		public string AddressId { get; set; }
		public KeyValueSources Source { get; set; }
		public KeyValueTargets ForeignTarget { get; set; }
		public string ForeignKey { get; set; }

		public T LocalValue { get; set; }

		[JsonIgnore]
		public object LocalValueRaw
		{
			get { return LocalValue; }
			set { LocalValue = value == null ? default(T) : (T)value; }
		}

		[JsonIgnore]
		public KeyValueTypes KeyValueType
		{
			get
			{
				var currType = typeof(T);
				if (currType == typeof(bool)) return KeyValueTypes.Boolean;
				if (currType == typeof(int)) return KeyValueTypes.Integer;
				if (currType == typeof(string)) return KeyValueTypes.String;
				if (currType == typeof(float)) return KeyValueTypes.Float;

				Debug.LogError("Unrecognized KeyValueAddress Type: " + currType.FullName);
				return KeyValueTypes.Unknown;
			}
		}

		KeyValueAddress(
			KeyValueSources source,
			KeyValueTargets foreignTarget,
			string foreignKey,

			T localValue
		)
		{
			AddressId = Guid.NewGuid().ToString();
			Source = source;
			ForeignTarget = foreignTarget;
			ForeignKey = foreignKey;

			LocalValue = localValue;
		}

		public void Get(
			Action<KeyValueRequest> requestCallback,
			Action<KeyValueResult<T>> done
		)
		{
			if (requestCallback == null) throw new ArgumentNullException("requestCallback");
			if (done == null) throw new ArgumentNullException("done");

			switch (Source)
			{
				case KeyValueSources.LocalValue:
					done(new KeyValueResult<T>(Source, KeyValueTargets.Unknown, null, LocalValue));
					break;
				case KeyValueSources.KeyValue:
					var source = Source;
					var foreignTarget = ForeignTarget;
					var foreignKey = ForeignKey;
					switch (KeyValueType)
					{
						case KeyValueTypes.Boolean:
							requestCallback(
								KeyValueRequest.GetBoolean(
									ForeignTarget,
									ForeignKey,
									result => done(new KeyValueResult<T>(source, foreignTarget, foreignKey, (T)(object)result.Value, result.Status, result.Error))
								)
							);
							break;
						case KeyValueTypes.Integer:
							requestCallback(
								KeyValueRequest.GetInteger(
									ForeignTarget,
									ForeignKey,
									result => done(new KeyValueResult<T>(source, foreignTarget, foreignKey, (T)(object)result.Value, result.Status, result.Error))
								)
							);
							break;
						case KeyValueTypes.String:
							requestCallback(
								KeyValueRequest.GetString(
									ForeignTarget,
									ForeignKey,
									result => done(new KeyValueResult<T>(source, foreignTarget, foreignKey, (T)(object)result.Value, result.Status, result.Error))
								)
							);
							break;
						case KeyValueTypes.Float:
							requestCallback(
								KeyValueRequest.GetFloat(
									ForeignTarget,
									ForeignKey,
									result => done(new KeyValueResult<T>(source, foreignTarget, foreignKey, (T)(object)result.Value, result.Status, result.Error))
								)
							);
							break;
						default:
							var keyValueTypeError = "Unrecognized KeyValueType: " + KeyValueType;
							Debug.LogError(keyValueTypeError);
							done(new KeyValueResult<T>(Source, ForeignTarget, ForeignKey, default(T), RequestStatus.Failure, keyValueTypeError));
							break;
					}
					break;
				default:
					var sourceError = "Unrecognized Source: " + Source;
					Debug.LogError(sourceError);
					done(new KeyValueResult<T>(Source, ForeignTarget, ForeignKey, default(T), RequestStatus.Failure, sourceError));
					break;
			}
		}

		public void Set(
			Action<KeyValueRequest> requestCallback,
			Action<KeyValueResult<T>> done,
			T value
		)
		{
			if (requestCallback == null) throw new ArgumentNullException("requestCallback");
			if (done == null) throw new ArgumentNullException("done");

			switch (Source)
			{
				case KeyValueSources.LocalValue:
					LocalValue = value;
					done(new KeyValueResult<T>(Source, KeyValueTargets.Unknown, null, LocalValue));
					break;
				case KeyValueSources.KeyValue:
					var source = Source;
					var foreignTarget = ForeignTarget;
					var foreignKey = ForeignKey;
					switch (KeyValueType)
					{
						case KeyValueTypes.Boolean:
							requestCallback(
								KeyValueRequest.SetBoolean(
									ForeignTarget,
									ForeignKey,
									Convert.ToBoolean(value),
									result => done(new KeyValueResult<T>(source, foreignTarget, foreignKey, (T)(object)result.Value, result.Status, result.Error))
								)
							);
							break;
						case KeyValueTypes.Integer:
							requestCallback(
								KeyValueRequest.SetInteger(
									ForeignTarget,
									ForeignKey,
									Convert.ToInt32(value),
									result => done(new KeyValueResult<T>(source, foreignTarget, foreignKey, (T)(object)result.Value, result.Status, result.Error))
								)
							);
							break;
						case KeyValueTypes.String:
							requestCallback(
								KeyValueRequest.SetString(
									ForeignTarget,
									ForeignKey,
									Convert.ToString(value),
									result => done(new KeyValueResult<T>(source, foreignTarget, foreignKey, (T)(object)result.Value, result.Status, result.Error))
								)
							);
							break;
						case KeyValueTypes.Float:
							requestCallback(
								KeyValueRequest.SetFloat(
									ForeignTarget,
									ForeignKey,
									Convert.ToSingle(value),
									result => done(new KeyValueResult<T>(source, foreignTarget, foreignKey, (T)(object)result.Value, result.Status, result.Error))
								)
							);
							break;
						default:
							var keyValueTypeError = "Unrecognized KeyValueType: " + KeyValueType;
							Debug.LogError(keyValueTypeError);
							done(new KeyValueResult<T>(Source, ForeignTarget, ForeignKey, default(T), RequestStatus.Failure, keyValueTypeError));
							break;
					}
					break;
				default:
					var sourceError = "Unrecognized Source: " + Source;
					Debug.LogError(sourceError);
					done(new KeyValueResult<T>(Source, ForeignTarget, ForeignKey, default(T), RequestStatus.Failure, sourceError));
					break;
			}
		}
	}
}