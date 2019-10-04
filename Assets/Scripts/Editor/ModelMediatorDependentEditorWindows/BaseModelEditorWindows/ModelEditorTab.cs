using System;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public abstract class ModelEditorTab<T, M>
		where T : BaseModelEditorWindow<T, M>
		where M : SaveModel, new()
	{
		protected readonly T Window;
		protected readonly string TabKeyPrefix;

		public readonly string Name;
		
		protected ModelEditorTab(
			T window,
			string name,
			string key = null
		)
		{
			Window = window;
			Name = name;
			TabKeyPrefix = Window.KeyPrefix + (string.IsNullOrEmpty(key) ? name : key);
		}
		
		public abstract void Gui(M model);
		
		#region Events
		public virtual void BeforeLoadSelection() { }
		public virtual void AfterLoadSelection(M model) { }
		public virtual void Deselect() { }
		public virtual void SettingsGui() { }
		public virtual void EditorUpdate(float delta) { }
		#endregion
	}
}