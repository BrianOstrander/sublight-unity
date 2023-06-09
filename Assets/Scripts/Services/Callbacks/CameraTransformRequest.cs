﻿using System;

using UnityEngine;

namespace LunraGames.SubLight
{
	public struct CameraTransformRequest
	{
		public enum States
		{
			Unknown = 0,
			Request = 10,
			Active = 20,
			Complete = 30
		}

		public enum Transforms
		{
			Unknown = 0,
			Input = 10,
			Animation = 20,
			Settle = 30
		}

		public static CameraTransformRequest Default
		{
			get
			{
				return new CameraTransformRequest(
					States.Complete,
					Transforms.Animation,
					null,
					null,
					null
				);
			}
		}

		public static CameraTransformRequest Input(
			float? yaw = null,
			float? pitch = null,
			float? radius = null
		)
		{
			return new CameraTransformRequest(
				States.Request,
				Transforms.Input,
				yaw,
				pitch,
				radius
			);
		}

		public static CameraTransformRequest InputComplete()
		{
			return new CameraTransformRequest(States.Complete, Transforms.Input, null, null, null);
		}

		public static CameraTransformRequest Animation(
			float? yaw = null,
			float? pitch = null,
			float? radius = null,
			float duration = 0f,
			Action done = null
		)
		{
			return new CameraTransformRequest(
				States.Request,
				Transforms.Animation,
				yaw,
				pitch,
				radius,
				duration,
				done
			);
		}

		public static CameraTransformRequest Settle(
			float? yaw = null,
			float? pitch = null,
			float? radius = null
		)
		{
			return new CameraTransformRequest(
				States.Request,
				Transforms.Settle,
				yaw,
				pitch,
				radius
			);
		}

		public readonly States State;
		public readonly Transforms Transform;
		public readonly float? Yaw;
		public readonly float? Pitch;
		public readonly float? Radius;
		public readonly float Duration;
		public readonly bool IsInstant;
		public readonly Action Done;

		public float YawValue(float defaultValue = 0f) { return Yaw.HasValue ? Yaw.Value : defaultValue; }
		public float PitchValue(float defaultValue = 0f) { return Pitch.HasValue ? Pitch.Value : defaultValue; }
		public float RadiusValue(float defaultValue = 0f) { return Radius.HasValue ? Radius.Value : defaultValue; }


		public CameraTransformRequest(
			States state,
			Transforms transform,
			float? yaw,
			float? pitch,
			float? radius,
			float duration = 0f,
			Action done = null
		)
		{
			State = state;
			Transform = transform;
			Yaw = yaw;
			Pitch = pitch;
			Radius = radius;
			Duration = duration;
			IsInstant = Mathf.Approximately(0f, duration);
			Done = done ?? ActionExtensions.Empty;
		}

		public CameraTransformRequest Duplicate(States? state)
		{
			return new CameraTransformRequest(
				state.HasValue ? state.Value : State,
				Transform,
				Yaw,
				Pitch,
				Radius,
				Duration,
				Done
			);
		}
	}
}