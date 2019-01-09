using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using LunraGames.SubLight.Models;
using LunraGames.SubLight.Views;
using LunraGames.SubLight.Presenters;

namespace LunraGames.SubLight
{
	public partial class GameState
	{
		public class Focuses : StateFocuses
		{
			const float PriorityDimming = 0.25f;

			public static void InitializePresenters(GameState state, Action done)
			{
				var payload = state.Payload;

				// Basics: Cameras, Room, etc
				var roomCamera = payload.MainCamera;
				var gantryAnchor = roomCamera.GantryAnchor;
				var fieldOfView = roomCamera.FieldOfView;

				new GenericFocusCameraPresenter<ToolbarFocusDetails>(gantryAnchor, fieldOfView);
				new GenericFocusCameraPresenter<SystemFocusDetails>(gantryAnchor, fieldOfView);
				new GenericFocusCameraPresenter<CommunicationsFocusDetails>(gantryAnchor, fieldOfView);
				new GenericFocusCameraPresenter<ShipFocusDetails>(gantryAnchor, fieldOfView);
				new GenericFocusCameraPresenter<EncyclopediaFocusDetails>(gantryAnchor, fieldOfView);

				new HoloPresenter(payload.Game);

				// All other presenters for this state...
				payload.ShowOnIdle.Add(new ToolbarPresenter(payload.Game));
				payload.ShowOnIdle.Add(new ToolbarBackPresenter(payload.Game, LanguageStringModel.Override("Back")));

				new FocusLipPresenter(SetFocusLayers.System, SetFocusLayers.Ship, SetFocusLayers.Communications, SetFocusLayers.Encyclopedia);

				// GRID INFO BEGIN
				var gridInfo = new GridInfoBlock();

				gridInfo.Scale = LanguageStringModel.Override("Scale");
				gridInfo.Scales = new UniverseScales[] {
					UniverseScales.System,
					UniverseScales.Local,
					UniverseScales.Stellar,
					UniverseScales.Quadrant,
					UniverseScales.Galactic,
					UniverseScales.Cluster
				};
				gridInfo.ScaleNames = new LanguageStringModel[] {
					LanguageStringModel.Override("System"),
					LanguageStringModel.Override("Local"),
					LanguageStringModel.Override("Interstellar"),
					LanguageStringModel.Override("Quadrant"),
					LanguageStringModel.Override("Galactic"),
					LanguageStringModel.Override("Cluster")
				};

				gridInfo.ThousandUnit = LanguageStringModel.Override("k");
				gridInfo.MillionUnit = LanguageStringModel.Override("m");

				gridInfo.AstronomicalUnit = new PluralLanguageStringBlock(LanguageStringModel.Override("Astronomical Unit"), LanguageStringModel.Override("Astronomical Unit"));
				gridInfo.LightYearUnit = new PluralLanguageStringBlock(LanguageStringModel.Override("Light Year"), LanguageStringModel.Override("Light Years"));
				// GRID INFO END

				new GridPresenter(payload.Game, gridInfo);
				new GridScalePresenter(payload.Game, gridInfo.Scale);

				var gridTimeChronometerLanguage = GridTimeLanguageBlock.Default;
				gridTimeChronometerLanguage.Title = LanguageStringModel.Override("Chronometer");
				gridTimeChronometerLanguage.SubTitle = LanguageStringModel.Override("Reference Frame");
				gridTimeChronometerLanguage.Tooltip = LanguageStringModel.Override("Elapsed time since leaving Sol. See the encyclopedia for additional info.");
				gridTimeChronometerLanguage.ReferenceFrameNames[ReferenceFrames.Ship] = LanguageStringModel.Override("Ark");
				gridTimeChronometerLanguage.ReferenceFrameNames[ReferenceFrames.Galactic] = LanguageStringModel.Override("Galactic");

				var gridTimeTransitLanguage = gridTimeChronometerLanguage.Duplicate;
				gridTimeTransitLanguage.Title = LanguageStringModel.Override("Transit Time");
				gridTimeTransitLanguage.Tooltip = LanguageStringModel.Override("Duration of the Ark's voyage to the specified system. See the encyclopedia for additional info.");

				new GridTimePresenter(
					payload.Game,
					gridTimeChronometerLanguage,
					gridTimeTransitLanguage
				);

				new GridVelocityPresenter(
					payload.Game,
					new GridVelocityLanguageBlock {
						Velocity = LanguageStringModel.Override("Velocity"),
						Resource = LanguageStringModel.Override("Propellant"),
						ResourceWarning = LanguageStringModel.Override("Insufficient Propellant")
					}
				);

				new GridTransitLockoutPresenter(
					payload.Game,
					new GridTransitLockoutLanguageBlock
					{
						TransitTitle = LanguageStringModel.Override("Crew Lockout"),
						TransitDescription = LanguageStringModel.Override("Interstellar ark is under automated\ncontrol until reaching destination"),
						DescriptionPrefix = LanguageStringModel.Override("In transit from"),
						UnlockLeftTitle = LanguageStringModel.Override("Unlocking"),
						UnlockRightTitle = LanguageStringModel.Override("System Survey"),
						UnlockLeftStatuses = LanguageStringModel.Overrides(
							"Restarting Crew Control System <b>Success</b>",
							"Confirming Subsystem Integrity <b>Success</b>",
							"Generating Administrator Privileges <b>Passed</b>",
							"Confirming Admin Identities <b>Validated</b>",
							"Unlocking Engine Control Systems <b>Success</b>",
							"Flushing Life Support Mechanisms <b>Vented</b>",
							"Updating Hull Damage Manifest <b>Recorded</b>",
							"Aligning Astrogation Sensors <b>Calibrated</b>",
							"Locating Tracer From Terra <b>Failed</b>",
							"Downloading Database Deltas <b>Disconnected</b>"
						),
						UnlockRightStatuses = LanguageStringModel.Overrides(
							"<b>Waking</b> Probe Bay",
							"<b>Scanning</b> Surface Of Primary Bodies",
							"<b>Running</b> Chemical Analysis",
							"<b>Collating</b> Results Of Scans",
							"<b>Processing</b> Atmospheric Data",
							"<b>Rendering</b> Orbital Anomalies",
							"<b>Sweeping</b> Debris Fields For Impacts",
							"<b>Integrating</b> Collected Data From Databases",
							"<b>Rewriting</b> Known System Entries",
							"<b>Assigning</b> Targets For Further Study"
						)
					}
				);

				new ClusterPresenter(payload.Game, payload.Game.Galaxy);
				new ClusterPresenter(payload.Game, payload.Game.GalaxyTarget, LanguageStringModel.Override("Click for information"));

				var foundEnd = false;
				var playerEnd = payload.Game.Galaxy.GetPlayerEnd(out foundEnd);

				if (!foundEnd) Debug.LogError("Provided galaxy has no defined player end");

				new SystemAlertPresenter(
					payload.Game,
					playerEnd,
					LanguageStringModel.Override("Sagittarius A*"),
					LanguageStringModel.Override("Click for information"),
					() => Debug.LogWarning("Todo: open the encyclopedia entry for Sagittarius A*"),
					UniverseScales.Galactic
				);

				new GalaxyPresenter(payload.Game);
				new GalaxyDetailPresenter(payload.Game, UniverseScales.Quadrant);
				new GalaxyDetailPresenter(payload.Game, UniverseScales.Stellar);

				new ShipPinPresenter(payload.Game, UniverseScales.Cluster);
				new ShipPinPresenter(payload.Game, UniverseScales.Galactic);
				new ShipPinPresenter(payload.Game, UniverseScales.Quadrant);
				new ShipPinPresenter(payload.Game, UniverseScales.Stellar);
				new ShipPinPresenter(payload.Game, UniverseScales.Local);

				new GalaxyLabelsPresenter(payload.Game, UniverseScales.Galactic, UniverseScales.Galactic);
				new GalaxyLabelsPresenter(payload.Game, UniverseScales.Quadrant, UniverseScales.Quadrant);
				new GalaxyLabelsPresenter(payload.Game, UniverseScales.Stellar, UniverseScales.Quadrant, UniverseScales.Stellar);

				new RegionLabelPresenter(payload.Game, UniverseScales.Local);
				new RegionLabelPresenter(payload.Game, UniverseScales.Stellar);
				new RegionLabelPresenter(payload.Game, UniverseScales.Quadrant);
				new RegionLabelPresenter(payload.Game, UniverseScales.Galactic);
				new RegionLabelPresenter(payload.Game, UniverseScales.Cluster);

				new BodiesDisabledPresenter(
					payload.Game,
					LanguageStringModel.Override("Telescope Malfunction"),
					LanguageStringModel.Override("< unable to retrieve system survey >")
				);

				new CelestialSystemDistanceLinePresenter(payload.Game, UniverseScales.Local);

				var celestialLanguageBlock = new CelestialSystemLanguageBlock
				{
					Confirm = LanguageStringModel.Override("Confirm"),
					ConfirmDescription = LanguageStringModel.Override("Click again"),
					OutOfRange = LanguageStringModel.Override("Out Of Range"),
					OutOfRangeDescription = LanguageStringModel.Override("Transit not possible"),
					DistanceUnit = LanguageStringModel.Override("ly"),
					Analysis = LanguageStringModel.Override("System Analysis"),
					AnalysisDescription = LanguageStringModel.Override("Click for details"),
					PrimaryClassifications = new Dictionary<SystemClassifications, LanguageStringModel> {
						{ SystemClassifications.Unknown,LanguageStringModel.Override("Unrecognized Classification") },
						{ SystemClassifications.Stellar,LanguageStringModel.Override("Stellar") },
						{ SystemClassifications.Degenerate,LanguageStringModel.Override("Degenerate") },
						{ SystemClassifications.Special,LanguageStringModel.Override("Special") },
						{ SystemClassifications.Anomalous,LanguageStringModel.Override("Anomalous") }
					}
				};

				for (var i = 0; i < payload.LocalSectorCount; i++)
				{
					var sector = new SectorInstanceModel();
					sector.Sector.Value = App.Universe.GetSector(payload.Game.Galaxy, payload.Game.Universe, new UniversePosition(new Vector3Int(i, 0, 0)));
					var systems = new SystemInstanceModel[payload.Game.Galaxy.MaximumSectorSystemCount.Value];
					for (var s = 0; s < systems.Length; s++)
					{
						new CelestialSystemPresenter(
							payload.Game,
							UniverseScales.Local,
							systems[s] = new SystemInstanceModel(s),
							celestialLanguageBlock
						);
					}
					sector.SystemModels.Value = systems;
					payload.LocalSectorInstances.Add(sector);
				}

				var systemPreviewLanguageBlock = new SystemPreviewLanguageBlock
				{
					PrimaryClassifications = celestialLanguageBlock.PrimaryClassifications
				};

				new SystemPreviewLabelPresenter(payload.Game, systemPreviewLanguageBlock);

				done();
			}

