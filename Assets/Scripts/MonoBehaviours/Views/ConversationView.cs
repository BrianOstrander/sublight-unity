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

			public Entry PreviousEntry;
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
			public Vector3 ColumnPosition;
			#endregion

			/// <summary>
			/// The vertical offset without any scrolling.
			/// </summary>
			public float VerticalOffset;
		}

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
		[SerializeField]
		GameObject conversationArea;
		[SerializeField]
		Transform incomingAnchor;
		[SerializeField]
		Transform outgoingAnchor;
		[SerializeField]
		Vector3 lookOffset;
		[SerializeField]
		float verticalSpacing;
		[SerializeField]
		float verticalScrollDuration;

		[SerializeField]
		MessageConversationLeaf messageIncomingPrefab;
		[SerializeField]
		MessageConversationLeaf messageOutgoingPrefab;
		[SerializeField]
		AttachmentConversationLeaf attachmentIncomingPrefab;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null

		//Vector3 OutgoingBottomPosition { get { return outgoingAnchor.position + (bottomAnchor.position - topAnchor.position); } }

		List<Entry> entries = new List<Entry>();
		float verticalScrollCurrent;
		float verticalScrollCurrentAtStartOfAnimation;
		float? verticalScrollTarget;
		bool waitingForInstantScroll;
		float verticalScrollRemaining;

		public void AddToConversation(bool instant, params IConversationBlock[] blocks)
		{
			waitingForInstantScroll = instant;
			if (!instant)
			{
				verticalScrollCurrentAtStartOfAnimation = verticalScrollCurrent;
				verticalScrollRemaining = verticalScrollDuration;
			}

			Entry previousEntry = entries.LastOrDefault();
			foreach (var block in blocks)
			{
				ConversationLeaf instance;
				Action<Entry> initializeLayout;
				if (!InstantiatePrefab(block, out instance, out initializeLayout)) continue;

				var entry = new Entry();
				entry.PreviousEntry = previousEntry;
				entry.Block = block;
				entry.Instance = instance;
				entry.InitializeLayout = initializeLayout;

				if (instance.gameObject.activeInHierarchy) entry.InitializeLayout(entry);

				entries.Add(entry);

				previousEntry = entry;
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

			switch (entry.Alignment)
			{
				case Entry.Alignments.Incoming: entry.ColumnPosition = incomingAnchor.position; break;
				case Entry.Alignments.Outgoing: entry.ColumnPosition = outgoingAnchor.position; break;
				default:
					Debug.LogError("Unrecognized Alignment: " + entry.Alignment);
					break;
			}

			instance.MessageLabel.text = entry.Block.Message;

			LayoutRebuilder.ForceRebuildLayoutImmediate(instance.RootCanvas);
			instance.MessageLabel.ForceMeshUpdate(false);

			entry.Height = Mathf.Abs(instance.SizeArea.WorldCornerSize().y);

			float? scrollDelta = null;

			if (entry.PreviousEntry == null) entry.VerticalOffset = 0f;
			else
			{
				scrollDelta = verticalSpacing + entry.Height;
				entry.VerticalOffset = entry.PreviousEntry.VerticalOffset - scrollDelta.Value;
			}

			UpdateVerticalScroll(entry);

			instance.transform.LookAt(instance.transform.position - lookOffset);

			if (scrollDelta.HasValue)
			{
				verticalScrollTarget = (verticalScrollTarget ?? 0f) + scrollDelta.Value;
			}
		}

		public override void Reset()
		{
			base.Reset();

			entries.Clear();
			verticalScrollCurrent = 0f;
			verticalScrollCurrentAtStartOfAnimation = 0f;
			verticalScrollTarget = null;
			waitingForInstantScroll = false;
			verticalScrollRemaining = 0f;

			conversationArea.transform.ClearChildren();

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

		protected override void OnIdle(float delta)
		{
			base.OnIdle(delta);

			if (!verticalScrollTarget.HasValue) return;

			if (waitingForInstantScroll)
			{
				verticalScrollCurrent += verticalScrollTarget.Value;
				verticalScrollTarget = null;
				waitingForInstantScroll = false;
			}
			else
			{
				verticalScrollRemaining = Mathf.Max(0f, verticalScrollRemaining - delta);

				var currDistance = verticalScrollTarget.Value;

				if (Mathf.Approximately(0f, verticalScrollRemaining)) verticalScrollTarget = null;
				else currDistance = currDistance * (1f - (verticalScrollRemaining / verticalScrollDuration));

				verticalScrollCurrent = verticalScrollCurrentAtStartOfAnimation + currDistance;
			}

			foreach (var entry in entries) UpdateVerticalScroll(entry);
		}

		void UpdateVerticalScroll(Entry entry)
		{
			entry.Instance.transform.position = entry.ColumnPosition.NewY(entry.ColumnPosition.y + entry.VerticalOffset + verticalScrollCurrent);
		}

		#region Events
		#endregion

		void OnDrawGizmos()
		{
			if (verticalScrollTarget.HasValue)
			{
				Gizmos.color = Color.green;
				Gizmos.DrawLine(incomingAnchor.position, incomingAnchor.position + (Vector3.up * verticalScrollTarget.Value));
			}

			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(incomingAnchor.position, 0.05f);
			Gizmos.DrawLine(incomingAnchor.position, incomingAnchor.position + lookOffset);
			Gizmos.DrawLine(incomingAnchor.position, outgoingAnchor.position);

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