using System;
using UnityEngine;

namespace LunraGames.SubLight
{
	public interface IKeyValueResult
	{
		RequestResult GenericResult { get; }
	}

	public struct KeyValueResult<T> : IKeyValueResult
		where T : IConvertible
	{
		public readonly KeyValueSources Source;
		public readonly KeyValueTargets Target;
		public readonly string Key;
		public readonly T Value;
		public readonly RequestStatus Status;
		public readonly string Error;
		public readonly string TargetKey;

		public RequestResult GenericResult { get { return new RequestResult(Status, Error); } }

		public KeyValueResult(
			KeyValueSources source,
			KeyValueTargets target,
			string key,
			T value,
			RequestStatus status = RequestStatus.Success,
			string error = null
		)
		{
			Source = source;
			Target = target;
			Key = key;
			Value = value;
			Status = status;
			Error = error;
			TargetKey = Target + Key;
		}

		public KeyValueResult(
			KeyValueRequest request,
			string error
		)
		{
			Source = KeyValueSources.KeyValue;
			Target = request.Target;
			Key = request.Key;
			Status = RequestStatus.Failure;
			Error = error;
			TargetKey = Target + Key;

			switch(request.ValueType)
			{
				case KeyValueTypes.Boolean:
					Value = (T)(object)request.BooleanValue;
					break;
				case KeyValueTypes.Integer:
					Value = (T)(object)request.IntegerValue;
					break;
				case KeyValueTypes.String:
					Value = (T)(object)request.StringValue;
					break;
				case KeyValueTypes.Float:
					Value = (T)(object)request.FloatValue;
					break;
				default:
					Debug.LogError("Unrecognized ValueType: " + request.ValueType);
					Value = default(T);
					break;
			}
		}
	}

	public struct KeyValueRequest
	{
		public enum States
		{
			Unknown = 0,
			GetRequest = 10,
			SetRequest = 20,
			// TODO: Add these callbacks when requests are done?
			//GetComplete = 30,
			//SetComplete = 40
		}

		#region Defined Key Get & Set
		public static KeyValueRequest SetDefined(KeyDefinitions.Boolean definedKey, bool value, Action<KeyValueResult<bool>> done = null)
		{
			return new KeyValueRequest(definedKey.Target, definedKey.Key, States.SetRequest, KeyValueTypes.Boolean, booleanValue: value, booleanDone: done);
		}

		public static KeyValueRequest SetDefined(KeyDefinitions.Integer definedKey, int value, Action<KeyValueResult<int>> done = null)
		{
			return new KeyValueRequest(definedKey.Target, definedKey.Key, States.SetRequest, KeyValueTypes.Integer, integerValue: value, integerDone: done);
		}

		public static KeyValueRequest SetDefined(KeyDefinitions.String definedKey, string value, Action<KeyValueResult<string>> done = null)
		{
			return new KeyValueRequest(definedKey.Target, definedKey.Key, States.SetRequest, KeyValueTypes.String, stringValue: value, stringDone: done);
		}

		public static KeyValueRequest SetDefined(KeyDefinitions.Float definedKey, float value, Action<KeyValueResult<float>> done = null)
		{
			return new KeyValueRequest(definedKey.Target, definedKey.Key, States.SetRequest, KeyValueTypes.Float, floatValue: value, floatDone: done);
		}

		public static KeyValueRequest GetDefined(KeyDefinitions.Boolean definedKey, Action<KeyValueResult<bool>> done)
		{
			return new KeyValueRequest(definedKey.Target, definedKey.Key, States.GetRequest, KeyValueTypes.Boolean, booleanDone: done);
		}

		public static KeyValueRequest GetDefined(KeyDefinitions.Integer definedKey, Action<KeyValueResult<int>> done)
		{
			return new KeyValueRequest(definedKey.Target, definedKey.Key, States.GetRequest, KeyValueTypes.Integer, integerDone: done);
		}

		public static KeyValueRequest GetDefined(KeyDefinitions.String definedKey, Action<KeyValueResult<string>> done)
		{
			return new KeyValueRequest(definedKey.Target, definedKey.Key, States.GetRequest, KeyValueTypes.String, stringDone: done);
		}

		public static KeyValueRequest GetDefined(KeyDefinitions.Float definedKey, Action<KeyValueResult<float>> done)
		{
			return new KeyValueRequest(definedKey.Target, definedKey.Key, States.GetRequest, KeyValueTypes.Float, floatDone: done);
		}
		#endregion

		#region String Key Get & Set
		public static KeyValueRequest Set(KeyValueTargets target, string key, bool value, Action<KeyValueResult<bool>> done = null)
		{
			return new KeyValueRequest(target, key, States.SetRequest, KeyValueTypes.Boolean, booleanValue: value, booleanDone: done);
		}

		public static KeyValueRequest Set(KeyValueTargets target, string key, int value, Action<KeyValueResult<int>> done = null)
		{
			return new KeyValueRequest(target, key, States.SetRequest, KeyValueTypes.Integer, integerValue: value, integerDone: done);
		}

		public static KeyValueRequest Set(KeyValueTargets target, string key, string value, Action<KeyValueResult<string>> done = null)
		{
			return new KeyValueRequest(target, key, States.SetRequest, KeyValueTypes.String, stringValue: value, stringDone: done);
		}

