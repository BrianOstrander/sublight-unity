using System.Collections.Generic;

using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class PreferencesModel : SaveModel
	{
		[JsonProperty] float cameraSystemDragMoveScalar = 16f;
		[JsonProperty] float cameraSystemDragRotateScalar = 64f;
		[JsonProperty] float sectorUnloadRadius = 1f;
		[JsonProperty] bool encounterLogsAutoNext;
		[JsonProperty]
		Dictionary<string, int> languageTagOrder = new Dictionary<string, int>
		{
			{ "en-US", 2 },
			{ "en", 1 },
			{ "Dev", 0 }
		};

		/// <summary>
		/// The scalar applied to dragging around the camera in the system area.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<float> CameraSystemDragMoveScalar;
		/// <summary>
		/// The scalar applied to rotating around the camera in the system area.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<float> CameraSystemDragRotateScalar;
		/// <summary>
		/// The radius, in sectors, the camera can move away before sectors are
		/// unloaded.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<float> SectorUnloadRadius;
		/// <summary>
		/// Do encounters logs automatically load the next one?
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<bool> EncounterLogsAutoNext;

		[JsonIgnore]
		readonly ListenerProperty<Dictionary<string, int>> languageTagOrderListener;
		/// <summary>
		/// The fallback order of language database models with the specified
		/// tags. Higher languages are selected first.
		/// </summary>
		[JsonIgnore]
		public readonly ReadonlyProperty<Dictionary<string, int>> LanguageTagOrder;

		public PreferencesModel()
		{
			SaveType = SaveTypes.Preferences;

			CameraSystemDragMoveScalar = new ListenerProperty<float>(
				value => cameraSystemDragMoveScalar = value, 
				() => cameraSystemDragMoveScalar
			);

			CameraSystemDragRotateScalar = new ListenerProperty<float>(
				value => cameraSystemDragRotateScalar = value,
				() => cameraSystemDragRotateScalar
			);

			SectorUnloadRadius = new ListenerProperty<float>(
				value => sectorUnloadRadius = value,
				() => sectorUnloadRadius
			);

			EncounterLogsAutoNext = new ListenerProperty<bool>(
				value => encounterLogsAutoNext = value,
				() => encounterLogsAutoNext
			);

			LanguageTagOrder = new ReadonlyProperty<Dictionary<string, int>>(
				value => languageTagOrder = value,
				() => languageTagOrder,
				out languageTagOrderListener
			);
		}
	}
}