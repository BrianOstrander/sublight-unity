using UnityEngine;

namespace LunraGames.SubLight
{
	public static class GDCHackGlobals
	{
		public const int PlanetCountMaximum = 6;
		public const int ScanLevelMaximum = 2;
		public const int HabitableMaximum = 3;
		public const int AnomalyMaximum = 1;

		public const string PlanetDetectedTrigger = "planet*detected";

		public static string PlanetPositionReadable(int position)
		{
			switch (position)
			{
				case 1: return "first";
				case 2: return "second";
				case 3: return "third";
				case 4: return "fourth";
				case 5: return "fifth";
				case 6: return "sixth";
				case 7: return "seventh";
				default: return string.Empty;
			}
		}

		public static string[] HabitableAtmosphereDescriptions = new string[]
		{
			"Vacuum",
			"Rarefied",
			"Noxious",
			"Breathable"
		};

		public static string[] HabitableGravityDescriptions = new string[]
		{
			"Crushing",
			"Micro-Gravity",
			"Light",
			"Earth-Like"
		};

		public static string[] HabitableTemperatureDescriptions = new string[]
		{
			"Boiling",
			"Frigid",
			"Temperate",
			"Pleasant"
		};

		public static string[] HabitableWaterDescriptions = new string[]
		{
			"None",
			"Dry",
			"Entirely Ocean",
			"Plentiful"
		};

		public static string[] HabitableResourcesDescriptions = new string[]
		{
			"Rare",
			"Minimal",
			"Moderate",
			"Rich"
		};

		public static string GetReading(
			int value,
			int scanLevel,
			int propertyScanLevel,
			string[] options,
			bool hasScanned,
			string category
		)
		{
			var result = options[value].ToUpper();
			var color = Color.black;

			var hValue = 0.81f;

			switch (value)
			{
				case 0: color = Color.HSVToRGB(0f, 1f, hValue); break;
				case 1: color = Color.HSVToRGB(0.106f, 1f, hValue); break;
				case 2: color = Color.HSVToRGB(0.153f, 1f, hValue); break;
				case 3: color = Color.HSVToRGB(0.247f, 1f, hValue); break;
			}

			if (propertyScanLevel != 0)
			{
				var wasSet = false;
				if (hasScanned)
				{
					if (scanLevel < propertyScanLevel) result = "NOT SCANNABLE";
					else
					{
						wasSet = true;
						result = "[ " + DeveloperStrings.GetColor(DeveloperStrings.GetBold(result), color) + " ]";
					}
				}
				else if (propertyScanLevel <= scanLevel) result = "REQUIRES SURFACE PROBE";
				else result = "NOT SCANNABLE";

				if (!wasSet) result = "[ " + DeveloperStrings.GetBold(result) + " ]";
			}
			else
			{
				result = "[ " + DeveloperStrings.GetColor(DeveloperStrings.GetBold(result), color) + " ]";
			}

			result = category + "\t" + result;
			return result;
		}
	}
}