		public static KeyValueRequest Set(KeyValueTargets target, string key, float value, Action<KeyValueResult<float>> done = null)
		{
			return new KeyValueRequest(target, key, States.SetRequest, KeyValueTypes.Float, floatValue: value, floatDone: done);
		}

		public static KeyValueRequest Get(KeyValueTargets target, string key, Action<KeyValueResult<bool>> done)
		{
			return new KeyValueRequest(target, key, States.GetRequest, KeyValueTypes.Boolean, booleanDone: done);
		}

		public static KeyValueRequest Get(KeyValueTargets target, string key, Action<KeyValueResult<int>> done)
		{
			return new KeyValueRequest(target, key, States.GetRequest, KeyValueTypes.Integer, integerDone: done);
		}

		public static KeyValueRequest Get(KeyValueTargets target, string key, Action<KeyValueResult<string>> done) 
		{ 
			return new KeyValueRequest(target, key, States.GetRequest, KeyValueTypes.String, stringDone: done); 
		}

		public static KeyValueRequest Get(KeyValueTargets target, string key, Action<KeyValueResult<float>> done)
		{
			return new KeyValueRequest(target, key, States.GetRequest, KeyValueTypes.Float, floatDone: done);
		}
		#endregion

		#region String Key Get & Set Explicit
		public static KeyValueRequest SetBoolean(KeyValueTargets target, string key, bool value, Action<KeyValueResult<bool>> done = null)
		{
			return new KeyValueRequest(target, key, States.SetRequest, KeyValueTypes.Boolean, booleanValue: value, booleanDone: done);
		}

		public static KeyValueRequest SetInteger(KeyValueTargets target, string key, int value, Action<KeyValueResult<int>> done = null)
		{
			return new KeyValueRequest(target, key, States.SetRequest, KeyValueTypes.Integer, integerValue: value, integerDone: done);
		}

		public static KeyValueRequest SetString(KeyValueTargets target, string key, string value, Action<KeyValueResult<string>> done = null)
		{
			return new KeyValueRequest(target, key, States.SetRequest, KeyValueTypes.String, stringValue: value, stringDone: done);
		}

		public static KeyValueRequest SetFloat(KeyValueTargets target, string key, float value, Action<KeyValueResult<float>> done = null)
		{
			return new KeyValueRequest(target, key, States.SetRequest, KeyValueTypes.Float, floatValue: value, floatDone: done);
		}

		public static KeyValueRequest GetBoolean(KeyValueTargets target, string key, Action<KeyValueResult<bool>> done)
		{
			return new KeyValueRequest(target, key, States.GetRequest, KeyValueTypes.Boolean, booleanDone: done);
		}

		public static KeyValueRequest GetInteger(KeyValueTargets target, string key, Action<KeyValueResult<int>> done)
		{
			return new KeyValueRequest(target, key, States.GetRequest, KeyValueTypes.Integer, integerDone: done);
		}

		public static KeyValueRequest GetString(KeyValueTargets target, string key, Action<KeyValueResult<string>> done)
		{
			return new KeyValueRequest(target, key, States.GetRequest, KeyValueTypes.String, stringDone: done);
		}

		public static KeyValueRequest GetFloat(KeyValueTargets target, string key, Action<KeyValueResult<float>> done)
		{
			return new KeyValueRequest(target, key, States.GetRequest, KeyValueTypes.Float, floatDone: done);
		}
		#endregion

		public readonly KeyValueTargets Target;
		public readonly string Key;
		public readonly States State;
		public readonly KeyValueTypes ValueType;

		public readonly bool BooleanValue;
		public readonly int IntegerValue;
		public readonly string StringValue;
		public readonly float FloatValue;

		public readonly Action<KeyValueResult<bool>> BooleanDone;
		public readonly Action<KeyValueResult<int>> IntegerDone;
		public readonly Action<KeyValueResult<string>> StringDone;
		public readonly Action<KeyValueResult<float>> FloatDone;

		KeyValueRequest(
			KeyValueTargets target,
			string key,
			States state,
			KeyValueTypes valueType,

			bool booleanValue = false,
			int integerValue = 0,
			string stringValue = null,
			float floatValue = 0f,

			Action<KeyValueResult<bool>> booleanDone = null,
			Action<KeyValueResult<int>> integerDone = null,
			Action<KeyValueResult<string>> stringDone = null,
			Action<KeyValueResult<float>> floatDone = null
		)
		{
			Target = target;
			Key = key;
			State = state;
			ValueType = valueType;

			BooleanValue = booleanValue;
			IntegerValue = integerValue;
			StringValue = stringValue;
			FloatValue = floatValue;

			BooleanDone = booleanDone ?? ActionExtensions.GetEmpty<KeyValueResult<bool>>();
			IntegerDone = integerDone ?? ActionExtensions.GetEmpty<KeyValueResult<int>>();
			StringDone = stringDone ?? ActionExtensions.GetEmpty<KeyValueResult<string>>();
			FloatDone = floatDone ?? ActionExtensions.GetEmpty<KeyValueResult<float>>();
		}

		public KeyValueRequest Duplicate(States state = States.Unknown)
		{
			return new KeyValueRequest(
				Target,
				Key,
				state == States.Unknown ? State : state,
				ValueType,

				BooleanValue,
				IntegerValue,
				StringValue,
				FloatValue,

				BooleanDone,
				IntegerDone,
				StringDone,
				FloatDone
			);
		}
	}
}