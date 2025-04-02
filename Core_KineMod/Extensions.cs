using RootMotion.FinalIK;
using Studio;
using System;
using System.Linq;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

internal static class Extensions
{
	#region UILevelFunctions

	/// <summary>
	/// UI function. Should only be used by UI.
	/// </summary>
	/// <param name="mpCharCtrl"></param>
	/// <param name="resetBone"></param>
	public static void CopyFkBone(this MPCharCtrl mpCharCtrl, string[] resetBone, bool resetToZero = false)
	{
		//KineMod.PluginLogger.LogDebug("Will now attempt single reset!");

		if (mpCharCtrl.disposableFK != null)
		{
			mpCharCtrl.disposableFK.Dispose();
			mpCharCtrl.disposableFK = null;
		}
		mpCharCtrl.disposableFK = new SingleAssignmentDisposable();
		mpCharCtrl.disposableFK.Disposable = mpCharCtrl.LateUpdateAsObservable().Take(1)
			.Subscribe(_ =>
			{
				//KineMod.PluginLogger.LogDebug("Resetting FK Bone...");
				foreach (var bone in resetBone)
				{
					mpCharCtrl.ociChar.fkCtrl.ResetFkBone(bone, resetToZero);
				}
			}, () =>
			{
				//KineMod.PluginLogger.LogDebug("Reset On Complete!");
				mpCharCtrl.disposableFK.Dispose();
				mpCharCtrl.disposableFK = null;
			});
	}
	/// <summary>
	/// UI function. Should only be used by the UI.
	/// </summary>
	/// <param name="ikInfo"></param>
	/// <param name="boneGroup"></param>
	/// <param name="value"></param>
	public static void OnChangeValueIndividual(this MPCharCtrl.IKInfo ikInfo, OIBoneInfo.BoneGroup boneGroup,
		bool value)
	{
		if (ikInfo.isUpdateInfo)
		{
			return;
		}

		ikInfo.ociChar.ActiveIK(boneGroup, value);
		ikInfo.isUpdateInfo = true;
		var flag = false;
		for (var i = 0; i < 5; i++)
		{
			flag |= ikInfo.ociChar.oiCharInfo.activeIK[i];
		}

		ikInfo.toggleAll.isOn = flag;
		ikInfo.isUpdateInfo = false;
	}

	#endregion UILevelFunctions

	/*
	public static bool GetFkBoneState(this OCIChar character, string boneName)
	{
		return (character.listBones
				.FirstOrDefault(r => r.guideObject.transformTarget.name.Equals(boneName, StringComparison.OrdinalIgnoreCase))?.active)
			.GetValueOrDefault(false);
	}
	*/
	public static void SetFkBoneState(this OCIChar character, string boneName, bool isActive, bool force = false)
	{
		if ((force || character.oiCharInfo.enableFK) == false || character.charInfo.GetComponent<KineModController>().SystemActive == false)
		{
			return;
		}

		// Update the active state of each relevant bone
		foreach (var boneInfo in character.listBones)
		{
			if (!boneInfo.guideObject.transformTarget.name.Equals(boneName, StringComparison.OrdinalIgnoreCase))
			{
				continue;
			}

			if (character.IsFkBoneGroupActive(boneInfo.boneGroup) && isActive == false)
			{
				continue;
			}

			var targetInfo = character.fkCtrl.GetTargetBoneInfo(boneInfo);
			boneInfo.active = isActive;
			targetInfo.enable = isActive;
		}

		/*
		foreach (var boneTargetInfo in character.fkCtrl.listBones)
		{
			if (!boneTargetInfo.gameObject.name.Equals(boneName, StringComparison.OrdinalIgnoreCase))
			{
				continue;
			}
		}
		*/
	}
	public static void ResetFkBone(this FKCtrl fkCtrl, string boneName, bool resetToZero = false)
	{
		var targetBone = fkCtrl.listBones
			.First(r => r.gameObject.name.Equals(boneName, StringComparison.OrdinalIgnoreCase));
		if (resetToZero == false)
		{
			targetBone.CopyBone();
		}
		else
		{
			targetBone.changeAmount.Reset();
		}
	}
	/*
	public static bool GetFkBoneGroupState(this OCIChar character, OIBoneInfo.BoneGroup boneGroup)
	{
		return character.listBones.Where(r => r.boneGroup == boneGroup).Any(m => m.active);
	}
	public static bool GetIkBoneGroupState(this OCIChar character, OIBoneInfo.BoneGroup boneGroup)
	{
		return character.listIKTarget.Where(r => r.boneGroup == boneGroup).Any(m => m.active);
	}
	*/
	public static FKCtrl.TargetInfo GetTargetBoneInfo(this FKCtrl fkCtrl, OCIChar.BoneInfo boneInfo)
	{
		var targetName = boneInfo.guideObject.transformTarget.name;
		foreach (var boneTargetInfo in fkCtrl.listBones)
		{
			if (boneTargetInfo.gameObject.name.Equals(targetName, StringComparison.OrdinalIgnoreCase))
			{
				return boneTargetInfo;
			}
		}

		return null;
	}

	public static bool IsIkBoneGroupActive(this OCIChar character, OIBoneInfo.BoneGroup boneGroup)
	{
		var activeIk = character.oiCharInfo.activeIK;
		for (var i = 0; i < activeIk.Length; i++)
		{
			var target = (OIBoneInfo.BoneGroup)(1 << i);
			var isPartOfGroup = (boneGroup & target) != 0;

			if (isPartOfGroup && activeIk[i])
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsFkBoneGroupActive(this OCIChar character, OIBoneInfo.BoneGroup boneGroup)
	{
		// Iterate through all the possible FK bone groups
		for (var i = 0; i < FKCtrl.parts.Length; i++)
		{
			// Check if the current FK bone group is part of the compound boneGroup
			if ((boneGroup & FKCtrl.parts[i]) != 0)
			{
				// If any part is active, return true
				if (character.oiCharInfo.activeFK[i])
				{
					return true;
				}
			}
		}

		// If no matching active FK bone group is found, return false
		return false;
	}

	public static IKEffector GetEffectorByTargetName(this OCIChar character, string targetName)
	{
		var effectors = character.finalIK.solver.effectors;
		foreach (var effector in effectors)
		{
			if (effector.target.gameObject.name.Equals(targetName, StringComparison.OrdinalIgnoreCase) == false)
			{
				continue;
			}

			return effector;
		}

		return null;
	}

	public static IKConstraintBend GetBendGoalByTargetName(this OCIChar character, string targetName)
	{
		var constraintBends = character.finalIK.solver.chain
			.Select(r => r.bendConstraint)
			.Where(m => m?.bendGoal != null);

		foreach (var effector in constraintBends)
		{
			if (effector.bendGoal.name.Equals(targetName, StringComparison.OrdinalIgnoreCase) == false)
			{
				continue;
			}

			return effector;
		}

		return null;
	}

	public static T GetComponentInParents<T>(this GameObject gameObject)
	{
		while (gameObject != null)
		{
			var component = gameObject.transform.GetComponent<T>();
			if (component != null)
			{
				return component;
			}
			gameObject = gameObject.transform.parent.gameObject;
		}

		return default;
	}
}