			#region Focus Building
			public static SetFocusBlock[] GetDefaultFocuses()
			{
				var results = new List<SetFocusBlock>();

				foreach (var layer in AllLayers)
				{
					switch (layer)
					{
						case SetFocusLayers.Room: results.Add(GetFocus<RoomFocusDetails>()); break;
						case SetFocusLayers.Priority: results.Add(GetFocus<PriorityFocusDetails>()); break;
						case SetFocusLayers.Toolbar: results.Add(GetFocus<ToolbarFocusDetails>()); break;
						case SetFocusLayers.System: results.Add(GetFocus<SystemFocusDetails>()); break;
						case SetFocusLayers.Communications: results.Add(GetFocus<CommunicationsFocusDetails>()); break;
						case SetFocusLayers.Ship: results.Add(GetFocus<ShipFocusDetails>()); break;
						case SetFocusLayers.Encyclopedia: results.Add(GetFocus<EncyclopediaFocusDetails>()); break;
						case SetFocusLayers.Home:
							// I don't know why I do this...
							break;
						default:
							Debug.LogError("Unrecognized Layer " + layer);
							break;
					}
				}

				return results.ToArray();
			}

			static List<SetFocusBlock> GetBaseEnabledFocuses(int startIndex = -1)
			{
				var results = new List<SetFocusBlock>();

				results.Add(GetFocus<RoomFocusDetails>(startIndex, true, 1f));

				return results;
			}

