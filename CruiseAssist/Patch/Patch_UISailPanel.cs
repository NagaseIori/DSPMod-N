﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace tanu.CruiseAssist
{
	[HarmonyPatch(typeof(UISailPanel))]
	public class Patch_UISailPanel
	{
		[HarmonyPatch("_OnUpdate"), HarmonyTranspiler]
		public static IEnumerable<CodeInstruction> OnUpdate_Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			CodeMatcher matcher = new CodeMatcher(instructions);

			matcher.
				MatchForward(true,
					new CodeMatch(OpCodes.Ldarg_0)); // 47:0

			//LogManager.LogInfo("matcher.Pos=" + matcher.Pos);

			matcher.
				InsertAndAdvance(Transpilers.EmitDelegate<Action>(
					() =>
					{
						CruiseAssistPlugin.ReticuleTargetPlanet = null;
						CruiseAssistPlugin.ReticuleTargetStar = null;
					}));

			matcher.
				MatchForward(true,
					new CodeMatch(OpCodes.Bge_Un),
					new CodeMatch(OpCodes.Ldloc_S),
					new CodeMatch(OpCodes.Stloc_S),
					new CodeMatch(OpCodes.Ldc_I4_1),
					new CodeMatch(OpCodes.Stloc_S),
					new CodeMatch(OpCodes.Ldloc_S), // 93:156
					new CodeMatch(OpCodes.Stloc_S)); // 93:157

			//LogManager.LogInfo("matcher.Pos=" + matcher.Pos);

			matcher.
				Advance(1).
				InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_0)).
				InsertAndAdvance(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(StarData), nameof(StarData.planets)))).
				InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 21)).
				InsertAndAdvance(Transpilers.EmitDelegate<Action<PlanetData[], int>>(
					(planets, planetIndex) =>
					{
						CruiseAssistPlugin.ReticuleTargetPlanet = planets[planetIndex];
					}));

			matcher.
				MatchForward(true,
					new CodeMatch(OpCodes.Bge_Un),
					new CodeMatch(OpCodes.Ldloc_S),
					new CodeMatch(OpCodes.Stloc_S),
					new CodeMatch(OpCodes.Ldc_I4_1),
					new CodeMatch(OpCodes.Stloc_S),
					new CodeMatch(OpCodes.Ldloc_S), // 116:253
					new CodeMatch(OpCodes.Stloc_S)); // 116:254

			//LogManager.LogInfo("matcher.Pos=" + matcher.Pos);

			matcher.
				Advance(1).
				InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 20)).
				InsertAndAdvance(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(GalaxyData), nameof(GalaxyData.stars)))).
				InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 24)).
				InsertAndAdvance(Transpilers.EmitDelegate<Action<StarData[], int>>(
					(stars, starIndex) =>
					{
						CruiseAssistPlugin.ReticuleTargetStar = stars[starIndex];
					}));

			return matcher.InstructionEnumeration();
		}
#if false
		private static string CLASS_NAME = "UISailPanel";

		[HarmonyPatch("_OnCreate"), HarmonyPrefix]
		public static void OnCreate_Prefix() =>
			LogManager.LogInfo($"enter {CLASS_NAME}._OnCreate");

		[HarmonyPatch("_OnDestroy"), HarmonyPrefix]
		public static void OnDestroy_Prefix() =>
			LogManager.LogInfo($"enter {CLASS_NAME}._OnDestroy");

		[HarmonyPatch("_OnInit"), HarmonyPrefix]
		public static void OnInit_Prefix() =>
			LogManager.LogInfo($"enter {CLASS_NAME}._OnInit");

		[HarmonyPatch("_OnFree"), HarmonyPrefix]
		public static void OnFree_Prefix() =>
			LogManager.LogInfo($"enter {CLASS_NAME}._OnFree");

		[HarmonyPatch("_OnUpdate"), HarmonyPrefix]
		public static void OnUpdate_Prefix() =>
			LogManager.LogInfo($"enter {CLASS_NAME}._OnUpdate");
#endif
		[HarmonyPatch("_OnOpen"), HarmonyPrefix]
		public static void OnOpen_Prefix()
		{
			if (CruiseAssistPlugin.AutoDisableLockCursorFlag)
			{
				UIRoot.instance.uiGame.disableLockCursor = true;
			}
		}

		[HarmonyPatch("_OnClose"), HarmonyPrefix]
		public static void OnClose_Prefix()
		{
			ConfigManager.CheckConfig(ConfigManager.Step.STATE);
		}
	}
}
