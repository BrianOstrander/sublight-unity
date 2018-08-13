using UnityEngine;

namespace LunraGames.SubLight.Views
{
	public interface IDragView : IView
	{
		/// <summary>
		/// The multiplier for dragging around the camera.
		/// </summary>
		/// <value>The drag move scalar.</value>
		float DragMoveScalar { get; } // TODO: Move this to some preferences thing the player can edit.
		/// <summary>
		/// The multiplier for drag rotating around the camera.
		/// </summary>
		/// <remarks>
		/// This should probably not be stored on the view, but a general 
		/// preferences object...
		/// </remarks>
		/// <value>The drag move scalar.</value>
		float DragRotateScalar { get; } // TODO: Move this to some preferences thing the player can edit.
		/// <summary>
		/// Typically equivelent to the root of the View.
		/// </summary>
		/// <value>The drag root.</value>
		Transform DragRoot { get; }
		/// <summary>
		/// Forward vector of the view, would be the camera if this is a camera.
		/// </summary>
		/// <value>The drag forward.</value>
		Transform DragForward { get; }
		/// <summary>
		/// What is being dragged around the rotation point, a camera for
		/// example.
		/// </summary>
		/// <value>The drag axis root.</value>
		Transform DragAxisRoot { get; }
		/// <summary>
		/// The point that we're being rotated around, cached with ecah drag.
		/// </summary>
		/// <value>The drag axis.</value>
		Vector3 DragAxis { get; set; }
	}
}