using System;
using System.Collections.Generic;

using UnityEngine;

namespace LunraGames.SubLight
{
	public class KeyValueService
	{
		static string GetNoHandlerError(KeyValueRequest.States state, KeyValueTargets target, KeyValueTypes valueType)
		{
			return "No "+state+" handler found for " + target + "." + valueType;
		}

		CallbackService callbacks;
		Dictionary<KeyValueTargets, Action<KeyValueRequest, Action<KeyValueResult<bool>>>> booleanGetHandlers = new Dictionary<KeyValueTargets, Action<KeyValueRequest, Action<KeyValueResult<bool>>>>();
		Dictionary<KeyValueTargets, Action<KeyValueRequest, Action<KeyValueResult<int>>>> integerGetHandlers = new Dictionary<KeyValueTargets, Action<KeyValueRequest, Action<KeyValueResult<int>>>>();
		Dictionary<KeyValueTargets, Action<KeyValueRequest, Action<KeyValueResult<string>>>> stringGetHandlers = new Dictionary<KeyValueTargets, Action<KeyValueRequest, Action<KeyValueResult<string>>>>();
		Dictionary<KeyValueTargets, Action<KeyValueRequest, Action<KeyValueResult<float>>>> floatGetHandlers = new Dictionary<KeyValueTargets, Action<KeyValueRequest, Action<KeyValueResult<float>>>>();

		Dictionary<KeyValueTargets, Action<KeyValueRequest, Action<KeyValueResult<bool>>>> booleanSetHandlers = new Dictionary<KeyValueTargets, Action<KeyValueRequest, Action<KeyValueResult<bool>>>>();
		Dictionary<KeyValueTargets, Action<KeyValueRequest, Action<KeyValueResult<int>>>> integerSetHandlers = new Dictionary<KeyValueTargets, Action<KeyValueRequest, Action<KeyValueResult<int>>>>();
		Dictionary<KeyValueTargets, Action<KeyValueRequest, Action<KeyValueResult<string>>>> stringSetHandlers = new Dictionary<KeyValueTargets, Action<KeyValueRequest, Action<KeyValueResult<string>>>>();
		Dictionary<KeyValueTargets, Action<KeyValueRequest, Action<KeyValueResult<float>>>> floatSetHandlers = new Dictionary<KeyValueTargets, Action<KeyValueRequest, Action<KeyValueResult<float>>>>();

		public KeyValueService(CallbackService callbacks)
		{
			if (callbacks == null) throw new ArgumentNullException("callbacks");

			this.callbacks = callbacks;
			this.callbacks.KeyValueRequest += OnKeyValueRequest;
		}

		#region Get Registration
		public void RegisterGet(
			KeyValueTargets target,
			Action<KeyValueRequest, Action<KeyValueResult<bool>>> handler
		)
		{
			booleanGetHandlers[target] = handler;
		}

		public void RegisterGet(
			KeyValueTargets target,
			Action<KeyValueRequest, Action<KeyValueResult<int>>> handler
		)
		{
			integerGetHandlers[target] = handler;
		}

		public void RegisterGet(
			KeyValueTargets target,
			Action<KeyValueRequest, Action<KeyValueResult<string>>> handler
		)
		{
			stringGetHandlers[target] = handler;
		}

		public void RegisterGet(
			KeyValueTargets target,
			Action<KeyValueRequest, Action<KeyValueResult<float>>> handler
		)
		{
			floatGetHandlers[target] = handler;
		}
		#endregion

		#region Set Registration
		public void RegisterSet(
			KeyValueTargets target,
			Action<KeyValueRequest, Action<KeyValueResult<bool>>> handler
		)
		{
			booleanSetHandlers[target] = handler;
		}

		public void RegisterSet(
			KeyValueTargets target,
			Action<KeyValueRequest, Action<KeyValueResult<int>>> handler
		)
		{
			integerSetHandlers[target] = handler;
		}

		public void RegisterSet(
			KeyValueTargets target,
			Action<KeyValueRequest, Action<KeyValueResult<string>>> handler
		)
		{
			stringSetHandlers[target] = handler;
		}

		public void RegisterSet(
			KeyValueTargets target,
			Action<KeyValueRequest, Action<KeyValueResult<float>>> handler
		)
		{
			floatSetHandlers[target] = handler;
		}
		#endregion

		#region Get UnRegistration
		public void UnRegisterGet(
			KeyValueTargets target,
			Action<KeyValueRequest, Action<KeyValueResult<bool>>> handler
		)
		{
			if (booleanGetHandlers.ContainsKey(target) && booleanGetHandlers[target] == handler) booleanGetHandlers.Remove(target);
		}

		public void UnRegisterGet(
			KeyValueTargets target,
			Action<KeyValueRequest, Action<KeyValueResult<int>>> handler
		)
		{
			if (integerGetHandlers.ContainsKey(target) && integerGetHandlers[target] == handler) integerGetHandlers.Remove(target);
		}

		public void UnRegisterGet(
			KeyValueTargets target,
			Action<KeyValueRequest, Action<KeyValueResult<string>>> handler
		)
		{
			if (stringGetHandlers.ContainsKey(target) && stringGetHandlers[target] == handler) stringGetHandlers.Remove(target);
		}

