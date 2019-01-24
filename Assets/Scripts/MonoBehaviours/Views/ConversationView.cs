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
		class Entry
		{
			public IConversationBlock Block;
			public ConversationLeaf Instance;
			/// <summary>
			/// Called when the Instance is active for the first time.
			/// </summary>
			/// <remarks>
			/// This will be null once it is run once.
			/// </remarks>
			public Action InitializeLayout;
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
				ConversationLeaf instance;
				Action initializeLayout;
				if (!InstantiatePrefab(block, out instance, out initializeLayout)) continue;

				var entry = new Entry();
				entry.Block = block;
				entry.Instance = instance;
				entry.InitializeLayout = initializeLayout;

				entries.Add(entry);
			}
		}

		public bool InstantiatePrefab(
			IConversationBlock block,
			out ConversationLeaf instance,
			out Action initializeLayout
		)
		{
			instance = null;
			initializeLayout = null;

			switch (block.Type)
			{
				case ConversationTypes.MessageIncoming:
					instance = ApplyConversation(
						conversationArea.InstantiateChild(messageIncomingPrefab, localScale: messageIncomingPrefab.transform.localScale),
						(MessageConversationBlock)block,
						out initializeLayout
					);
					return true;
				case ConversationTypes.MessageOutgoing:
					instance = ApplyConversation(
						conversationArea.InstantiateChild(messageOutgoingPrefab, localScale: messageOutgoingPrefab.transform.localScale),
						(MessageConversationBlock)block,
						out initializeLayout
					);
					return true;
				default:
					Debug.LogError("Unrecognized ConversationType: " + block.Type);
					return false;
			}
		}

		public ConversationLeaf ApplyConversation(
			MessageConversationLeaf instance,
			MessageConversationBlock block,
			out Action initializeLayout
		)
		{
			initializeLayout = null;

			instance.gameObject.SetActive(true);
			instance.transform.position = block.Type == ConversationTypes.MessageIncoming ? bottomAnchor.position : OutgoingBottomPosition;
			instance.transform.LookAt(instance.transform.position - lookOffset);

			instance.MessageLabel.text = block.Message;

			Action onInitializeLayout = () =>
			{
				if (instance.RootArea == null) Debug.LogError("No RootArea specified", instance);
				LayoutRebuilder.ForceRebuildLayoutImmediate(instance.RootArea);
				instance.MessageLabel.ForceMeshUpdate(false);
				Debug.Log("after Y: " + instance.SizeArea.WorldCornerSize().y);
				Debug.Log("after Lines: " + instance.MessageLabel.textInfo.lineCount);
			};

			if (instance.gameObject.activeInHierarchy) onInitializeLayout();
			else initializeLayout = onInitializeLayout;

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

		protected override void OnPrepare()
		{
			base.OnPrepare();

			foreach (var entry in entries)
			{
				if (entry.InitializeLayout != null)
				{
					entry.InitializeLayout();
					entry.InitializeLayout = null;
				}
			}
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