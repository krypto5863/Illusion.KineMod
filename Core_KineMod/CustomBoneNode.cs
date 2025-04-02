using System.Collections.Generic;

namespace Core_KineMod.UGUIResources
{
	internal class CustomBoneNode
	{
		public static List<CustomBoneNode> CustomNodes = new List<CustomBoneNode>
		{
#if KKS
			new CustomBoneNode("cf_s_wrist_L", "L. Wrist", 4, 1), new CustomBoneNode("cf_s_wrist_R", "R. Wrist", 3, 1)
#elif HS2
			new CustomBoneNode("cf_J_ArmLow01_s_L", "L. Forearm", 4, 1), new CustomBoneNode("cf_J_ArmLow01_s_R", "R. Forearm", 3, 1),
			new CustomBoneNode("cf_J_ArmLow02_s_L", "L. Wrist", 4, 1), new CustomBoneNode("cf_J_ArmLow02_s_R", "R. Wrist", 3, 1)
#endif
		};

		public string Bone { get; }
		public string Name { get; }
		public int Group { get; }
		public int Level { get; }

		public CustomBoneNode(string bone, string name, int group, int level)
		{
			Bone = bone;
			Name = name;
			Group = group;
			Level = level;
		}
	}
}