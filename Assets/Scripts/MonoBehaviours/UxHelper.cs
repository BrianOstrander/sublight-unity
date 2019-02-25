using UnityEngine;

namespace LunraGames.SubLight
{
	[ExecuteInEditMode]
	public class UxHelper : MonoBehaviour
	{
		public static UxHelper Instance;

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
		[SerializeField]
		bool listeningInEditMode;
		[SerializeField]
		GameObject canvas;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null

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