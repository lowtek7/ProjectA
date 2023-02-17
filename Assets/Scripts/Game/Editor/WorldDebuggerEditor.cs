using System;
using Game.World;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
	[CustomEditor(typeof(WorldDebugger))]
	public class WorldDebuggerEditor : UnityEditor.Editor
	{
		private WorldDebugger current;
		private string targetStageGuid = string.Empty;

		public void OnEnable()
		{
			current = target as WorldDebugger;
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			targetStageGuid = EditorGUILayout.TextField("StageGuid", targetStageGuid);
			if (GUILayout.Button("Transition") && Guid.TryParse(targetStageGuid, out var stageGuid))
			{
				current.StageTransition(stageGuid);
			}
		}
	}
}
