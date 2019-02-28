using System;
using System.Linq;

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

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
		[SerializeField]
		string feedbackFormPrefix;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null

		public string BlogUrl;
		public string DiscordUrl;
		public string TwitterUrl;

		public Changelog[] ChangeLogs;

		public Changelog Current { get { return ChangeLogs.FirstOrDefault(c => c.Version == Info.Version); } }

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