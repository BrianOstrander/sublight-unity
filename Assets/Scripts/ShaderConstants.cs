namespace LunraGames.SubLight
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
			public const string VoidInterior = "_VoidInterior";
		}

		public static class HoloLayerShared
		{
			//public const string Name = 

			public const int LayerCount = 4;
			public static int MaxLayer { get { return LayerCount - 1; } }

			public static string GetLayer(int order) { return "_Layer_" + order; }

			public static string GetWeight(int order) { return "_Weight_" + order; }
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