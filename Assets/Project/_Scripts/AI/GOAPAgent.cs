using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace GOAP
{
    /// <summary>
    /// MonoBehaviour that drives all GOAP agents and their behaviour.
    /// 
    /// Authorised Use Instructions:
    ///     - Must create runtime action instances during initialisation from action data
    ///       (prevents coupled action data and logic)
    ///     - Must constantly evaluate goal priorities for intelligent behaviour
    ///     - Must drive/call action execution loop (OnStart, Perform, OnEnd, OnReset)
    ///     - Must monitor for changes to the WorldState that could trigger a re-plan and
    ///       invalidate the current plan + call for a new one if necessary
    ///     - Must decouple planning rate from Unity player loop (xHz < every update call) 
    ///       to prevent large performance loss
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public sealed class GOAPAgent : MonoBehaviour
    {
        // ───── Serialized properties ────────────────────────────────────────────────
        
        [SerializeField] private List<GOAPActionData> _actionDataAssets = new();
        [SerializeField] private List<GOAPGoal> _goals = new();
        [SerializeField] private WaypointPatrolPath _waypointNetwork;
        [SerializeField] [Range(1.0f, 30.0f)] float _planningTickRate = 3.0f;
        [SerializeField] private bool _debugLogging = true;

        [Space(10.0f)]

        [Header("Gizmos")]
        [SerializeField] private bool _drawVisualiserGizmos = true;
        [SerializeField] private bool _drawPlaningStatusInWorldSpaceGizmos = true;
        [SerializeField] private bool _drawNavigationGizmos = true;

        // ───── Private properties ────────────────────────────────────────────────
        
        private List<GOAPActionInstance> _actionInstances;
        private GOAPPlanner _planner;
        private List<GOAPActionInstance> _currentPlan;
        private GOAPGoal _activeGoal;
        private readonly List<PlanHistoryEntry> _planHistory = new();

        private int _currentActionIndex;
        private float _planningTickTimer;

        // ───── Public properties ────────────────────────────────────────────────
        
        public NavMeshAgent NavAgent { get; private set; }
        public Animator Animator { get; private set; }
        public AgentBlackboard Blackboard { get; private set; } = new AgentBlackboard();
        public WaypointPatrolPath WaypointNetwork => _waypointNetwork;
        public IReadOnlyList<PlanHistoryEntry> PlanHistory => _planHistory;

        // ───── Constants ────────────────────────────────────────────────
        
        private const int MAX_HISTORY_ENTRIES  = 50;

        // ───── Editor properties ────────────────────────────────────────────────
        
        public GOAPGoal ActiveGoal => _activeGoal;
        
        public List<GOAPActionInstance> CurrentPlan => _currentPlan;
        
        public IReadOnlyList<GOAPGoal> Goals => _goals;
        public IReadOnlyList<GOAPActionInstance> ActionInstances => _actionInstances;
        
        public int CurrentActionIndex => _currentActionIndex;
        
        public bool IsPaused { get; private set; }

        // ───── Lifecycle methods ────────────────────────────────────────────────
        
        private void Awake()
        {
            NavAgent = GetComponent<NavMeshAgent>();
            Animator = GetComponentInChildren<Animator>();
            Blackboard = new AgentBlackboard();
            _planner = new GOAPPlanner();

            InitializeActionInstances();
            RegisterReplanCallbacks();
        }

        private void Update()
        {
            TickActionExecution();
            TickPlanning();
        }

        // ───── Public methods ────────────────────────────────────────────────
        
        /// <summary>
        /// Aborts the current plan, requests a fresh plan from the GOAP planner with the given goal and 
        /// runs the plan if one is returned.
        /// </summary>
        /// <param name="goal"></param>
        /// <param name="triggerReason"></param>
        public void RequestReplan(GOAPGoal goal, string triggerReason = "Manual")
        {
            if (goal == null) { return; }

            AbortCurrentPlan();

            WorldState currentState = Blackboard.WorldState;

            List<GOAPActionInstance> newPlan = _planner.Plan(currentState, goal.GetDesiredState(), _actionInstances, out int nodesExpanded);

            bool planFound = newPlan != null && newPlan.Count > 0;
            RecordPlanHistory(goal, triggerReason, newPlan, nodesExpanded, planFound);

            if (!planFound)
            {
                if (_debugLogging)
                    Debug.Log($"[GOAPAgent] '{name}' could not find a plan for goal: '{goal.GoalName}'. Retrying on next tick.", this);

                return;
            }

            _currentPlan = newPlan;
            _currentActionIndex = 0;
            _activeGoal = goal;

            if (_debugLogging)
            {
                string planString = string.Join
                (
                    "-->",
                    _currentPlan.ConvertAll(a => a.Data.ActionName)
                );

                Debug.Log($"[GOAPAgent] '{name}' new plan for '{goal.GoalName}': [{planString}]", this);
            }

            GOAPActionInstance firstAction = _currentPlan[_currentActionIndex];

            if (!firstAction.CheckProceduralPreconditions())
            {
                // First action already invalid — discard plan and wait for next tick
                if (_debugLogging)
                    Debug.Log($"[GOAPAgent] '{name}' first action " +
                            $"'{firstAction.Data.name}' failed procedural " +
                            $"preconditions. Discarding plan.");

                _currentPlan          = null;
                _currentActionIndex   = 0;
                return;
            }


            // Start first action in new plan immediately
            firstAction.OnStart();
        }

        // ───── Private methods ────────────────────────────────────────────────

        /// <summary>
        /// Builds a GOAPActionInstance for every GOAPActionData the agent has.
        /// Each will be used for whole agent lifecycle.
        /// </summary>
        private void InitializeActionInstances()
        {
            _actionInstances = new List<GOAPActionInstance>(_actionDataAssets.Count);

            foreach (GOAPActionData data in _actionDataAssets)
            {
                if (data == null)
                {
                    Debug.LogWarning($"[GOAPAgent] Action data null on {name}. Skipping this GOAPActionData.");
                    continue;
                }

                _actionInstances.Add(data.CreateInstance(this));
            }
        }

        private void RegisterReplanCallbacks()
        {
            Blackboard.RegisterChangeCallback(BlackboardKeys.TARGET_VISIBLE, _ => OnSignificantFactChanged());
            Blackboard.RegisterChangeCallback(BlackboardKeys.TARGET_IN_MELEE_RANGE, _ => OnSignificantFactChanged());
        }

        /// <summary>
        /// Runs every frame. Handles the current action's execution loop and transitions between actions in the plan.
        /// </summary>
        private void TickActionExecution()
        {
            if (IsPaused) { return; }
            
            if (_currentPlan == null || _currentActionIndex >= _currentPlan.Count) { return; }

            GOAPActionInstance activeAction = _currentPlan[_currentActionIndex];
            ActionStatus status = activeAction.Perform();

            switch(status)
            {
                case ActionStatus.Running:
                // no-op
                // Keep executing action next frame
                break;

                case ActionStatus.Succeeded:
                OnActionSucceeded(activeAction);
                break;

                case ActionStatus.Failed:
                OnActionFailed(activeAction);
                break;
            }
        }

        /// <summary>
        /// Runs every specified planning tick (1-10Hz usually).
        /// Constantly calculates the most important goal and triggers a re-plan if the best
        /// goal is not the current goal OR if there is no current plan.
        /// </summary>
        private void TickPlanning()
        {
            if (IsPaused) { return; }
            
            // Re-plan optimisation
            _planningTickTimer -= Time.deltaTime;
            if (_planningTickTimer > 0.0f) { return; }
            _planningTickTimer = 1.0f / _planningTickRate;

            GOAPGoal bestGoal = SelectBestGoal();
            if (bestGoal == null) { return; }

            bool needsReplan = _currentPlan == null || bestGoal != _activeGoal;
            if (!needsReplan) { return; }

            RequestReplan(bestGoal, "Goal changed");
        }

        /// <summary>
        /// Evaluates all agent goals and returns the one with the highest priority given the WorldState
        /// </summary>
        /// <returns></returns>
        private GOAPGoal SelectBestGoal()
        {
            GOAPGoal bestGoal = null;
            int bestPriority = int.MinValue;

            WorldState currentState = Blackboard.WorldState;

            foreach (GOAPGoal goal in _goals)
            {
                if (goal == null) { continue; }

                int priority = goal.EvaluatePriority(currentState);

                if (priority > bestPriority)
                {
                    bestPriority = priority;
                    bestGoal = goal;
                }
            }

            return bestGoal;
        }
        
        private void RecordPlanHistory(GOAPGoal goal, string triggerReason, List<GOAPActionInstance> plan, int nodesExpanded, bool planFound)
        {
            var actionNames = planFound ? plan.ConvertAll(a => a.Data.ActionName) : new List<string>();

            var entry = new PlanHistoryEntry(Time.time, goal.GoalName, triggerReason, actionNames, nodesExpanded, planFound);

            _planHistory.Add(entry);

            if (_planHistory.Count > MAX_HISTORY_ENTRIES)
                _planHistory.RemoveAt(0);
        }

        /// <summary>
        /// Terminates the current plan by ending + resetting the current action, then clearing all planning state.
        /// </summary>
        private void AbortCurrentPlan()
        {
            if (_currentPlan == null) { return; }

            if (_currentActionIndex < _currentPlan.Count)
            {
                GOAPActionInstance activeAction = _currentPlan[_currentActionIndex];
                activeAction.OnEnd();
                activeAction.OnReset();
            }

            _currentPlan = null;
            _currentActionIndex = 0;
        }

        /// <summary>
        /// Begins next action in plan OR clears planning status if last action completed.
        /// </summary>
        /// <param name="action"></param>
        private void OnActionSucceeded(GOAPActionInstance action)
        {
            action.OnEnd();
            action.OnReset();

            _currentActionIndex++;

            // Plan is complete
            if (_currentActionIndex >= _currentPlan.Count)
            {
                if (_debugLogging)
                    Debug.Log($"[GOAPAgent] '{name}' completed plan for '{_activeGoal.GoalName}'.", this);

                _currentPlan = null;
                _activeGoal = null;
                return;
            }

            GOAPActionInstance nextAction = _currentPlan[_currentActionIndex];

            if (!nextAction.CheckProceduralPreconditions())
            {
                if (_debugLogging)
                    Debug.Log($"[GOAPAgent] '{name}' next action " +
                            $"'{nextAction.Data.name}' failed procedural " +
                            $"preconditions. Replanning.");

                RequestReplan(_activeGoal, "Procedural preconditions failed");
                return;
            }

            nextAction.OnStart();
        }

        /// <summary>
        /// Aborts the current plan and forces a re-plan so the AI agent can try again.
        /// </summary>
        /// <param name="action"></param>
        private void OnActionFailed(GOAPActionInstance action)
        {
            if (_debugLogging)
                Debug.Log($"[GOAPAgent] Action: '{action.Data.ActionName}' failed on '{name}'. Replanning.", this);

            // Re-plan IMMEDIATELY with no delay to prevent delayed behaviour
            GOAPGoal failedGoal = _activeGoal;
            AbortCurrentPlan();
            RequestReplan(failedGoal, $"Action failed: {action.Data.ActionName}");
        }

        /// <summary>
        /// Called when a significant fact on the blackboard has changed, rendering the current plan stale.
        /// Requests a replan of the goal.
        /// </summary>
        private void OnSignificantFactChanged()
        {
            if (_activeGoal == null) { return; }

            GOAPGoal currentGoal = _activeGoal;
            AbortCurrentPlan();
            RequestReplan(currentGoal, "Significant fact changed");
        }

        // ───── Helper methods ────────────────────────────────────────────────
        
        #if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying) { return; }
            if (!_drawVisualiserGizmos) { return; }

            if (_drawPlaningStatusInWorldSpaceGizmos)
            {    
                string goalName = _activeGoal?.GoalName ?? "No Goal";
                string actionName = (_currentPlan != null && _currentActionIndex < _currentPlan.Count) ? _currentPlan[_currentActionIndex].Data.ActionName : "No Action";

                UnityEditor.Handles.Label(
                    transform.position + Vector3.up * 2.2f, 
                    $"{gameObject.name}\n{goalName}\n{actionName}",
                    new GUIStyle
                    {
                        normal = { textColor = Color.white },
                        fontSize = 10,
                        alignment = TextAnchor.MiddleCenter,
                        richText = true,
                    });
            }

            if (_currentPlan != null && NavAgent.hasPath && _drawNavigationGizmos)
            {
                Gizmos.color = new Color(0.4f, 0.8f, 1.0f, 0.6f);
                Gizmos.DrawLine(transform.position, NavAgent.destination);
                Gizmos.DrawWireSphere(NavAgent.destination, 0.3f);
            }
        }
        
        /// <summary>
        /// Used by GOAP debugger to clear history at runtime
        /// </summary>
        public void ClearPlanHistory() => _planHistory.Clear();
        
        public void Pause()
        {
            IsPaused = true;
            NavAgent.isStopped = true;
        }
        
        public void Resume()
        {
            IsPaused = false;
            NavAgent.isStopped = false;
        }
        
        public void Step()
        {
            if (IsPaused) { return; }
            if (_currentPlan == null || _currentActionIndex >= _currentPlan.Count) {  return; }
            
            GOAPActionInstance current = _currentPlan[_currentActionIndex];
            current.OnEnd();
            current.OnReset();
            
            _currentActionIndex++;
            
            if (_currentActionIndex >= _currentPlan.Count)
            {
                if (_debugLogging)
                    Debug.Log($"[GOAPAgent] '{name}' stepped through last action. Plan complete.", this);
                
                _currentPlan = null;
                _activeGoal = null;
                return;
            }
            
            GOAPActionInstance next = _currentPlan[_currentActionIndex];
            next.OnStart();
        }
        #endif
    }
}

