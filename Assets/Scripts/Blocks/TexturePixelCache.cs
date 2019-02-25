using UnityEngine;

namespace LunraGames.SubLight
{
	public struct TexturePixelCache
	{
		public int X;
		public int Y;

		public float Red;
		public float Green;
		public float Blue;
		public float Alpha;

		public float RgbAverage;
		public float RgbaAverage;

		public TexturePixelCache(int x, int y, Color color)
		{
			X = x;
			Y = y;
			Red = color.r;
			Green = color.g;
			Blue = color.b;
			Alpha = color.a;

			RgbAverage = (Red + Green + Blue) / 3f;
			RgbaAverage = (Red + Green + Blue + Alpha) / 4f;
		}
	}
}