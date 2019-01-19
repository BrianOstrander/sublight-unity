using UnityEngine;

using TMPro;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight.Views
{
	public struct BustBlock
	{
		public string BustId;

		public string TitleSource;
		public string TitleClassification;

		public string TransmitionType;
		public string TransmitionStrength;
		public int TransmitionStrengthMinimimum;
		public int TransmitionStrengthMaximum;
		public float TransmitionStrengthCycleDuration;

		public string PlacardName;
		public string PlacardDescription;

		public int AvatarStaticIndex;
	}

	public class BustView : View, IBustView
	{
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
		[SerializeField]
		Transform lookAtArea;

		[SerializeField]
		public Texture2D[] AvatarsStatic;
		[SerializeField]
		public GameObject[] SignalStrengths;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null

		public override void Reset()
		{
			base.Reset();


		}

		public void SetBust(BustBlock bust)
		{
			Debug.Log("lol todo");
		}

		public void FocusBust(string bustId)
		{
			Debug.Log("lol todo");
		}

		protected override void OnLateIdle(float delta)
		{
			base.OnLateIdle(delta);

			lookAtArea.LookAt(lookAtArea.position + (lookAtArea.position - App.V.CameraPosition).FlattenY());
		}

		#region Events

		#endregion
	}

	public interface IBustView : IView
	{
		void SetBust(BustBlock bust);
		void FocusBust(string bustId);
	}
}