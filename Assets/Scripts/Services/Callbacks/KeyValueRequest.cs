using System;

using UnityEngine;

namespace LunraGames.SubLight
{
	public struct KeyValueResult<T> where T : IConvertible
	{
		public readonly KeyValueTargets Target;
		public readonly string Key;
		public readonly T Value;
		public readonly RequestStatus Status;
		public readonly string Error;
		public readonly string TargetKey;

		public KeyValueResult(
			KeyValueTargets target,
			string key,
			T value,
			RequestStatus status = RequestStatus.Success,
			string error = null
		)
		{
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
			Target = request.Target;
			Key = request.Key;
			Status = RequestStatus.Failure;
			Error = error;
			TargetKey = Target + Key;

			switch(request.ValueType)
			{
				case KeyValueRequest.ValueTypes.Boolean:
					Value = (T)(object)request.BooleanValue;
					break;
				case KeyValueRequest.ValueTypes.Integer:
					Value = (T)(object)request.IntegerValue;
					break;
				case KeyValueRequest.ValueTypes.String:
					Value = (T)(object)request.StringValue;
					break;
				case KeyValueRequest.ValueTypes.Float:
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
			GetComplete = 30,
			SetComplete = 40
		}

		public enum ValueTypes
		{
			Unknown = 0,
			Boolean = 10,
			Integer = 20,
			String = 30,
			Float = 40
		}

		#region Enumerated Key Get & Set
		/*
		public static KeyValueRequest Set<T>(KeyValueTargets target, T key, bool value, Action<KeyValueResult<bool>> done = null) 
			where T : struct, IConvertible
		{
			if (!typeof(T).IsEnum) throw new ArgumentException("Must be an enum.", "key");
			return Set(target, Enum.GetName(typeof(T), key), value, done);
		}

		public static KeyValueRequest Set<T>(KeyValueTargets target, T key, int value, Action<KeyValueResult<int>> done = null)
			where T : struct, IConvertible
		{
			if (!typeof(T).IsEnum) throw new ArgumentException("Must be an enum.", "key");
			return Set(target, Enum.GetName(typeof(T), key), value, done);
		}

		public static KeyValueRequest Set<T>(KeyValueTargets target, T key, string value, Action<KeyValueResult<string>> done = null)
			where T : struct, IConvertible
		{
			if (!typeof(T).IsEnum) throw new ArgumentException("Must be an enum.", "key");
			return Set(target, Enum.GetName(typeof(T), key), value, done);
		}

		public static KeyValueRequest Set<T>(KeyValueTargets target, T key, float value, Action<KeyValueResult<float>> done = null)
			where T : struct, IConvertible
		{
			if (!typeof(T).IsEnum) throw new ArgumentException("Must be an enum.", "key");
			return Set(target, Enum.GetName(typeof(T), key), value, done);
		}

		public static KeyValueRequest Get<T>(KeyValueTargets target, T key, Action<KeyValueResult<bool>> done)
			where T : struct, IConvertible
		{
			if (!typeof(T).IsEnum) throw new ArgumentException("Must be an enum.", "key");
			return Get(target, Enum.GetName(typeof(T), key), done);
		}

		public static KeyValueRequest Get<T>(KeyValueTargets target, T key, Action<KeyValueResult<int>> done)
			where T : struct, IConvertible
		{
			if (!typeof(T).IsEnum) throw new ArgumentException("Must be an enum.", "key");
			return Get(target, Enum.GetName(typeof(T), key), done);
		}

		public static KeyValueRequest Get<T>(KeyValueTargets target, T key, Action<KeyValueResult<string>> done)
			where T : struct, IConvertible
		{
			if (!typeof(T).IsEnum) throw new ArgumentException("Must be an enum.", "key");
			return Get(target, Enum.GetName(typeof(T), key), done);
		}

		public static KeyValueRequest Get<T>(KeyValueTargets target, T key, Action<KeyValueResult<float>> done)
			where T : struct, IConvertible
		{
			if (!typeof(T).IsEnum) throw new ArgumentException("Must be an enum.", "key");
			return Get(target, Enum.GetName(typeof(T), key), done);
		}

		/// <summary>
		/// Takes an enum value and returns it as a string.
		/// </summary>
		/// <returns>The enumerated key.</returns>
		/// <param name="key">Key.</param>
		/// <typeparam name="T">Key.</typeparam>
		static string GetEnumeratedKey<T>(T key)
		{
			if (!typeof(T).IsEnum) throw new ArgumentException("Must be an enum.", "key");
			return 
		}
		*/
		#endregion

		#region String Key Get & Set
		public static KeyValueRequest Set(KeyValueTargets target, string key, bool value, Action<KeyValueResult<bool>> done = null)
		{
			return new KeyValueRequest(target, key, States.SetRequest, ValueTypes.Boolean, booleanValue: value, booleanDone: done);
		}

		public static KeyValueRequest Set(KeyValueTargets target, string key, int value, Action<KeyValueResult<int>> done = null)
		{
			return new KeyValueRequest(target, key, States.SetRequest, ValueTypes.Integer, integerValue: value, integerDone: done);
		}

		public static KeyValueRequest Set(KeyValueTargets target, string key, string value, Action<KeyValueResult<string>> done = null)
		{
			return new KeyValueRequest(target, key, States.SetRequest, ValueTypes.String, stringValue: value, stringDone: done);
		}

		public static KeyValueRequest Set(KeyValueTargets target, string key, float value, Action<KeyValueResult<float>> done = null)
		{
			return new KeyValueRequest(target, key, States.SetRequest, ValueTypes.Float, floatValue: value, floatDone: done);
		}

		public static KeyValueRequest Get(KeyValueTargets target, string key, Action<KeyValueResult<bool>> done)
		{
			return new KeyValueRequest(target, key, States.GetRequest, ValueTypes.Boolean, booleanDone: done);
		}

		public static KeyValueRequest Get(KeyValueTargets target, string key, Action<KeyValueResult<int>> done)
		{
			return new KeyValueRequest(target, key, States.GetRequest, ValueTypes.Integer, integerDone: done);
		}

		public static KeyValueRequest Get(KeyValueTargets target, string key, Action<KeyValueResult<string>> done) 
		{ 
			return new KeyValueRequest(target, key, States.GetRequest, ValueTypes.String, stringDone: done); 
		}

		public static KeyValueRequest Get(KeyValueTargets target, string key, Action<KeyValueResult<float>> done)
		{
			return new KeyValueRequest(target, key, States.GetRequest, ValueTypes.Float, floatDone: done);
		}
		#endregion

		public readonly KeyValueTargets Target;
		public readonly string Key;
		public readonly States State;
		public readonly ValueTypes ValueType;

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
			ValueTypes valueType,

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