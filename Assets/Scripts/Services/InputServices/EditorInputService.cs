#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LunraGames.SubLight
{
	public class EditorInputService : DesktopInputService 
	{
		public EditorInputService(Heartbeat heartbeat, CallbackService callbacks) : base(heartbeat, callbacks) { }

#if UNITY_EDITOR
		protected override bool IsFocused()
		{
			return EditorWindow.focusedWindow?.titleContent.text == "Game";
		}
#endif
	}
}