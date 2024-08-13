﻿using HarmonyLib;

namespace tanu.CruiseAssist
{
	[HarmonyPatch(typeof(UITechTree))]
	public class Patch_UITechTree
	{
		[HarmonyPatch("_OnOpen"), HarmonyPrefix]
		public static void OnOpen_Prefix()
		{
			ConfigManager.CheckConfig(ConfigManager.Step.STATE);
            CruiseAssistPlugin.extensions.ForEach(delegate (CruiseAssistExtensionAPI extension)
            {
                extension.CheckConfig(ConfigManager.Step.STATE.ToString());
            });
        }
	}
}
