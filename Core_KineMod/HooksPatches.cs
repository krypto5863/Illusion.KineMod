using HarmonyLib;
using RootMotion.FinalIK;
using Studio;
using System;
using System.Collections.Generic;

public static class Hooks
{
	[HarmonyPostfix]
	[HarmonyPatch(typeof(OCIChar), "ActiveFK")]
	public static void EnforceCustomBones(OCIChar __instance)
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
	public static void FixNeckPoint(ref OCIChar __instance, OIBoneInfo.BoneGroup __0, bool __1)
	{
		if (__0 == OIBoneInfo.BoneGroup.Neck && __1)
		{
			//This lets FK pose the neck.
			__instance.ChangeLookNeckPtn(3);
		}
	}
}
public static class FkCtrlPatch
{
	private static readonly Dictionary<FKCtrl, FullBodyBipedIK> FkSlaveToIk = new Dictionary<FKCtrl, FullBodyBipedIK>();

	[HarmonyReversePatch]
	[HarmonyPatch(typeof(FKCtrl), "LateUpdate")]
	public static void FakeLateUpdate(FKCtrl instance)
	{
		throw new NotImplementedException("It's a stub");
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(FKCtrl), "LateUpdate")]
	public static bool ConditionLateUpdate(FKCtrl __instance)
	{
		if (FkSlaveToIk.TryGetValue(__instance, out var value))
		{
			return !value.enabled;
		}

		return true;
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(FKCtrl), "InitBones")]
	public static void SubToIkPreUpdate(FKCtrl __instance)
	{
		var finalIk = __instance.gameObject.GetComponentInChildren<FullBodyBipedIK>();
		finalIk.solver.OnPreRead += () => FakeLateUpdate(__instance);
		FkSlaveToIk[__instance] = finalIk;
	}
}