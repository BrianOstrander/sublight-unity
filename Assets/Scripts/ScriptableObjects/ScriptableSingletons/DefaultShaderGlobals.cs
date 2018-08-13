using UnityEngine;

using LunraGames.Singletonnes;
using LunraGames.SubLight;

namespace LunraGames.SubLight
{
	public class DefaultShaderGlobals : ScriptableSingleton<DefaultShaderGlobals>
	{
		//[Header("Some Header")]
		//[SerializeField, Tooltip("Some tooltip")]
		//float someFloat;

		public void Apply()
		{
			//Shader.SetGlobalFloat(ShaderConstants.Globals.SomeFloat, someFloat);
		}
	}
}