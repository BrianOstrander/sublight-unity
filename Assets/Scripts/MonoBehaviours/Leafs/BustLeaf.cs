using UnityEngine;
using UnityEngine.UI;

using TMPro;

namespace LunraGames.SubLight.Views
{
	public class BustLeaf : MonoBehaviour
	{
		public TextMeshProUGUI TitleLabel;
		public TextMeshProUGUI TransmissionLabel;

		public GameObject[] TransmissionStrengths;

		public TextMeshProUGUI PlacardName;
		public TextMeshProUGUI PlacardDescription;

		public RawImage AvatarStaticImage;

		public CanvasGroup TitleGroup;
		public CanvasGroup AvatarGroup;
		public CanvasGroup PlacardGroup;

		public Transform AvatarAnchor;
		public Transform AvatarDepthAnchor;

		public ParticleSystem TerminalTextParticles;
		public ParticleSystemRenderer TerminalTextParticlesRenderer;
	}
}