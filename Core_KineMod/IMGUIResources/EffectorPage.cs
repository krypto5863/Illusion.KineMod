using System.Collections.Generic;
using System.Linq;
using Studio;
using UnityEngine;

//Todo: Overhaul effector menu
internal static class EffectorPage
{
	private static readonly Dictionary<object, string> BonesUserFriendlyNames = new Dictionary<object, string>
	{
#if HS2
		{ "f_t_shoulder_r(work)", "R. Shoulder" },
		{ "f_t_shoulder_l(work)", "L. Shoulder" },
		{ "f_t_elbo_r(work)", "R. Elbow" },
		{ "f_t_elbo_l(work)", "L. Elbow" },
		{ "f_t_arm_r(work)", "R. Hand" },
		{ "f_t_arm_l(work)", "L. Hand" },
		{ "f_t_hips(work)", "Waist" },
		{ "f_t_thigh_r(work)", "R. Hips" },
		{ "f_t_thigh_l(work)", "L. Hips" },
		{ "f_t_knee_r(work)", "R. Knee" },
		{ "f_t_knee_l(work)", "L. Knee" },
		{ "f_t_leg_r(work)", "R. Foot" },
		{ "f_t_leg_l(work)", "L. Foot" },		
#else
		{ "cf_t_shoulder_r(work)", "R. Shoulder" },
		{ "cf_t_shoulder_l(work)", "L. Shoulder" },
		{ "cf_t_elbo_r(work)", "R. Elbow" },
		{ "cf_t_elbo_l(work)", "L. Elbow" },
		{ "cf_t_hand_r(work)", "R. Hand" },
		{ "cf_t_hand_l(work)", "L. Hand" },
		{ "cf_t_hips(work)", "Waist" },
		{ "cf_t_waist_r(work)", "R. Hips" },
		{ "cf_t_waist_l(work)", "L. Hips" },
		{ "cf_t_knee_r(work)", "R. Knee" },
		{ "cf_t_knee_l(work)", "L. Knee" },
		{ "cf_t_leg_r(work)", "R. Foot" },
		{ "cf_t_leg_l(work)", "L. Foot" },	
#endif
	};
	private static readonly string[] LimbNames =
	{
		"R. Arm",
		"L. Arm",
		"Waist",
		"R. Leg",
		"L. Leg",
	};
	private static readonly Dictionary<string, string> BonesToLimbName = new Dictionary<string, string>
	{
#if HS2
		{ "f_t_shoulder_r(work)", LimbNames[0] },
		{ "f_t_shoulder_l(work)", LimbNames[1] },
		{ "f_t_elbo_r(work)", LimbNames[0] },
		{ "f_t_elbo_l(work)", LimbNames[1] },
		{ "f_t_arm_r(work)", LimbNames[0] },
		{ "f_t_arm_l(work)", LimbNames[1] },
		{ "f_t_hips(work)", LimbNames[2] },
		{ "f_t_thigh_r(work)", LimbNames[3] },
		{ "f_t_thigh_l(work)", LimbNames[4] },
		{ "f_t_knee_r(work)", LimbNames[3] },
		{ "f_t_knee_l(work)", LimbNames[4] },
		{ "f_t_leg_r(work)", LimbNames[3] },
		{ "f_t_leg_l(work)", LimbNames[4] },
#else
		{ "cf_t_shoulder_r(work)", LimbNames[0] },
		{ "cf_t_shoulder_l(work)", LimbNames[1] },
		{ "cf_t_elbo_r(work)", LimbNames[0] },
		{ "cf_t_elbo_l(work)", LimbNames[1] },
		{ "cf_t_hand_r(work)", LimbNames[0] },
		{ "cf_t_hand_l(work)", LimbNames[1] },
		{ "cf_t_hips(work)", LimbNames[2] },
		{ "cf_t_waist_r(work)", LimbNames[3] },
		{ "cf_t_waist_l(work)", LimbNames[4] },
		{ "cf_t_knee_r(work)", LimbNames[3] },
		{ "cf_t_knee_l(work)", LimbNames[4] },
		{ "cf_t_leg_r(work)", LimbNames[3] },
		{ "cf_t_leg_l(work)", LimbNames[4] },
#endif
	};

