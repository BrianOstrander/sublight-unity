using UnityEngine;

namespace LunraGames.SubLight
{
	public class Rotator : MonoBehaviour
	{
		[SerializeField]
		Vector3 rotation;

		void Update()
		{
			transform.localRotation = Quaternion.Euler(transform.localRotation.eulerAngles + (rotation * Time.deltaTime));
		}
	}
}