		public void UnRegisterGet(
			KeyValueTargets target,
			Action<KeyValueRequest, Action<KeyValueResult<float>>> handler
		)
		{
			if (floatGetHandlers.ContainsKey(target) && floatGetHandlers[target] == handler) floatGetHandlers.Remove(target);
		}
		#endregion

		#region Set UnRegistration
		public void UnRegisterSet(
			KeyValueTargets target,
			Action<KeyValueRequest, Action<KeyValueResult<bool>>> handler
		)
		{
			if (booleanSetHandlers.ContainsKey(target) && booleanSetHandlers[target] == handler) booleanSetHandlers.Remove(target);
		}

		public void UnRegisterSet(
			KeyValueTargets target,
			Action<KeyValueRequest, Action<KeyValueResult<int>>> handler
		)
		{
			if (integerSetHandlers.ContainsKey(target) && integerSetHandlers[target] == handler) integerSetHandlers.Remove(target);
		}

		public void UnRegisterSet(
			KeyValueTargets target,
			Action<KeyValueRequest, Action<KeyValueResult<string>>> handler
		)
		{
			if (stringSetHandlers.ContainsKey(target) && stringSetHandlers[target] == handler) stringSetHandlers.Remove(target);
		}

		public void UnRegisterSet(
			KeyValueTargets target,
			Action<KeyValueRequest, Action<KeyValueResult<float>>> handler
		)
		{
			if (floatSetHandlers.ContainsKey(target) && floatSetHandlers[target] == handler) floatSetHandlers.Remove(target);
		}
		#endregion

		#region Events
		void OnKeyValueRequest(KeyValueRequest request)
		{
			switch (request.State)
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
				case KeyValueTypes.Boolean:
					Action<KeyValueRequest, Action<KeyValueResult<bool>>> booleanHandler;
					if (booleanGetHandlers.TryGetValue(request.Target, out booleanHandler)) booleanHandler(request, request.BooleanDone);
					else OnError(request.BooleanDone, request, GetNoHandlerError(request.State, request.Target, request.ValueType));
					break;
				case KeyValueTypes.Integer:
					Action<KeyValueRequest, Action<KeyValueResult<int>>> integerHandler;
					if (integerGetHandlers.TryGetValue(request.Target, out integerHandler)) integerHandler(request, request.IntegerDone);
					else OnError(request.IntegerDone, request, GetNoHandlerError(request.State, request.Target, request.ValueType));
					break;
				case KeyValueTypes.String:
					Action<KeyValueRequest, Action<KeyValueResult<string>>> stringHandler;
					if (stringGetHandlers.TryGetValue(request.Target, out stringHandler)) stringHandler(request, request.StringDone);
					else OnError(request.StringDone, request, GetNoHandlerError(request.State, request.Target, request.ValueType));
					break;
				case KeyValueTypes.Float:
					Action<KeyValueRequest, Action<KeyValueResult<float>>> floatHandler;
					if (floatGetHandlers.TryGetValue(request.Target, out floatHandler)) floatHandler(request, request.FloatDone);
					else OnError(request.FloatDone, request, GetNoHandlerError(request.State, request.Target, request.ValueType));
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
				case KeyValueTypes.Boolean:
					Action<KeyValueRequest, Action<KeyValueResult<bool>>> booleanHandler;
					if (booleanSetHandlers.TryGetValue(request.Target, out booleanHandler)) booleanHandler(request, request.BooleanDone);
					else OnError(request.BooleanDone, request, GetNoHandlerError(request.State, request.Target, request.ValueType));
					break;
				case KeyValueTypes.Integer:
					Action<KeyValueRequest, Action<KeyValueResult<int>>> integerHandler;
					if (integerSetHandlers.TryGetValue(request.Target, out integerHandler)) integerHandler(request, request.IntegerDone);
					else OnError(request.IntegerDone, request, GetNoHandlerError(request.State, request.Target, request.ValueType));
					break;
				case KeyValueTypes.String:
					Action<KeyValueRequest, Action<KeyValueResult<string>>> stringHandler;
					if (stringSetHandlers.TryGetValue(request.Target, out stringHandler)) stringHandler(request, request.StringDone);
					else OnError(request.StringDone, request, GetNoHandlerError(request.State, request.Target, request.ValueType));
					break;
				case KeyValueTypes.Float:
					Action<KeyValueRequest, Action<KeyValueResult<float>>> floatHandler;
					if (floatSetHandlers.TryGetValue(request.Target, out floatHandler)) floatHandler(request, request.FloatDone);
					else OnError(request.FloatDone, request, GetNoHandlerError(request.State, request.Target, request.ValueType));
					break;
				default:
					Debug.LogError("Unrecognized ValueType: " + request.ValueType);
					break;
			}
		}

		void OnError<T>(Action<KeyValueResult<T>> callback, KeyValueRequest request, string error) where T : IConvertible
		{
			Debug.LogError("Failure on " + request.State + " for " + request.Target + "." + request.Key + "\nError: " + error);
			callback(new KeyValueResult<T>(request, error));
		}
		#endregion
	}
}