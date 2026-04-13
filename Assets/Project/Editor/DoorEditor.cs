#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.AI;

namespace GOAP.Editor
{
    /// <summary>
    /// Editor helper class for Door.cs
    /// Aids in designer choices when placing and modifying doors in the environment.
    /// Syncs door state and rotation for GOAP Agent and Door Smart Object.
    /// </summary>
    [CustomEditor(typeof(Door))]
    public sealed class DoorEditor : UnityEditor.Editor
    {
        // -----Inspector-----
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            Door door = (Door)target;
            
            EditorGUILayout.Space(20.0f);
            
            EditorGUILayout.LabelField("Door Tools",  EditorStyles.boldLabel);
            
            // Display current door state for debugging
            GUI.enabled = false;
            EditorGUILayout.EnumPopup("Current State", door.State);
            GUI.enabled = true;
            
            SerializedObject so = new SerializedObject(door);
            EditorGUILayout.Space(10.0f);
            
            SerializedProperty doorState = so.FindProperty("_state");
            so.Update();
            EditorGUILayout.BeginHorizontal();
            if (doorState.enumValueIndex == (int)Door.DoorState.Open)
            {
                GUI.enabled = false;
                if (GUILayout.Button("Open", GUILayout.Height(32.0f)))
                {
                    SetStartState(door, Door.DoorState.Open);
                }
                GUI.enabled = true;
            }
            else
            {
                if (GUILayout.Button("Open", GUILayout.Height(32.0f)))
                {
                    SetStartState(door, Door.DoorState.Open);
                }
            }
            
            if (doorState.enumValueIndex == (int)Door.DoorState.Closed)
            {
                GUI.enabled = false;
                if (GUILayout.Button("Close", GUILayout.Height(32.0f)))
                {
                    SetStartState(door, Door.DoorState.Closed);
                }
                GUI.enabled = true;
            }
            else
            {
                if (GUILayout.Button("Close", GUILayout.Height(32.0f)))
                {
                    SetStartState(door, Door.DoorState.Closed);
                }
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.HelpBox("Start Open/Closed sets the door's hinge rotation and door state appropriately to sync AI and Door terms.", MessageType.Info);
        }
        
        // -----Private methods-----
        private void SetStartState(Door door, Door.DoorState state)
        {
            SerializedObject so = new SerializedObject(door);
            so.Update();
            
            SerializedProperty hinge = so.FindProperty("_hinge");
            SerializedProperty closedAngle = so.FindProperty("_closedAngle");
            SerializedProperty openAngle = so.FindProperty("_openAngle");
            SerializedProperty doorState = so.FindProperty("_state");
            SerializedProperty navMeshObstacle = so.FindProperty("_navMeshObstacle");
            
            if (hinge.objectReferenceValue is Transform hingeTransform)
            {
                Undo.RecordObject(hingeTransform, $"Door Set Start {state}");
                
                Vector3 euler = hingeTransform.localEulerAngles;
                
                if (state == Door.DoorState.Open)
                    euler.y = closedAngle.floatValue + openAngle.floatValue;
                else
                    euler.y = closedAngle.floatValue;
                
                hingeTransform.localEulerAngles = euler;
                EditorUtility.SetDirty(hingeTransform);
            }
            
            doorState.enumValueIndex = (int)state;
            
            if (navMeshObstacle.objectReferenceValue is NavMeshObstacle obstacle)
            {
                Undo.RecordObject(obstacle, $"Door NavMeshObstacle {state}");
                obstacle.enabled = state == Door.DoorState.Open;
                EditorUtility.SetDirty(obstacle);
            }
            
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(door);
            
            Debug.Log($"[DoorEditor] '{door.name}' set to start in the {state} state.");
        }
    }
}
#endif
