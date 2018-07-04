using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LunraGames.SpaceFarm
{
	public enum LogTypes
	{
		Uncatagorized,
		Initialization,
		StateMachine,
		ToDo
	}

	public interface ILogService
	{
		bool IsCollapsing { get; set; }
		bool GetActive(LogTypes logType);
		void SetActive(LogTypes logType, bool isActive);
		void Log(object message, LogTypes logType = LogTypes.Uncatagorized, Object context = null, bool onlyOnce = false);
	}

	public abstract class LogService : ILogService
	{
		class LogEntry
		{
			public object Message { get; private set; }
			public LogTypes LogType { get; private set; }
			public Object Context { get; private set; }

			public LogEntry(object message, LogTypes logType, Object context)
			{
				Message = message;
				LogType = logType;
				Context = context;
			}
		}

		List<LogEntry> logs = new List<LogEntry>();

		bool AlreadyLogged(object message, LogTypes logType, Object context)
		{
			if (!IsCollapsing) return false;
			return logs.FirstOrDefault(l => l.Message == message && l.LogType == logType && l.Context == context) != null;
		}

		public virtual bool IsCollapsing { get; set; }

		public virtual bool GetActive(LogTypes logType) { return true; }
		public virtual void SetActive(LogTypes logType, bool isActive) {}

		public void Log(object message, LogTypes logType = LogTypes.Uncatagorized, Object context = null, bool onlyOnce = false)
		{
			if (!GetActive(logType)) return;

			if (onlyOnce)
			{
				if (AlreadyLogged(message, logType, context)) return;
				logs.Add(new LogEntry(message, logType, context));
			}

			Debug.Log(logType + ": " + message, context);
		}

	}
}