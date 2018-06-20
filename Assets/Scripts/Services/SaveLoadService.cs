﻿using System;
using System.Collections.Generic;

using UnityEngine;

using LunraGames.SpaceFarm.Models;

namespace LunraGames.SpaceFarm
{
	public struct SaveLoadRequest<M> where M : SaveModel
	{
		public RequestStatus Status { get; private set; }
		public SaveModel Model { get; private set; }
		public M TypedModel { get; private set; }
		public string Error { get; private set; }

		public static SaveLoadRequest<M> Success(SaveModel model, M typedModel)
		{
			return new SaveLoadRequest<M>(
				RequestStatus.Success,
				model,
				typedModel
			);
		}

		public static SaveLoadRequest<M> Failure(SaveModel model, M typedModel, string error)
		{
			return new SaveLoadRequest<M>(
				RequestStatus.Failure,
				model,
				typedModel,
				error
			);
		}

		SaveLoadRequest(
			RequestStatus status,
			SaveModel model,
			M typedModel,
			string error = null
		)
		{
			Status = status;
			Model = model;
			TypedModel = typedModel;
			Error = error;
		}
	}

	public struct SaveLoadArrayRequest<M> where M : SaveModel
	{
		public RequestStatus Status { get; private set; }
		public SaveModel[] Models { get; private set; }
		public M[] TypedModels { get; private set; }
		public string Error { get; private set; }

		public static SaveLoadArrayRequest<M> Success(SaveModel[] models, M[] typedModels)
		{
			return new SaveLoadArrayRequest<M>(
				RequestStatus.Success,
				models,
				typedModels
			);
		}

		public static SaveLoadArrayRequest<M> Failure(SaveModel[] models, M[] typedModels, string error)
		{
			return new SaveLoadArrayRequest<M>(
				RequestStatus.Failure,
				models,
				typedModels,
				error
			);
		}

		SaveLoadArrayRequest(
			RequestStatus status,
			SaveModel[] models,
			M[] typedModels,
			string error = null
		)
		{
			Status = status;
			Models = models;
			TypedModels = typedModels;
			Error = error;
		}
	}
	
	public abstract class SaveLoadService : ISaveLoadService
	{
		protected Type ToType(SaveTypes saveType)
		{
			switch(saveType)
			{
				case SaveTypes.Game: return typeof(GameSaveModel);
				default: throw new ArgumentOutOfRangeException("saveType", saveType + " is not handled.");
			}
		}

		protected SaveTypes ToEnum(Type type)
		{
			if (type == typeof(GameSaveModel)) return SaveTypes.Game;

			throw new ArgumentOutOfRangeException("type", type.FullName + " is not handled.");
		}

		public abstract void Initialize(Action<RequestStatus> done);

		protected abstract Dictionary<SaveTypes, int> MinimumSupportedSaves { get; }

		protected abstract string GetUniquePath(SaveTypes saveType);

		protected bool IsSupportedVersion(SaveTypes type, int version)
		{
			if (!MinimumSupportedSaves.ContainsKey(type)) return false;
			return MinimumSupportedSaves[type] <= version;
		}

		public M Create<M>(string meta = null) where M : SaveModel, new()
		{
			var result = new M();
			result.SupportedVersion.Value = true;
			result.Version.Value = App.BuildPreferences.Info.Version;
			result.Meta.Value = meta;
			result.Path.Value = GetUniquePath(result.SaveType);
			result.Created.Value = DateTime.MinValue;
			result.Modified.Value = DateTime.MinValue;
			return result;
		}

		public void Load<M>(SaveModel model, Action<SaveLoadRequest<M>> done) where M : SaveModel
		{
			if (model == null) throw new ArgumentNullException("model");
			if (done == null) throw new ArgumentNullException("done");
			if (ToEnum(typeof(M)) != model.SaveType) 
			{
				done(SaveLoadRequest<M>.Failure(
					model,
					null,
					"Cannot cast a " + model.SaveType + " model to type " + typeof(M).FullName
				));
				return;
			}

			if (!model.SupportedVersion) 
			{
				done(SaveLoadRequest<M>.Failure(
					model,
					null,
					"Version " + model.Version + " of " + model.SaveType + " is not supported."
				));
				return;
			}

			try { OnLoad(model, done); }
			catch (Exception exception) 
			{
				Debug.LogException(exception);
				done(SaveLoadRequest<M>.Failure(
					model,
					null,
					exception.Message
				));
			}
		}

		protected abstract void OnLoad<M>(SaveModel model, Action<SaveLoadRequest<M>> done) where M : SaveModel;

		public void Save<M>(M model, Action<SaveLoadRequest<M>> done = null) where M : SaveModel
		{
			if (model == null) throw new ArgumentNullException("model");
			done = done ?? OnUnhandledError;

			if (ToEnum(typeof(M)) != model.SaveType) 
			{
				done(SaveLoadRequest<M>.Failure(
					model,
					model,
					"Cannot cast a " + model.SaveType + " model to type " + typeof(M).FullName
				));
				return;
			}

			if (!model.SupportedVersion)
			{
				done(SaveLoadRequest<M>.Failure(
					model,
					model,
					"Version " + model.Version + " of " + model.SaveType + " is not supported."
				));
				return;
			}

			if (string.IsNullOrEmpty(model.Path))
			{
				done(SaveLoadRequest<M>.Failure(
					model,
					model,
					"Path is null or empty."
				));
				return;
			}

			var wasCreated = model.Created.Value;
			var wasModified = model.Modified.Value;

			model.Modified.Value = DateTime.Now;
			if (model.Created == DateTime.MinValue) model.Created.Value = model.Modified.Value;

			try { OnSave(model, done); }
			catch (Exception exception)
			{
				model.Modified.Value = wasModified;
				model.Created.Value = wasCreated;

				Debug.LogException(exception);
				done(SaveLoadRequest<M>.Failure(
					model,
					model,
					exception.Message
				));
			}
		}

		protected abstract void OnSave<M>(M model, Action<SaveLoadRequest<M>> done) where M : SaveModel;

		public void List<M>(Action<SaveLoadArrayRequest<SaveModel>> done) where M : SaveModel
		{
			if (done == null) throw new ArgumentNullException("done");

			try { OnList<M>(done); }
			catch (Exception exception)
			{
				Debug.LogException(exception);
				done(SaveLoadArrayRequest<SaveModel>.Failure(
					null,
					null,
					exception.Message
				));
			}
		}

		protected abstract void OnList<M>(Action<SaveLoadArrayRequest<SaveModel>> done) where M : SaveModel;

		void OnUnhandledError<M>(SaveLoadRequest<M> result) where M : SaveModel
		{
			Debug.LogError("Unhandled error: " + result.Error);
		}
	}

	public interface ISaveLoadService
	{
		void Initialize(Action<RequestStatus> done);
		M Create<M>(string meta = null) where M : SaveModel, new();
		void Save<M>(M model, Action<SaveLoadRequest<M>> done = null) where M : SaveModel;
		void Load<M>(SaveModel model, Action<SaveLoadRequest<M>> done) where M : SaveModel;
		void List<M>(Action<SaveLoadArrayRequest<SaveModel>> done) where M : SaveModel;
	}
}