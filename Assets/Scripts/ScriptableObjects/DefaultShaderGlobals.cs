using UnityEngine;

using LunraGames.Singletonnes;
using LunraGames.SpaceFarm;

namespace LunraGames.SpaceFarm
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