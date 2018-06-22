namespace LunraGames.SpaceFarm
{
	/// <summary>
	/// This class should have subclasses for each Shader, with the property names of each shader.
	/// </summary>
	public static class ShaderConstants
	{
		/// <summary>
		/// Globals used across multiple shaders.
		/// </summary>
		public static class Globals
		{
			/// <summary>
			/// Intensity of the wind.
			/// </summary>
			public const string WindIntensity = "_WindIntensity";
		}

		public static class Cursor
		{
			/// <summary>
			/// The color of the cursor.
			/// </summary>
			public const string Color = "_Color";
		}

		public static class VoidRim
		{
			/// <summary>
			/// The name of the shader.
			/// </summary>
			public const string Name = "graphs/VoidRim";
			/// <summary>
			/// The texture that's splated in screen space of the void.
			/// </summary>
			public const string VoidInterior = "Texture_E1313884";
		}

		/*
		public static class SomeShader
		{
			/// <summary>
			/// The name of the shader.
			/// </summary>
			public const string Name = "LG/SomeShader";
			/// <summary>
			/// Some variable.
			/// </summary>
			public const string SomeVariable = "_SomeVariable";
		}
		*/
	}
}