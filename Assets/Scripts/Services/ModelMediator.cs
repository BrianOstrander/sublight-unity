using System;
using System.Collections.Generic;

using UnityEngine;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
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
		public string Error { get; private set; }
		public int Length { get { return Models.Length; } }

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
		}
	}
	
	public abstract class ModelMediator : IModelMediator
	{
		protected IBuildInfo BuildInfo { get; set; }

		protected Type ToType(SaveTypes saveType)
		{
			switch(saveType)
			{
				case SaveTypes.Game: return typeof(GameModel);
				case SaveTypes.Preferences: return typeof(PreferencesModel);
				case SaveTypes.EncounterInfo: return typeof(EncounterInfoModel);
				case SaveTypes.GlobalKeyValues: return typeof(GlobalKeyValuesModel);
				// -- Interacted
				case SaveTypes.InteractedEncounterInfoList: return typeof(InteractedEncounterInfoListModel);
				case SaveTypes.InteractedInventoryReferenceList: return typeof(InteractedInventoryReferenceListModel);
				// -- Inventory References
				case SaveTypes.ModuleReference: return typeof(ModuleReferenceModel);
				case SaveTypes.OrbitalCrewReference: return typeof(OrbitalCrewReferenceModel);
				// --
				case SaveTypes.LanguageDatabase: return typeof(LanguageDatabaseModel);
				default: throw new ArgumentOutOfRangeException("saveType", saveType + " is not handled.");
			}
		}

		protected SaveTypes ToEnum(Type type)
		{
			if (type == typeof(GameModel)) return SaveTypes.Game;
			if (type == typeof(PreferencesModel)) return SaveTypes.Preferences;
			if (type == typeof(EncounterInfoModel)) return SaveTypes.EncounterInfo;
			if (type == typeof(GlobalKeyValuesModel)) return SaveTypes.GlobalKeyValues;
			// -- Interacted
			if (type == typeof(InteractedEncounterInfoListModel)) return SaveTypes.InteractedEncounterInfoList;
			if (type == typeof(InteractedInventoryReferenceListModel)) return SaveTypes.InteractedInventoryReferenceList;
			// -- Inventory References
			if (type == typeof(ModuleReferenceModel)) return SaveTypes.ModuleReference;
			if (type == typeof(OrbitalCrewReferenceModel)) return SaveTypes.OrbitalCrewReference;
			// --
			if (type == typeof(LanguageDatabaseModel)) return SaveTypes.LanguageDatabase;
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

		public M Create<M>(string meta = null) where M : SaveModel, new()
		{
			var result = new M();
			result.SupportedVersion.Value = true;
			result.Version.Value = BuildInfo.Version;
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
	}

	public interface IModelMediator
	{
		void Initialize(IBuildInfo info, Action<RequestStatus> done);
		M Create<M>(string meta = null) where M : SaveModel, new();
		void Save<M>(M model, Action<SaveLoadRequest<M>> done = null, bool updateModified = true) where M : SaveModel;
		void Load<M>(SaveModel model, Action<SaveLoadRequest<M>> done) where M : SaveModel;
		void List<M>(Action<SaveLoadArrayRequest<SaveModel>> done) where M : SaveModel;
		void Delete<M>(M model, Action<SaveLoadRequest<M>> done) where M : SaveModel;
	}
}