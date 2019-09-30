﻿using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public struct SaveLoadRequest<M> where M : SaveModel
	{
		public readonly RequestStatus Status;
		public readonly SaveModel Model;
		public readonly M TypedModel;
		public readonly string Error;

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
		public readonly RequestStatus Status;
		public readonly SaveModel[] Models;
		public readonly string Error;
		public readonly int Length;

		public static SaveLoadArrayRequest<M> Success(SaveModel[] models)
		{
			return new SaveLoadArrayRequest<M>(
				RequestStatus.Success,
				models
			);
		}

		public static SaveLoadArrayRequest<M> Failure(string error)
		{
			return new SaveLoadArrayRequest<M>(
				RequestStatus.Failure,
				null,
				error
			);
		}

		SaveLoadArrayRequest(
			RequestStatus status,
			SaveModel[] models,
			string error = null
		)
		{
			Status = status;
			Models = models;
			Error = error;
			Length = models == null ? 0 : models.Length;
		}
	}

	public struct ReadWriteRequest
	{
		public readonly RequestStatus Status;
		public readonly string Path;
		public readonly byte[] Bytes;
		public readonly string Error;

		public static ReadWriteRequest Success(string path, byte[] bytes)
		{
			return new ReadWriteRequest(
				RequestStatus.Success,
				path,
				bytes
			);
		}

		public static ReadWriteRequest Failure(string path, string error)
		{
			return new ReadWriteRequest(
				RequestStatus.Failure,
				path,
				null,
				error
			);
		}

		ReadWriteRequest(
			RequestStatus status,
			string path,
			byte[] bytes,
			string error = null
		)
		{
			Status = status;
			Path = path;
			Bytes = bytes;
			Error = error;
		}
	}
	
	public abstract class ModelMediator : IModelMediator
	{
		protected IBuildInfo BuildInfo { get; set; }

		protected virtual bool SuppressErrorLogging => false;

		protected Type ToType(SaveTypes saveType)
		{
			switch(saveType)
			{
				case SaveTypes.Game: return typeof(GameModel);
				case SaveTypes.Preferences: return typeof(PreferencesModel);
				case SaveTypes.EncounterInfo: return typeof(EncounterInfoModel);
				// -- Meta Key Values
				case SaveTypes.GlobalKeyValues: return typeof(GlobalKeyValuesModel);
				case SaveTypes.PreferencesKeyValues: return typeof(PreferencesKeyValuesModel);
				// -- Galaxies
				case SaveTypes.GalaxyPreview: return typeof(GalaxyPreviewModel);
				case SaveTypes.GalaxyDistant: return typeof(GalaxyDistantModel);
				case SaveTypes.GalaxyInfo: return typeof(GalaxyInfoModel);
				// -- Interacted
				case SaveTypes.InteractedEncounterInfoList: return typeof(InteractedEncounterInfoListModel);
				// --
				case SaveTypes.GamemodeInfo: return typeof(GamemodeInfoModel);
				case SaveTypes.ModuleTrait: return typeof(ModuleTraitModel);
				default: throw new ArgumentOutOfRangeException("saveType", saveType + " is not handled.");
			}
		}

		protected SaveTypes[] ToEnum(Type type)
		{
			if (type == typeof(GameModel)) return new SaveTypes[] { SaveTypes.Game };
			if (type == typeof(PreferencesModel)) return new SaveTypes[] { SaveTypes.Preferences };
			if (type == typeof(EncounterInfoModel)) return new SaveTypes[] { SaveTypes.EncounterInfo };
			// -- Meta Key Values
			if (type == typeof(GlobalKeyValuesModel)) return new SaveTypes[] { SaveTypes.GlobalKeyValues };
			if (type == typeof(PreferencesKeyValuesModel)) return new SaveTypes[] { SaveTypes.PreferencesKeyValues };
			// -- Galaxies
			if (type == typeof(GalaxyPreviewModel) || type == typeof(GalaxyDistantModel) || type == typeof(GalaxyInfoModel))
			{
				return new SaveTypes[] { SaveTypes.GalaxyPreview, SaveTypes.GalaxyDistant, SaveTypes.GalaxyInfo };
			}
			// -- Interacted
			if (type == typeof(InteractedEncounterInfoListModel)) return new SaveTypes[] { SaveTypes.InteractedEncounterInfoList };
			// --
			if (type == typeof(GamemodeInfoModel)) return new SaveTypes[] { SaveTypes.GamemodeInfo };
			if (type == typeof(ModuleTraitModel)) return new SaveTypes[] { SaveTypes.ModuleTrait };
			throw new ArgumentOutOfRangeException("type", type.FullName + " is not handled.");
		}

		public abstract void Initialize(IBuildInfo info, Action<RequestStatus> done);

		/// <summary>
		/// Gets the minimum supported saves by SaveTypes, -1 means it only 
		/// supports saves equal to the current version.
		/// </summary>
		/// <value>The minimum supported saves.</value>
		protected abstract Dictionary<SaveTypes, int> MinimumSupportedSaves { get; }
		/// <summary>
		/// Can these models be saved, or are they readonly.
		/// </summary>
		/// <value>Can save if true.</value>
		protected abstract Dictionary<SaveTypes, bool> CanSave { get; }

		protected abstract string GetUniquePath(SaveTypes saveType);

		protected bool IsSupportedVersion(SaveTypes type, int version)
		{
			if (!MinimumSupportedSaves.ContainsKey(type)) return false;
			var min = MinimumSupportedSaves[type];
			// If min is -1, then it means we can only load saves that equal 
			// this version.
			if (min < 0) min = BuildInfo.Version;
			return min <= version;
		}

		public M Create<M>() where M : SaveModel, new()
		{
			var result = new M();
			result.SupportedVersion.Value = true;
			result.Version.Value = BuildInfo.Version;
			result.Path.Value = GetUniquePath(result.SaveType);
			result.Created.Value = DateTime.MinValue;
			result.Modified.Value = DateTime.MinValue;
			return result;
		}

		public void Load<M>(SaveModel model, Action<SaveLoadRequest<M>> done) where M : SaveModel
		{
			if (model == null) throw new ArgumentNullException("model");
			if (done == null) throw new ArgumentNullException("done");
			if (!ToEnum(typeof(M)).Contains(model.SaveType))
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

		public void Load<M>(string modelId, Action<SaveLoadRequest<M>> done) where M : SaveModel
		{
			if (string.IsNullOrEmpty(modelId)) throw new ArgumentException("modelId cannot be null or empty", "modelId");
			if (done == null) throw new ArgumentNullException("done");

			List<M>(listResults => OnLoadFromIdList(listResults, modelId, done));
		}

		void OnLoadFromIdList<M>(SaveLoadArrayRequest<SaveModel> results, string modelId, Action<SaveLoadRequest<M>> done) where M : SaveModel
		{
			if (results.Status != RequestStatus.Success)
			{
				Debug.LogError("Listing models failed with status: " + results.Status + " and error: " + results.Error);
				done(SaveLoadRequest<M>.Failure(
					null,
					null,
					results.Error
				));
				return;
			}
			var result = results.Models.FirstOrDefault(m => m.Id.Value == modelId);

			if (result == null)
			{
				var error = "A model with Id " + modelId + " was not found";
				if (!SuppressErrorLogging) Debug.LogError(error);
				done(SaveLoadRequest<M>.Failure(
					null,
					null,
					error
				));
				return;
			}

			Load(result, done);
		}

		protected abstract void OnLoad<M>(SaveModel model, Action<SaveLoadRequest<M>> done) where M : SaveModel;

		public void Save<M>(M model, Action<SaveLoadRequest<M>> done = null, bool updateModified = true) where M : SaveModel
		{
			if (model == null) throw new ArgumentNullException("model");
			done = done ?? OnUnhandledError;

			if (!CanSave.ContainsKey(model.SaveType) || !CanSave[model.SaveType])
			{
				done(SaveLoadRequest<M>.Failure(
					model,
					model,
					"Cannot save a " + model.SaveType + " on this platform."
				));
				return;
			}

			if (!ToEnum(typeof(M)).Contains(model.SaveType))
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
			var wasVersion = model.Version.Value;

			if (updateModified || model.Created == DateTime.MinValue)
			{
				model.Modified.Value = DateTime.Now;
				if (model.Created == DateTime.MinValue) model.Created.Value = model.Modified.Value;
			}

			model.Version.Value = BuildInfo.Version;

			try { OnSave(model, done); }
			catch (Exception exception)
			{
				model.Modified.Value = wasModified;
				model.Created.Value = wasCreated;
				model.Version.Value = wasVersion;

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
					exception.Message
				));
			}
		}

		protected abstract void OnList<M>(Action<SaveLoadArrayRequest<SaveModel>> done) where M : SaveModel;

		public void Delete<M>(M model, Action<SaveLoadRequest<M>> done) where M : SaveModel
		{
			if (model == null) throw new ArgumentNullException("model");
			done = done ?? OnUnhandledError;

			try { OnDelete(model, done); }
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

		protected abstract void OnDelete<M>(M model, Action<SaveLoadRequest<M>> done) where M : SaveModel;

		void OnUnhandledError<M>(SaveLoadRequest<M> result) where M : SaveModel
		{
			Debug.LogError("Unhandled error: " + result.Error);
		}

		protected void Read(string path, Action<ReadWriteRequest> done)
		{
			if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
			if (done == null) throw new ArgumentNullException("done");

			try { OnRead(path, done); }
			catch (Exception exception)
			{
				Debug.LogException(exception);
				done(ReadWriteRequest.Failure(
					path,
					exception.Message
				));
			}
		}

		protected abstract void OnRead(string path, Action<ReadWriteRequest> done);
	}

	public interface IModelMediator
	{
		void Initialize(IBuildInfo info, Action<RequestStatus> done);
		M Create<M>() where M : SaveModel, new();
		void Save<M>(M model, Action<SaveLoadRequest<M>> done = null, bool updateModified = true) where M : SaveModel;
		void Load<M>(SaveModel model, Action<SaveLoadRequest<M>> done) where M : SaveModel;
		void Load<M>(string modelId, Action<SaveLoadRequest<M>> done) where M : SaveModel;
		void List<M>(Action<SaveLoadArrayRequest<SaveModel>> done) where M : SaveModel;
		void Delete<M>(M model, Action<SaveLoadRequest<M>> done) where M : SaveModel;
	}
}