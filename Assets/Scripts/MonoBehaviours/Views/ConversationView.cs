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
	public interface IConversationBlock
	{
		ConversationTypes Type { get; set; }
		string Message { get; set; }
	}

	public struct MessageConversationBlock : IConversationBlock
	{
		public ConversationTypes Type { get; set; }
		public string Message { get; set; }
	}

	public class ConversationView : View, IConversationView
	{
		struct Entry
		{
			public IConversationBlock Block;
			public ConversationLeaf Instance;
		}

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

		List<Entry> entries = new List<Entry>();

		Vector3 OutgoingBottomPosition { get { return outgoingAnchor.position + (bottomAnchor.position - topAnchor.position); } }

		public void AddToConversation(bool instant, params IConversationBlock[] blocks)
		{
			foreach (var block in blocks)
			{
				var entry = new Entry();
				entry.Block = block;
				entry.Instance = InstantiatePrefab(block);
				if (entry.Instance == null) continue;

			}
		}

		public ConversationLeaf InstantiatePrefab(IConversationBlock block)
		{
			switch (block.Type)
			{
				case ConversationTypes.MessageIncoming:
					return ApplyConversation(
						conversationArea.InstantiateChild(messageIncomingPrefab, localScale: messageIncomingPrefab.transform.localScale),
						(MessageConversationBlock)block
					);
				case ConversationTypes.MessageOutgoing:
					return ApplyConversation(
						conversationArea.InstantiateChild(messageOutgoingPrefab, localScale: messageOutgoingPrefab.transform.localScale),
						(MessageConversationBlock)block
					);
				default:
					Debug.LogError("Unrecognized ConversationType: " + block.Type);
					return null;
			}
		}

		public ConversationLeaf ApplyConversation(MessageConversationLeaf instance, MessageConversationBlock block)
		{
			instance.gameObject.SetActive(true);
			instance.transform.position = block.Type == ConversationTypes.MessageIncoming ? bottomAnchor.position : OutgoingBottomPosition;
			instance.transform.LookAt(instance.transform.position - lookOffset);
			instance.MessageLabel.text = block.Message;
			LayoutRebuilder.ForceRebuildLayoutImmediate(instance.RootArea);

			return instance;
		}

		public override void Reset()
		{
			base.Reset();

			entries.Clear();
			conversationArea.transform.ClearChildren();

			messageIncomingPrefab.gameObject.SetActive(false);
			messageOutgoingPrefab.gameObject.SetActive(false);
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
		void AddToConversation(bool instant, params IConversationBlock[] blocks);
	}
}