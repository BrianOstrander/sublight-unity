using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using LunraGames.NumberDemon;
using LunraGames.SpaceFarm.Models;

namespace LunraGames.SpaceFarm
{
	public class KeyValueService
	{
		static string GetNoHandlerError(KeyValueTargets target, KeyValueRequest.ValueTypes valueType)
		{
			return "No handler found for " + target + "." + valueType;
		}

		CallbackService callbacks;
		Dictionary<KeyValueTargets, Action<KeyValueRequest, Action<KeyValueResult<bool>>>> booleanGetHandlers = new Dictionary<KeyValueTargets, Action<KeyValueRequest, Action<KeyValueResult<bool>>>>();
		Dictionary<KeyValueTargets, Action<KeyValueRequest, Action<KeyValueResult<int>>>> integerGetHandlers = new Dictionary<KeyValueTargets, Action<KeyValueRequest, Action<KeyValueResult<int>>>>();
		Dictionary<KeyValueTargets, Action<KeyValueRequest, Action<KeyValueResult<string>>>> stringGetHandlers = new Dictionary<KeyValueTargets, Action<KeyValueRequest, Action<KeyValueResult<string>>>>();

		Dictionary<KeyValueTargets, Action<KeyValueRequest, Action<KeyValueResult<bool>>>> booleanSetHandlers = new Dictionary<KeyValueTargets, Action<KeyValueRequest, Action<KeyValueResult<bool>>>>();
		Dictionary<KeyValueTargets, Action<KeyValueRequest, Action<KeyValueResult<int>>>> integerSetHandlers = new Dictionary<KeyValueTargets, Action<KeyValueRequest, Action<KeyValueResult<int>>>>();
		Dictionary<KeyValueTargets, Action<KeyValueRequest, Action<KeyValueResult<string>>>> stringSetHandlers = new Dictionary<KeyValueTargets, Action<KeyValueRequest, Action<KeyValueResult<string>>>>();

		public KeyValueService(CallbackService callbacks)
		{
			this.callbacks = callbacks;
		}

		void OnKeyValueRequest(KeyValueRequest request)
		{
			switch(request.State)
			{
				case KeyValueRequest.States.GetRequest:
					OnGetRequest(request);
					break;
				case KeyValueRequest.States.SetRequest:
					OnSetRequest(request);
					break;
				default:
					Debug.LogError("Unrecognized ValueType: " + request.ValueType);
					break;
			}
		}

		void OnGetRequest(KeyValueRequest request)
		{
			switch (request.ValueType)
			{
				case KeyValueRequest.ValueTypes.Boolean:
					Action<KeyValueRequest, Action<KeyValueResult<bool>>> booleanHandler;
					if (booleanGetHandlers.TryGetValue(request.Target, out booleanHandler)) booleanHandler(request, request.BooleanDone);
					else OnError(request.BooleanDone, request, GetNoHandlerError(request.Target, request.ValueType));
					break;
				case KeyValueRequest.ValueTypes.Integer:
					Action<KeyValueRequest, Action<KeyValueResult<int>>> integerHandler;
					if (integerGetHandlers.TryGetValue(request.Target, out integerHandler)) integerHandler(request, request.IntegerDone);
					else OnError(request.IntegerDone, request, GetNoHandlerError(request.Target, request.ValueType));
					break;
				case KeyValueRequest.ValueTypes.String:
					Action<KeyValueRequest, Action<KeyValueResult<string>>> stringHandler;
					if (stringGetHandlers.TryGetValue(request.Target, out stringHandler)) stringHandler(request, request.StringDone);
					else OnError(request.StringDone, request, GetNoHandlerError(request.Target, request.ValueType));
					break;
				default:
					Debug.LogError("Unrecognized ValueType: " + request.ValueType);
					break;
			}
		}

		void OnSetRequest(KeyValueRequest request)
		{
			switch (request.ValueType)
			{
				case KeyValueRequest.ValueTypes.Boolean:
					Action<KeyValueRequest, Action<KeyValueResult<bool>>> booleanHandler;
					if (booleanSetHandlers.TryGetValue(request.Target, out booleanHandler)) booleanHandler(request, request.BooleanDone);
					else OnError(request.BooleanDone, request, GetNoHandlerError(request.Target, request.ValueType));
					break;
				case KeyValueRequest.ValueTypes.Integer:
					Action<KeyValueRequest, Action<KeyValueResult<int>>> integerHandler;
					if (integerSetHandlers.TryGetValue(request.Target, out integerHandler)) integerHandler(request, request.IntegerDone);
					else OnError(request.IntegerDone, request, GetNoHandlerError(request.Target, request.ValueType));
					break;
				case KeyValueRequest.ValueTypes.String:
					Action<KeyValueRequest, Action<KeyValueResult<string>>> stringHandler;
					if (stringSetHandlers.TryGetValue(request.Target, out stringHandler)) stringHandler(request, request.StringDone);
					else OnError(request.StringDone, request, GetNoHandlerError(request.Target, request.ValueType));
					break;
				default:
					Debug.LogError("Unrecognized ValueType: " + request.ValueType);
					break;
			}
		}

		void OnError<T>(Action<KeyValueResult<T>> callback, KeyValueRequest request, string error) where T : IConvertible
		{
			callback(new KeyValueResult<T>(request, error));
		}
	}
}