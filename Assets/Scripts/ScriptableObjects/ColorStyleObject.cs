using UnityEngine;

namespace LunraGames.SubLight
{
	public class ColorStyleObject : ScriptableObject
	{
		[SerializeField]
		Color color = Color.white;

		public Color Color { get { return color; } }
	}
}