using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

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
			public enum Alignments
			{
				Unknown = 0,
				Incoming = 10,
				Outgoing = 20
			}

			public Entry NextEntry;
			public IConversationBlock Block;
			public ConversationLeaf Instance;

			/// <summary>
			/// Called when the Instance is active for the first time.
			/// </summary>
			/// <remarks>
			/// This will be null once it is run once.
			/// </remarks>
			public Action<Entry> InitializeLayout;

			#region Set on InitializeLayout
			public bool IsInitialized;
			/// <summary>
			/// The height in unity world units.
			/// </summary>
			/// <remarks>
			/// Layout must be initialized before this is valid.
			/// </remarks>
			public float Height;
			public Alignments Alignment;
   			#endregion
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
		float spacing;

		[SerializeField]
		MessageConversationLeaf messageIncomingPrefab;
		[SerializeField]
		MessageConversationLeaf messageOutgoingPrefab;
		[SerializeField]
		AttachmentConversationLeaf attachmentIncomingPrefab;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null

		Vector3 OutgoingBottomPosition { get { return outgoingAnchor.position + (bottomAnchor.position - topAnchor.position); } }

		List<Entry> entries = new List<Entry>();
		float verticalOffset;

		public void AddToConversation(bool instant, params IConversationBlock[] blocks)
		{
			Entry nextEntry = entries.LastOrDefault();
			foreach (var block in blocks)
			{
				ConversationLeaf instance;
				Action<Entry> initializeLayout;
				if (!InstantiatePrefab(block, out instance, out initializeLayout)) continue;

				var entry = new Entry();
				entry.NextEntry = nextEntry;
				entry.Block = block;
				entry.Instance = instance;
				entry.InitializeLayout = initializeLayout;

				if (instance.gameObject.activeInHierarchy) entry.InitializeLayout(entry);

				entries.Add(entry);

				nextEntry = entry;
			}
		}

		bool InstantiatePrefab(
			IConversationBlock block,
			out ConversationLeaf instance,
			out Action<Entry> initializeLayout
		)
		{
			instance = null;
			initializeLayout = null;

			switch (block.Type)
			{
				case ConversationTypes.MessageOutgoing:
				case ConversationTypes.MessageIncoming:
					instance = OnInstantiatePrefab(
						block.Type == ConversationTypes.MessageOutgoing ? messageOutgoingPrefab : messageIncomingPrefab,
						(MessageConversationBlock)block,
						out initializeLayout
					);
					return true;
				default:
					Debug.LogError("Unrecognized ConversationType: " + block.Type);
					return false;
			}
		}

		ConversationLeaf OnInstantiatePrefab(
			MessageConversationLeaf prefab,
			MessageConversationBlock block, // This may be needed for ather instantiators, leave it...
			out Action<Entry> initializeLayout
		)
		{
			var instance = conversationArea.InstantiateChild(prefab);

			initializeLayout = entry =>
			{
				OnInitializeEntry(entry, instance);

				var isSmall = instance.MessageLabel.textInfo.lineCount < 5;

				instance.BackgroundSmall.SetActive(isSmall);
				instance.BackgroundLarge.SetActive(!isSmall);
			};

			return instance;
		}

		void OnInitializeEntry(Entry entry, ConversationLeaf instance)
		{
			Assert.IsNotNull(entry, "A ConversationView.Entry must be specified");
			Assert.IsNotNull(instance, "ConversationLeaf Instance must be specified");
			Assert.IsFalse(entry.IsInitialized, "Entry has already been initialized");

			Assert.IsNotNull(instance.MessageLabel, "MessageLabel cannot be null");
			Assert.IsNotNull(instance.RootCanvas, "RootArea cannot be null");
			Assert.IsNotNull(instance.SizeArea, "SizeArea cannot be null");
			Assert.IsNotNull(instance.Group, "Group cannot be null");

			Assert.IsFalse(instance.gameObject.activeInHierarchy, "Unpredictable behaviour occurs when initializing inactive ConversationLeaf instance.");

			entry.IsInitialized = true;

			switch (entry.Block.Type)
			{
				case ConversationTypes.MessageIncoming:
					entry.Alignment = Entry.Alignments.Incoming;
					break;
				case ConversationTypes.MessageOutgoing:
					entry.Alignment = Entry.Alignments.Outgoing;
					break;
				default:
					Debug.LogError("Unrecognized ConversationType: " + entry.Block.Type);
					break;
			}

			instance.gameObject.SetActive(true);

			var beginPosition = Vector3.zero;

			switch (entry.Alignment)
			{
				case Entry.Alignments.Incoming: beginPosition = bottomAnchor.position; break;
				case Entry.Alignments.Outgoing: beginPosition = OutgoingBottomPosition; break;
				default:
					Debug.LogError("Unrecognized Alignment: " + entry.Alignment);
					break;
			}

			instance.transform.position = beginPosition;
			instance.transform.LookAt(instance.transform.position - lookOffset);

			instance.MessageLabel.text = entry.Block.Message;

			LayoutRebuilder.ForceRebuildLayoutImmediate(instance.RootCanvas);
			instance.MessageLabel.ForceMeshUpdate(false);

			entry.Height = Mathf.Abs(instance.SizeArea.WorldCornerSize().y);
		}

		public override void Reset()
		{
			base.Reset();

			entries.Clear();
			conversationArea.transform.ClearChildren();

			verticalOffset = 0f;

			messageIncomingPrefab.gameObject.SetActive(false);
			messageOutgoingPrefab.gameObject.SetActive(false);
		}

		protected override void OnPrepare()
		{
			base.OnPrepare();

			foreach (var entry in entries.Where(e => !e.IsInitialized))
			{
				if (entry.InitializeLayout == null) Debug.LogError("Cannot initialize an entry with no InitializeLayout specified", entry.Instance);
				else entry.InitializeLayout(entry);
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