using BepInEx.Bootstrap;
using Core_KineMod.UGUIResources;
using ExtensibleSaveFormat;
using KKAPI;
using KKAPI.Chara;
using KKAPI.Studio;
using MessagePack;
using Studio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using static Studio.OIBoneInfo;

internal class KineModController : CharaCustomFunctionController
{
	public Dictionary<string, CustomBoneGroup> CustomNodeGroups { get; private set; } =
			CustomBoneInfo.BoneNames
			.ToDictionary(
			kvp => kvp.Key,
			kvp => new CustomBoneGroup(kvp.Key)
		);

	public Dictionary<string, float[]> Effectors = EffectorsInfo.BonesInfo
		.Where(d => d.Value.IsBendGoal == false)
		.ToDictionary(m => m.Key, r => new[] { 1f, 1f });

	public Dictionary<string, float> BendGoals = EffectorsInfo.BonesInfo
		.Where(d => d.Value.IsBendGoal)
		.ToDictionary(m => m.Key, r => 1f);

	public Dictionary<BoneGroup, HandState> HandStates { get; private set; } =  new Dictionary<BoneGroup, HandState>
	{
		{BoneGroup.LeftHand, new HandState(BoneGroup.LeftHand)},
		{BoneGroup.RightHand, new HandState(BoneGroup.RightHand)}
	};

	// Add this as a static field in the KineModController class
	private static readonly List<int> MirroredBoneIds = new List<int>
	{
		//The first joint of each finger must be mirrored.-
		22, 25, 28, 31, 34, 37, 40, 43, 46, 49
#if HS2
		//The last joint of the thumbs must also be mirrored.
		, 24, 39
#endif
	};

