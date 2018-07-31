using System;
using System.Linq;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

using LunraGamesEditor;

using LunraGames.SpaceFarm.Models;

namespace LunraGames.SpaceFarm
{
	public partial class InventoryReferenceEditorWindow
	{
		EditorPrefsFloat moduleSlotsScroll = new EditorPrefsFloat(KeyPrefix + "ModuleSlotsScroll");

		void OnEditModule(ModuleReferenceModel reference)
		{
			OnRefrenceHeader<ModuleReferenceModel, ModuleInventoryModel>(reference, "Module");

			var model = reference.Model.Value;

			EditorGUI.BeginChangeCheck();
			{
				model.IsRoot.Value = EditorGUILayout.Toggle("Is Root", model.IsRoot.Value);
			}
			selectedReferenceModified |= EditorGUI.EndChangeCheck();

			OnListModuleSlots(reference);
		}

		void OnListModuleSlots(ModuleReferenceModel reference)
		{
			var model = reference.Model.Value;

			EditorGUI.BeginChangeCheck();
			{
				GUILayout.BeginHorizontal();
				{
					GUILayout.Label("Slot Count: " + model.Slots.All.Value.Length + " |", GUILayout.ExpandWidth(false));
					GUILayout.Label("Append New Slot", GUILayout.ExpandWidth(false));
					var result = EditorGUILayoutExtensions.HelpfulEnumPopup("- Select Slot Type -", SlotTypes.Unknown);
					switch (result)
					{
						case SlotTypes.Unknown:
							break;
						case SlotTypes.Crew:
							CreateSlot<CrewModuleSlotModel>(reference);
							break;
						case SlotTypes.Module:
							CreateSlot<ModuleModuleSlotModel>(reference);
							break;
						case SlotTypes.Resource:
							CreateSlot<ResourceModuleSlotModel>(reference);
							break;
						default:
							Debug.LogError("Unrecognized SlotType:" + result);
							break;
					}
					GUILayout.Label("Hold 'Ctrl' to rearrange entries.", GUILayout.ExpandWidth(false));
				}
				GUILayout.EndHorizontal();
			}
			selectedReferenceModified |= EditorGUI.EndChangeCheck();

			moduleSlotsScroll.Value = GUILayout.BeginScrollView(new Vector2(0f, moduleSlotsScroll), false, true).y;
			{
				EditorGUI.BeginChangeCheck();
				{
					var deleted = string.Empty;

					ModuleSlotModel indexSwap0 = null;
					ModuleSlotModel indexSwap1 = null;

					var isMoving = Event.current.control;

					var existingSlotIds = new List<string>();
					var duplicateSlotIds = new List<string>();

					foreach (var slot in model.Slots.All.Value.Select(s => s.SlotId.Value))
					{
						if (string.IsNullOrEmpty(slot)) continue;
						if (existingSlotIds.Contains(slot))
						{
							if (!duplicateSlotIds.Contains(slot)) duplicateSlotIds.Add(slot);
						}
						else existingSlotIds.Add(slot);
					}

					var sorted = model.Slots.All.Value.OrderBy(l => l.Index.Value).ToList();
					var sortedCount = sorted.Count;

					ModuleSlotModel lastSlot = null;

					for (var i = 0; i < sortedCount; i++)
					{
						var slot = sorted[i];
						var nextSlot = (i + 1 < sorted.Count) ? sorted[i + 1] : null;
						int currMoveDelta;

						var isDuplicateSlotId = duplicateSlotIds.Contains(slot.SlotId.Value);

						if (OnSlotBegin(i, sortedCount, reference, slot, isDuplicateSlotId, isMoving, out currMoveDelta)) deleted = slot.SlotId;

						if (currMoveDelta != 0)
						{
							indexSwap0 = slot;
							indexSwap1 = currMoveDelta == 1 ? nextSlot : lastSlot;
						}

						OnSlot(reference, slot);
						OnSlotEnd(reference, slot);

						lastSlot = slot;
					}
					if (!string.IsNullOrEmpty(deleted))
					{
						model.Slots.All.Value = model.Slots.All.Value.Where(l => l.SlotId.Value != deleted).ToArray();
					}
					if (indexSwap0 != null && indexSwap1 != null)
					{
						var swap0 = indexSwap0.Index.Value;
						var swap1 = indexSwap1.Index.Value;

						indexSwap0.Index.Value = swap1;
						indexSwap1.Index.Value = swap0;
					}
				}
				selectedReferenceModified |= EditorGUI.EndChangeCheck();
			}
			GUILayout.EndScrollView();
		}

		/// <summary>
		/// Creates a new instance of the specified slot and adds it to the list
		/// of slots in the specified reference's model.
		/// </summary>
		/// <returns>The created slot.</returns>
		/// <param name="reference">Reference.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		T CreateSlot<T>(ModuleReferenceModel reference) where T : ModuleSlotModel, new()
		{
			var result = new T();
			result.Index.Value = reference.Model.Value.Slots.All.Value.OrderBy(l => l.Index.Value).Select(l => l.Index.Value).LastOrFallback(-1) + 1;
			result.SlotId.Value = Guid.NewGuid().ToString();
			reference.Model.Value.Slots.All.Value = reference.Model.Value.Slots.All.Value.Append(result).ToArray();
			return result;
		}