	internal static void Draw(OCIChar character)
	{
		var effectors = character.finalIK.solver.effectors.GroupBy(m => character.finalIK.solver.chain[m.chainIndex]);

		var massSet = -1;

		GUILayout.BeginHorizontal();
		{
			if (GUILayout.Button("All 0"))
			{
				massSet = 0;
			}

			if (GUILayout.Button("All 1"))
			{
				massSet = 1;
			}
		}
		GUILayout.EndHorizontal();

		foreach (var chainEffector in effectors)
		{
			var limbName = BonesToLimbName.TryGetValue(chainEffector.FirstOrDefault()?.target?.name.ToLower() ?? string.Empty, out var boneName)
				? boneName
				: string.Empty;

			GUILayout.BeginVertical(Styles.LightSection);
			GUILayout.BeginHorizontal(Styles.DarkerSection);
			GUILayout.FlexibleSpace();
			GUILayout.Label(limbName);
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			foreach (var effector in chainEffector.OrderBy(m => m.nodeIndex))
			{
				GUILayout.BeginHorizontal();
				{
					var displayName = (BonesUserFriendlyNames.TryGetValue(effector.target.name.ToLower(), out var newName)) ? newName : effector.target.name;

					GUILayout.Label(displayName);
					GUILayout.FlexibleSpace();
					if (GUILayout.Button("0"))
					{
						effector.positionWeight = 0;
						effector.rotationWeight = 0;
					}

					if (GUILayout.Button("1"))
					{
						effector.positionWeight = 1;
						effector.rotationWeight = 1;
					}
				}
				GUILayout.EndHorizontal();

				GUILayout.BeginVertical(Styles.DarkerSection);
				{
					GUILayout.Label($"Position Weight ({effector.positionWeight:0.00})");
					effector.positionWeight = GUILayout.HorizontalSlider(effector.positionWeight, 0, 1);
					if (effector.isEndEffector)
					{
						GUILayout.Label($"Rotation Weight ({effector.rotationWeight:0.00})");
						effector.rotationWeight = GUILayout.HorizontalSlider(effector.rotationWeight, 0, 1);
					}
				}
				GUILayout.EndVertical();

				if (massSet != -1)
				{
					effector.positionWeight = massSet;
					effector.rotationWeight = massSet;
				}
			}

			if (chainEffector.Key.bendConstraint?.bendGoal == null)
			{
				//Ends the vertical at the start of the foreach.
				GUILayout.EndVertical();
				continue;
			}
			var displayName1 = (BonesUserFriendlyNames.TryGetValue(chainEffector.Key.bendConstraint.bendGoal.name.ToLower(), out var newName1)) ? newName1 : chainEffector.Key.bendConstraint.bendGoal.name;
			GUILayout.BeginHorizontal();
			{
				GUILayout.Label(displayName1);
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("0"))
				{
					chainEffector.Key.bendConstraint.weight = 0;
				}

				if (GUILayout.Button("1"))
				{
					chainEffector.Key.bendConstraint.weight = 1;
				}
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginVertical(Styles.DarkerSection);
			{
				GUILayout.Label($"Weight ({chainEffector.Key.bendConstraint.weight:0.00})");
				chainEffector.Key.bendConstraint.weight =
					GUILayout.HorizontalSlider(chainEffector.Key.bendConstraint.weight, 0, 1);
			}
			GUILayout.EndVertical();
			if (massSet != -1)
			{
				chainEffector.Key.bendConstraint.weight = massSet;
			}

			GUILayout.EndVertical();
		}
	}
}
