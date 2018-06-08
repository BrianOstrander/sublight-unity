using UnityEngine;
using LunraGames.SpaceFarm;

namespace LunraGames.SpaceFarm 
{
	/// <summary>
	/// Main kickstarts the App class and provides MonoBehaviour functionality without the annoying life cycle
	/// constraints of an actual MonoBehaviour.
	/// </summary>
	/// <remarks>
	/// This class should never be called directly, it simply gets the App singleton going, and passes any unity 
	/// specific events back to it.
	/// </remarks>
	public class Main : MonoBehaviour 
	{

		[SerializeField]
		GameObject audioRoot;
		[SerializeField]
		Transform canvasRoot;
		[SerializeField]
		DefaultViews defaultViews;
		[SerializeField]
		DefaultShaderGlobals defaultShaderGlobals;

		App app;
		
		void Awake() 
		{
			app = new App(this, defaultShaderGlobals, defaultViews.Prefabs, audioRoot, canvasRoot);
			DontDestroyOnLoad(gameObject);
			app.Awake();
		}

		void Start() 
		{
			app.Start ();
		}

		void Update() 
		{
			app.Update(Time.deltaTime);
		}

		void LateUpdate()
		{
			app.LateUpdate(Time.deltaTime);
		}

		void FixedUpdate() 
		{
			app.FixedUpdate();	
		}

		void OnApplicationPause(bool paused) 
		{
			app.OnApplicationPause(paused);
		}

		void OnApplicationQuit() 
		{
			app.OnApplicationQuit();
		}
	}
}
