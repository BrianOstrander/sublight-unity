using UnityEngine;

using LunraGames.Singletonnes;

namespace LunraGames.SpaceFarm
{
	public interface IBuildInfo
	{
		int Version { get; }
	}

	public struct BuildInfo : IBuildInfo
	{
		public int Version { get; set; }

		public BuildInfo(int version)
		{
			Version = version;
		}
	}

	public class BuildPreferences : ScriptableSingleton<BuildPreferences>
	{
		//[Header("Global")]
		//[SerializeField]
		//int version;

		public IBuildInfo Info
		{
			get
			{
				return new BuildInfo(int.Parse(Application.version));
			}
		}
	}
}