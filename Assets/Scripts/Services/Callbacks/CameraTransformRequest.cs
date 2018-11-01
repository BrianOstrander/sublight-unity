using System;

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
			Animation = 20
		}

		public static CameraTransformRequest Default { get { return new CameraTransformRequest(States.Complete, Transforms.Animation, 0f, 0f, 0f); } }

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

		public static CameraTransformRequest Animation(
			float? yaw = null,
			float? pitch = null,
			float? radius = null,
			Action done = null
		)
		{
			return new CameraTransformRequest(
				States.Request,
				Transforms.Animation,
				yaw,
				pitch,
				radius,
				done
			);
		}

		public readonly States State;
		public readonly Transforms Transform;
		public readonly float? Yaw;
		public readonly float? Pitch;
		public readonly float? Radius;
		public readonly Action Done;

		public CameraTransformRequest(
			States state,
			Transforms transform,
			float? yaw,
			float? pitch,
			float? radius,
			Action done = null
		)
		{
			State = state;
			Transform = transform;
			Yaw = yaw;
			Pitch = pitch;
			Radius = radius;
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
				Done
			);
		}
	}
}