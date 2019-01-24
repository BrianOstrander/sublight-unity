using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

//#if UNITY_EDITOR
//using UnityEditor;
//#endif

using TMPro;

namespace LunraGames.SubLight.Views
{
	public class ConversationView : View, IConversationView
	{
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
		[SerializeField]
		GameObject conversationArea;
		[SerializeField]
		Transform topAnchor;
		[SerializeField]
		Transform bottomAnchor;
		[SerializeField]
		Transform outgoingAnchor;
		[SerializeField]
		Vector3 lookOffset;

		[SerializeField]
		MessageConversationLeaf messageIncomingPrefab;
		[SerializeField]
		MessageConversationLeaf messageOutgoingPrefab;
		[SerializeField]
		AttachmentConversationLeaf attachmentIncomingPrefab;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null

		public override void Reset()
		{
			base.Reset();
		}

		protected override void OnLateIdle(float delta)
		{
			base.OnLateIdle(delta);

			//lookAtArea.LookAt(lookAtArea.position + (lookAtArea.position - App.V.CameraPosition).FlattenY());
		}

		#region Events
		#endregion

		void OnDrawGizmos()
		{
			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere(bottomAnchor.position, 0.05f);
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(topAnchor.position, 0.05f);
			Gizmos.DrawLine(topAnchor.position, topAnchor.position + lookOffset);
			Gizmos.DrawLine(topAnchor.position, outgoingAnchor.position);

			//#if UNITY_EDITOR
			//lookOffset = lookOffset.normalized;
			//Handles.color = Color.red;
			//Handles.Label(topAnchor.position + lookOffset, "( "+lookNormal.x.ToString("N3")+" , "+lookNormal.y.ToString("N3")+" , "+lookNormal.z.ToString("N3")+" )");
			//#endif
		}
	}

	public interface IConversationView : IView
	{
	}
}