using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace GOAP.Editor
{
    /// <summary>
    /// Floating Editor window that provides live diagnostic data about any active GOAP
    /// agent in the scene.
    /// </summary>
    public sealed class GOAPDebuggerWindow : EditorWindow
    {
        // -----Window methods-----
        [MenuItem("GOAP/GOAP Debugger")]
        public static void OpenWindow()
        {
            GOAPDebuggerWindow window = GetWindow<GOAPDebuggerWindow>();
            window.titleContent = new GUIContent("GOAP Debugger", EditorGUIUtility.IconContent("d_UnityEditor.ProfilerWindow").image);
            window.minSize = new Vector2(420.0f, 300.0f);
            window.Show();
        }

        // -----Styles-----
        private static class Styles
        {
            
            public static readonly GUIStyle WindowBackground;
            public static readonly GUIStyle SectionHeader;
            public static readonly GUIStyle StatusBar;
            public static readonly GUIStyle StatusLabel;
            public static readonly GUIStyle PlanBox;
            public static readonly GUIStyle PlanBoxActive;
            public static readonly GUIStyle PlanBoxComplete;
            public static readonly GUIStyle PlanBoxPending;
            public static readonly GUIStyle PlanArrow;
            public static readonly GUIStyle BlackboardKeyStyle;
            public static readonly GUIStyle BlackboardValueStyle;
            public static readonly GUIStyle BlackboardChangedStyle;
            public static readonly GUIStyle HistorySuccess;
            public static readonly GUIStyle HistoryFailure;
            public static readonly GUIStyle HistoryTimestamp;
            public static readonly GUIStyle NoAgentMessage;
            public static readonly GUIStyle TabButton;
            public static readonly GUIStyle TabButtonActive;

            static Styles()
            {
                bool pro = EditorGUIUtility.isProSkin;

                WindowBackground  = new GUIStyle
                {
                    normal = { background = MakeTex(1, 1, pro ? new Color(0.18f, 0.18f, 0.18f) : new Color(0.88f, 0.88f, 0.88f)) }
                };

                SectionHeader = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize  = 15,
                    alignment = TextAnchor.MiddleLeft,
                    padding   = new RectOffset(6, 0, 4, 4)
                };
                SectionHeader.normal.textColor = pro
                    ? new Color(0.6f, 0.85f, 1f)
                    : new Color(0.1f, 0.3f, 0.6f);

                StatusBar = new GUIStyle
                {
                    normal = { background = MakeTex(1, 1,
                        pro ? new Color(0.6f, 0.85f, 1f, 0.075f)
                            : new Color(0.1f, 0.3f, 0.6f, 0.075f)) },
                    padding = new RectOffset(8, 8, 6, 6)
                };

                StatusLabel = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize  = 12,
                    alignment = TextAnchor.MiddleCenter,
                    richText  = true
                };

                PlanBox = new GUIStyle(EditorStyles.helpBox)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize  = 10,
                    richText  = true,
                    padding   = new RectOffset(6, 6, 6, 6)
                };
                PlanBox.normal.textColor = pro
                    ? new Color(0.75f, 0.75f, 0.75f)
                    : new Color(0.3f, 0.3f, 0.3f);

                PlanBoxActive = new GUIStyle(PlanBox);
                PlanBoxActive.normal.background = MakeTex(1, 1,
                    new Color(0.1f, 0.35f, 0.65f, 0.7f));
                PlanBoxActive.normal.textColor = Color.white;

                PlanBoxComplete = new GUIStyle(PlanBox);
                PlanBoxComplete.normal.background = MakeTex(1, 1,
                    new Color(0.1f, 0.45f, 0.15f, 0.5f));
                PlanBoxComplete.normal.textColor =
                    new Color(0.6f, 1f, 0.6f);

                PlanBoxPending = new GUIStyle(PlanBox);
                PlanBoxPending.normal.background = MakeTex(1, 1,
                    new Color(0.25f, 0.25f, 0.25f, 0.5f));

                PlanArrow = new GUIStyle(EditorStyles.centeredGreyMiniLabel)
                {
                    fontSize  = 14,
                    alignment = TextAnchor.MiddleCenter
                };

                BlackboardKeyStyle = new GUIStyle(EditorStyles.miniLabel)
                {
                    fontStyle = FontStyle.Bold,
                    richText  = true
                };

                BlackboardValueStyle = new GUIStyle(EditorStyles.miniLabel)
                {
                    alignment = TextAnchor.MiddleRight,
                    richText  = true
                };

                BlackboardChangedStyle = new GUIStyle(EditorStyles.miniLabel)
                {
                    alignment  = TextAnchor.MiddleRight,
                    fontStyle  = FontStyle.Bold,
                    richText   = true
                };
                BlackboardChangedStyle.normal.textColor =
                    new Color(1f, 0.85f, 0.2f);

                HistorySuccess = new GUIStyle(EditorStyles.miniLabel)
                {
                    richText = true
                };
                HistorySuccess.normal.textColor = pro
                    ? new Color(0.5f, 0.9f, 0.5f)
                    : new Color(0.1f, 0.5f, 0.1f);

                HistoryFailure = new GUIStyle(EditorStyles.miniLabel)
                {
                    richText = true
                };
                HistoryFailure.normal.textColor = pro
                    ? new Color(0.9f, 0.4f, 0.4f)
                    : new Color(0.6f, 0.1f, 0.1f);

                HistoryTimestamp = new GUIStyle(EditorStyles.miniLabel)
                {
                    alignment = TextAnchor.MiddleRight
                };
                HistoryTimestamp.normal.textColor =
                    new Color(0.5f, 0.5f, 0.5f);

                NoAgentMessage = new GUIStyle(EditorStyles.centeredGreyMiniLabel)
                {
                    fontSize  = 13,
                    alignment = TextAnchor.MiddleCenter
                };

                TabButton = new GUIStyle(EditorStyles.miniButton)
                {
                    fontSize = 10,
                    padding  = new RectOffset(8, 8, 4, 4)
                };

                TabButtonActive = new GUIStyle(TabButton);
                TabButtonActive.normal.background = MakeTex(1, 1,
                    new Color(0.2f, 0.5f, 0.8f, 0.5f));
                TabButtonActive.normal.textColor = Color.white;
            }

            /// <summary>
            /// Used to style a background color as they're rendered as Texture2D type, not color.
            /// </summary>
            /// <param name="width"></param>
            /// <param name="height"></param>
            /// <param name="color"></param>
            /// <returns></returns>
            private static Texture2D MakeTex(int width, int height, Color color)
            {
                Color[] pixels = new Color[width * height];
                for (int i = 0; i < pixels.Length; i++)
                    pixels[i] = color;

                Texture2D tex = new Texture2D(width, height);
                tex.SetPixels(pixels);
                tex.Apply();
                return tex;
            }
        }
    
        // -----Private properties-----
        private GOAPAgent _selectedAgent;
        private int _activeTab = 0;

        private Vector2 _planScroll;
        private Vector2 _blackboardScroll;
        private Vector2 _historyScroll;

        private WorldState _cachedSnapshot;
        private WorldState _previousBlackboardSnapshot;

        private GOAPAgent[] _cachedAgents;

        private float _lastRepaintTime;
        private float _agentCacheTime;

        private readonly Dictionary<string, float> _recentlyChangedKeys = new();

        // -----Constants-----
        private const float CHANGED_KEY_HIGHLIGHT_DURATION = 1.5f;
        private const float AGENT_CACHE_REFRESH_INTERVAL = 1.0f;
        private const float REPAINT_INTERVAL = 0.067f; // ~15Hz

        // -----Lifecycle methods-----
        private void OnEnable()
        {
            EditorApplication.update += OnEditorUpdate;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

            if (_selectedAgent == null)
                TryAutoSelectAgent();
        }

        private void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        private void OnEditorUpdate()
        {
            if (!EditorApplication.isPlaying) { return; }

            if (_selectedAgent != null)
                TrackBlackboardChanges();

            // Throttle repaint to save on performance
            if (Time.realtimeSinceStartup - _lastRepaintTime > REPAINT_INTERVAL)
            {
                _lastRepaintTime = Time.realtimeSinceStartup;
                Repaint();
            }
        }

        /// <summary>
        /// Additional safety check to ensure agent auto-selection runs when entering play mode
        /// </summary>
        /// <param name="state"></param>
        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                _selectedAgent = null;
                _previousBlackboardSnapshot = null;
                _cachedAgents = null;
                _cachedSnapshot = null;
                _recentlyChangedKeys.Clear();
                TryAutoSelectAgent();
            }

            // Clean up references when leaving play mode
            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                _selectedAgent = null;
                _previousBlackboardSnapshot = null;
                _cachedAgents = null;
                _cachedSnapshot = null;
                _recentlyChangedKeys.Clear();
            }
        }

        // -----GUI-----
        private void OnGUI()
        {
            if (!EditorApplication.isPlaying)
            {
                DrawNotPlayingMessage();
                return;
            }

            DrawAgentSelector();
            DrawDivider();

            if (_selectedAgent == null)
            {
                DrawNoAgentMessage();
                return;
            }

            DrawStatusBar();
            DrawDivider();
            DrawTabBar();
            DrawDivider();
            DrawActiveTab();
        }

        // -----Private methods-----
        private void DrawAgentSelector()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            EditorGUILayout.LabelField("Agent", GUILayout.Width(50.0f));

            GOAPAgent[] agents = GetAgents();

            if (agents.Length == 0)
            {
                EditorGUILayout.LabelField("No GOAP agents in scene.", EditorStyles.miniLabel);
                EditorGUILayout.EndHorizontal();
                return;
            }

            // Build display names
            string[] agentNames = new string[agents.Length];
            int selectedIndex = 0;

            for (int i = 0; i < agents.Length; i++)
            {
                agentNames[i] = agents[i].name;
                if (agents[i] == _selectedAgent)
                    selectedIndex = i;
            }

            int newIndex = EditorGUILayout.Popup(selectedIndex, agentNames, EditorStyles.toolbarPopup);
            if (_selectedAgent != agents[newIndex])
            {
                _selectedAgent = agents[newIndex];
                _previousBlackboardSnapshot = null;
                _cachedSnapshot = null;
                _recentlyChangedKeys.Clear();
            }

            // Ping button to select agent in hierarchy
            if (GUILayout.Button("Ping", EditorStyles.toolbarButton, GUILayout.Width(36.0f)))
            {
                Selection.activeGameObject = _selectedAgent.gameObject;
                EditorGUIUtility.PingObject(_selectedAgent.gameObject);
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawStatusBar()
        {
            EditorGUILayout.BeginHorizontal(Styles.StatusBar);

            string goalName = _selectedAgent.ActiveGoal?.GoalName ?? "<color=#888888>No Goal</color>";
            string actionName = 
            (
                _selectedAgent.CurrentPlan != null 
                && 
                _selectedAgent.CurrentActionIndex < _selectedAgent.CurrentPlan.Count 
                ? 
                _selectedAgent.CurrentPlan[_selectedAgent.CurrentActionIndex].Data.ActionName
                :
                "<color=#888888>No Action</color>"
            );

            int planLength = _selectedAgent.CurrentPlan?.Count ?? 0;

            GUILayout.Label($"<b>Goal:</b> {goalName}", Styles.StatusLabel, GUILayout.ExpandWidth(true));

            DrawVerticalDivider();

            GUILayout.Label($"<b>Action:</b> {actionName}", Styles.StatusLabel, GUILayout.ExpandWidth(true));

            DrawVerticalDivider();

            GUILayout.Label($"<b>Plan:</b> {planLength} steps", Styles.StatusLabel, GUILayout.ExpandWidth(true));

            EditorGUILayout.EndHorizontal();
        }

        private void DrawTabBar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.FlexibleSpace();

            DrawTab("Plan", 0);
            DrawTab("Blackboard", 1);
            DrawTab("History", 2);

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawTab(string label, int index)
        {
            bool isActive = _activeTab == index;

            if (GUILayout.Toggle(isActive, label, EditorStyles.toolbarButton, GUILayout.Width(150.0f)) && !isActive)
            {
                _activeTab = index;
            }
        }

        private void DrawActiveTab()
        {
            switch (_activeTab)
            {
                case 0: DrawPlanTab(); break;
                case 1: DrawBlackboardTab(); break;
                case 2: DrawHistoryTab(); break;
            }
        }

        private void DrawPlanTab()
        {
            EditorGUILayout.Space(4.0f);
            EditorGUILayout.LabelField("Current Plan", new GUIStyle(Styles.SectionHeader) { fontSize = 15 });

            List<GOAPActionInstance> plan = _selectedAgent.CurrentPlan;
            int activeIndex = _selectedAgent.CurrentActionIndex;

            if (plan == null || plan.Count == 0)
            {
                EditorGUILayout.Space(15.0f);
                EditorGUILayout.LabelField("No active plan.", Styles.NoAgentMessage, GUILayout.ExpandWidth(true));
                EditorGUILayout.Space(15.0f);
                return;
            }

            EditorGUILayout.Space(4.0f);

            _planScroll = EditorGUILayout.BeginScrollView(_planScroll, GUILayout.Height(90.0f));

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            for (int i = 0; i < plan.Count; i++)
            {
                GOAPActionInstance action = plan[i];

                GUIStyle boxStyle;
                string statusIcon;

                if (i < activeIndex)
                {
                    boxStyle = Styles.PlanBoxComplete;
                    statusIcon = "✓";
                }
                else if (i == activeIndex)
                {
                    boxStyle = Styles.PlanBoxActive;
                    statusIcon = "▶";
                }
                else
                {
                    boxStyle = Styles.PlanBoxPending;
                    statusIcon = "○";
                }

                EditorGUILayout.BeginVertical(boxStyle, GUILayout.Width(100.0f), GUILayout.Height(60.0f));
                GUILayout.FlexibleSpace();

                EditorGUILayout.LabelField(action.Data.ActionName, new GUIStyle(boxStyle) { fontStyle = FontStyle.Bold }, GUILayout.Width(88.0f));
                EditorGUILayout.LabelField($"{statusIcon}  cost: {action.Cost:F1}", boxStyle, GUILayout.Width(88.0f));

                GUILayout.FlexibleSpace();
                EditorGUILayout.EndVertical();

                if (i < plan.Count - 1)
                {
                    GUILayout.Label("-->", Styles.PlanArrow, GUILayout.Width(20.0f));
                }
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndScrollView();

            // Total plan cost calculation
            float totalCost = 0.0f;
            foreach (GOAPActionInstance a in plan)
                totalCost += a.Cost;
            
            EditorGUILayout.Space(2.0f);
            EditorGUILayout.LabelField($"Total Plan Cost: {totalCost:F1}", EditorStyles.centeredGreyMiniLabel);
        }

        private void DrawBlackboardTab()
        {
            EditorGUILayout.Space(4.0f);
            EditorGUILayout.LabelField("Blackboard", Styles.SectionHeader);
            EditorGUILayout.Space(4.0f);

            WorldState snapshot = _cachedSnapshot;

            if (snapshot == null)
            {
                EditorGUILayout.LabelField("Waiting for WorldState data...", EditorStyles.centeredGreyMiniLabel);
            }

            bool hasAnyFacts = false;

            _blackboardScroll = EditorGUILayout.BeginScrollView(_blackboardScroll);

            // For drawing row backgrounds for readability
            int rowIndex = 0;

            foreach (KeyValuePair<string, object> fact in snapshot.GetFacts())
            {
                hasAnyFacts = true;

                bool recentlyChanged = _recentlyChangedKeys.TryGetValue(fact.Key, out float changedAt) && (Time.realtimeSinceStartup - changedAt) < CHANGED_KEY_HIGHLIGHT_DURATION;

                Rect rowRect = EditorGUILayout.BeginHorizontal(GUILayout.Height(18.0f));

                Color rowColor = rowIndex % 2 == 0 ? new Color(0f, 0f, 0f, 0.05f) : new Color(0f, 0f, 0f, 0f);

                if (recentlyChanged)
                    rowColor = new Color(0.8f, 0.7f, 0.0f, 0.12f);
                
                EditorGUI.DrawRect(rowRect, rowColor);

                string changedIndicator = recentlyChanged ? "●" : " ";
                EditorGUILayout.LabelField(changedIndicator, recentlyChanged ? Styles.BlackboardChangedStyle : EditorStyles.miniLabel, GUILayout.Width(14.0f));
            
                // Key + Value
                EditorGUILayout.LabelField(fact.Key, Styles.BlackboardKeyStyle, GUILayout.Width(position.width * 0.55f));
                EditorGUILayout.LabelField(FormatBlackboardValue(fact.Value), recentlyChanged ? Styles.BlackboardChangedStyle : Styles.BlackboardValueStyle);
            
                EditorGUILayout.EndHorizontal();
                rowIndex++;
            }

            if (!hasAnyFacts)
            {
                EditorGUILayout.Space(12.0f);
                EditorGUILayout.LabelField("Blackboard is empty.", Styles.NoAgentMessage, GUILayout.ExpandWidth(true));
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawHistoryTab()
        {
            EditorGUILayout.Space(4.0f);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Planning History", Styles.SectionHeader, GUILayout.Height(20.0f));
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Clear", EditorStyles.miniButton, GUILayout.Width(44.0f)))
                _selectedAgent.ClearPlanHistory();

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(10.0f);

            IReadOnlyList<PlanHistoryEntry> history = _selectedAgent.PlanHistory;

            if (history.Count == 0)
            {
                EditorGUILayout.Space(12f);
                EditorGUILayout.LabelField("No planning entries recorded yet.", Styles.NoAgentMessage, GUILayout.ExpandWidth(true));
                return;
            }

            _historyScroll = EditorGUILayout.BeginScrollView(_historyScroll);

            // Draws most recent entries first
            for (int i = history.Count - 1; i >= 0; i--)
            {
                PlanHistoryEntry entry = history[i];
                DrawHistoryEntry(entry, i == history.Count - 1);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawHistoryEntry(PlanHistoryEntry entry, bool isMostRecent)
        {
            Rect entryRect = EditorGUILayout.BeginVertical(GUILayout.MinHeight(42.0f));

            Color entryColor = isMostRecent ? new Color(0.2f, 0.4f, 0.6f, 0.15f) : new Color(0.0f, 0.0f, 0.0f, 0.05f);

            EditorGUI.DrawRect(entryRect, entryColor);

            // Row 1
            // Contains timestamp/goal/result
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(entry.PlanFound ? "✓" : "✗", entry.PlanFound ? Styles.HistorySuccess : Styles.HistoryFailure, GUILayout.Width(16.0f));

            EditorGUILayout.LabelField(entry.GoalName, entry.PlanFound ? Styles.HistorySuccess : Styles.HistoryFailure, GUILayout.Width(130.0f));

            EditorGUILayout.LabelField($"{entry.NodesExpanded} nodes", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(60.0f));

            GUILayout.FlexibleSpace();

            EditorGUILayout.LabelField($"{entry.Timestamp:F1}s", Styles.HistoryTimestamp, GUILayout.Width(48.0f));

            EditorGUILayout.EndHorizontal();

            // Row 2
            // Contains trigger reason
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(18.0f);

            EditorGUILayout.LabelField($"↳ Reason: {entry.TriggerReason}", EditorStyles.miniLabel, GUILayout.Width(180.0f));

            EditorGUILayout.EndHorizontal();

            // Row 3
            // Contains action chain
            if (entry.PlanFound && entry.PlanActionNames.Count > 0)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(18.0f);

                string planChain = string.Join(" --> ", entry.PlanActionNames);

                EditorGUILayout.LabelField(planChain, EditorStyles.miniLabel);

                EditorGUILayout.EndHorizontal();
            }
            else if (!entry.PlanFound)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(18.0f);

                EditorGUILayout.LabelField("No valid plan found.", Styles.HistoryFailure);

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
            DrawDivider();
        }

        private void TrackBlackboardChanges()
        {
            if (_selectedAgent == null) { return; }

            _cachedSnapshot = _selectedAgent.Blackboard.GetWorldStateSnapshot();

            if (_previousBlackboardSnapshot == null)
            {
                _previousBlackboardSnapshot = _cachedSnapshot;
                return;
            }

            // Checks for new/changed facts
            foreach (KeyValuePair<string, object> fact in _cachedSnapshot.GetFacts())
            {
                bool existedBefore = _previousBlackboardSnapshot.TryGetBoxed(fact.Key, out object previousValue);
                bool changed = !existedBefore || !Equals(previousValue, fact.Value);

                if (changed)
                    _recentlyChangedKeys[fact.Key] = Time.realtimeSinceStartup;
            }

            // Clean up expired highlights (tracked changes)
            var expiredKeys = new List<string>();

            foreach (KeyValuePair<string, float> entry in _recentlyChangedKeys)
            {
                if (Time.realtimeSinceStartup - entry.Value > CHANGED_KEY_HIGHLIGHT_DURATION)
                    expiredKeys.Add(entry.Key);
            }

            foreach (string key in expiredKeys)
                _recentlyChangedKeys.Remove(key);
            
            _previousBlackboardSnapshot = _cachedSnapshot;
        }

        // -----Helpers methods-----
        private GOAPAgent[] GetAgents()
        {
            if (_cachedAgents == null || Time.realtimeSinceStartup - _agentCacheTime > AGENT_CACHE_REFRESH_INTERVAL)
            {
                _cachedAgents = FindObjectsByType<GOAPAgent>(FindObjectsSortMode.None);
                _agentCacheTime = Time.realtimeSinceStartup;
            }

            return _cachedAgents;
        }

        private void DrawNotPlayingMessage()
        {
            EditorGUILayout.BeginVertical();
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("Enter Play Mode to use the GOAP Debugger.", Styles.NoAgentMessage, GUILayout.ExpandWidth(true));
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndVertical();
        }

        private void DrawNoAgentMessage()
        {
            EditorGUILayout.BeginVertical();
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("No GOAP Agent found in scene.", Styles.NoAgentMessage, GUILayout.ExpandWidth(true));
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndVertical();
        }

        private void TryAutoSelectAgent()
        {
            GOAPAgent[] agents = GetAgents();
            if (agents.Length > 0)
                _selectedAgent = agents[0];
        }

        private static void DrawDivider()
        {
            Rect rect = EditorGUILayout.GetControlRect(false, 1.0f, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.25f));
        }

        private static void DrawVerticalDivider()
        {
            Rect rect = EditorGUILayout.GetControlRect(false, GUILayout.Width(1.0f));
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.3f));
        }

        /// <summary>
        /// Safely formats a blackboard value for display. Prevents Runtime errors due to null object
        /// references.
        /// </summary>
        private static string FormatBlackboardValue(object value)
        {
            if (value == null) return "null";

            if (value is UnityEngine.Object unityObj)
                return unityObj != null ? $"{unityObj.name} ({value.GetType().Name})" : "null (destroyed)";

            if (value is Vector3 v) return $"({v.x:F2}, {v.y:F2}, {v.z:F2})";

            if (value is Quaternion q)
            {
                Vector3 euler = q.eulerAngles;
                return $"euler({euler.x:F1}, {euler.y:F1}, {euler.z:F1})";
            }

            return value.ToString() ?? "null";
        }
    }
}

