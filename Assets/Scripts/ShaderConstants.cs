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
			public const string Name = "SubLight/VoidRim";
			/// <summary>
			/// The texture that's splated in screen space of the void.
			/// </summary>
			public const string VoidInterior = "_VoidInterior";
		}

		public static class HoloLip
		{
			/// <summary>
			/// The name of the shader.
			/// </summary>
			public const string Name = "SubLight/Holo/Lip";
			/// <summary>
			/// The color of the lip.
			/// </summary>
			public const string LipColor = "_LipColor";
			/// <summary>
			/// The minimum radius.
			/// </summary>
			public const string LipMin = "_LipMin";
			/// <summary>
			/// The maximum radius.
			/// </summary>
			public const string LipMax = "_LipMax";
		}

		public static class HoloMask
		{
			/// <summary>
			/// The name of the shader.
			/// </summary>
			public const string Name = "SubLight/Holo/Mask";
			/// <summary>
			/// The opacity
			/// </summary>
			public const string Opacity = "_Opacity";
		}

		public static class HoloGalaxyPreviewBasic
		{
			/// <summary>
			/// The name of the shader.
			/// </summary>
			public const string Name = "SubLight/Holo/GalaxyPreviewBasic";
			/// <summary>
			/// The RGBA texture used for the outline of each layer.
			/// </summary>
			public const string LayerTexture = "_LayerTexture";
			/// <summary>
			/// The RGBA channel being used.
			/// </summary>
			public const string Channel = "_Channel";
			/// <summary>
			/// The color for this layer.
			/// </summary>
			public const string ChannelColor = "_ChannelColor";
			/// <summary>
			/// How much of the texture, from the UV center, has been revealed.
			/// </summary>
			public const string Revealed = "_Revealed";
		}

		public static class HoloGalaxy
		{
			/// <summary>
			/// The name of the shader.
			/// </summary>
			public const string Name = "SubLight/Holo/Galaxy";
			/// <summary>
			/// The RGBA texture used for the outline of each layer.
			/// </summary>
			public const string LayerTexture = "_LayerTexture";
			/// <summary>
			/// The RGBA channel being used.
			/// </summary>
			public const string Channel = "_Channel";
			/// <summary>
			/// The color for this layer.
			/// </summary>
			public const string ChannelColor = "_ChannelColor";
			public const string WorldOrigin = "_WorldOrigin";
			public const string WorldRadius = "_WorldRadius";
			public const string Alpha = "_Alpha";
		}

		public static class HoloGridBasic
		{
			/// <summary>
			/// The name of the shader.
			/// </summary>
			public const string Name = "SubLight/Holo/GridBasic";
			/// <summary>
			/// The color.
			/// </summary>
			public const string MainColor = "_MainColor";
			public const string Tint = "_Tint";
			/// <summary>
			/// How many tiles.
			/// </summary>
			public const string Tiling = "_Tiling";
			/// <summary>
			/// The offset, a vector2 (or 4?) between (0,0) and (1,1)... check
			/// that though... not positive, might allow negatives.
			/// </summary>
			public const string Offset = "_Offset";
			public const string Alpha = "_Alpha";
		}

		public static class HoloGridBackground
		{
			/// <summary>
			/// The name of the shader.
			/// </summary>
			public const string Name = "SubLight/Holo/GridBackground";
			/// <summary>
			/// The color.
			/// </summary>
			public const string MainColor = "_MainColor";
			public const string Tint = "_Tint";
			public const string Alpha = "_Alpha";
		}

		public static class HoloGrid
		{
			/// <summary>
			/// The name of the shader.
			/// </summary>
			public const string Name = "SubLight/Holo/Grid";
			/// <summary>
			/// The color.
			/// </summary>
			public const string GridColor = "_GridColor";
			public const string GridTint = "_GridTint";
			/// <summary>
			/// How zoomed in the grid is, from 0 to 1.
			/// </summary>
			public const string Zoom = "_Zoom";
			/// <summary>
			/// How many tiles.
			/// </summary>
			public const string Tiling = "_Tiling";
			/// <summary>
			/// The offset, a vector2 (or 4?) between (0,0) and (1,1)... check
			/// that though... not positive, might allow negatives.
			/// </summary>
			public const string Offset = "_Offset";
			/// <summary>
			/// How much of the grid is revealed, from 0 to 1.
			/// </summary>
			public const string RadiusProgress = "_RadiusProgress";
			public const string Alpha = "_Alpha";
		}

		public static class HoloGridDynamic
		{
			/// <summary>
			/// The name of the shader.
			/// </summary>
			public const string Name = "SubLight/Holo/GridDynamic";
			/// <summary>
			/// The color.
			/// </summary>
			public const string GridColor = "_GridColor";
			public const string GridTint = "_GridTint";
			/// <summary>
			/// How zoomed in the grid is, from 0 to 1.
			/// </summary>
			public const string Zoom = "_Zoom";
			/// <summary>
			/// How many tiles.
			/// </summary>
			public const string Tiling = "_Tiling";
			public const string TilingScalar = "_TilingScalar";
			/// <summary>
			/// The offset, a vector2 (or 4?) between (0,0) and (1,1)... check
			/// that though... not positive, might allow negatives.
			/// </summary>
			public const string Offset = "_Offset";
			/// <summary>
			/// How much of the grid is revealed, from 0 to 1.
			/// </summary>
			public const string RadiusProgress = "_RadiusProgress";
			public const string Alpha = "_Alpha";
		}

		public static class HoloGridScale
		{
			/// <summary>
			/// The name of the shader.
			/// </summary>
			public const string Name = "SubLight/Holo/GridScale";
			/// <summary>
			/// The color.
			/// </summary>
			public const string ColorTint = "_ColorTint";
			/// <summary>
			/// How zoomed in the grid is, from 0 to 5.
			/// </summary>
			public const string Zoom = "_Zoom";
			public const string Alpha = "_Alpha";
		}

		public static class HoloGridUnitScale
		{
			/// <summary>
			/// The name of the shader.
			/// </summary>
			public const string Name = "SubLight/Holo/GridUnitScale";
			/// <summary>
			/// The color.
			/// </summary>
			public const string ColorTint = "_ColorTint";
			/// <summary>
			/// From 0 to 1.
			/// </summary>
			public const string Progress = "_Progress";
			/// <summary>
			/// Does the bar progress from right to left, or the other way?
			/// </summary>
			public const string ProgressToRight = "_ProgressToRight";
			/// <summary>
			/// From 0 to 1.
			/// </summary>
			public const string FullProgress = "_FullProgress";
			public const string Alpha = "_Alpha";
		}

		public static class CameraMask
		{
			/// <summary>
			/// The name of the shader.
			/// </summary>
			public const string Name = "SubLight/CameraMask";
			/// <summary>
			/// The color and opacity of the mask.
			/// </summary>
			public const string MaskColor = "_MaskColor";
		}

		public static class RoomProjectionShared
		{
			//public const string Name = 

			public const int LayerCount = 3;
			public static int MaxLayer { get { return LayerCount - 1; } }

			public static string GetLayer(int order) { return "_Layer_" + order; }

			public static string GetWeight(int order) { return "_Weight_" + order; }
		}

		public static class RoomIrisGlow
		{
			/// <summary>
			/// The name of the shader.
			/// </summary>
			public const string Name = "SubLight/Room/IrisGlow";
			/// <summary>
			/// The color.
			/// </summary>
			public const string GlowColor = "_GlowColor";
		}

		public static class RoomIrisIdle
		{
			/// <summary>
			/// The name of the shader.
			/// </summary>
			public const string Name = "SubLight/Room/IrisIdle";
			/// <summary>
			/// The color.
			/// </summary>
			public const string GlowColor = "_GlowColor";
		}

		public static class RoomIrisGrid
		{
			/// <summary>
			/// The name of the shader.
			/// </summary>
			public const string Name = "SubLight/Room/IrisGrid";
			/// <summary>
			/// The color.
			/// </summary>
			public const string GridColor = "_GridColor";
			/// <summary>
			/// How zoomed in the grid is, from 0 to 1.
			/// </summary>
			public const string Zoom = "_Zoom";
			/// <summary>
			/// How many tiles.
			/// </summary>
			public const string Tiling = "_Tiling";
			/// <summary>
			/// The offset, a vector2 (or 4?) between (0,0) and (1,1)... check
			/// that though... not positive, might allow negatives.
			/// </summary>
			public const string Offset = "_Offset";
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