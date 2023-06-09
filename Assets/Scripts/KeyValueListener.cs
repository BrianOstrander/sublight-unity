﻿using System;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public class KeyValueListener
	{
		KeyValueTargets target;
		KeyValueListModel keyValues;
		KeyValueService keyValueService;

		public KeyValueListener(KeyValueTargets target, KeyValueListModel keyValues, KeyValueService keyValueService)
		{
			this.target = target;
			this.keyValues = keyValues;
			this.keyValueService = keyValueService;
		}

		public KeyValueListener Register()
		{
			keyValueService.RegisterGet(target, OnGetBoolean);
			keyValueService.RegisterGet(target, OnGetInteger);
			keyValueService.RegisterGet(target, OnGetString);
			keyValueService.RegisterGet(target, OnGetFloat);

			keyValueService.RegisterSet(target, OnSetBoolean);
			keyValueService.RegisterSet(target, OnSetInteger);
			keyValueService.RegisterSet(target, OnSetString);
			keyValueService.RegisterSet(target, OnSetFloat);

			return this;
		}

		public KeyValueListener UnRegister()
		{
			keyValueService.UnRegisterGet(target, OnGetBoolean);
			keyValueService.UnRegisterGet(target, OnGetInteger);
			keyValueService.UnRegisterGet(target, OnGetString);
			keyValueService.UnRegisterGet(target, OnGetFloat);

			keyValueService.UnRegisterSet(target, OnSetBoolean);
			keyValueService.UnRegisterSet(target, OnSetInteger);
			keyValueService.UnRegisterSet(target, OnSetString);
			keyValueService.UnRegisterSet(target, OnSetFloat);

			return this;
		}

		KeyValueResult<T> Result<T>(string key, T value) where T : IConvertible
		{
			return new KeyValueResult<T>(KeyValueSources.KeyValue, target, key, value);
		}

		void OnGetBoolean(KeyValueRequest request, Action<KeyValueResult<bool>> done)
		{
			done(Result(request.Key, keyValues.GetBoolean(request.Key)));
		}

		void OnGetInteger(KeyValueRequest request, Action<KeyValueResult<int>> done)
		{
			done(Result(request.Key, keyValues.GetInteger(request.Key)));
		}

		void OnGetString(KeyValueRequest request, Action<KeyValueResult<string>> done)
		{
			done(Result(request.Key, keyValues.GetString(request.Key)));
		}

		void OnGetFloat(KeyValueRequest request, Action<KeyValueResult<float>> done)
		{
			done(Result(request.Key, keyValues.GetFloat(request.Key)));
		}

		void OnSetBoolean(KeyValueRequest request, Action<KeyValueResult<bool>> done)
		{
			done(Result(request.Key, keyValues.SetBoolean(request.Key, request.BooleanValue)));
		}

		void OnSetInteger(KeyValueRequest request, Action<KeyValueResult<int>> done)
		{
			done(Result(request.Key, keyValues.SetInteger(request.Key, request.IntegerValue)));
		}

		void OnSetString(KeyValueRequest request, Action<KeyValueResult<string>> done)
		{
			done(Result(request.Key, keyValues.SetString(request.Key, request.StringValue)));
		}

		void OnSetFloat(KeyValueRequest request, Action<KeyValueResult<float>> done)
		{
			done(Result(request.Key, keyValues.SetFloat(request.Key, request.FloatValue)));
		}
	}
}