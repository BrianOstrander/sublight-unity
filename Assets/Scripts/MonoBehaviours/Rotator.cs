using UnityEngine;

namespace LunraGames.SubLight
{
	public class Rotator : MonoBehaviour
	{
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
		[SerializeField]
		Vector3 rotation;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null

		void Update()
		{
			transform.localRotation = Quaternion.Euler(transform.localRotation.eulerAngles + (rotation * Time.deltaTime));
		}
	}
}