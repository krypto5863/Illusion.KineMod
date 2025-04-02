using System.Collections.Generic;

internal class CustomBoneInfo
{
	public static readonly Dictionary<string, CustomBoneInfo> BoneNames = new Dictionary<string, CustomBoneInfo>
	{
#if HS2
        {"L. Arm", new CustomBoneInfo(new []{"cf_j_armup00_L", "cf_j_armlow01_L", "cf_j_hand_L"})},
		{"R. Arm", new CustomBoneInfo(new []{"cf_j_armup00_R", "cf_j_armlow01_R", "cf_j_hand_R"})},
		{"Forearms", new CustomBoneInfo(new []{ "cf_J_ArmLow01_s_L", "cf_J_ArmLow01_s_R"}, true)},
		{"Wrists", new CustomBoneInfo(new []{ "cf_J_ArmLow02_s_L", "cf_J_ArmLow02_s_R"}, true)},
		{"Clavicle", new CustomBoneInfo(new []{"cf_j_Shoulder_R", "cf_j_Shoulder_L"})},
		{"Upper Body", new CustomBoneInfo(new []{"cf_j_Spine03"})},
		{"Spine", new CustomBoneInfo(new []{"cf_j_Spine01", "cf_j_Spine02"})},
		{"Waist", new CustomBoneInfo(new []{"cf_j_kosi01"})},
		{"Hips", new CustomBoneInfo(new []{"cf_j_kosi02"})},
		{"L. Leg", new CustomBoneInfo(new []{"cf_j_foot01_L", "cf_j_leglow01_L", "cf_j_legup00_L"})},
		{"R. Leg", new CustomBoneInfo(new []{"cf_j_foot01_R", "cf_j_leglow01_R", "cf_j_legup00_R"})},
		{"Feet", new CustomBoneInfo(new []{"cf_j_Foot02_R", "cf_j_Foot02_L"})},
		{"Toes", new CustomBoneInfo(new []{"cf_j_Toes01_R", "cf_j_Toes01_L"})},
#else
		{"L. Arm", new CustomBoneInfo(new []{"cf_j_arm00_L", "cf_j_forearm01_L", "cf_j_hand_L"})},
		{"R. Arm", new CustomBoneInfo(new []{"cf_j_arm00_R", "cf_j_forearm01_R", "cf_j_hand_R"})},
		{"Wrists", new CustomBoneInfo(new []{"cf_s_wrist_L", "cf_s_wrist_R"}, true)},
		{"Clavicle", new CustomBoneInfo(new []{"cf_j_Shoulder_R", "cf_j_Shoulder_L"})},
		{"Spine", new CustomBoneInfo(new []{"cf_j_Spine01", "cf_j_Spine02"})},
		{"Waist", new CustomBoneInfo(new []{"cf_j_waist01"})},
		{"L. Leg", new CustomBoneInfo(new []{"cf_j_leg01_L", "cf_j_leg03_L", "cf_j_thigh00_L"})},
		{"R. Leg", new CustomBoneInfo(new []{"cf_j_leg01_R", "cf_j_leg03_R", "cf_j_thigh00_R"})},
		{"Toes", new CustomBoneInfo(new []{"cf_j_Toes_R", "cf_j_Toes_L"})}
#endif
	};

	public readonly string[] _boneNames;
	public readonly bool _resetToZero;

	public CustomBoneInfo(string[] boneNames, bool resetToZero = false)
	{
		_boneNames = boneNames;
		_resetToZero = resetToZero;
	}
}

internal class CustomBoneGroup
{
	public readonly string Name;
	public string[] BoneNameStrings => CustomBoneInfo.BoneNames[Name]._boneNames;
	public bool State;
	public CustomBoneGroup(string name)
	{
		Name = name;
	}
}