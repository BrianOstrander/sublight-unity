using UnityEngine;

namespace LunraGames.SpaceFarm.Views
{
	public class StarSystemView : SystemView
	{
		[SerializeField]
		MeshRenderer meshRenderer;
		
		public override SystemTypes SystemType { get { return SystemTypes.Star; } }
		public override Color TravelColor { set { meshRenderer.material.color = value; } }
	}
}