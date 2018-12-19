using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace LunraGames.SubLight
{
	public class XButton :
		Selectable,
		IPointerDownHandler,
		IPointerClickHandler,
		IPointerEnterHandler,
		IPointerExitHandler,
		IBeginDragHandler,
		IEndDragHandler
	{
		const float FadeDurationDefault = 0.1f;

		new public enum SelectionState
		{
			Normal,
			Highlighted,
			Pressed
		}

		#region Inspector
		[SerializeField]
		FloatOverrideBlock fadeDuration;
		[SerializeField]
		XButtonLeaf[] leafs = new XButtonLeaf[0];
		[SerializeField]
		XButtonSoundObject globalSounds;
		[SerializeField]
		XButtonSoundBlock localSounds;
		public XButtonSoundBlock Sounds 
		{
			get { return globalSounds == null ? localSounds : globalSounds.Block; }
		}

		[SerializeField]
		UnityEvent onEnter;
		public UnityEvent OnEnter { get { return onEnter; } }
		[SerializeField]
		UnityEvent onExit;
		public UnityEvent OnExit { get { return onExit; } }
		[SerializeField]
		UnityEvent onClick;
		public UnityEvent OnClick { get { return onClick; } }
		[SerializeField]
		UnityEvent onDown;
		public UnityEvent OnDown { get { return onDown; } }
		[SerializeField]
		UnityEvent onDragBegin;
		public UnityEvent OnDragBegin { get { return onDragBegin; } }
		[SerializeField]
		UnityEvent onDragEnd;
		public UnityEvent OnDragEnd { get { return onDragEnd; } }
		#endregion

		float FadeDuration { get { return fadeDuration.Override ? fadeDuration.Value : FadeDurationDefault; } }

		SelectionState state;
		SelectionState State 
		{
			get { return state; }
			set 
			{
				if (state == value) return;
				state = value;
				CacheState();
				fadeTimeRemaining = FadeDuration;
			}
		}

		bool dragging;
		bool highlighted;
		float? fadeTimeRemaining;

		new void Awake() 
		{
			base.Awake();
			if (Application.isEditor && DevPrefs.ApplyXButtonStyleInEditMode) ApplyState(State);
		}

		new void OnEnable() 
		{
			base.OnEnable();
			if (!Application.isPlaying) return;

			highlighted = false;
			State = SelectionState.Normal;
			ApplyState(State);

			if (App.V != null) App.V.InteractionCount++;
		}

		new void OnDisable() 
		{
			base.OnDisable();
			if (!Application.isPlaying) return;
			StopHighlightSound();
			if (App.V != null) App.V.InteractionCount--;
		}

		void Update()
		{
			// If we're in the editor, we apply the config every frame so we can see changes live.
			if (Application.isEditor && DevPrefs.ApplyXButtonStyleInEditMode) ApplyState(State);

			if (fadeTimeRemaining.HasValue) 
			{
				var scalar = 1f - (fadeTimeRemaining.Value / FadeDuration);

				ApplyState(State, scalar);

				// We want to make sure we update the state at least once when zero time remains on the transition.
				if (Mathf.Approximately(0f, fadeTimeRemaining.Value)) 
				{
					fadeTimeRemaining = null;
					// Only set the state if it's going to change, since that resets the timer;
					if (!dragging)
					{
						var nextState = highlighted ? SelectionState.Highlighted : SelectionState.Normal;
						State = nextState;
					}
				} 
				else 
				{
					fadeTimeRemaining = Mathf.Max(fadeTimeRemaining.Value - Time.deltaTime, 0f);
				}
			}
		}

		public void ForceApplyState()
		{
			ApplyState(state);
		}

		void ApplyState(SelectionState appliedState, float scalar = 1f)
		{
			foreach (var leaf in leafs)
			{
				if (leaf == null) continue;
				leaf.UpdateState(appliedState, scalar, interactable);
			}
		}

		void CacheState()
		{
			foreach (var leaf in leafs)
			{
				if (leaf == null) continue;
				leaf.CacheState();
			}
		}

		void HighlightSoundBegin()
		{
			// TODO: This
			//if (sounds.HighlightedSound != null) App.P.Audio.PlayClip(sounds.HighlightedSound, looping: true)
		}

		void StopHighlightSound()
		{
			// TODO: This
			//if (_highlightSource == null || !_highlightSource.isPlaying) return;
			//_highlightSource.Stop();
		}

		public override void OnPointerDown(PointerEventData eventData)
		{
			base.OnPointerDown(eventData);
			if (!IsActive() || !IsInteractable()) return;
			State = SelectionState.Pressed;
			if (onDown != null) onDown.Invoke();
		}

		public void OnBeginDrag(PointerEventData eventData)
		{
			if (gameObject == null || !gameObject.activeInHierarchy || !IsInteractable()) return;
			if (onDragBegin != null) onDragBegin.Invoke();
			dragging = true;
		}

		public void OnEndDrag(PointerEventData eventData)
		{
			if (gameObject == null || !gameObject.activeInHierarchy || !IsInteractable()) return;
			if (onDragEnd != null) onDragEnd.Invoke();
			dragging = false;
			if (highlighted)
			{
				if (!fadeTimeRemaining.HasValue) State = SelectionState.Highlighted;
			}
			else
			{
				State = SelectionState.Normal;
				StopHighlightSound();
				if (onExit != null) onExit.Invoke();
			}
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			if (!IsActive()) return;
			if (!IsInteractable())
			{
				if (Sounds.PressedSound != null) App.Audio.PlayClip(Sounds.DisabledSound);
				return;
			}
			StopHighlightSound();
			if (Sounds.PressedSound != null) App.Audio.PlayClip(Sounds.PressedSound);
			if (onClick != null) onClick.Invoke();
			dragging = false;
		}

		public override void OnPointerEnter(PointerEventData eventData)
		{
			base.OnPointerEnter(eventData);
			if (!IsActive() || !IsInteractable()) return;
			highlighted = true;

			if (State == SelectionState.Pressed) return;

			if (Sounds.EnteredSound != null) App.Audio.PlayClip(Sounds.EnteredSound);
			State = SelectionState.Highlighted;
			HighlightSoundBegin();
			if (onEnter != null) onEnter.Invoke();
		}

		public override void OnPointerExit(PointerEventData eventData)
		{
			base.OnPointerExit(eventData);
			if (!IsActive() || !IsInteractable()) return;
			highlighted = false;

			if (dragging) return;

			State = SelectionState.Normal;
			StopHighlightSound();
			if (onExit != null) onExit.Invoke();
		}
	}
}
