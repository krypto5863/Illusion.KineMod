using System.Collections.Generic;
using MessagePack;
using Studio;
using UnityEngine;

namespace Core_KineMod.UGUIResources
{
	[MessagePackObject]
	public struct HandState
	{
		[Key(0)]
		public OIBoneInfo.BoneGroup Hand;
		[Key(1)]
		public int Pattern;
		[Key(2)]
		public float Blending;

		[SerializationConstructor]
		public HandState(OIBoneInfo.BoneGroup hand, int pattern = 0, float blending = 0)
		{
			Hand = hand;
			Pattern = pattern;
			Blending = blending;
		}
	}

	[MessagePackObject]
	public struct HandPose
	{
		[Key(0)]
		public string Name;
		[Key(1)]
		public OIBoneInfo.BoneGroup Hand;
		[Key(2)]
		public Dictionary<string, Vector3> BoneRotations;
		[Key(3)]
		public int Pattern1;
		[Key(4)]
		public int Pattern2;
		[Key(5)]
		public float Blending;

		[SerializationConstructor]
		public HandPose(string name, OIBoneInfo.BoneGroup hand, Dictionary<string, Vector3> boneRotations, int pattern1 = 0, int pattern2 = 0, float blending = 0)
		{
			Name = name;
			Hand = hand;
			BoneRotations = boneRotations;
			Pattern1 = pattern1;
			Pattern2 = pattern2;
			Blending = blending;
		}
	}
}