		bool OnSlotBegin(int count, int maxCount, ModuleReferenceModel reference, ModuleSlotModel slot, bool isDuplicateSlotId, bool isMoving, out int indexDelta)
		{
			var deleted = false;
			indexDelta = 0;
			var isAlternate = count % 2 == 0;
			if (isAlternate) EditorGUILayoutExtensions.PushColor(Color.gray);
			GUILayout.BeginVertical(EditorStyles.helpBox);
			if (isAlternate) EditorGUILayoutExtensions.PopColor();
			GUILayout.BeginHorizontal();
			{
				var header = "#" + (count + 1) + " | " + slot.SlotType+ ".SlotId:";
				GUILayout.Label(header, EditorStyles.boldLabel, GUILayout.ExpandWidth(false));
				GUILayout.BeginVertical();
				{
					GUILayout.Space(6f);
					slot.SlotId.Value = GUILayout.TextField(slot.SlotId.Value);
				}
				GUILayout.EndVertical();

				if (isMoving)
				{
					GUILayout.Space(10f);
					EditorGUILayoutExtensions.PushEnabled(0 < count);
					if (GUILayout.Button("^", EditorStyles.miniButtonLeft, GUILayout.Width(60f), GUILayout.Height(18f)))
					{
						indexDelta = -1;
					}
					EditorGUILayoutExtensions.PopEnabled();
					EditorGUILayoutExtensions.PushEnabled(count < maxCount - 1);
					if (GUILayout.Button("v", EditorStyles.miniButtonRight, GUILayout.Width(60f), GUILayout.Height(18f)))
					{
						indexDelta = 1;
					}
					EditorGUILayoutExtensions.PopEnabled();
				}
				EditorGUILayoutExtensions.PushEnabled(!isMoving);
				deleted = EditorGUILayoutExtensions.XButton();
				EditorGUILayoutExtensions.PopEnabled();
			}
			GUILayout.EndHorizontal();

			if (string.IsNullOrEmpty(slot.SlotId.Value))
			{
				EditorGUILayout.HelpBox("The slot id cannot be null or empty.", MessageType.Error);
			}
			else if (isDuplicateSlotId)
			{
				EditorGUILayout.HelpBox("A slot with id \"" + slot.SlotId.Value + "\" already exists in this module.", MessageType.Error);
			}

			return deleted;
		}

		void OnSlot(ModuleReferenceModel reference, ModuleSlotModel slot)
		{
			switch (slot.SlotType)
			{
				case SlotTypes.Crew:
					OnCrewSlot(reference, slot as CrewModuleSlotModel);
					break;
				case SlotTypes.Module:
					OnModuleSlot(reference, slot as ModuleModuleSlotModel);
					break;
				case SlotTypes.Resource:
					OnResourceSlot(reference, slot as ResourceModuleSlotModel);
					break;
				default:
					EditorGUILayout.HelpBox("Unrecognized SlotType: " + slot.SlotType, MessageType.Error);
					break;
			}
		}

		void OnCrewSlot(ModuleReferenceModel reference, CrewModuleSlotModel slot)
		{
			slot.ValidInventoryTypes.Value = EditorGUILayoutExtensions.EnumArray(
				new GUIContent("Valid Inventory Types", "A list of valid items that may be slotted here."),
				slot.ValidInventoryTypes.Value,
				"- Select a InventoryType -"
			);
		}

		void OnModuleSlot(ModuleReferenceModel reference, ModuleModuleSlotModel slot)
		{
			// nothing to do here.
		}

		void OnResourceSlot(ModuleReferenceModel reference, ResourceModuleSlotModel slot)
		{
			GUILayout.BeginHorizontal();
			{
				GUILayout.Space(16f);
				GUILayout.BeginVertical();
				{
					var altColor = Color.grey.NewV(0.5f);

					EditorGUILayoutResource.Field(
						new GUIContent("Refill", "Amount of resources this module should create or consume when active, always."),
						slot.RefillResources
					);

					EditorGUILayoutResource.Field(
						new GUIContent("Refill Logistics", "The amount of resources this module should create when active and logistics space is available. Should not be negative."),
						slot.RefillLogisticsResources,
						altColor
					);

					EditorGUILayoutResource.Field(
						new GUIContent("Maximum Logistics", "The amount this module increases the logisics maximum by."),
						slot.MaximumLogisticsResources
					);

					EditorGUILayoutResource.Field(
						new GUIContent("Maximum", "The amount this module increases the maximum storable resources by."),
						slot.MaximumResources,
						altColor
					);
				}
				GUILayout.EndVertical();
			}
			GUILayout.EndHorizontal();
		}

		void OnSlotEnd(ModuleReferenceModel reference, ModuleSlotModel slot)
		{
			GUILayout.EndVertical();
		}
	}
}