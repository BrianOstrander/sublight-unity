using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

namespace LunraGames.SubLight.Views
{
	public struct BustButtonsBlock
	{
		public string Message;
		public bool Used;
		public bool Interactable;
		public Action Click;
	}

	public class BustButtonsView : View, IBustButtonsView
	{
		class ButtonEntry
		{
			public BustButtonsBlock Block;
			public BustButtonLeaf Instance;

			public float IndexNormal;
			public bool IsSelection;
		}

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
		[SerializeField]
		Transform lookAtArea;
		[SerializeField]
		GameObject buttonArea;
		[SerializeField]
		BustLeaf bustPrefab;

		[Header("Button Animations ( 0 is hidden 1 is shown )")]
		[SerializeField]
		CurveRange buttonOpacityByOrder = CurveRange.Normal;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null

		//List<BustEntry> entries = new List<BustEntry>();

		public override void Reset()
		{
			base.Reset();

			//bustArea.transform.ClearChildren<BustLeaf>();
			//entries.Clear();

			bustPrefab.gameObject.SetActive(false);
		}

		protected override void OnLateIdle(float delta)
		{
			base.OnLateIdle(delta);

			lookAtArea.LookAt(lookAtArea.position + (lookAtArea.position - App.V.CameraPosition).FlattenY());
		}

		#region Events

		#endregion

		void OnDrawGizmos()
		{
			//Gizmos.color = Color.red;
			//Gizmos.DrawLine(bustPrefab.AvatarAnchor.position, bustPrefab.AvatarDepthAnchor.position);
		}
	}

	public interface IBustButtonsView : IView
	{

	}
}