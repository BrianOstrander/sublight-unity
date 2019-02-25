using UnityEngine;
using UnityEngine.UI;

using TMPro;

namespace LunraGames.SubLight.Views
{
	public class GridVelocityOptionLeaf : MonoBehaviour
	{
		public XButton Button;
		public XButtonLeaf Toggle;
		public ParticleSystem EnterParticles;

		public GameObject EnabledArea;

		public GameObject DisabledArea;
		public ParticleSystem DisabledParticles;
	}
}