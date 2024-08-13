﻿using HarmonyLib;

namespace tanu.CruiseAssist
{
	[HarmonyPatch(typeof(GameMain))]
	public class Patch_GameMain
	{
		[HarmonyPatch(nameof(GameMain.Begin)), HarmonyPostfix]
		public static void Begin_Postfix()
		{
			ConfigManager.CheckConfig(ConfigManager.Step.GAME_MAIN_BEGIN);
            CruiseAssistPlugin.extensions.ForEach(delegate (CruiseAssistExtensionAPI extension)
            {
                extension.CheckConfig(ConfigManager.Step.GAME_MAIN_BEGIN.ToString());
            });
        }

		[HarmonyPatch(nameof(GameMain.Pause)), HarmonyPrefix]
		public static void Pause_Prefix()
		{
			ConfigManager.CheckConfig(ConfigManager.Step.STATE);
            CruiseAssistPlugin.extensions.ForEach(delegate (CruiseAssistExtensionAPI extension)
            {
                extension.CheckConfig(ConfigManager.Step.STATE.ToString());
            });
        }
	}
}
