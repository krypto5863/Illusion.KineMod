﻿using BepInEx.Bootstrap;
using Core_KineMod.UGUIResources;
using ExtensibleSaveFormat;
using KKAPI;
using KKAPI.Chara;
using KKAPI.Studio;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

internal class KineModController : CharaCustomFunctionController
{
	public Dictionary<string, CustomBoneGroup> CustomNodeGroups { get; private set; } =
		CustomBoneGroup.BoneNames.ToDictionary(
			kvp => kvp.Key, // Key is the original key (e.g., "Clavicle")
			kvp => new CustomBoneGroup(kvp.Key) // Value is the corresponding ID (e.g., "Clavicle")
		);

	public Dictionary<string, float[]> Effectors = EffectorsInfo.BonesInfo
		.Where(d => d.Value.IsBendGoal == false)
		.ToDictionary(m => m.Key, r => new[] { 0f, 0f });

	public Dictionary<string, float> BendGoals = EffectorsInfo.BonesInfo
		.Where(d => d.Value.IsBendGoal)
		.ToDictionary(m => m.Key, r => 0f);

	public bool SystemActive { get; set; }
	public bool EnforceEffectors { get; set; }

	protected override void Start()
	{
		base.Start();
		foreach (var effector in ChaControl.GetOCIChar().finalIK.solver.effectors)
		{
			KineMod.PluginLogger.LogDebug($"Found effector {effector.target.name}");
		}
	}
	protected override void Update()
	{
		base.Update();
		var charInfo = ChaControl.GetOCIChar().oiCharInfo;
		if (SystemActive && (charInfo.enableFK && charInfo.enableIK) == false
#if KKS
			&& !checkForCoordianteLoadOption() // prevent disabling of SystemActive during Coordinate Load Option load
#endif
			)
		var character = ChaControl.GetOCIChar();

		var charInfo = character.oiCharInfo;
		if (SystemActive && (charInfo.enableFK && charInfo.enableIK) == false)
		{
			SystemActive = false;
		}
	}

#if KKS
	/// <summary>
	/// Checks if Coordiante Load Option is currenlty being used
	/// </summary>
	/// <returns>True if CLO is used</returns>
	private bool checkForCoordianteLoadOption()
	{
		if (StudioAPI.InsideStudio && Chainloader.PluginInfos.ContainsKey("com.jim60105.kks.coordinateloadoption"))
		{
			GameObject CLOpanelobject = GameObject.Find("CoordinateTooglePanel");
			if (CLOpanelobject != null && CLOpanelobject.activeInHierarchy) return true;
		}
		return false;
	}

	protected override void OnCoordinateBeingLoaded(ChaFileCoordinate coordinate)
	{
		// compatibility with Coordinate Load Option
		if (checkForCoordianteLoadOption() && SystemActive)
		{
			KineMod.EnableFkIk(ChaControl.GetOCIChar());
			KineMod.PluginLogger.LogDebug($"Renabled because of Coordiante Load Option");
		}

		foreach (var effector in character.finalIK.solver.effectors)
		{
			var effectorName = effector.target.name;
			if (!Effectors.TryGetValue(effectorName, out var values))
			{
				continue;
			}

		base.OnCoordinateBeingLoaded(coordinate);
	}
#endif
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
	protected override void OnCardBeingSaved(GameMode currentGameMode)
	{
		if (currentGameMode != GameMode.Studio)
		{
			return;
		}

		var pluginData = new PluginData
		{
			version = 1,
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
						"Effectors", Effectors
					},
					{
						"BendGoals", BendGoals
					}
				}
		};

		SetExtendedData(pluginData);
	}
	protected override void OnReload(GameMode currentGameMode, bool mainState)
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