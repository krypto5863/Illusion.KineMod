using System.Collections.Generic;

internal class CustomBoneGroup
{
	public static readonly Dictionary<string, string[]> BoneNames = new Dictionary<string, string[]>
	{
#if HS2
		{"L. Arm", new []{"cf_j_armup00_L", "cf_j_armlow01_L", "cf_j_hand_L"}},
		{"R. Arm", new []{"cf_j_armup00_R", "cf_j_armlow01_R", "cf_j_hand_R"}},
		{"Clavicle", new []{"cf_j_Shoulder_R", "cf_j_Shoulder_L"}},
		{"Upper Body", new []{"cf_j_Spine03"}},
		{"Spine", new []{"cf_j_Spine01", "cf_j_Spine02"}},
		{"Waist", new []{"cf_j_kosi01"}},
		{"Hips", new []{"cf_j_kosi02"}},
		{"L. Leg", new []{"cf_j_foot01_L", "cf_j_leglow01_L", "cf_j_legup00_L"}},
		{"R. Leg", new []{"cf_j_foot01_R", "cf_j_leglow01_R", "cf_j_legup00_R"}},
		{"Feet", new []{"cf_j_Foot02_R", "cf_j_Foot02_L"}},
		{"Toes", new []{"cf_j_Toes01_R", "cf_j_Toes01_L"}},
#else
		{"L. Arm", new []{"cf_j_arm00_L", "cf_j_forearm01_L", "cf_j_hand_L"}},
		{"R. Arm", new []{"cf_j_arm00_R", "cf_j_forearm01_R", "cf_j_hand_R"}},
		{"Clavicle", new []{"cf_j_Shoulder_R", "cf_j_Shoulder_L"}},
		{"Spine", new []{"cf_j_Spine01", "cf_j_Spine02"}},
		{"Waist", new []{"cf_j_waist01"}},
		{"L. Leg", new []{"cf_j_leg01_L", "cf_j_leg03_L", "cf_j_thigh00_L"}},
		{"R. Leg", new []{"cf_j_leg01_R", "cf_j_leg03_R", "cf_j_thigh00_R"}},
		{"Toes", new []{"cf_j_Toes_R", "cf_j_Toes_L"}}
#endif
	};
	public string[] BoneNameStrings => BoneNames[Name];
	public readonly string Name;
	public bool State;
	public CustomBoneGroup(string name)
	{
		Name = name;
	}
}