﻿using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// https://docs.unity3d.com/ja/2018.4/Manual/ExecutionOrder.html

namespace tanu.CruiseAssist
{
	[BepInPlugin(ModGuid, ModName, ModVersion)]
	public class CruiseAssist : BaseUnityPlugin
	{
		public const string ModGuid = "tanu.CruiseAssist";
		public const string ModName = "CruiseAssist";
		public const string ModVersion = "0.0.30";

		public static bool Enable = true;
		public static bool MarkVisitedFlag = true;
		public static bool SelectFocusFlag = false;
		public static bool HideDuplicateHistoryFlag = true;
		public static bool AutoDisableLockCursorFlag = false;
		public static StarData ReticuleTargetStar = null;
		public static PlanetData ReticuleTargetPlanet = null;
		public static StarData SelectTargetStar = null;
		public static PlanetData SelectTargetPlanet = null;
		public static int SelectTargetAstroId = 0;
		public static StarData TargetStar = null;
		public static PlanetData TargetPlanet = null;
		public static CruiseAssistState State = CruiseAssistState.INACTIVE;

		public static List<int> History = new List<int>();
		public static List<int> Bookmark = new List<int>();

		public static Func<StarData, string> GetStarName = star => star.displayName;
		public static Func<PlanetData, string> GetPlanetName = planet => planet.displayName;

		private Harmony harmony;

		public void Awake()
		{
			LogManager.Logger = base.Logger;
			new CruiseAssistConfigManager(base.Config);
			ConfigManager.CheckConfig(ConfigManager.Step.AWAKE);
			harmony = new Harmony($"{ModGuid}.Patch");
			harmony.PatchAll(typeof(Patch_GameMain));
			harmony.PatchAll(typeof(Patch_UISailPanel));
			harmony.PatchAll(typeof(Patch_UIStarmap));
			harmony.PatchAll(typeof(Patch_PlayerMoveSail));
		}

		public void OnDestroy()
		{
			harmony.UnpatchSelf();
		}

		public void OnGUI()
		{
			if (DSPGame.IsMenuDemo || GameMain.mainPlayer == null)
			{
				return;
			}
			var uiGame = UIRoot.instance.uiGame;
			if (!uiGame.guideComplete || uiGame.techTree.active || uiGame.escMenu.active || uiGame.dysonEditor.active || uiGame.hideAllUI0 || uiGame.hideAllUI1)
			{
				return;
			}
			if (GameMain.mainPlayer.sailing || uiGame.starmap.active)
			{
				Check();

				CruiseAssistMainUI.wIdx = uiGame.starmap.active ? 1 : 0;

				var scale = CruiseAssistMainUI.Scale / 100.0f;

				GUIUtility.ScaleAroundPivot(new Vector2(scale, scale), Vector2.zero);

				CruiseAssistMainUI.OnGUI();
				if (CruiseAssistStarListUI.Show[CruiseAssistMainUI.wIdx])
				{
					CruiseAssistStarListUI.OnGUI();
				}
				if (CruiseAssistConfigUI.Show[CruiseAssistMainUI.wIdx])
				{
					CruiseAssistConfigUI.OnGUI();
				}
				if (CruiseAssistDebugUI.Show)
				{
					CruiseAssistDebugUI.OnGUI();
				}

				bool resetInputFlag = false;

				resetInputFlag = ResetInput(CruiseAssistMainUI.Rect[CruiseAssistMainUI.wIdx], scale);

				if (!resetInputFlag && CruiseAssistStarListUI.Show[CruiseAssistMainUI.wIdx])
				{
					resetInputFlag = ResetInput(CruiseAssistStarListUI.Rect[CruiseAssistMainUI.wIdx], scale);
				}

				if (!resetInputFlag && CruiseAssistConfigUI.Show[CruiseAssistMainUI.wIdx])
				{
					resetInputFlag = ResetInput(CruiseAssistConfigUI.Rect[CruiseAssistMainUI.wIdx], scale);
				}

				if (!resetInputFlag && CruiseAssistDebugUI.Show)
				{
					resetInputFlag = ResetInput(CruiseAssistDebugUI.Rect, scale);
				}
			}
		}

		private void Check()
		{
			var astroId = GameMain.mainPlayer.navigation.indicatorAstroId;

			if (CruiseAssist.SelectTargetAstroId != astroId)
			{
				CruiseAssist.SelectTargetAstroId = astroId;
				if (astroId % 100 != 0)
				{
					CruiseAssist.SelectTargetPlanet = GameMain.galaxy.PlanetById(astroId);
					CruiseAssist.SelectTargetStar = CruiseAssist.SelectTargetPlanet.star;
				}
				else
				{
					CruiseAssist.SelectTargetPlanet = null;
					CruiseAssist.SelectTargetStar = GameMain.galaxy.StarById(astroId / 100);
				}
			}

			if (GameMain.localPlanet != null)
			{
				if (CruiseAssist.History.Count == 0 || CruiseAssist.History.Last() != GameMain.localPlanet.id)
				{
					if (CruiseAssist.History.Count >= 128)
					{
						CruiseAssist.History.RemoveAt(0);
					}
					CruiseAssist.History.Add(GameMain.localPlanet.id);
					ConfigManager.CheckConfig(ConfigManager.Step.STATE);
				}
			}
		}

		private bool ResetInput(Rect rect, float scale)
		{
			var left = rect.xMin * scale;
			var right = rect.xMax * scale;
			var top = rect.yMin * scale;
			var bottom = rect.yMax * scale;
			var inputX = Input.mousePosition.x;
			var inputY = Screen.height - Input.mousePosition.y;
			if (left <= inputX && inputX <= right && top <= inputY && inputY <= bottom)
			{
				int[] zot = { 0, 1, 2 };
				if (zot.Any(Input.GetMouseButton) || Input.mouseScrollDelta.y != 0)
				{
					Input.ResetInputAxes();
					return true;
				}
			}
			return false;
		}
	}
}
