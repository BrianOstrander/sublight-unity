using System;
using System.Collections.Generic;

namespace LunraGames.SpaceFarm
{
	public class ModelProperty<T>
	{
		T currentValue;
		public Action<T> Changed = value => {};
		public T Value 
		{ 
			get { return currentValue; }
			set 
			{
				if (EqualityComparer<T>.Default.Equals(currentValue, value)) return;
				currentValue = value;
				Changed(value);
			}
		}

		public ModelProperty(T value = default(T))
		{
			currentValue = value;
		}

		/// <summary>
		/// Converts the ModelProperty to the associated type.
		/// </summary>
		/// <remarks>
		/// Only one way casting is supported so that the callbacks bound to the Changed action aren't lost.
		/// </remarks>
		/// <returns>The implicit.</returns>
		/// <param name="p">P.</param>
		public static implicit operator T(ModelProperty<T> p)
		{
			return p.Value;
		}
	}
}