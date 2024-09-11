using HarmonyLib;
using RootMotion.FinalIK;
using Studio;
using System;
using System.Collections.Generic;

internal static class Hooks
{
	internal static bool CharacterChanging { get; private set; }

	[HarmonyPostfix]
	[HarmonyPatch(typeof(OCIChar), nameof(OCIChar.ActiveFK))]
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

	[HarmonyPrefix]
	[HarmonyPatch(typeof(OCIChar), nameof(OCIChar.ChangeChara))]
	private static void TrackChangeChara(OCIChar __instance)
	{
		var controller = __instance.charInfo.GetComponentInChildren<KineModController>();
		CharacterChanging = controller && controller.SystemActive;
		//KineMod.PluginLogger.LogDebug($"Chara change, will reset? {CharacterChanging}");
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(OCIChar), nameof(OCIChar.ChangeChara))]
	private static void ActOnChangeChara(OCIChar __instance)
	{
		if (!CharacterChanging)
		{
			return;
		}

		CharacterChanging = false;
		var controller = __instance.charInfo.GetComponentInChildren<KineModController>();
		controller.ChangeSystemState(true);
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(OCIChar), nameof(OCIChar.ActiveFKGroup))]
	private static void FixNeckPointRestore(ref OCIChar __instance, OIBoneInfo.BoneGroup __0, bool __1)
	{
		if (KineMod.PoseableFixedHead.Value == false && __0 == OIBoneInfo.BoneGroup.Neck && !__1)
		{
			//Undoes neck changes when toggling neck. Technically no longer required.
			__instance.neckPtnOld = __instance.charFileStatus.neckLookPtn;
		}
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(OCIChar), nameof(OCIChar.ActiveFKGroup))]
	private static void FixNeckPoint(ref OCIChar __instance, OIBoneInfo.BoneGroup __0, bool __1)
	{
		if (KineMod.PoseableFixedHead.Value == false && __0 == OIBoneInfo.BoneGroup.Neck && __1)
		{
			//Undoes neck changes when toggling neck. Technically no longer required.
			__instance.ChangeLookNeckPtn(__instance.neckPtnOld);
		}
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(MPCharCtrl.NeckInfo), nameof(MPCharCtrl.NeckInfo.UpdateInfo))]
	private static void UnlockHeadLookMenu(MPCharCtrl.NeckInfo __instance)
	{
		for (var i = 0; i < __instance.buttonMode.Length; i++)
		{
			__instance.buttonMode[i].interactable = true;
		}
	}
}
internal static class FkCtrlPatch
{
	private static readonly Dictionary<FKCtrl, FullBodyBipedIK> FkSlaveToIk = new Dictionary<FKCtrl, FullBodyBipedIK>();
	internal static readonly Dictionary<FKCtrl, NeckLookControllerVer2> NeckLookControllers = new Dictionary<FKCtrl, NeckLookControllerVer2>();

	[HarmonyReversePatch(HarmonyReversePatchType.Snapshot)]
	[HarmonyPatch(typeof(FKCtrl), nameof(FKCtrl.LateUpdate))]
	private static void FakeLateUpdate(FKCtrl __instance)
	{
		throw new NotImplementedException("It's a stub");
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(FKCtrl), nameof(FKCtrl.LateUpdate))]
	private static bool ConditionLateUpdate(FKCtrl __instance)
	{
		bool returnValue;
		if (FkSlaveToIk.TryGetValue(__instance, out var value))
		{
			returnValue = !value.enabled;
		}
		else
		{
			returnValue = true;
		}

		EarlyNeckUpdate(__instance, returnValue);
		return returnValue;
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(FKCtrl), nameof(FKCtrl.InitBones))]
	private static void SubToIkPreUpdate(FKCtrl __instance)
	{
		var neckLook = __instance.gameObject.GetComponentInChildren<NeckLookControllerVer2>();
		NeckLookControllers[__instance] = neckLook;

		var finalIk = __instance.gameObject.GetComponentInChildren<FullBodyBipedIK>();
		finalIk.solver.OnPreRead += () =>
		{
			EarlyNeckUpdate(__instance);
			FakeLateUpdate(__instance);
		};
		FkSlaveToIk[__instance] = finalIk;
	}
	private static void EarlyNeckUpdate(FKCtrl __instance, bool returnValue = true)
	{
		//Force an early update to the lookController to maintain it enslaved to FK.
		if (KineMod.PoseableFixedHead.Value && returnValue && NeckLookControllers.TryGetValue(__instance, out var lookController) && lookController.ptnNo == 4)
		{
			KineMod.PluginLogger.LogDebug("Early updating neck, no ik...");
			NeckLookControllerPatch.FakeLateUpdate(lookController);
		}
	}
}

internal static class NeckLookControllerPatch
{
	private static readonly Dictionary<NeckLookControllerVer2, FKCtrl> FkCtrls = new Dictionary<NeckLookControllerVer2, FKCtrl>();

	[HarmonyReversePatch]
	[HarmonyPatch(typeof(NeckLookControllerVer2), nameof(NeckLookControllerVer2.LateUpdate))]
	internal static void FakeLateUpdate(NeckLookControllerVer2 __instance)
	{
		throw new NotImplementedException("It's a stub");
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(NeckLookControllerVer2), nameof(NeckLookControllerVer2.LateUpdate))]
	private static bool IgnoreOnFixedAndFK(ref NeckLookControllerVer2 __instance)
	{
		if (__instance.ptnNo != 4)
		{
			return true;
		}

		if (KineMod.PoseableFixedHead.Value == false)
		{
			return true;
		}

		//Grab FKCtrl
		if (FkCtrls.TryGetValue(__instance, out var fkCtrl) == false)
		{
			fkCtrl = __instance.gameObject.GetComponentInParents<FKCtrl>();

			if (fkCtrl == null)
			{
				return true;
			}

			FkCtrls[__instance] = fkCtrl;
		}
		//KineMod.PluginLogger.LogDebug($"Values for NeckLookUpdateSkip are {fkCtrl == null} {!fkCtrl?.enabled}");
		return fkCtrl == null || !fkCtrl.enabled;
	}
}