using System;

using UnityEngine;

namespace LunraGames.SubLight
{
	[Serializable]
	public struct SkinColor
	{
		public static SkinColor Default
		{
			get
			{
				return new SkinColor
				{
					pro = Color.white,
					personal = Color.white
				};
			}
		}

		[SerializeField]
		Color pro;
		[SerializeField]
		Color personal;

		public Color Pro
		{
			get { return pro; }
			set { pro = value; }
		}

		public Color Personal
		{
			get { return personal; }
			set { personal = value; }
		}

		public Color Current
		{
			get
			{
#if UNITY_EDITOR
				return UnityEditor.EditorGUIUtility.isProSkin ? pro : personal;
#else
				return pro;
#endif
			}
		}

		public static implicit operator Color(SkinColor c) { return c.Current; }
	}
}