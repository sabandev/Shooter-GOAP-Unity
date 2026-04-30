#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace GOAP.Editor
{
    /// <summary>
    /// Custom editor window for displaying all the agent types in one place.
    /// </summary>
    public sealed class GOAPAgentTypeWindow : EditorWindow
    {
        // ───── Constants ────────────────────────────────────────────────
        
        private const float LEFT_PANEL_WIDTH = 210.0f;
        private const float ROW_HEIGHT = 46.0f;

        // ───── Private properties ────────────────────────────────────────────────
        
        private readonly List<GOAPAgentType> _types = new();
        
        private GOAPAgentType _selected;
        private SerializedObject _serializedType;
        
        private Vector2 _leftScroll;
        private Vector2 _rightScroll;

        // ───── Menu ────────────────────────────────────────────────
        
        [MenuItem("GOAP/Agent Type Editor")]
        public static void Open() => GetWindow<GOAPAgentTypeWindow>("Agent Types");

        // ───── Lifecycle methods ────────────────────────────────────────────────
        
        private void OnEnable() => RefreshTypeList();
        private void OnDisable() => RefreshTypeList();

        // ───── GUI ────────────────────────────────────────────────
        
        private void OnGUI()
        {
            DrawToolbar();
            
            EditorGUILayout.BeginHorizontal();
            DrawLeftPanel();
            DrawVerticalDivider();
            DrawRightPanel();
            EditorGUILayout.EndHorizontal();
        }

        // ───── Private methods ────────────────────────────────────────────────
        
        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            
            GUILayout.Label("GOAP Agent Types", EditorStyles.toolbarButton);
            
            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button("New Type",  EditorStyles.toolbarButton)) { CreateNewType(); }
            if (GUILayout.Button("Refresh",  EditorStyles.toolbarButton)) { RefreshTypeList(); }
            
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawLeftPanel()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(LEFT_PANEL_WIDTH));
            _leftScroll = EditorGUILayout.BeginScrollView(_leftScroll);
            
            if (_types.Count == 0)
            {
                EditorGUILayout.Space(10.0f);
                EditorGUILayout.LabelField("No Agent Types found.", EditorStyles.centeredGreyMiniLabel);
            }

            foreach (GOAPAgentType type in _types)
            {
                if (type == null) { continue; }
                DrawTypeRow(type);
            }
            
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }
        
        private void DrawRightPanel()
        {
            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            
            if (_selected == null || _serializedType == null)
            {
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField("Select a type from the list top inspect/edit it.", EditorStyles.centeredGreyMiniLabel);
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndVertical();
                return;
            }
            _serializedType.Update();
            _rightScroll = EditorGUILayout.BeginScrollView(_rightScroll);
            
            EditorGUILayout.Space(10.0f);
            
            // Header
            EditorGUILayout.BeginHorizontal();
            
            EditorGUILayout.LabelField(_selected.TypeName, EditorStyles.largeLabel);
            
            if (GUILayout.Button("Ping Asset", EditorStyles.miniButton, GUILayout.Width(80.0f)))
                EditorGUIUtility.PingObject(_selected);
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(10.0f);
            
            // Type name
            SerializedProperty nameProp = _serializedType.FindProperty("_typeName");
            EditorGUILayout.PropertyField(nameProp, new GUIContent("Type Name"));
            
            EditorGUILayout.Space(10.0f);
            DrawHorizontalDivider();
            EditorGUILayout.Space(10.0f);
            
            // Actions
            EditorGUILayout.LabelField("ACTIONS", EditorStyles.miniBoldLabel);
            
            EditorGUILayout.Space(5.0f);
            
            SerializedProperty actionsProp = _serializedType.FindProperty("_actions");
            EditorGUILayout.PropertyField(actionsProp, GUIContent.none, true);
            
            EditorGUILayout.Space(10.0f);
            DrawHorizontalDivider();
            EditorGUILayout.Space(10.0f);
            
            // Goals
            EditorGUILayout.LabelField("GOALS", EditorStyles.miniBoldLabel);
            
            EditorGUILayout.Space(5.0f);
            
            SerializedProperty goalsProp = _serializedType.FindProperty("_goals");
            EditorGUILayout.PropertyField(goalsProp, GUIContent.none, true);
            
            EditorGUILayout.Space(10.0f);

            EditorGUILayout.EndScrollView();
            
            _serializedType.ApplyModifiedProperties();
            EditorGUILayout.EndVertical();
        }
        
        private void DrawTypeRow(GOAPAgentType type)
        {
            bool isSelected = type == _selected;
            Rect rowRect = GUILayoutUtility.GetRect(LEFT_PANEL_WIDTH, ROW_HEIGHT);
            
            if (isSelected)
                EditorGUI.DrawRect(rowRect, new Color(0.2f, 0.45f, 0.8f, 0.25f));
            else if (rowRect.Contains(Event.current.mousePosition))
                EditorGUI.DrawRect(rowRect, new Color(1.0f, 1.0f, 1.0f, 0.05f));
            
            if (Event.current.type == EventType.MouseDown && rowRect.Contains(Event.current.mousePosition))
            {
                Select(type);
                Event.current.Use();
                Repaint();
            }
            
            Rect nameRect = new Rect(rowRect.x + 10.0f, rowRect.y + 8.0f, rowRect.width - 10.0f, 18.0f);
            Rect infoRect = new Rect(rowRect.x + 10.0f, rowRect.y + 26.0f, rowRect.width - 10.0f, 14.0f);
            
            GUI.Label(nameRect, type.TypeName, EditorStyles.boldLabel);
            GUI.Label(infoRect, $"{type.Actions.Count} actions  ·  {type.Goals.Count} goals", EditorStyles.miniLabel);
            
            Rect divRect = new Rect(rowRect.x, rowRect.yMax - 1.0f, rowRect.width, 1.0f);
            EditorGUI.DrawRect(divRect, new Color(0.5f, 0.5f, 0.5f, 0.15f));
        }

        // ───── Helper methods ────────────────────────────────────────────────
        
        private void CreateNewType()
        {
            string path = EditorUtility.SaveFilePanelInProject("New Agent Type", "AIType_New", "asset", "Choose where to save the new Agent Type asset.");
            
            if (string.IsNullOrEmpty(path)) { return; }
            
            GOAPAgentType created = CreateInstance<GOAPAgentType>();
            AssetDatabase.CreateAsset(created, path);
            AssetDatabase.SaveAssets();
            
            RefreshTypeList();
            Select(created);
        }
        
        private void Select(GOAPAgentType type)
        {
            _selected = type;
            _serializedType = type != null ? new SerializedObject(type) : null;
            Repaint();
        }
        
        private void RefreshTypeList()
        {
            _types.Clear();

            foreach (string guid in AssetDatabase.FindAssets("t:GOAPAgentType"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GOAPAgentType type = AssetDatabase.LoadAssetAtPath<GOAPAgentType>(path);
                if (type != null)  { _types.Add(type); }
            }
            
            if (_selected != null && !_types.Contains(_selected))
                Select(null);
            
            Repaint();
        }
        
        private static void DrawHorizontalDivider()
        {                                          
            Rect rect = EditorGUILayout.GetControlRect(false, 1.0f, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.25f));                        
        }                                                                                                                                                                                                                       
                                                                                                                                                                                                                                  
        private static void DrawVerticalDivider()                                                                                                                                                                               
        {                                                                                                                                                                                                                       
            Rect rect = GUILayoutUtility.GetRect(1.0f, float.MaxValue, GUILayout.Width(1.0f));
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.25f));                     
        }  
    }
}
#endif