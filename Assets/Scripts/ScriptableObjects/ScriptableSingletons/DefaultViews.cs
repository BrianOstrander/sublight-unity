using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using LunraGames.Singletonnes;

namespace LunraGames.SubLight
{
	public class DefaultViews : ScriptableSingleton<DefaultViews>
	{
		[SerializeField]
		List<Object> prefabs = new List<Object>();

		public List<GameObject> Prefabs { get { return prefabs.Where(p => p != null).Cast<MonoBehaviour>().Select(m => m.gameObject).ToList(); } }

		public void Add(Object prefab)
		{
			if (!Application.isEditor)
			{
				Debug.LogError("Adding default views should not occur outside the editor");
				return;
			}
			if (Contains(prefab)) Debug.Log("Prefab already added to DefaultViews");
			else
			{
				prefabs.Add(prefab);
				SetConfigDirty();
			}
		}

		public void Remove(Object prefab)
		{
			if (!Application.isEditor)
			{
				Debug.LogError("Removing default views should not occur outside the editor");
				return;
			}
			if (Contains(prefab))
			{
				prefabs.RemoveAll(g => g.GetInstanceID() == prefab.GetInstanceID());
				SetConfigDirty();
			}
			else Debug.Log("Prefab is not listed in DefaultViews");
		}

		public bool Contains(Object prefab)
		{
			prefabs = prefabs ?? new List<Object>();
			return prefabs.FirstOrDefault(g => g != null && g.GetInstanceID() == prefab.GetInstanceID()) != null;
		}

		void SetConfigDirty()
		{
#if UNITY_EDITOR
			UnityEditor.EditorUtility.SetDirty(Instance);
#else
	Debug.LogError("Setting dirty should not occur outside the editor");
#endif
		}
	}
}