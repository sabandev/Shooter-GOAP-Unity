#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace GOAP.Editor
{
    /// <summary>
    /// Custom inspector for GOAPAgent script.
    /// Used to display relevant live diagnostics on the GOAP agent.
    /// </summary>
    [CustomEditor(typeof(GOAPAgent))]
    public sealed class GOAPAgentEditor : UnityEditor.Editor
    {
        // ───── Styles ────────────────────────────────────────────────
        
        private static class Styles
        {
            public static readonly GUIStyle DiagnosticsHeader;
            public static readonly GUIStyle SectionHeader;
            public static readonly GUIStyle ActionAvailable;
            public static readonly GUIStyle ActionUnavailable;
            public static readonly GUIStyle PlanActionPending;
            public static readonly GUIStyle PlanActionRunning;
            public static readonly GUIStyle PlanActionSucceeded;
            public static readonly GUIStyle PlanActionFailed;
            public static readonly GUIStyle BlackboardKey;
            public static readonly GUIStyle BlackboardValue;
            public static readonly GUIStyle ActiveGoalLabel;

            static Styles()
            {
                DiagnosticsHeader = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 30,
                    alignment = TextAnchor.UpperCenter,
                    fixedHeight = 40
                };
                DiagnosticsHeader.normal.textColor = new Color(1.0f, 1.0f, 1.0f, 0.9f);

                SectionHeader = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 15,
                    alignment = TextAnchor.MiddleLeft
                };
                SectionHeader.normal.textColor = new Color(1.0f, 1.0f, 1.0f, 0.9f);

                ActionAvailable = new GUIStyle(EditorStyles.label)
                {
                    richText = true,
                };
                
                ActionUnavailable = new GUIStyle(EditorStyles.label)
                {
                    richText = true,
                };
                ActionUnavailable.normal.textColor = new Color(0.6f, 0.6f, 0.6f);

                PlanActionPending = new GUIStyle(EditorStyles.miniLabel)
                {
                    alignment = TextAnchor.MiddleCenter,
                    richText = true,
                };

                PlanActionRunning = new GUIStyle(PlanActionPending);
                PlanActionRunning.normal.textColor = new Color(0.4f, 0.8f, 1.0f);

                PlanActionSucceeded  = new GUIStyle(PlanActionPending);
                PlanActionSucceeded.normal.textColor = new Color(0.4f, 1.0f, 0.4f);

                PlanActionFailed = new GUIStyle(PlanActionPending);
                PlanActionFailed.normal.textColor = new Color(1.0f, 0.4f, 0.4f);

                BlackboardKey = new GUIStyle(EditorStyles.miniLabel)
                {
                    richText = true,
                    fontStyle = FontStyle.Bold,
                };

                BlackboardValue = new GUIStyle(EditorStyles.miniLabel)
                {
                    richText = true,
                    alignment = TextAnchor.MiddleRight,
                };

                ActiveGoalLabel = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 15,
                    richText = true,
                    alignment = TextAnchor.MiddleLeft,
                };
                ActiveGoalLabel.normal.textColor = EditorGUIUtility.isProSkin ? new Color(0.4f, 1.0f, 0.6f) : new Color(0.1f, 0.5f, 0.2f);
            }
        }

        // ───── Private properties ────────────────────────────────────────────────
        
        private bool _showGoals = true;
        private bool _showActions = true;
        private bool _showPlan = true;
        private bool _showBlackboard = true;
        private Vector2 _blackboardScroll;

        private readonly Dictionary<string, bool> _proceduralCache = new();
        private float _proceduralCacheTime;
        
        private List<GOAPActionInstance> _planSnapshot;
        private int _planActiveIndexSnapshot;
        private Dictionary<string, bool> _proceduralSnapshot = new();

        // ───── Constants ────────────────────────────────────────────────
        
        private const float PROCEDURAL_CACHE_REFRESH_INTERVAL = 0.2f;

        // ───── Lifecycle methods ────────────────────────────────────────────────
        
        private void OnEnable()
        {
            // Allows inspector to constantly be re-painted, not just when it is interacted with
            EditorApplication.update += OnEditorUpdate;
        }

        private void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
        }

        private void OnEditorUpdate()
        {
            if (EditorApplication.isPlaying)
                Repaint();
        }

        // ───── Inspector GUI ────────────────────────────────────────────────
        
        public override void OnInspectorGUI()
        {
            GOAPAgent agent = (GOAPAgent)target;
            
            if (Event.current.type == EventType.Layout && EditorApplication.isPlaying)
            {
                _planSnapshot = agent.CurrentPlan != null ? new List<GOAPActionInstance>(agent.CurrentPlan) : null;
                _planActiveIndexSnapshot = agent.CurrentActionIndex;
                RefreshProceduralSnapshot(agent);
            }

            DrawDefaultInspector();

            EditorGUILayout.Space(10.0f);
            DrawDivider();

            if (!EditorApplication.isPlaying)
            {
                DrawEditModeMessage();
                return;
            }

            EditorGUILayout.Space(10.0f);

            EditorGUILayout.LabelField("Live Diagnostics", Styles.DiagnosticsHeader);
            EditorGUILayout.Space(25.0f);

            DrawActiveGoalPanel(agent);
            EditorGUILayout.Space(10.0f);

            DrawPlanPanel(agent);
            EditorGUILayout.Space(10.0f);
            DrawGoalsPanel(agent);
            EditorGUILayout.Space(10.0f);
            DrawActionsPanel(agent);            
            EditorGUILayout.Space(10.0f);
            DrawBlackboardPanel(agent);
            EditorGUILayout.Space(10.0f);
            DrawControlsPanel(agent);
        }

        // ───── Private methods ────────────────────────────────────────────────
        
        private void DrawEditModeMessage()
        {
            EditorGUILayout.Space(4.0f);
            EditorGUILayout.HelpBox("Enter Play Mode to view live GOAP diagnostics", MessageType.Info);
        }

        private void DrawActiveGoalPanel(GOAPAgent agent)
        {
            string goalName = agent.ActiveGoal?.GoalName ?? "Goal";
            string goalDisplay = agent.ActiveGoal != null ? $"<b>{goalName}</b>" : "<color=#888888>None</color>";

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Foldout(false, $"Active Goal:", true, Styles.SectionHeader);
            EditorGUILayout.LabelField(goalDisplay, Styles.ActiveGoalLabel);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawPlanPanel(GOAPAgent agent)
        {
            _showPlan = EditorGUILayout.Foldout(_showPlan, "Current Plan:", true, Styles.SectionHeader);
            if (!_showPlan) { return;}

            EditorGUI.indentLevel++;

            List<GOAPActionInstance> plan = _planSnapshot;
            int activeIndex = _planActiveIndexSnapshot;

            if (plan == null || plan.Count == 0)
            {
                EditorGUILayout.LabelField("No active plan.", EditorStyles.miniLabel);
                EditorGUI.indentLevel--;
                return;
            }
            
            EditorGUILayout.BeginHorizontal();

            for (int i = 0; i < plan.Count; i++)
            {
                GOAPActionInstance action = plan[i];

                DrawPlanActionBox(action.Data.ActionName, i, activeIndex, plan.Count);

                if (i < plan.Count - 1)
                {
                    GUILayout.Label("-->", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(16.0f));
                }
            }

            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel--;
        }

        private void DrawPlanActionBox(string actionName, int index, int activeIndex, int planCount)
        {
            Color boxColor;
            string stateLabel;
            GUIStyle labelStyle;

            if (index < activeIndex)
            {
                boxColor = new Color(0.2f, 0.5f, 0.2f, 0.4f);
                stateLabel = "✓";
                labelStyle = Styles.PlanActionSucceeded;
            }
            else if (index == activeIndex)
            {
                boxColor = new Color(0.1f, 0.4f, 0.7f, 0.5f);
                stateLabel = "▶";
                labelStyle = Styles.PlanActionRunning;
            }
            else
            {
                boxColor = new Color(0.3f, 0.3f, 0.3f, 0.3f);
                stateLabel = "○";
                labelStyle = Styles.PlanActionPending;
            }

            Rect rect = EditorGUILayout.BeginVertical(GUILayout.MinWidth(80.0f), GUILayout.Height(40.0f));
            EditorGUI.DrawRect(rect, boxColor);

            GUILayout.FlexibleSpace();
            GUILayout.Label(actionName, labelStyle);
            GUILayout.Label(stateLabel, labelStyle);
            GUILayout.FlexibleSpace();

            EditorGUILayout.EndVertical();
        }

        private void DrawGoalsPanel(GOAPAgent agent)
        {
            _showGoals = EditorGUILayout.Foldout(_showGoals, "Goals", true, Styles.SectionHeader);
            if (!_showGoals) { return; }

            EditorGUI.indentLevel++;

            WorldState snapshot = agent.Blackboard.GetWorldStateSnapshot();

            foreach (GOAPGoal goal in agent.Goals)
            {
                if (goal == null) { continue; }

                int priority = goal.EvaluatePriority(snapshot);
                bool isActive = goal == agent.ActiveGoal;

                EditorGUILayout.BeginHorizontal();

                string prefix = isActive ? "▶ " : "    ";
                float normalisedPriority = Mathf.Clamp01(priority / 100.0f);

                EditorGUILayout.LabelField($"{prefix}{goal.GoalName}", isActive ? Styles.ActiveGoalLabel : EditorStyles.label, GUILayout.Width(180.0f));

                Rect barRect = EditorGUILayout.GetControlRect(GUILayout.Width(80.0f), GUILayout.Height(14.0f));

                EditorGUI.DrawRect(barRect, new Color(0.2f, 0.2f, 0.2f, 0.5f));

                Rect fillRect = new Rect(barRect.x, barRect.y, barRect.width * normalisedPriority, barRect.height);
                Color barColor = isActive ? new Color(0.3f, 0.8f, 0.4f, 0.8f) : new Color(0.4f, 0.6f, 0.8f, 0.6f);

                EditorGUI.DrawRect(fillRect, barColor);

                EditorGUILayout.LabelField(priority.ToString(), EditorStyles.miniLabel, GUILayout.Width(50.0f));

                EditorGUILayout.EndHorizontal();
            }

            EditorGUI.indentLevel--;
        }

        private void DrawActionsPanel(GOAPAgent agent)
        {
            _showActions = EditorGUILayout.Foldout(_showActions, "Actions", true, Styles.SectionHeader);
            if (!_showActions) { return; }

            EditorGUI.indentLevel++;

            foreach (GOAPActionInstance instance in agent.ActionInstances)
            {
                bool proceduralPass = GetProceduralResult(instance);

                EditorGUILayout.BeginHorizontal();

                Color dotColor = proceduralPass ? new Color(0.3f, 0.9f, 0.4f) : new Color(0.8f, 0.3f, 0.3f);

                Rect dotRect = EditorGUILayout.GetControlRect(GUILayout.Width(12.0f), GUILayout.Height(14.0f));

                EditorGUI.DrawRect(new Rect(dotRect.x + 2.0f, dotRect.y + 3.0f, 8.0f, 8.0f), dotColor);

                EditorGUILayout.LabelField(instance.Data.ActionName, proceduralPass ? Styles.ActionAvailable : Styles.ActionUnavailable, GUILayout.Width(180.0f));
                EditorGUILayout.LabelField($"cost: {instance.Cost:F1}", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(60.0f));

                if (!proceduralPass)
                {
                    EditorGUILayout.LabelField("Procedural Preconditions Failed.", EditorStyles.centeredGreyMiniLabel);
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUI.indentLevel--;
        }

        private void DrawBlackboardPanel(GOAPAgent agent)
        {
            _showBlackboard = EditorGUILayout.Foldout(_showBlackboard, "Blackboard", true, Styles.SectionHeader);
            if (!_showBlackboard) { return; }

            _blackboardScroll = EditorGUILayout.BeginScrollView(_blackboardScroll, GUILayout.MaxHeight(150.0f));

            WorldState snapshot = agent.Blackboard.GetWorldStateSnapshot();

            foreach (KeyValuePair<string, object> fact in snapshot.GetFacts())
            {
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField(fact.Key, Styles.BlackboardKey, GUILayout.Width(200.0f));
                EditorGUILayout.LabelField(fact.Value?.ToString() ?? "Null Value", Styles.BlackboardValue);

                EditorGUILayout.EndHorizontal();
            }

            if (!snapshot.GetFacts().Any())
            {
                EditorGUILayout.LabelField("Blackboard is empty.", EditorStyles.centeredGreyMiniLabel);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawControlsPanel(GOAPAgent agent)
        {
            DrawDivider();

            EditorGUILayout.Space(2.0f);
            EditorGUILayout.LabelField("Controls", Styles.SectionHeader);
            EditorGUILayout.Space(2.0f);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Force Re-plan", GUILayout.Height(24.0f)))
            {
                agent.RequestReplan(agent.ActiveGoal);
                Debug.Log($"[GOAPAgentEditor] Forced re-plan triggered on {agent.name}.");
            }

            string pauseLabel = agent.IsPaused ? "Resume" : "Pause";
            if (GUILayout.Button(pauseLabel, GUILayout.Height(24.0f)))
            {
                if (agent.IsPaused) { agent.Resume(); }
                else { agent.Pause(); }
            }
            
            bool canStep = agent.IsPaused && agent.CurrentPlan != null && agent.CurrentActionIndex < agent.CurrentPlan.Count;
            
            GUI.enabled = canStep;
            if (GUILayout.Button("Step", GUILayout.Height(24.0f)))
                agent.Step();
            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();
            
            if (agent.IsPaused)
            {
                EditorGUILayout.Space(2.0f);
                EditorGUILayout.HelpBox("Agent is paused. Step advances to the next action. Inspect the Blackboard between steps.", MessageType.Info);
            }
        }

        // ───── Helper methods ────────────────────────────────────────────────
        
        private static void DrawDivider()
        {
            Rect rect = EditorGUILayout.GetControlRect(false, 1.0f, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.3f));
        }

        
        private void RefreshProceduralSnapshot(GOAPAgent agent)
        {
            if (Time.realtimeSinceStartup - _proceduralCacheTime <= PROCEDURAL_CACHE_REFRESH_INTERVAL) { return; }
            
            _proceduralSnapshot.Clear();
            
            foreach (GOAPActionInstance a in agent.ActionInstances)
                _proceduralSnapshot[a.Data.ActionName] = a.CheckProceduralPreconditions();
            
            _proceduralCacheTime = Time.realtimeSinceStartup;
        }
        
        /// <summary>
        /// Checks procedural preconditions of the agent's action instances.
        /// Used purely for optimisation so the results can be cached and the check can be throttled.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        private bool GetProceduralResult(GOAPActionInstance instance) => _proceduralSnapshot.TryGetValue(instance.Data.ActionName, out bool result) && result;
    }
}
#endif