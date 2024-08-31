using ExtensibleSaveFormat;
using KKAPI;
using KKAPI.Chara;
using KKAPI.Studio;
using System.Collections.Generic;
using System.Linq;

internal class CustomNodeGroup
{
	public static readonly Dictionary<string, string[]> BoneNames = new Dictionary<string, string[]>
		{
			{"Clavicle", new []{"cf_j_Shoulder_R", "cf_j_Shoulder_L"}},
#if HS2
			{"LeftArm", new []{"cf_j_armup00_L", "cf_j_armlow01_L", "cf_j_hand_L"}},
			{"RightArm", new []{"cf_j_armup00_R", "cf_j_armlow01_R", "cf_j_hand_R"}},
			{"UpperBody", new []{"cf_j_Spine03"}},
#elif KKS
			{"LeftArm", new []{"cf_j_arm00_L", "cf_j_forearm01_L", "cf_j_hand_L"}},
			{"RightArm", new []{"cf_j_arm00_R", "cf_j_forearm01_R", "cf_j_hand_R"}},
#endif
			{"Spine", new []{"cf_j_Spine01", "cf_j_Spine02"}},
#if HS2
			{"LeftLeg", new []{"cf_j_foot01_L", "cf_j_leglow01_L", "cf_j_legup00_L"}},
			{"RightLeg", new []{"cf_j_foot01_R", "cf_j_leglow01_R", "cf_j_legup00_R"}},
#elif KKS
			{"LeftLeg", new []{"cf_j_leg01_L", "cf_j_leg03_L", "cf_j_thigh00_L"}},
			{"RightLeg", new []{"cf_j_leg01_R", "cf_j_leg03_R", "cf_j_thigh00_R"}},
#endif
#if HS2
			{"Feet", new []{"cf_j_Foot02_R", "cf_j_Foot02_L"}},
			{"Toes", new []{"cf_j_Toes01_R", "cf_j_Toes01_L"}},
#elif KKS
			{"Toes", new []{"cf_j_Toes_R", "cf_j_Toes_L"}}
#endif
		};
	public string[] BoneNameStrings => BoneNames[Name];
	public readonly string Name;
	public bool State;
	public CustomNodeGroup(string name)
	{
		Name = name;
	}
}
internal class KineModController : CharaCustomFunctionController
{
	//Done: Define a dictionary for easier iteration through custom groupings.
	//Done: Add state definition
	//Done: Handle effector weights.
	// Step 1: Create the Reference Dictionary
	public static readonly Dictionary<string, string> NodeGroupIds = new Dictionary<string, string>
	{
		{ "Clavicle", "Clavicle" },
		{ "R. Arm", "RightArm" },
		{ "L. Arm", "LeftArm" },
#if HS2
		{ "Upper Body", "UpperBody" },
#endif
		{ "Spine", "Spine" },
		{ "R. Leg", "RightLeg" },
		{ "L. Leg", "LeftLeg" },
#if HS2
		{ "Feet", "Feet" },
#endif
		{ "Toes", "Toes" },
	};

	// Step 2: Initialize the CustomNodeGroups using the Reference Dictionary
	public Dictionary<string, CustomNodeGroup> CustomNodeGroups { get; private set; } =
		NodeGroupIds.ToDictionary(
			kvp => kvp.Key, // Key is the original key (e.g., "Clavicle")
			kvp => new CustomNodeGroup(kvp.Value) // Value is the corresponding ID (e.g., "Clavicle")
		);


	public bool SystemActive { get; set; }

	protected override void Update()
	{
		base.Update();
		var charInfo = ChaControl.GetOCIChar().oiCharInfo;
		if (SystemActive && (charInfo.enableFK && charInfo.enableIK) == false)
		{
			SystemActive = false;
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
						nameof(CustomNodeGroups),
						CustomNodeGroups.ToDictionary(r => r.Key, m => m.Value.State)
					},
					{
						"Effectors",
						ChaControl.GetOCIChar().finalIK.solver.effectors
							.Where(d => d.target?.name != null)
							.ToDictionary(m => m.target.name, r => new[]{r.positionWeight, r.rotationWeight})
					},
					{
						"BendGoals",
						ChaControl.GetOCIChar().finalIK.solver.chain
							.Where(r => r.bendConstraint?.bendGoal != null)
							.ToDictionary(m => m.bendConstraint.bendGoal.name, r => r.bendConstraint.weight)
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
			else if (dataEntry.Key.Equals(nameof(CustomNodeGroups)))
			{
				var nodeSettings = dataEntry.Value as Dictionary<object, object>;

				foreach (var keyPair in nodeSettings)
				{
					CustomNodeGroups[(string)keyPair.Key].State = (bool)keyPair.Value;
				}
			}
			else if (dataEntry.Key.Equals("Effectors"))
			{
				if (dataEntry.Value is Dictionary<object, object> effectorWeights == false)
				{
					continue;
				}

				foreach (var effector in ChaControl.GetOCIChar().finalIK.solver.effectors)
				{
					if (effector?.target?.name == null)
					{
						continue;
					}
					if (!effectorWeights.TryGetValue(effector.target.name, out var value))
					{
						continue;
					}

					if (!(value is object[] objectArray))
					{
						continue;
					}

					effector.positionWeight = (float)objectArray[0];
					effector.rotationWeight = (float)objectArray[1];
				}
			}
			else if (dataEntry.Key.Equals("BendGoals"))
			{
				if (!(dataEntry.Value is Dictionary<object, object> bendGoalWeights))
				{
					continue;
				}

				foreach (var chain in ChaControl.GetOCIChar().finalIK.solver.chain)
				{
					if (chain.bendConstraint?.bendGoal?.name == null)
					{
						continue;
					}

					if (!bendGoalWeights.TryGetValue(chain.bendConstraint.bendGoal.name, out var value))
					{
						continue;
					}
					chain.bendConstraint.weight = (float)value;
				}
			}
		}

		foreach (var groupKey in CustomNodeGroups)
		{
			SetBonesInArray(groupKey.Value.BoneNameStrings, groupKey.Value.State);
		}
	}

	private void SetBonesInArray(string[] boneNames, bool active)
	{
		foreach (var boneName in boneNames)
		{
			ChaControl.GetOCIChar().SetFkBoneState(boneName, active);
		}
	}
}