			public static SetFocusBlock[] GetNoFocus()
			{
				return GetBaseEnabledFocuses().ToArray();
			}

			public static SetFocusBlock[] GetToolbarSelectionFocus(ToolbarSelections selection)
			{
				var results = GetBaseEnabledFocuses();

				results.Add(GetFocus<ToolbarFocusDetails>(0, true, 1f, true));

				switch (selection)
				{
					case ToolbarSelections.System:
						results.Add(GetFocus<SystemFocusDetails>(1, true, 1f, true));
						break;
					case ToolbarSelections.Ship:
						results.Add(GetFocus<ShipFocusDetails>(1, true, 1f, true));
						break;
					case ToolbarSelections.Communications:
						results.Add(GetFocus<CommunicationsFocusDetails>(1, true, 1f, true));
						break;
					case ToolbarSelections.Encyclopedia:
						results.Add(GetFocus<EncyclopediaFocusDetails>(1, true, 1f, true));
						break;
					default:
						Debug.LogError("Unrecognized selection: " + selection);
						break;
				}

				return results.ToArray();
			}

			public static SetFocusBlock[] GetPriorityFocus(ToolbarSelections selection)
			{
				var results = GetBaseEnabledFocuses();

				results.Add(GetFocus<PriorityFocusDetails>(0, true, 1f, true));

				results.Add(GetFocus<ToolbarFocusDetails>(1, true, PriorityDimming, false));

				switch (selection)
				{
					case ToolbarSelections.System:
						results.Add(GetFocus<SystemFocusDetails>(1, true, PriorityDimming, false));
						break;
					case ToolbarSelections.Ship:
						results.Add(GetFocus<ShipFocusDetails>(1, true, PriorityDimming, false));
						break;
					case ToolbarSelections.Communications:
						results.Add(GetFocus<CommunicationsFocusDetails>(1, true, PriorityDimming, false));
						break;
					case ToolbarSelections.Encyclopedia:
						results.Add(GetFocus<EncyclopediaFocusDetails>(1, true, PriorityDimming, false));
						break;
					default:
						Debug.LogError("Unrecognized selection: " + selection);
						break;
				}

				return results.ToArray();
			}
			#endregion
		}
	}
}