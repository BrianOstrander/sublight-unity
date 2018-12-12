using UnityEngine;
using UnityEngine.UI;

using TMPro;

namespace LunraGames.SubLight.Views
{
	public class GridTimeStampLeaf : MonoBehaviour
	{
		[SerializeField]
		GameObject deltaArea;
		[SerializeField]
		XButtonLeaf buttonLeaf;
		[SerializeField]
		TextMeshProUGUI yearLabel;
		[SerializeField]
		TextMeshProUGUI monthLabel;
		[SerializeField]
		TextMeshProUGUI dayLabel;
	}
}