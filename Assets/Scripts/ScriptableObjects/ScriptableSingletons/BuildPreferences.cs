using System;

using UnityEngine;
using UnityEngine.Analytics;

using LunraGames.Singletonnes;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
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
		public int TargetFrameRate;
		public int VSyncCount;

		[SerializeField]
		string feedbackFormPrefix;

		public IBuildInfo Info
		{
			get
			{
				return new BuildInfo(int.Parse(Application.version));
			}
		}

		public string FeedbackForm(
			string trigger,
			KeyValueListModel globalSource = null
		)
		{
			var result = string.Empty;
			var index = 0;
			var info = Info;

			var persistentId = string.Empty;

			if (globalSource != null)
			{
				persistentId = globalSource.Get(KeyDefines.Global.PersistentId);
			}

			AppendFeedbackFormEntry(
				ref index,
				ref result,
				info.Version.ToString()
			);
			AppendFeedbackFormEntry(
				ref index,
				ref result,
				Application.platform.ToString()
			);
			AppendFeedbackFormEntry(
				ref index,
				ref result,
				SystemInfo.operatingSystem
			);
			AppendFeedbackFormEntry(
				ref index,
				ref result,
				persistentId
			);
			AppendFeedbackFormEntry(
				ref index,
				ref result,
				AnalyticsSessionInfo.userId
			);
			AppendFeedbackFormEntry(
				ref index,
				ref result,
				AnalyticsSessionInfo.sessionId.ToString()
			);
			AppendFeedbackFormEntry(
				ref index,
				ref result,
				trigger
			);

			result = feedbackFormPrefix + Uri.EscapeUriString(result);

			return result;
		}

		void AppendFeedbackFormEntry(
			ref int index,
			ref string result,
			string value
		)
		{
			index++;

			if (string.IsNullOrEmpty(value)) return;

			result += (index - 1) + ":" + value+",";
		}
	}
}