using System;
using UnityEngine;
using Random = System.Random;

namespace LunraGames.NumberDemon
{
	public class Demon 
	{
		static Random SeedGenerator = new Random();
		Random Generator;

		public Demon() : this(SeedGenerator.Next()) {}
		public Demon(int seed) { Generator = new Random(seed); }

		#region Properties
		public int NextInteger { get { return Generator.Next(); } }
		public long NextLong { get { return BitConverter.ToInt64(NextBytes(8), 0); } }
		public float NextFloat { get { return (float)Generator.NextDouble(); } }
		public Color NextColor { get { return new Color(NextFloat, NextFloat, NextFloat); } }

		#endregion

		#region Methods
		public byte[] NextBytes(int count)
		{
			var bytes = new byte[count];
			Generator.NextBytes(bytes);
			return bytes;
		}

		public int GetNextInteger(int min = 0, int max = int.MaxValue) { return Generator.Next(min, max); }

		public float GetNextFloat(float value0 = 0f, float value1 = float.MaxValue) 
		{
			if (value1 < value0) return GetNextFloat(value1, value0);
			if (Mathf.Approximately(value0, value1)) return value0;
			var delta = value1 - value0;
			return value0 + (NextFloat * delta);
		}
		#endregion

	}
}