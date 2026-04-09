using UnityEngine;
using UnityEditor;

namespace GOAP.Editor
{
    /// <summary>
    /// Custom inspector for GOAPGoal ScriptableObjects.
    /// Mirrors action data custom inspector. See GOAPActionDataEditor.
    /// </summary>
    [CustomEditor(typeof(GOAPGoal), true)]
    public sealed class GOAPGoalEditor : UnityEditor.Editor
    {
        // -----Private properties-----
        private SerializedProperty _goalNameProp;
        private SerializedProperty _basePriorityProp;
        private SerializedProperty _desiredStateProp;
        private string _validationMessage;
        private bool _validationPassed;
        private bool _hasValidated;

        // -----Lifecycle methods-----
        private void OnEnable()
        {
            _goalNameProp = serializedObject.FindProperty("_goalName");
            _basePriorityProp = serializedObject.FindProperty("_basePriority");
            _desiredStateProp = serializedObject.FindProperty("_desiredState");
        }

        // -----GUI-----
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.BeginVertical(GOAPActionDataEditor.Styles.CardBackground);

            DrawGoalHeader();
            DrawDivider();
            EditorGUILayout.Space(4.0f);
            DrawDesiredState();
            EditorGUILayout.Space(4.0f);
            DrawAdditionalProperties();
            EditorGUILayout.Space(4.0f);
            DrawValidateButton();

            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }

        // -----Private methods-----
        private void DrawGoalHeader()
        {
            EditorGUILayout.LabelField(_goalNameProp.stringValue, GOAPActionDataEditor.Styles.CardHeader);

            EditorGUILayout.Space(4.0f);

            EditorGUILayout.PropertyField(_goalNameProp, new GUIContent("Goal Name"));
            EditorGUILayout.PropertyField(_basePriorityProp, new GUIContent("Base Priority"));
        }

        private void DrawDesiredState()
        {
            EditorGUILayout.LabelField("DESIRED STATE", GOAPActionDataEditor.Styles.SectionLabel);

            EditorGUILayout.Space(4.0f);

            if (_desiredStateProp.arraySize == 0)
            {
                EditorGUILayout.LabelField("No desired state defined.", EditorStyles.centeredGreyMiniLabel);
            }
            else
            {
                for (int i = 0; i < _desiredStateProp.arraySize; i++)
                {
                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.PropertyField(_desiredStateProp.GetArrayElementAtIndex(i), GUIContent.none);

                    if (GUILayout.Button("✕", EditorStyles.miniButton, GUILayout.Width(20.0f)))
                    {
                        _desiredStateProp.DeleteArrayElementAtIndex(i);
                        break;
                    }

                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.Space(4.0f);

            if (GUILayout.Button("+ Add Condition", EditorStyles.miniButton))
                _desiredStateProp.InsertArrayElementAtIndex(_desiredStateProp.arraySize);
        }

        private void DrawAdditionalProperties()
        {
            DrawPropertiesExcluding
            (
                serializedObject,
                "_goalName",
                "_basePriority",
                "_desiredState",
                "m_Script"
            );
        }

        private void DrawValidateButton()
        {
            DrawDivider();

            EditorGUILayout.Space(4.0f);

            if (GUILayout.Button("Validate", GUILayout.Height(24.0f)))
            {
                _validationMessage = Validate();
                _validationPassed  = string.IsNullOrEmpty(_validationMessage);
                _hasValidated      = true;
            }

            if (_hasValidated)
            {
                EditorGUILayout.Space(2.0f);
                EditorGUILayout.LabelField
                (
                    _validationPassed ? "✓ Goal data is valid." : $"✗ {_validationMessage}", 
                    _validationPassed ? GOAPActionDataEditor.Styles.ValidateSuccess : GOAPActionDataEditor.Styles.ValidateFailure
                );
            }
        }

        private string Validate()
        {
            string[] allKeys = BlackboardKeysReflector.GetAllKeys();

            if (string.IsNullOrEmpty(_goalNameProp.stringValue))
                return "Goal name is empty.";

            if (_desiredStateProp.arraySize == 0)
                return "Goal has no desired state — the planner has nothing to satisfy.";

            for (int i = 0; i < _desiredStateProp.arraySize; i++)
            {
                SerializedProperty pair = _desiredStateProp.GetArrayElementAtIndex(i);
                SerializedProperty keyProp = pair.FindPropertyRelative("_key");
                string key = keyProp.stringValue;

                if (!System.Array.Exists(allKeys, k => k == key))
                    return $"Desired state key '{key}' does not exist in BlackboardKeys.";
            }

            return null;
        }

        // -----Helper methods-----
        private static void DrawDivider()
        {
            Rect rect = EditorGUILayout.GetControlRect(false, 1.0f, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.25f));
        }
    }
}

