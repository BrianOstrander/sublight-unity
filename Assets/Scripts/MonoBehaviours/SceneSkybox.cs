using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LunraGames.SubLight
{
	[ExecuteInEditMode]
	public class SceneSkybox : MonoBehaviour
	{
		[SerializeField]
		Material skybox;
		[SerializeField]
		Color ambientLight = Color.white;

		void Start()
		{
			Apply();
		}

#if UNITY_EDITOR
		void Update()
		{
			if (!Application.isPlaying && DevPrefs.AutoApplySkybox) Apply();	
		}
#endif

		void Apply()
		{
			RenderSettings.skybox = new Material(skybox);
			RenderSettings.ambientLight = ambientLight;
		}
	}
}