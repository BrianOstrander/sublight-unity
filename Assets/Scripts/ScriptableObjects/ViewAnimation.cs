using UnityEngine;

namespace LunraGames.SpaceFarm
{
	public abstract class ViewAnimation : ScriptableObject
	{
		public virtual void OnPrepare(IView view){}
		public virtual void OnShowing(IView view, float scalar) {}
		public virtual void OnShown(IView view) {}
		public virtual void OnIdle(IView view) {}
		public virtual void OnLateIdle(IView view) {}
		public virtual void OnClosing(IView view, float scalar) {}
		public virtual void OnClosed(IView view) {}
	}
}