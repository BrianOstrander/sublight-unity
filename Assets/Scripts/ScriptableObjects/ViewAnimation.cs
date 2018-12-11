using UnityEngine;

namespace LunraGames.SubLight
{
	public abstract class ViewAnimation : ScriptableObject
	{
		public virtual void OnPrepare(IView view){}
		public virtual void OnShowing(IView view, float scalar) {}
		public virtual void OnShown(IView view) {}
		public virtual void OnIdle(IView view, float delta) {}
		public virtual void OnLateIdle(IView view,float delta) {}
		public virtual void OnPrepareClose(IView view) { }
		public virtual void OnClosing(IView view, float scalar) {}
		public virtual void OnClosed(IView view) {}
		public virtual void OnConstant(IView view, float delta) {}
		public virtual void OnLateConstant(IView view, float delta) { }
	}
}