using Newtonsoft.Json;

namespace LunraGames.SpaceFarm.Models
{
	public class PreferencesModel : SaveModel
	{
		[JsonProperty] float cameraSystemDragMoveScalar = 16f;
		[JsonProperty] float cameraSystemDragRotateScalar = 64f;
		[JsonProperty] float sectorUnloadRadius = 2f;

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
		}
	}
}