	public bool SystemActive { get; set; }
	public bool EnforceEffectors { get; set; } = true;
	protected override void Update()
	{
		base.Update();

		//Avoids a possible null ref when doing stuff.
		var character = ChaControl.GetOCIChar();
		var oiCharInfo = character?.oiCharInfo;
		if (oiCharInfo == null)
		{
			return;
		}

		if (SystemActive && (oiCharInfo.enableFK && oiCharInfo.enableIK) == false)
		{
#if DEBUG
			KineMod.PluginLogger.LogDebug("System now inactive due to IK or FK being disabled!");
#endif

#if KKS
			if (!IsCoordinateLoadOption())
			{
				SystemActive = false;
			}
#else

			SystemActive = false;
#endif
		}

		foreach (var effector in character.finalIK.solver.effectors)
		{
			var effectorName = effector.target.name;
			if (!Effectors.TryGetValue(effectorName, out var values))
			{
				continue;
			}

			if (EnforceEffectors)
			{
				effector.positionWeight = values[0];
				effector.rotationWeight = values[1];
			}
			else
			{
				values[0] = effector.positionWeight;
				values[1] = effector.rotationWeight;
			}
		}

		foreach (var chain in character.finalIK.solver.chain)
		{
			var bendConstraint = chain.bendConstraint;
			if (bendConstraint.bendGoal == null)
			{
				continue;
			}
			if (!BendGoals.TryGetValue(bendConstraint.bendGoal.name, out var weight))
			{
				continue;
			}
			if (EnforceEffectors)
			{
				bendConstraint.weight = weight;
			}
			else
			{
				BendGoals[bendConstraint.bendGoal.name] = bendConstraint.weight;
			}
		}
	}

#if KKS
	/// <summary>
	/// Checks if Coordinate Load Option is currently being used
	/// </summary>
	/// <returns>True if CLO is used</returns>
	private static bool IsCoordinateLoadOption()
	{
		if (!Chainloader.PluginInfos.ContainsKey("com.jim60105.kks.coordinateloadoption"))
		{
			return false;
		}

		var cloPanelObject = GameObject.Find("CoordinateTooglePanel");
		return cloPanelObject != null && cloPanelObject.activeInHierarchy;
	}
	protected override void OnCoordinateBeingLoaded(ChaFileCoordinate coordinate)
	{
		KineMod.PluginLogger.LogDebug("Checking if CLO is active...");

		//Compatibility with Coordinate Load Option
		if (SystemActive && IsCoordinateLoadOption())
		{
			KineMod.PluginLogger.LogDebug("CLO active, re-enabling system.");
			KineMod.EnableFkIk(ChaControl.GetOCIChar());
		}

		base.OnCoordinateBeingLoaded(coordinate);
	}
#endif
	protected override void OnCardBeingSaved(GameMode currentGameMode)
	{
		if (currentGameMode != GameMode.Studio)
		{
			return;
		}

		var pluginData = new PluginData
		{
			version = 2,
			data =
				{
					{
						nameof(SystemActive), SystemActive
					},
					{
						nameof(EnforceEffectors), EnforceEffectors
					},
					{
						nameof(CustomNodeGroups),
						CustomNodeGroups.ToDictionary(r => r.Key, m => m.Value.State)
					},
					{
						nameof(HandStates), MessagePackSerializer.Serialize(HandStates)
					},
					{
						"Effectors", Effectors
					},
					{
						"BendGoals", BendGoals
					}
				}
		};

		SetExtendedData(pluginData);
	}
	protected override void OnReload(GameMode currentGameMode)
	{
		if (currentGameMode != GameMode.Studio)
		{
			return;
		}

		var extData = GetExtendedData();

		if (extData == null)
		{
			return;
		}


#if DEBUG
		for (var i = 0; i < FKCtrl.parts.Length; i++)
		{
			var part = FKCtrl.parts[i];
			var state = ChaControl.GetOCIChar().oiCharInfo.activeFK[i];

			KineMod.PluginLogger.LogDebug($"{part.ToString()} is {state} before changes!");
		}
#endif

		var dictionary = extData.data;

		foreach (var dataEntry in dictionary)
		{
			if (dataEntry.Key.Equals(nameof(SystemActive)))
			{
				SystemActive = (bool)dataEntry.Value;

				if (SystemActive)
				{
					KineMod.EnableFkIk(ChaControl.GetOCIChar());
				}
			}
			else if (dataEntry.Key.Equals(nameof(EnforceEffectors)))
			{
				EnforceEffectors = (bool)dataEntry.Value;
			}
			else if (dataEntry.Key.Equals(nameof(CustomNodeGroups)))
			{
				var nodeSettings = (Dictionary<object, object>)dataEntry.Value;

				foreach (var keyPair in nodeSettings)
				{
					CustomNodeGroups[(string)keyPair.Key].State = (bool)keyPair.Value;
				}
			}
			else if (dataEntry.Key.Equals(nameof(HandStates)))
			{
				var handStates = MessagePackSerializer.Deserialize<Dictionary<BoneGroup, HandState>>((byte[])dataEntry.Value);
				HandStates = handStates;

				UpdateHand(BoneGroup.LeftHand);
				UpdateHand(BoneGroup.RightHand);
			}
			else if (dataEntry.Key.Equals("Effectors"))
			{
				var effectors = (Dictionary<object, object>)dataEntry.Value;
				foreach (var effectorVal in effectors)
				{
					var effectorKey = (string)effectorVal.Key;
					var effectorValues = (object[])effectorVal.Value;
					ChangeEffectorWeight(effectorKey, (float)effectorValues[0], false);
					ChangeEffectorWeight(effectorKey, (float)effectorValues[1], true);
				}
			}
			else if (dataEntry.Key.Equals("BendGoals"))
			{
				var bendGoals = (Dictionary<object, object>)dataEntry.Value;
				foreach (var bendGoal in bendGoals)
				{
					var bendGoalTarget = (string)bendGoal.Key;
					var bendGoalWeight = (float)bendGoal.Value;
					ChangeBendGoalWeight(bendGoalTarget, bendGoalWeight);
				}
			}
		}

		foreach (var groupKey in CustomNodeGroups)
		{
			SetBonesInArray(groupKey.Value.BoneNameStrings, groupKey.Value.State);
		}
	}
	internal void ChangeSystemState(bool state)
	{

#if DEBUG
		KineMod.PluginLogger.LogDebug($"Changing system state for {ChaFileControl.charaFileName} to {state}.");
#endif

		SystemActive = state;
		if (SystemActive)
		{
			KineMod.EnableFkIk(ChaControl.GetOCIChar());
		}
		else
		{
			KineMod.DisableFkIk(ChaControl.GetOCIChar());
		}
	}
	internal void UpdateHandPattern(BoneGroup group, int pattern)
	{
		var state = HandStates[group];
		state.Pattern = pattern;
		HandStates[group] = state;

		UpdateHand(group);
	}
	internal void UpdateHandBlending(BoneGroup group, float value)
	{
		var state = HandStates[group];
		state.Blending = value;
		HandStates[group] = state;

		UpdateHand(group);
	}
	private void UpdateHand(BoneGroup group, int value = -1)
	{
		var hand = group.GetHandNumber();
		value = value == -1 ? ChaControl.GetOCIChar().oiCharInfo.handPtn[hand] : value;

		ChaControl.GetOCIChar().ChangeHandAnime(hand, value);
	}
	public void MirrorHand(BoneGroup boneGroup)
	{
		var oppositeHand = boneGroup == BoneGroup.LeftHand
			? BoneGroup.RightHand
			: BoneGroup.LeftHand;

		var handNum = boneGroup.GetHandNumber();
		var oppositeHandNum = oppositeHand.GetHandNumber();

		var newState = HandStates[boneGroup];
		newState.Hand = oppositeHand;
		HandStates[oppositeHand] = newState;

		ChaControl.GetOCIChar().ChangeHandAnime(oppositeHandNum, ChaControl.GetOCIChar().oiCharInfo.handPtn[handNum]);

		var bones = ChaControl.GetOCIChar().fkCtrl.GetTargetInfosForGroup(boneGroup);
		var oppositeBones = ChaControl.GetOCIChar().fkCtrl.GetTargetInfosForGroup(oppositeHand)
			.ToArray();

		var regex = new Regex(@"(.*)_(\w)\b");

		foreach (var targetInfo in bones)
		{
			var match = regex.Match(targetInfo.gameObject.name);

#if DEBUG
			KineMod.PluginLogger.LogDebug($"Captured {match.Groups[1].Value} in {targetInfo.gameObject.name}.");
#endif

			foreach (var oppositeBone in oppositeBones)
			{
				var match2 = regex.Match(oppositeBone.gameObject.name);

				if (!match.Groups[1].Value.Equals(match2.Groups[1].Value, StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}

#if DEBUG
				KineMod.PluginLogger.LogDebug($"Matched {match.Value} with {match2.Value}.");
#endif

				MirrorBoneRotation(oppositeBone.boneID, targetInfo.changeAmount.rot, out var newRot);

				oppositeBone.changeAmount.rot = newRot;
				break;
			}
		}
	}

	//Todo HS2 Finger IDs
	public static bool MirrorBoneRotation(int boneId, Vector3 rot, out Vector3 mirroredRot)
	{
		mirroredRot = rot;

		// Determine if the bone ID belongs to right (22-36) or left (37-51) hand
		if (boneId < 22 || boneId > 51)
		{
			return false;
		}

		// Only some bones are mirrored. Make sure to check if the bone ID is in the list of mirrored bones.
		if (MirroredBoneIds.Contains(boneId))
		{
#if DEBUG
			KineMod.PluginLogger.LogDebug($"Mirroring {boneId}.");
#endif
#if KKS
			mirroredRot.x = 360f - rot.x;
			mirroredRot.y = (180f - rot.y) % 360f;
			mirroredRot.z = (180f + rot.z) % 360f;
#elif HS2
			mirroredRot.y = (360f - rot.y) % 360f;
#endif
		}

#if HS2
		mirroredRot.z = (360f - rot.z) % 360f;
#endif

		return true;
	}

	public void SaveHandPose(string name, BoneGroup boneGroup)
	{
		var bones = ChaControl
			.GetOCIChar()
			.fkCtrl
			.GetTargetInfosForGroup(boneGroup);

		var stuff = bones.ToDictionary(d => d.gameObject.name, k => k.changeAmount.rot);

		var handState = HandStates[boneGroup];

		if (name.IsNullOrWhiteSpace())
		{
			name = $"HandPose_{DateTime.Now.ToFileTime()}";
		}

		var handNum = boneGroup.GetHandNumber();
		var handPose = new HandPose(name, boneGroup, stuff, ChaControl.GetOCIChar().oiCharInfo.handPtn[handNum], handState.Pattern, handState.Blending);

		KineMod.SaveHandPose(handPose);
	}

	public void LoadHandPose(BoneGroup boneGroup, string poseName)
	{
		if (poseName.IsNullOrWhiteSpace())
		{
			return;
		}

		var pose = KineMod.HandPoseList.First(d => d.Name.Equals(poseName));

		var state = HandStates[boneGroup];
		state.Pattern = pose.Pattern2;
		state.Blending = pose.Blending;
		HandStates[boneGroup] = state;

		UpdateHand(boneGroup, pose.Pattern1);

		var targetBones = ChaControl.GetOCIChar().fkCtrl.GetTargetInfosForGroup(boneGroup)
			.ToArray();

		if (boneGroup != pose.Hand)
		{
			var regex = new Regex(@"(.*)_(\w)\b");

			foreach (var targetInfo in pose.BoneRotations)
			{
				var match = regex.Match(targetInfo.Key);

#if DEBUG
				KineMod.PluginLogger.LogDebug($"Captured {match.Groups[1].Value} in {targetInfo.Key}.");
#endif

				foreach (var oppositeBone in targetBones)
				{
					var match2 = regex.Match(oppositeBone.gameObject.name);

					if (!match.Groups[1].Value.Equals(match2.Groups[1].Value, StringComparison.OrdinalIgnoreCase))
					{
						continue;
					}

#if DEBUG
					KineMod.PluginLogger.LogDebug($"Matched {match.Value} with {match2.Value}.");
#endif

					MirrorBoneRotation(oppositeBone.boneID, targetInfo.Value, out var newRot);

					oppositeBone.changeAmount.rot = newRot;
					break;
				}
			}
		}
		else
		{
			foreach (var targetInfo in targetBones)
			{
				foreach (var poseBoneRotation in pose.BoneRotations)
				{
					if (poseBoneRotation.Key.Equals(targetInfo.gameObject.name, StringComparison.OrdinalIgnoreCase))
					{
						targetInfo.changeAmount.rot = poseBoneRotation.Value;
					}
				}
			}
		}
	}

	internal void ChangeEnforceEffectors(bool state)
	{
		EnforceEffectors = state;
	}
	internal void ChangeCustomBoneState(string groupString, bool state)
	{
		if (!CustomNodeGroups.TryGetValue(groupString, out var groupValue))
		{
			return;
		}

		groupValue.State = state;
		foreach (var boneName in groupValue.BoneNameStrings)
		{
			ChaControl.GetOCIChar().SetFkBoneState(boneName, groupValue.State);
		}
	}
	internal void ChangeEffectorWeight(string target, float value, bool rotation)
	{
		var effector = ChaControl.GetOCIChar().GetEffectorByTargetName(target);
		if (effector == null || !Effectors.TryGetValue(target, out var values))
		{
			return;
		}

		if (rotation)
		{
			effector.rotationWeight = value;
			values[1] = value;
		}
		else
		{
			effector.positionWeight = value;
			values[0] = value;
		}
	}

	internal void ChangeBendGoalWeight(string target, float value)
	{
		var bendGoal = ChaControl.GetOCIChar().GetBendGoalByTargetName(target);
		if (bendGoal == null || !BendGoals.ContainsKey(target))
		{
			return;
		}

		bendGoal.weight = value;
		BendGoals[target] = value;
	}

	private void SetBonesInArray(string[] boneNames, bool active)
	{
		foreach (var boneName in boneNames)
		{
			ChaControl.GetOCIChar().SetFkBoneState(boneName, active);
		}
	}
}