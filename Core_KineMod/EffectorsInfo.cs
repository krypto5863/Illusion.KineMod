using System.Collections.Generic;

namespace Core_KineMod.UGUIResources
{
	public class EffectorsInfo
	{
		public static readonly Dictionary<string, EffectorsInfo> BonesInfo = new Dictionary<string, EffectorsInfo>
		{
#if HS2
			{ "f_t_shoulder_R(work)", new EffectorsInfo("R. Shoulder", "R. Arm") },
			{ "f_t_shoulder_L(work)", new EffectorsInfo("L. Shoulder", "L. Arm") },
			{ "f_t_elbo_R(work)", new EffectorsInfo("R. Elbow", "R. Arm", true) },
			{ "f_t_elbo_L(work)", new EffectorsInfo("L. Elbow", "L. Arm", true) },
			{ "f_t_arm_R(work)", new EffectorsInfo("R. Hand", "R. Arm", hasRotation:true) },
			{ "f_t_arm_L(work)", new EffectorsInfo("L. Hand", "L. Arm", hasRotation:true) },
			{ "f_t_hips(work)", new EffectorsInfo("Waist", "Waist") },
			{ "f_t_thigh_R(work)", new EffectorsInfo("R. Hips", "R. Leg") },
			{ "f_t_thigh_L(work)", new EffectorsInfo("L. Hips", "L. Leg") },
			{ "f_t_knee_R(work)", new EffectorsInfo("R. Knee", "R. Leg", true) },
			{ "f_t_knee_L(work)", new EffectorsInfo("L. Knee", "L. Leg", true) },
			{ "f_t_leg_R(work)", new EffectorsInfo("R. Foot", "R. Leg", hasRotation:true) },
			{ "f_t_leg_L(work)", new EffectorsInfo("L. Foot", "L. Leg", hasRotation:true) },
#else
			{ "cf_t_shoulder_R(work)", new EffectorsInfo("R. Shoulder", "R. Arm") },
			{ "cf_t_shoulder_L(work)", new EffectorsInfo("L. Shoulder", "L. Arm") },
			{ "cf_t_elbo_R(work)", new EffectorsInfo("R. Elbow", "R. Arm", true) },
			{ "cf_t_elbo_L(work)", new EffectorsInfo("L. Elbow", "L. Arm", true) },
			{ "cf_t_hand_R(work)", new EffectorsInfo("R. Hand", "R. Arm", hasRotation : true) },
			{ "cf_t_hand_L(work)", new EffectorsInfo("L. Hand", "L. Arm", hasRotation: true) },
			{ "cf_t_hips(work)", new EffectorsInfo("Waist", "Waist") },
			{ "cf_t_waist_R(work)", new EffectorsInfo("R. Hips", "R. Leg") },
			{ "cf_t_waist_L(work)", new EffectorsInfo("L. Hips", "L. Leg") },
			{ "cf_t_knee_R(work)", new EffectorsInfo("R. Knee", "R. Leg", true) },
			{ "cf_t_knee_L(work)", new EffectorsInfo("L. Knee", "L. Leg", true) },
			{ "cf_t_leg_R(work)", new EffectorsInfo("R. Foot", "R. Leg", hasRotation: true) },
			{ "cf_t_leg_L(work)", new EffectorsInfo("L. Foot", "L. Leg", hasRotation: true) },
#endif
		};
		public string UserFriendlyName { get; }
		public string LimbName { get; }
		public bool IsBendGoal { get; }
		public bool HasRotation { get; }

		public EffectorsInfo(string userFriendlyName, string limbName, bool isBendGoal = false, bool hasRotation = false)
		{
			UserFriendlyName = userFriendlyName;
			LimbName = limbName;
			IsBendGoal = isBendGoal;
			HasRotation = hasRotation;
		}
	}
}