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
			public Vector3 ColumnLocalPosition;
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
		GameObject conversationAnchor;
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

		[SerializeField]
		AnimationCurve scrollCurve;

		[Header("Transition Animation")]
		[SerializeField]
		CurveRange showHorizontalOffsetCurve;
		[SerializeField]
		CurveRange showDepthCurve;
		[SerializeField]
		CurveRange closeHorizontalOffsetCurve;
		[SerializeField]
		CurveRange closeDepthCurve;

		[Header("Entry Animation: Bottom")]
		[SerializeField]
		float bottomOffsetThreshold;
		[SerializeField]
		AnimationCurve bottomOffsetOpacityCurve;
		[SerializeField]
		AnimationCurve bottomOffsetScaleCurve;

		[Header("Entry Animation: Top")]
		[SerializeField]
		float topLimit;
		[SerializeField]
		float topOffsetThresholdDelta;
		[SerializeField]
		AnimationCurve topOffsetOpacityCurve;
		[SerializeField]
		AnimationCurve topOffsetScaleCurve;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null

		float TopOffsetThreshold { get { return topLimit + topOffsetThresholdDelta; } }

		Vector3 GetOffset(
			float scalar,
			CurveRange depthCurve,
			CurveRange horizontalCurve
		)
		{
			return  new Vector3(
				horizontalCurve.Evaluate(scalar),
				0f,
				depthCurve.Evaluate(scalar)
			);
		}

		Vector3 GetShowOffset(float scalar) { return GetOffset(scalar, showDepthCurve, showHorizontalOffsetCurve); }
		Vector3 GetCloseOffset(float scalar) { return GetOffset(scalar, closeDepthCurve, closeHorizontalOffsetCurve); }

		List<Entry> entries = new List<Entry>();
		float verticalScrollCurrent;
		float verticalScrollCurrentAtStartOfAnimation;
		float? verticalScrollTarget;
		bool waitingForInstantScroll;
		float verticalScrollRemaining;

		Action addDone;

		public void AddToConversation(
			bool instant,
			Action done,
			params IConversationBlock[] blocks
		)
		{
			waitingForInstantScroll = instant;
			addDone = done;
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
			var instance = conversationArea.InstantiateChild(prefab, setActive: true);

			initializeLayout = entry =>
			{
				OnInitializeEntry(entry, instance);

				var isSmall = instance.MessageLabel.textInfo.lineCount < 4;

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

			Assert.IsTrue(instance.gameObject.activeInHierarchy, "Unpredictable behaviour occurs when initializing inactive ConversationLeaf instance.");

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

			switch (entry.Alignment)
			{
				case Entry.Alignments.Incoming: entry.ColumnLocalPosition = incomingAnchor.localPosition; break;
				case Entry.Alignments.Outgoing: entry.ColumnLocalPosition = outgoingAnchor.localPosition; break;
				default:
					Debug.LogError("Unrecognized Alignment: " + entry.Alignment);
					break;
			}

			instance.MessageLabel.text = entry.Block.Message;

			LayoutRebuilder.ForceRebuildLayoutImmediate(instance.RootCanvas);
			instance.MessageLabel.ForceMeshUpdate(false);

			entry.Height = Mathf.Abs(instance.SizeArea.WorldCornerSize().y);

			var scrollDelta = verticalSpacing + entry.Height;

			if (entry.PreviousEntry == null) entry.VerticalOffset = -scrollDelta;
			else entry.VerticalOffset = entry.PreviousEntry.VerticalOffset - scrollDelta;

			UpdateVerticalScroll(entry);

			instance.transform.LookAt(instance.transform.position - lookOffset);

			verticalScrollTarget = (verticalScrollTarget ?? 0f) + scrollDelta;
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

			addDone = null;

			conversationArea.transform.ClearChildren();

			messageIncomingPrefab.gameObject.SetActive(false);
			messageOutgoingPrefab.gameObject.SetActive(false);

			conversationAnchor.transform.localPosition = Vector3.zero;
		}

		protected override void OnPrepare()
		{
			base.OnPrepare();

			foreach (var entry in entries.Where(e => !e.IsInitialized))
			{
				if (entry.InitializeLayout == null) Debug.LogError("Cannot initialize an entry with no InitializeLayout specified", entry.Instance);
				else entry.InitializeLayout(entry);
			}

			conversationAnchor.transform.localPosition = GetShowOffset(0f);
		}

		protected override void OnShowing(float scalar)
		{
			base.OnShowing(scalar);

			conversationAnchor.transform.localPosition = GetShowOffset(scalar);
		}

		protected override void OnShown()
		{
			base.OnShown();

			conversationAnchor.transform.localPosition = Vector3.zero;
		}

		protected override void OnClosing(float scalar)
		{
			base.OnClosing(scalar);

			conversationAnchor.transform.localPosition = GetCloseOffset(scalar);
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
				else currDistance = currDistance * scrollCurve.Evaluate(1f - (verticalScrollRemaining / verticalScrollDuration));

				verticalScrollCurrent = verticalScrollCurrentAtStartOfAnimation + currDistance;
			}

			foreach (var entry in entries) UpdateVerticalScroll(entry);

			if (!verticalScrollTarget.HasValue && addDone != null)
			{
				var oldAddDone = addDone;
				addDone = null;
				oldAddDone();
			}
		}

		protected override void OnOpacityStack(float opacity)
		{
			foreach (var entry in entries) entry.Instance.CanvasGroup.alpha = opacity;
		}

		void UpdateVerticalScroll(Entry entry)
		{
			var offset = entry.VerticalOffset + verticalScrollCurrent;

			var opacity = 0f;
			var scale = 0.001f;

			if (0f <= offset || offset <= topLimit)
			{
				opacity = 1f;
				scale = 1f;
			}
			else if (bottomOffsetThreshold < offset && offset < TopOffsetThreshold)
			{
				if (offset < 0f)
				{
					var progress = 1f - (Mathf.Abs(offset) / Mathf.Abs(bottomOffsetThreshold));
					opacity = bottomOffsetOpacityCurve.Evaluate(progress);
					scale = scale + (bottomOffsetScaleCurve.Evaluate(progress) * (1f - scale));
				}
				else
				{
					var progress = (offset - topLimit) / topOffsetThresholdDelta;
					opacity = topOffsetOpacityCurve.Evaluate(progress);
					scale = scale + (topOffsetScaleCurve.Evaluate(progress) * (1f - scale));
				}
			}

			entry.Instance.CanvasGroup.alpha = OpacityStack;

			entry.Instance.transform.localPosition = entry.ColumnLocalPosition.NewY(entry.ColumnLocalPosition.y + offset);
			entry.Instance.transform.localScale = Vector3.one * scale;
			entry.Instance.Group.alpha = opacity;
		}

		#region Events
		#endregion

		void OnDrawGizmos()
		{
			if (verticalScrollTarget.HasValue)
			{
				Gizmos.color = Color.blue;
				Gizmos.DrawLine(incomingAnchor.position, incomingAnchor.position + (Vector3.up * verticalScrollTarget.Value));
			}

			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(incomingAnchor.position, 0.05f);
			Gizmos.DrawLine(incomingAnchor.position, incomingAnchor.position + lookOffset);
			Gizmos.DrawLine(incomingAnchor.position, outgoingAnchor.position);
			Gizmos.DrawLine(incomingAnchor.position, incomingAnchor.position + (Vector3.up * topLimit));
			Gizmos.color = Color.white;
			Gizmos.DrawLine(incomingAnchor.position, incomingAnchor.position + (Vector3.up * bottomOffsetThreshold));
			Gizmos.DrawLine(incomingAnchor.position + (Vector3.up * topLimit), incomingAnchor.position + (Vector3.up * TopOffsetThreshold));

			Gizmos.color = Color.green;
			Gizmos.DrawLine(incomingAnchor.position + GetShowOffset(0f), incomingAnchor.position + GetShowOffset(1f));

			Gizmos.color = Color.red;
			Gizmos.DrawLine(incomingAnchor.position + GetCloseOffset(0f), incomingAnchor.position + GetCloseOffset(1f));
			//var showStart

			//Gizmos.DrawWireSphere(incomingAnchor.position, 0.05f);


			//#if UNITY_EDITOR
			//lookOffset = lookOffset.normalized;
			//Handles.color = Color.red;
			//Handles.Label(topAnchor.position + lookOffset, "( "+lookNormal.x.ToString("N3")+" , "+lookNormal.y.ToString("N3")+" , "+lookNormal.z.ToString("N3")+" )");
			//#endif
		}
	}

	public interface IConversationView : IView
	{
		void AddToConversation(
			bool instant,
			Action done,
			params IConversationBlock[] blocks
		);
	}
}