using UnityEngine;
using UnityEditor;

namespace GOAP.Editor
{
    /// <summary>
    /// Custom inspector for GOAPActionData ScriptableObjects.
    /// Displays properties in clean card layout and uses drop downs to avoid manual typing of preconditions/effects
    /// </summary>
    [CustomEditor(typeof(GOAPActionData), true)]
    public sealed class GOAPActionDataEditor : UnityEditor.Editor
    {
        // ───── Nested type ────────────────────────────────────────────────
        
        /// <summary>
        /// Public properties because GOAPGoalEditor shares these styles.
        /// </summary>
        public static class Styles
        {
            public static readonly GUIStyle CardBackground;
            public static readonly GUIStyle CardHeader;
            public static readonly GUIStyle SectionLabel;
            public static readonly GUIStyle CostLabel;
            public static readonly GUIStyle ValidateSuccess;
            public static readonly GUIStyle ValidateFailure;

            static Styles()
            {
                bool pro = EditorGUIUtility.isProSkin;

                CardBackground = new GUIStyle(EditorStyles.helpBox)
                {
                    padding = new RectOffset(10, 8, 8, 8)
                };

                CardHeader = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize  = 13,
                    alignment = TextAnchor.MiddleLeft
                };
                CardHeader.normal.textColor = pro
                    ? new Color(0.7f, 0.9f, 1.0f)
                    : new Color(0.1f, 0.3f, 0.6f);

                SectionLabel = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 10
                };
                SectionLabel.normal.textColor = pro
                    ? new Color(0.6f, 0.6f, 0.6f)
                    : new Color(0.4f, 0.4f, 0.4f);

                CostLabel = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize  = 11,
                    alignment = TextAnchor.MiddleRight,
                    padding = new RectOffset(0, 70, 0, 0),
                };
                CostLabel.normal.textColor = new Color(0.9f, 0.7f, 0.2f);

                ValidateSuccess = new GUIStyle(EditorStyles.helpBox);
                ValidateSuccess.normal.textColor = pro
                    ? new Color(0.4f, 0.9f, 0.4f)
                    : new Color(0.1f, 0.5f, 0.1f);

                ValidateFailure = new GUIStyle(EditorStyles.helpBox);
                ValidateFailure.normal.textColor = pro
                    ? new Color(0.9f, 0.4f, 0.4f)
                    : new Color(0.6f, 0.1f, 0.1f);
            }
        }

        // ───── Private properties ────────────────────────────────────────────────
        
        private SerializedProperty _actionNameProp;
        private SerializedProperty _costProp;
        private SerializedProperty _preconditionsProp;
        private SerializedProperty _effectsProp;

        private string _validationMessage;
        private bool _validationPassed;
        private bool _hasValidated;

        // ───── Lifecycle methods ────────────────────────────────────────────────
        
        private void OnEnable()
        {
            _actionNameProp    = serializedObject.FindProperty("_actionName");
            _costProp          = serializedObject.FindProperty("_cost");
            _preconditionsProp = serializedObject.FindProperty("_preconditions");
            _effectsProp       = serializedObject.FindProperty("_effects");
        }

        // ───── GUI ────────────────────────────────────────────────
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.BeginVertical(Styles.CardBackground);

            DrawActionDataHeader();
            DrawDivider();
            EditorGUILayout.Space(4.0f);
            DrawSection("Preconditions", _preconditionsProp);
            EditorGUILayout.Space(4.0f);
            DrawSection("Effects", _effectsProp);
            EditorGUILayout.Space(4.0f);
            DrawConfigurationSection();
            EditorGUILayout.Space(4.0f);
            DrawValidateButton();

            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }

        // ───── Private methods ────────────────────────────────────────────────
        
        private void DrawActionDataHeader()
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(_actionNameProp.stringValue, Styles.CardHeader);
            EditorGUILayout.LabelField($"Cost: {_costProp.floatValue:F1}", Styles.CostLabel);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4.0f);

            // Editable action name/cost
            EditorGUILayout.PropertyField(_actionNameProp, new GUIContent("Action Name"));
            EditorGUILayout.PropertyField(_costProp, new GUIContent("Cost"));
        }

        private void DrawSection(string title, SerializedProperty listProp)
        {
            EditorGUILayout.LabelField(title.ToUpper(), Styles.SectionLabel);
            EditorGUILayout.Space(4.0f);

            if (listProp.arraySize == 0)
            {
                EditorGUILayout.LabelField($"No {title.ToLower()}.", EditorStyles.centeredGreyMiniLabel);
            }
            else
            {
                for (int i = 0; i < listProp.arraySize; i++)
                {
                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.PropertyField(listProp.GetArrayElementAtIndex(i), GUIContent.none);

                    if (GUILayout.Button("✕", EditorStyles.miniButton, GUILayout.Width(20.0f)))
                    {
                        listProp.DeleteArrayElementAtIndex(i);
                        break;
                    }

                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.Space(4.0f);

            if (GUILayout.Button($"+ Add {title.TrimEnd('s')}", EditorStyles.miniButton))
                listProp.InsertArrayElementAtIndex(listProp.arraySize);
        }

        private void DrawConfigurationSection()
        {
            DrawPropertiesExcluding
            (
                serializedObject,
                "_actionName",
                "_cost",
                "_preconditions",
                "_effects",
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
                _validationPassed = string.IsNullOrEmpty(_validationMessage);
                _hasValidated = true;
            }

            if (_hasValidated)
            {
                EditorGUILayout.Space(4.0f);
                EditorGUILayout.LabelField(_validationPassed ? "✓ Action data is valid." : $"✗ {_validationMessage}", _validationPassed ? Styles.ValidateSuccess : Styles.ValidateFailure);
            }
        }

        /// <summary>
        /// Checks the action data for common mistakes made when setting up action data.
        /// Returns error messages when invalid/null.
        /// </summary>
        /// <returns></returns>
        private string Validate()
        {
            string[] allKeys = BlackboardKeysReflector.GetAllKeys();

            if (string.IsNullOrEmpty(_actionNameProp.stringValue))
                return "Action name is empty.";

            if (_costProp.floatValue < 0f)
                return "Cost cannot be negative.";

            if (_effectsProp.arraySize == 0)
                return "Action has no effects — the planner cannot use it to satisfy any goal.";
            
            // Check preconditions/effects exist in BlackboardKeys
            for (int i = 0; i < _preconditionsProp.arraySize; i++)
            {
                SerializedProperty pair = _preconditionsProp.GetArrayElementAtIndex(i);
                SerializedProperty keyProp = pair.FindPropertyRelative("_key");
                string key = keyProp.stringValue;

                if (!System.Array.Exists(allKeys, k => k == key))
                    return $"Precondition key '{key}' does not exist in BlackboardKeys.";
            }

            for (int i = 0; i < _effectsProp.arraySize; i++)
            {
                SerializedProperty pair = _effectsProp.GetArrayElementAtIndex(i);
                SerializedProperty keyProp = pair.FindPropertyRelative("_key");
                string key = keyProp.stringValue;

                if (!System.Array.Exists(allKeys, k => k == key))
                    return $"Effect key '{key}' does not exist in BlackboardKeys.";
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

