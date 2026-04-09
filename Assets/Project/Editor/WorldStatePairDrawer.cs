using UnityEngine;
using UnityEditor;

namespace GOAP.Editor
{
    /// <summary>
    /// Replaces the raw string key field with a dropdown of populated keys from BlackboardKeys
    /// </summary>
    [CustomPropertyDrawer(typeof(WorldStatePair))]
    public sealed class WorldStatePairDrawer : PropertyDrawer
    {
        // -----Constants-----
        private const float PADDING = 2.0f;
        private const float DROPDOWN_WIDTH = 0.5f;
        private const float TYPE_WIDTH = 70.0f;

        // -----GUI-----
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty keyProp = property.FindPropertyRelative("_key");
            SerializedProperty valueTypeProp = property.FindPropertyRelative("_valueType");
            SerializedProperty boolProp = property.FindPropertyRelative("_boolValue");
            SerializedProperty intProp = property.FindPropertyRelative("_intValue");
            SerializedProperty floatProp = property.FindPropertyRelative("_floatValue");

            float totalWidth = position.width;
            float dropWidth = totalWidth * DROPDOWN_WIDTH;
            float valueWidth = totalWidth - dropWidth - TYPE_WIDTH - (PADDING * 2.0f);

            // Key dropdown
            Rect dropRect = new Rect(position.x, position.y, dropWidth, position.height);

            string[] allKeys = BlackboardKeysReflector.GetAllKeys();
            int currentIndex = BlackboardKeysReflector.GetIndex(keyProp.stringValue);

            int newIndex = EditorGUI.Popup(dropRect, currentIndex, allKeys);

            if (newIndex != currentIndex || string.IsNullOrEmpty(keyProp.stringValue))
                keyProp.stringValue = allKeys.Length > 0 ? allKeys[newIndex] : string.Empty;
            
            // Value type dropdown
            Rect typeRect = new Rect(position.x + dropWidth + PADDING, position.y, TYPE_WIDTH, position.height);

            valueTypeProp.enumValueIndex = EditorGUI.Popup(typeRect, valueTypeProp.enumValueIndex, new[] { "Bool", "Int", "Float" });

            // Value field - only what's relevant
            Rect valueRect = new Rect(position.x + dropWidth + TYPE_WIDTH + (PADDING * 2.0f), position.y, valueWidth, position.height);

            switch (valueTypeProp.enumValueIndex)
            {
                case 0:
                    boolProp.boolValue = EditorGUI.Toggle(valueRect, boolProp.boolValue);
                    break;
                
                case 1:
                    intProp.intValue = EditorGUI.IntField(valueRect, intProp.intValue);
                    break;
                
                case 2:
                    floatProp.floatValue = EditorGUI.FloatField(valueRect, floatProp.floatValue);
                    break;
            }

            EditorGUI.EndProperty();
        }

        // -----Public methods-----
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }
}

