using HarmonyLib;
using RootMotion.FinalIK;
using Studio;
using System;
using System.Collections.Generic;

internal static class Hooks
{
	internal static bool CharacterChanging { get; private set; }

	[HarmonyPostfix]
	[HarmonyPatch(typeof(OCIChar), "ActiveFK")]
	private static void EnforceCustomBones(OCIChar __instance)
	{
		var nodeController = __instance.charInfo.GetComponent<KineModController>();

		if (nodeController.SystemActive == false)
		{
			return;
		}

		foreach (var customBoneGroup in nodeController.CustomNodeGroups)
		{
			if (customBoneGroup.Value.State == false)
			{
				continue;
			}

			foreach (var boneName in customBoneGroup.Value.BoneNameStrings)
			{
				__instance.SetFkBoneState(boneName, true);
			}
		}
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(OCIChar), "ActiveFKGroup")]
	private static void FixNeckPoint(ref OCIChar __instance, OIBoneInfo.BoneGroup __0, bool __1)
	{
		if (__0 == OIBoneInfo.BoneGroup.Neck && __1)
		{
			//This lets FK pose the neck.
			__instance.ChangeLookNeckPtn(3);
		}
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(OCIChar), "ChangeChara")]
	private static void TrackChangeChara(OCIChar __instance)
	{
		var controller = __instance.charInfo.GetComponentInChildren<KineModController>();
		CharacterChanging = controller && controller.SystemActive;

		KineMod.PluginLogger.LogDebug($"Chara change, will reset? {CharacterChanging}");
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(OCIChar), "ChangeChara")]
	private static void ActOnChangeChara(OCIChar __instance)
	{
		if (!CharacterChanging)
		{
			return;
		}

		KineMod.PluginLogger.LogDebug($"Chara change, now resetting.");

		CharacterChanging = false;
		var controller = __instance.charInfo.GetComponentInChildren<KineModController>();
		controller.ChangeSystemState(true);
	}
}
internal static class FkCtrlPatch
{
	private static readonly Dictionary<FKCtrl, FullBodyBipedIK> FkSlaveToIk = new Dictionary<FKCtrl, FullBodyBipedIK>();

	[HarmonyReversePatch]
	[HarmonyPatch(typeof(FKCtrl), "LateUpdate")]
	private static void FakeLateUpdate(FKCtrl instance)
	{
		throw new NotImplementedException("It's a stub");
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(FKCtrl), "LateUpdate")]
	private static bool ConditionLateUpdate(FKCtrl __instance)
	{
		if (FkSlaveToIk.TryGetValue(__instance, out var value))
		{
			return !value.enabled;
		}

		return true;
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(FKCtrl), "InitBones")]
	private static void SubToIkPreUpdate(FKCtrl __instance)
	{
		var finalIk = __instance.gameObject.GetComponentInChildren<FullBodyBipedIK>();
		finalIk.solver.OnPreRead += () => FakeLateUpdate(__instance);
		FkSlaveToIk[__instance] = finalIk;
	}
}