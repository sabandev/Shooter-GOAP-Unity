#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace GOAP.Editor
{
    public sealed class GOAPPlannerVisualizerWindow : EditorWindow
    {
        // ───── Styles ────────────────────────────────────────────────
        
        /// <summary>
        /// Static styles class for appearance editor window.
        /// </summary>
        private static class Styles                                                                                                                                                                                             
        {
            // ───── Public properties ────────────────────────────────────────────────
            
            public static GUIStyle WinningNode;                                                                                                                                                                                 
            public static GUIStyle NormalNode;
            public static GUIStyle HeaderBox;
            public static GUIStyle FactLabel;
            public static GUIStyle RejectedLabel;                                                                                                                                                                               
            public static bool     Initialised;

            // ───── Public methods ────────────────────────────────────────────────
            
            public static void Init()
            {                                                                                                                                                                                                                   
                if (Initialised) return;

                HeaderBox = new GUIStyle(EditorStyles.helpBox) { padding = new RectOffset(8, 8, 6, 6) };                                                                                                                        

                WinningNode = new GUIStyle(EditorStyles.helpBox) { padding = new RectOffset(6, 6, 4, 4) };                                                                                                                      
                WinningNode.normal.background = MakeTex(new Color(0.15f, 0.4f, 0.15f, 0.4f));
                                                                                                                                                                                                                              
                NormalNode = new GUIStyle(EditorStyles.helpBox) { padding = new RectOffset(6, 6, 4, 4) };
                                                                                                                                                                                                                              
                FactLabel = new GUIStyle(EditorStyles.miniLabel) { richText = true };                                                                                                                                           

                RejectedLabel = new GUIStyle(EditorStyles.miniLabel) { richText = true };                                                                                                                                       
                RejectedLabel.normal.textColor = new Color(0.85f, 0.35f, 0.35f);
                                                                                                                                                                                                                              
                Initialised = true;
            }

            // ───── Private methods ────────────────────────────────────────────────
            
            private static Texture2D MakeTex(Color col)                                                                                                                                                                         
            {
                var tex = new Texture2D(1, 1);                                                                                                                                                                                  
                tex.SetPixel(0, 0, col);
                tex.Apply();
                return tex;
            }                                                                                                                                                                                                                   
        }

        // ───── Private properties ────────────────────────────────────────────────
        
        private GOAPAgent[] _agents = Array.Empty<GOAPAgent>();
        
        private Vector2 _scroll;
        
        private int _selectedAgentIndex;
        
        private bool[] _foldouts = Array.Empty<bool>();

        // ───── Menu ────────────────────────────────────────────────
        
        [MenuItem("GOAP/Planner Visualizer")]
        public static void Open() => GetWindow<GOAPPlannerVisualizerWindow>("Planner Visualizer");

        // ───── Lifecycle methods ────────────────────────────────────────────────
        
        private void OnEnable() => EditorApplication.playModeStateChanged += _ => { _agents = Array.Empty<GOAPAgent>(); Repaint(); };

        // ───── GUI ────────────────────────────────────────────────
        
        private void OnGUI()
        {
            Styles.Init();
            
            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to use the Planner Visualizer.",  MessageType.Info);
                return;
            }
            
            if (_agents.Length == 0)
                _agents = FindObjectsByType<GOAPAgent>(FindObjectsSortMode.None);
            
            if (_agents.Length == 0)
            {
                EditorGUILayout.HelpBox("No GOAPAgents found in the scene.", MessageType.Info);
                return;
            }
            
            DrawToolbar();
            
            GOAPAgent agent = SelectedAgent();
            if (agent == null) { return; }
            
            PlanSearchTrace trace = agent.LastPlanTrace;
            
            if (trace == null)
            {
                EditorGUILayout.Space(10.0f);
                EditorGUILayout.LabelField("No plan trace detected. Click 'Capture Next Plan' and wait for the agent to re-plan.", EditorStyles.centeredGreyMiniLabel);
                return;
            }
            
            DrawHeader(trace);
            
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            
            if (_foldouts.Length != trace.Nodes.Count)
            {
                _foldouts = new bool[trace.Nodes.Count];
                for (int i = 0; i < _foldouts.Length; i++) { _foldouts[i] = true; }
            }
            
            if (trace.Nodes.Count > 0)
                DrawNode(trace, 0, 0);
            
            EditorGUILayout.EndScrollView();
        }
        
        // ───── Private methods ───────────────────────────────────────────────────────────                                                                                                                                            
   
          private void DrawToolbar()                                                                                                                                                                                              
          {       
              EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
                                                                                                                                                                                                                                  
              string[] names = Array.ConvertAll(_agents, a => a != null ? a.name : "(missing)");
              _selectedAgentIndex = Mathf.Clamp(_selectedAgentIndex, 0, _agents.Length - 1);                                                                                                                                      
              _selectedAgentIndex = EditorGUILayout.Popup(_selectedAgentIndex, names, EditorStyles.toolbarDropDown, GUILayout.Width(160.0f));                                                                                     
                                                                                                                                                                                                                                  
              GUILayout.FlexibleSpace();                                                                                                                                                                                          
                                                                                                                                                                                                                                  
              GOAPAgent agent = SelectedAgent();                                                                                                                                                                                  
              if (agent != null && agent.CaptureNextTrace)
              {                                                                                                                                                                                                                   
                  GUI.color = new Color(1.0f, 0.8f, 0.2f);
                  GUILayout.Label("Waiting for replan...", EditorStyles.toolbarButton);                                                                                                                                           
                  GUI.color = Color.white;                                                                                                                                                                                        
              }                                                                                                                                                                                                                   
                                                                                                                                                                                                                                  
              if (GUILayout.Button("Capture Next Plan", EditorStyles.toolbarButton) && agent != null)
                  agent.CaptureNextTrace = true;
                                                                                                                                                                                                                                  
              if (GUILayout.Button("Refresh", EditorStyles.toolbarButton))
                  _agents = FindObjectsByType<GOAPAgent>(FindObjectsSortMode.None);                                                                                                                                               
                                                                                                                                                                                                                                  
              EditorGUILayout.EndHorizontal();
          }                                                                                                                                                                                                                       
                  
          private void DrawHeader(PlanSearchTrace trace)
          {
              EditorGUILayout.Space(4.0f);
              EditorGUILayout.BeginVertical(Styles.HeaderBox);                                                                                                                                                                    
   
              // Title row                                                                                                                                                                                                        
              EditorGUILayout.BeginHorizontal();
              EditorGUILayout.LabelField($"Goal: <b>{trace.GoalName}</b>", Styles.FactLabel, GUILayout.ExpandWidth(true));                                                                                                        
              GUI.color = trace.PlanFound ? new Color(0.3f, 0.9f, 0.4f) : new Color(0.9f, 0.35f, 0.35f);                                                                                                                          
              EditorGUILayout.LabelField(trace.PlanFound ? "✓  Plan Found" : "✗  No Plan", EditorStyles.boldLabel, GUILayout.Width(110.0f));                                                                                      
              GUI.color = Color.white;                                                                                                                                                                                            
              EditorGUILayout.EndHorizontal();                                                                                                                                                                                    
                                                                                                                                                                                                                                  
              EditorGUILayout.LabelField($"Trigger: {trace.TriggerReason}   |   Time: {trace.Timestamp:F2}s   |   {trace.Nodes.Count} nodes expanded", EditorStyles.miniLabel);                                                     
                  
              EditorGUILayout.Space(4.0f);                                                                                                                                                                                        
                  
              DrawFactRow("Goal State",    trace.GoalStateFacts);                                                                                                                                                                 
              DrawFactRow("Current State", trace.CurrentStateFacts);
                                                                                                                                                                                                                                  
              EditorGUILayout.EndVertical();
              EditorGUILayout.Space(4.0f);
          }                                                                                                                                                                                                                       
   
          private static void DrawFactRow(string label, List<(string key, string value)> facts)                                                                                                                                   
          {       
              EditorGUILayout.BeginHorizontal();
              EditorGUILayout.LabelField(label, EditorStyles.miniBoldLabel, GUILayout.Width(90.0f));                                                                                                                              
              string joined = facts.Count > 0
                  ? string.Join("  |  ", facts.ConvertAll(f => $"<b>{f.key}</b>={f.value}"))                                                                                                                                      
                  : "—";                                                                                                                                                                                                          
              EditorGUILayout.LabelField(joined, Styles.FactLabel);                                                                                                                                                               
              EditorGUILayout.EndHorizontal();                                                                                                                                                                                    
          }                                                                                                                                                                                                                       
   
          private void DrawNode(PlanSearchTrace trace, int index, int depth)                                                                                                                                                      
          {       
              TraceNode node = trace.Nodes[index];

              GUIStyle boxStyle = node.IsOnWinningPath ? Styles.WinningNode : Styles.NormalNode;                                                                                                                                  
   
              EditorGUI.indentLevel = depth;                                                                                                                                                                                      
              EditorGUILayout.BeginVertical(boxStyle);
                                                                                                                                                                                                                                  
              // Node foldout label
              string prefix   = node.IsOnWinningPath ? "★ " : "";
              string costInfo = node.ActionName == null ? $"h={node.HCost:F0}" : $"g={node.GCost:F1}  h={node.HCost:F0}  f={node.FCost:F1}";                                                                                      
              string title    = node.ActionName == null ? $"ROOT  [{costInfo}]" : $"{prefix}{node.ActionName}  [{costInfo}]";                                                                                                     
                                                                                                                                                                                                                                  
              _foldouts[index] = EditorGUILayout.Foldout(_foldouts[index], title, true);                                                                                                                                          
                                                                                                                                                                                                                                  
              if (_foldouts[index])                                                                                                                                                                                               
              {   
                  EditorGUI.indentLevel++;
                                                                                                                                                                                                                                  
                  // Required world state
                  if (node.RequiredFacts.Count == 0)                                                                                                                                                                              
                  {
                      EditorGUILayout.LabelField("<color=#66cc66>✓ Current state satisfies all requirements — plan complete</color>", Styles.FactLabel);
                  }                                                                                                                                                                                                               
                  else
                  {                                                                                                                                                                                                               
                      EditorGUILayout.LabelField("Requires:", EditorStyles.miniBoldLabel);
                      foreach ((string key, string value) in node.RequiredFacts)
                          EditorGUILayout.LabelField($"  {key} = <b>{value}</b>", Styles.FactLabel);                                                                                                                              
                  }                                                                                                                                                                                                               
                                                                                                                                                                                                                                  
                  // Rejected actions                                                                                                                                                                                             
                  if (node.Rejected.Count > 0)
                  {
                      EditorGUILayout.Space(2.0f);
                      EditorGUILayout.LabelField("Pruned:", EditorStyles.miniBoldLabel);
                      foreach (RejectedAction r in node.Rejected)                                                                                                                                                                 
                          EditorGUILayout.LabelField($"  ✗ {r.ActionName}  — effects don't satisfy any requirement here", Styles.RejectedLabel);
                  }                                                                                                                                                                                                               
                  
                  EditorGUI.indentLevel--;                                                                                                                                                                                        
              }   

              EditorGUILayout.EndVertical();

              // Recurse into children
              for (int i = 0; i < trace.Nodes.Count; i++)
              {                                                                                                                                                                                                                   
                  if (trace.Nodes[i].ParentIndex == index)
                      DrawNode(trace, i, depth + 1);                                                                                                                                                                              
              }   

              EditorGUI.indentLevel = depth;                                                                                                                                                                                      
          }
                                                                                                                                                                                                                                  
          // ───── Helper methods ───────────────────────────────────────────────────────────

          private GOAPAgent SelectedAgent()
          {
              if (_agents.Length == 0) return null;
              _selectedAgentIndex = Mathf.Clamp(_selectedAgentIndex, 0, _agents.Length - 1);                                                                                                                                      
              return _agents[_selectedAgentIndex];
          }   
    }
}
#endif