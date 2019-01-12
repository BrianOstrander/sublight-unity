using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using TMPro;

namespace LunraGames.SubLight.Views
{
	public class WaypointView : UniverseScaleView, IWaypointView
	{
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
		[SerializeField]
		TextMeshProUGUI[] nameLabels;
		[SerializeField]
		TextMeshProUGUI[] distanceLabels;
		[SerializeField]
		TextMeshProUGUI[] unitLabels;
		[SerializeField]
		float lipRadius;
		[SerializeField]
		GameObject hintArea;
		[SerializeField]
		CanvasGroup group;

		[SerializeField]
		OpacityByDirection[] directions;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null

		string waypointName;

		public bool HintVisible { set { hintArea.SetActive(value); } }

		public float Orbit
		{
			set
			{

			}
		}

		public void SetDetails(string name, string distance, string units)
		{
			waypointName = name ?? "< Null >";

			foreach (var label in nameLabels) label.text = name ?? string.Empty;
			foreach (var label in distanceLabels) label.text = distance ?? string.Empty;
			foreach (var label in unitLabels) label.text = units ?? string.Empty;
		}

		protected override void OnPosition(Vector3 position, Vector3 rawPosition)
		{

		}

		protected override void OnOpacityStack(float opacity)
		{
			group.alpha = opacity;
		}

		public override void Reset()
		{
			base.Reset();

			SetDetails(null, null, null);
			HintVisible = false;

			foreach (var direction in directions) direction.enabled = false;
		}

		#region Events
		public void OnEnter()
		{
			foreach (var direction in directions) direction.enabled = true;
		}

		public void OnExit()
		{
			foreach (var direction in directions) direction.enabled = false;
		}

		public void OnClick()
		{

		}
		#endregion

		void OnDrawGizmos()
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(PositionArea.position, 0.05f);

#if UNITY_EDITOR
			Handles.color = Color.yellow;
			Handles.Label(PositionArea.position, new GUIContent(waypointName));
			Handles.color = Color.red;
			Handles.DrawWireDisc(Root.position, Vector3.up, lipRadius);
#endif

		}
	}

	public interface IWaypointView : IUniverseScaleView
	{
		bool HintVisible { set; }
		float Orbit { set; }
		void SetDetails(string name, string distance, string units);
	}
}