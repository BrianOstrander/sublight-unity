using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using UnityEditor;

namespace LunraGamesEditor
{
	public static class EditorGUIExtensions
	{
		enum ChangeCheckStates
		{
			Listening,
			Paused
		}

		static int CurrentLevel;
		static Stack<ChangeCheckStates> ChangeCheckStateStack = new Stack<ChangeCheckStates>();
		static Stack<bool> ChangeValueStack = new Stack<bool>();

		static bool IsActive { get { return 0 < CurrentLevel; } }

		public static void BeginChangeCheck()
		{
			if (IsActive)
			{
				if (ChangeCheckStateStack.Peek() == ChangeCheckStates.Paused)
				{
					Debug.LogError("Cannot begin a change check in the middle of a pause.");
					return;
				}

				// If anything messes up, it's probably because of the next two lines...
				var previous = false;
				try { previous = EditorGUI.EndChangeCheck(); } catch {}

				previous = ChangeValueStack.Pop() || previous;
				ChangeValueStack.Push(previous);
			}

			ChangeValueStack.Push(false);

			ChangeCheckStateStack.Push(ChangeCheckStates.Listening);
			CurrentLevel++;

			EditorGUI.BeginChangeCheck();
		}

		public static bool EndChangeCheck(ref bool changed)
		{
			changed = EndChangeCheck() || changed;
			return changed;
		}

		public static bool EndChangeCheck()
		{
			if (!IsActive)
			{
				Debug.LogError("Cannot end a change check that has not been started.");
				return false;
			}
			if (ChangeCheckStateStack.Peek() == ChangeCheckStates.Paused)
			{
				Debug.LogError("Cannot end a change check that has been paused.");
				return false;
			}

			ChangeCheckStateStack.Pop();
			CurrentLevel--;
			var result = ChangeValueStack.Pop();
			result = EditorGUI.EndChangeCheck() || result;
			return result;
		}

		public static void PauseChangeCheck()
		{
			if (!IsActive) return;
			if (ChangeCheckStateStack.Peek() == ChangeCheckStates.Paused) return;

			ChangeCheckStateStack.Push(ChangeCheckStates.Paused);

			var current = EditorGUI.EndChangeCheck();
			current = ChangeValueStack.Pop() || current;
			ChangeValueStack.Push(current);
		}

		public static void UnPauseChangeCheck()
		{
			if (!IsActive) return;
			if (ChangeCheckStateStack.Peek() != ChangeCheckStates.Paused)
			{
				Debug.LogError("Cannot unpause a change check that has not been paused.");
				return;
			}

			ChangeCheckStateStack.Pop();

			EditorGUI.BeginChangeCheck();
		}
	}
}