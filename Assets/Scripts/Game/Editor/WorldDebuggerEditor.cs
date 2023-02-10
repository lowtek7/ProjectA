using Game.World;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
	[CustomEditor(typeof(WorldDebugger))]
	public class WorldDebuggerEditor : UnityEditor.Editor
	{
		private WorldDebugger current;
		private int targetStageId = 0;

		public void OnEnable()
		{
			current = target as WorldDebugger;
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			targetStageId = EditorGUILayout.IntField("StageId", targetStageId);
			if (GUILayout.Button("Transition"))
			{
				current.StageTransition(targetStageId);
			}
		}
	}
}
