using System;
using UnityEngine;

namespace Core_KineMod.UGUIResources
{
	public class ValueMonitor : MonoBehaviour
	{
		private Action _checkFunc;

		public static ValueMonitor AddMonitor(GameObject gameObject, Action checkFunc)
		{
			var valueMonitor = gameObject.AddComponent<ValueMonitor>();
			valueMonitor._checkFunc = checkFunc;
			return valueMonitor;
		}

		private void Update()
		{
			_checkFunc?.Invoke();
		}
	}
}