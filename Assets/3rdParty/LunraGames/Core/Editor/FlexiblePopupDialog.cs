using System;

using UnityEditor;
using UnityEngine;

namespace LunraGamesEditor
{
	public class FlexiblePopupDialog : EditorWindow
	{
		/*
		static Vector2 GetScreenCenterPosition(Vector2 size)
		{
			return (new Vector2(Screen.width * 0.5f, Screen.height * 0.5f)) + (size * 0.5f);
		}
		*/

		static Vector2 GetCursorCenterPosition(Vector2 size)
		{
			return Event.current.mousePosition + (size * 0.5f);
		}

		//Vector2 size;
		Action onGui;
		Action onClose;
		Action onLostFocus;
		bool lostFocusCloses;

		bool onCloseOrLostFocusCalled;

		public static void Show(
			string title,
			Vector2 size,
			Action onGui,
			Action onClose = null,
			Action onLostFocus = null,
			bool lostFocusCloses = true
		)
		{
			if (title == null) throw new ArgumentNullException("title");
			if (onGui == null) throw new ArgumentNullException("onGui");

			var window = GetWindow(typeof(FlexiblePopupDialog), true, title, true) as FlexiblePopupDialog;

			//window.size = size;
			window.onGui = onGui;

			window.onClose = onClose;
			window.onLostFocus = onLostFocus;
			window.lostFocusCloses = lostFocusCloses;

			window.position = new Rect(GetCursorCenterPosition(size), size);

			window.Show();
		}

		void OnGUI()
		{
			onGui();
		}

		void OnDestroy()
		{
			if (onClose != null) onClose();
		}

		void OnLostFocus()
		{
			if (onLostFocus != null) onLostFocus();
			if (lostFocusCloses) Close();
		}
	}
}