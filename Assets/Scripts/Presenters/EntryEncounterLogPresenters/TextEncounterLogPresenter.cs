using UnityEngine;

using LunraGames.SubLight.Views;
using LunraGames.SubLight.Models;

namespace LunraGames.SubLight.Presenters
{
	public class TextEncounterLogPresenter : EntryEncounterLogPresenter<TextEncounterLogModel, ITextEncounterLogView>
	{
		public TextEncounterLogPresenter(GameModel model, TextEncounterLogModel logModel) : base(model, logModel) {}

		protected override void OnShow()
		{
			View.HeaderColor = Color.cyan.NewH(int.Parse(CreateMD5(LogModel.Header).Substring(0, 2), System.Globalization.NumberStyles.HexNumber) / 255f);
			View.Header = LogModel.Header;
			View.Message = LogModel.Message;
		}

		// TODO: Lol hacks.
		static string CreateMD5(string input)
		{
			input = input ?? string.Empty;
			// Use input string to calculate MD5 hash
			using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
			{
				var inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
				var hashBytes = md5.ComputeHash(inputBytes);

				// Convert the byte array to hexadecimal string
				var sb = new System.Text.StringBuilder();
				for (int i = 0; i < hashBytes.Length; i++)
				{
					sb.Append(hashBytes[i].ToString("X2"));
				}
				return sb.ToString();
			}
		}
	}
}