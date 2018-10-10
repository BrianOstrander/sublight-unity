using UnityEngine;

namespace LunraGames.SubLight
{
	[ExecuteInEditMode]
	public class UxHelper : MonoBehaviour
	{
		public static UxHelper Instance;

		[SerializeField]
		bool listeningInEditMode;
		[SerializeField]
		GameObject canvas;

#if UNITY_EDITOR
		void OnEnable()
		{
			if (listeningInEditMode) Instance = this;
		}

		void Update()
		{
			if (Application.isPlaying || listeningInEditMode) canvas.SetActive(DevPrefs.ShowUxHelper);
		}
#else
		void Awake()
		{
			canvas.SetActive(false);
		}
#endif

		public static void RunUpdate()
		{
#if UNITY_EDITOR
			if (Instance != null) Instance.Update();
			else Debug.Log("lol no instance");
#endif
		}
	}
}