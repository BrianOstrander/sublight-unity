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

			/// <summary>
			/// A value constantly looping from 0.0 to 1.0, speeding up or
			/// slowing down depending on what the transit state is.
			/// </summary>
			public const string TransitTimeNormal = "_TransitTimeNormal";
			/// <summary>
			/// A value constantly counting up at the rate the game is playing,
			/// and accounting for the time increase in transit.
			/// </summary>
			public const string TransitTimeElapsed = "_TransitTimeElapsed";
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

		public static class HoloLipAdditive
		{
			/// <summary>
			/// The name of the shader.
			/// </summary>
			public const string Name = "SubLight/Holo/LipAdditive";
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
			public const string Alpha = "_Alpha";
		}

		public static class HoloLipAlpha
		{
			/// <summary>
			/// The name of the shader.
			/// </summary>
			public const string Name = "SubLight/Holo/LipAlpha";
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
			public const string Alpha = "_Alpha";
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

		public static class HoloCircleMaskWorld
		{
			// This is a subgraph...

			public const string WorldOrigin = "_WorldOrigin";
			public const string WorldRadius = "_WorldRadius";
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

		public static class HoloTextureColorAlphaMasked
		{
			/// <summary>
			/// The name of the shader.
			/// </summary>
			public const string Name = "SubLight/Holo/TextureColorAlphaMasked";
			public const string PrimaryTexture = "_PrimaryTexture";
			public const string PrimaryColor = "_PrimaryColor";
			public const string Alpha = "_Alpha";

			public const string WorldOrigin = "_WorldOrigin";
			public const string WorldRadius = "_WorldRadius";
		}

		public static class HoloTextureColorAlphaMaskedLine
		{
			/// <summary>
			/// The name of the shader.
			/// </summary>
			public const string Name = "SubLight/Holo/TextureColorAlphaMaskedLine";
			public const string PrimaryTexture = "_PrimaryTexture";
			public const string PrimaryColor = "_PrimaryColor";
			public const string SecondaryColor = "_SecondaryColor";
			public const string Alpha = "_Alpha";

			public const string WorldOrigin = "_WorldOrigin";
			public const string WorldRadius = "_WorldRadius";
		}

		public static class HoloTextureColorAlpha
		{
			/// <summary>
			/// The name of the shader.
			/// </summary>
			public const string Name = "SubLight/Holo/TextureColorAlpha";
			public const string PrimaryTexture = "_PrimaryTexture";
			public const string PrimaryColor = "_PrimaryColor";
			public const string Alpha = "_Alpha";
		}

		public static class TextureColorAlphaScrollingRange
		{
			public const string Name = "SubLight/Holo/TextureColorAlphaScrollingRange";
			public const string PrimaryTexture = "_PrimaryTexture";
			public const string PrimaryColor = "_PrimaryColor";
			public const string Alpha = "_Alpha";
			public const string ScrollSpeed = "_ScrollSpeed";
			public const string Range = "_Range";
			public const string RangeBegin = "_RangeBegin";
			public const string RangeThreshold = "_RangeThreshold";
			public const string RangeFadeFalloff = "_RangeFadeFalloff";
		}

		public static class HoloDistanceFieldColor
		{
			/// <summary>
			/// The name of the shader.
			/// </summary>
			public const string Name = "SubLight/Holo/DistanceField/Color";
			public const string DistanceField = "_DistanceField";
			public const string PrimaryColor = "_PrimaryColor";
			public const string Alpha = "_Alpha";
			public const string DistanceFieldRange = "_DistanceFieldRange";
		}

		public static class HoloDistanceFieldColorConstant
		{
			/// <summary>
			/// The name of the shader.
			/// </summary>
			public const string Name = "SubLight/Holo/DistanceField/ColorConstant";
			public const string DistanceField = "_DistanceField";
			public const string PrimaryColor = "_PrimaryColor";
			public const string Alpha = "_Alpha";
			public const string DistanceFieldRange = "_DistanceFieldRange";
			public const string NearFarPlanes = "_NearFarPlanes";
		}

		public static class HoloDistanceFieldColorConstantVanish
		{
			/// <summary>
			/// The name of the shader.
			/// </summary>
			public const string Name = "SubLight/Holo/DistanceField/ColorConstantVanish";
			public const string DistanceField = "_DistanceField";
			public const string PrimaryColor = "_PrimaryColor";
			public const string Alpha = "_Alpha";
			public const string DistanceFieldRange = "_DistanceFieldRange";
			public const string NearFarPlanes = "_NearFarPlanes";
			public const string Vanish = "_Vanish";
		}

		public static class HoloDistanceFieldColorConstantScrollingRange
		{
			/// <summary>
			/// The name of the shader.
			/// </summary>
			public const string Name = "SubLight/Holo/DistanceField/ColorConstantScrollingRange";
			public const string DistanceField = "_DistanceField";
			public const string PrimaryColor = "_PrimaryColor";
			public const string Alpha = "_Alpha";
			public const string DistanceFieldRange = "_DistanceFieldRange";
			public const string NearFarPlanes = "_NearFarPlanes";
			public const string ScrollSpeed = "_ScrollSpeed";
			public const string Range = "_Range";
			public const string RangeBegin = "_RangeBegin";
			public const string RangeThreshold = "_RangeThreshold";
			public const string RangeFadeFalloff = "_RangeFadeFalloff";
		}

		public static class HoloDistanceFieldColorShiftConstant
		{
			/// <summary>
			/// The name of the shader.
			/// </summary>
			public const string Name = "SubLight/Holo/DistanceField/ColorShiftConstant";
			public const string DistanceField = "_DistanceField";
			public const string PrimaryColor = "_PrimaryColor";
			public const string SecondaryColor = "_SecondaryColor";
			public const string ShiftVertical = "_ShiftVertical";
			public const string Alpha = "_Alpha";
			public const string DistanceFieldRange = "_DistanceFieldRange";
			public const string NearFarPlanes = "_NearFarPlanes";
			public const string ShiftMapProgress = "_ShiftMapProgress";
		}

		public static class HoloCelestialSystemIconColor
		{
			/// <summary>
			/// The name of the shader.
			/// </summary>
			public const string Name = "SubLight/Holo/CelestialSystemIconColor";
			public const string PrimaryColor = "_PrimaryColor";
			public const string SecondaryColor = "_SecondaryColor";
			public const string TertiaryColor = "_TertiaryColor";
			public const string ShiftedPrimaryColor = "_ShiftedPrimaryColor";
			public const string ShiftedSecondaryColor = "_ShiftedSecondaryColor";
			public const string ShiftedTertiaryColor = "_ShiftedTertiaryColor";
			public const string ShiftProgress = "_ShiftProgress";
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

		public static class HoloGridBackgroundRange
		{
			/// <summary>
			/// The name of the shader.
			/// </summary>
			public const string Name = "SubLight/Holo/GridBackgroundRange";
			/// <summary>
			/// The color.
			/// </summary>
			public const string Alpha = "_Alpha";
			public const string RangeOrigin = "_RangeOrigin";
			public const string RangeRadius = "_RangeRadius";
			public const string RangeFalloff = "_RangeFalloff";
			public const string RangeColorPrimary = "_RangeColorPrimary";
			public const string RangeColorSecondary = "_RangeColorSecondary";
			public const string RangeColorTertiary = "_RangeColorTertiary";
			public const string RangeThreshold = "_RangeThreshold";
			public const string RangeFalloffGradient = "_RangeFalloffGradient";

			public const string TargetRangeColorPrimary = "_TargetRangeColorPrimary";
			public const string TargetRangeOrigin = "_TargetRangeOrigin";
			public const string TargetRangeRadius = "_TargetRangeRadius";
			public const string TargetRangeProgress = "_TargetRangeProgress";
		}

		public static class HoloGridRange
		{
			/// <summary>
			/// The name of the shader.
			/// </summary>
			public const string Name = "SubLight/Holo/GridRange";
			/// <summary>
			/// The color.
			/// </summary>
			public const string Alpha = "_Alpha";
			public const string RangeOrigin = "_RangeOrigin";
			public const string RangeRadius = "_RangeRadius";
			public const string RangeFalloff = "_RangeFalloff";
			public const string RangeColorPrimary = "_RangeColorPrimary";
			public const string RangeColorSecondary = "_RangeColorSecondary";
			public const string RangeThreshold = "_RangeThreshold";
			public const string RangeFalloffGradient = "_RangeFalloffGradient";
			public const string RadiusV = "_RadiusV";
			public const string RadiusVFalloff = "_RadiusVFalloff";
			public const string RadiusUTiling = "_RadiusUTiling";
			public const string GridRangeMap = "_GridRangeMap";
			public const string RadiusUOffset = "_RadiusUOffset";
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
			public const string GlowIntensity = "_GlowIntensity";
			public const string VerticalOffset = "_VerticalOffset";
			public const string Speed = "_Speed";
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
			public const string GridIntensity = "_GridIntensity";
		}

		public static class HoloWidgetGridVelocitySelection
		{
			public const string Name = "Sublight/Holo/Widget/GridVelocitySelection";
			public const string Maximized = "_Maximized";
			public const string AnchorMinimum = "_AnchorMinimum";
			public const string AnchorCurrent = "_AnchorCurrent";
			public const string AnchorMaximum = "_AnchorMaximum";

			public const string MinimumColor = "_MinimumColor";
			public const string CurrentColor = "_CurrentColor";

			public const string Alpha = "_Alpha";
		}

		public static class HoloPinWheel
		{
			public const string Name = "SubLight/Holo/PinWheel";
			public const string Speed = "_Speed";
			public const string Rotation = "_Rotation";
		}

		public static class HoloPinInside
		{
			public const string Name = "SubLight/Holo/PinInside";
			public const string PrimaryColor = "_PrimaryColor";
			public const string Alpha = "_Alpha";
		}

		public static class HoloPinOutside
		{
			public const string Name = "SubLight/Holo/PinOutside";
			public const string PrimaryColor = "_PrimaryColor";
			public const string Alpha = "_Alpha";
		}

		public static class HoloBustAvatarStatic
		{
			public const string Name = "SubLight/Holo/Bust/AvatarStatic";
			public const string ColorMap = "_ColorMap";
		}

		public static class HoloWidgetGridRing
		{
			public const string Name = "SubLight/Holo/Widget/GridRing";
			public const string Alpha = "_Alpha";
			public const string VMaximum = "_VMaximum";
			public const string UOffset = "_UOffset";

			public const string WorldOrigin = "_WorldOrigin";
			public const string WorldRadius = "_WorldRadius";
		}

		public static class HoloVerticalMaskAdditive
		{
			/// <summary>
			/// The name of the shader.
			/// </summary>
			public const string Name = "SubLight/Holo/VerticalMaskAdditive";
			/// <summary>
			/// The opacity
			/// </summary>
			public const string WorldOriginHeight = "_WorldOriginHeight";
			public const string Alpha = "_Alpha";
		}

        public static class HoloGamemodePortalRim
        {
            /// <summary>
            /// The name of the shader.
            /// </summary>
            public const string Name = "SubLight/Holo/GamemodePortalRim";
            public const string PrimaryColor = "_PrimaryColor";
        }

        public static class HoloGamemodePortalParallax
		{
			/// <summary>
			/// The name of the shader.
			/// </summary>
			public const string Name = "SubLight/Holo/GamemodePortalParallax";
			public const string PrimaryTexture = "_PrimaryTexture";
			public const string ParallaxCoordinates = "_ParallaxCoordinates";
			public const string ChromaticOffsetScale = "_ChromaticOffsetScale";
			public const string Alpha = "_Alpha";
		}

		#region NonSubLight Shaders
		public static class SkyboxPanoramic
		{
			public const string Name = "Skybox/Panoramic";
			public const string Rotation = "_Rotation";
			public const string Exposure = "_Exposure";
		}
  		#endregion

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