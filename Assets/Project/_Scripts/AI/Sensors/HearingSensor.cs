using System.Collections.Generic;
using UnityEngine;

namespace GOAP
{
    /// <summary>
    /// Responsible for the agent's hearing capabilities.
    ///
    /// Authorised Use Instructions:
    ///     - The AI may hear many sounds while performing other duties so the Sense()
    ///       method MUST guard against hearing sounds while in combat or performing a duty
    ///       it should not be distracted from. It MUST ignore sounds while busy with important
    ///       tasks to prevent logical drift.
    /// </summary>
    public sealed class HearingSensor : Sensor
    {
        // ───── Serialized properties ────────────────────────────────────────────────
        
        [Header("Hearing Settings")]
        [SerializeField] [Min(0.0f)] private float _hearingRange = 20.0f;
        [SerializeField] [Min(0.0f)] private float _newSoundDistanceThreshold = 3.0f;
        
        [Space(10.0f)]
        
        [Header("Gizmos")]
        [SerializeField] private bool _showHearingGizmos = true;
        [SerializeField] private Color _hearingGizmoColor = Color.red;

        // ───── Private properties ────────────────────────────────────────────────
        
        private readonly List<SoundEvent> _recentEvents = new();

        // ───── Implementation ────────────────────────────────────────────────
        
        protected override void Sense()
        {
            // DO NOT REACT TO SOUND IF:
            if (Agent.Blackboard.Get<bool>(BlackboardKeys.TARGET_VISIBLE)) { return; }
            
            SoundEventRegistry.GetRecent(_recentEvents);
            
            if (!TryFindAudibleEvent(out SoundEvent heard)) { return; }
            
            bool alreadyInvestigating = Agent.Blackboard.Get<bool>(BlackboardKeys.IS_INVESTIGATING);
            Vector3 currentTarget = Agent.Blackboard.Get<Vector3>(BlackboardKeys.TARGET_LAST_KNOWN_POS);
            
            Agent.Blackboard.Set(BlackboardKeys.TARGET_LAST_KNOWN_POS, heard.Position);
            Agent.Blackboard.Set(BlackboardKeys.AT_INVESTIGATION_POINT, false);
            
            // Dynamically respond to new sounds while investigating
            if (!alreadyInvestigating) { Agent.Blackboard.Set(BlackboardKeys.IS_INVESTIGATING, true); }
            else if (Vector3.Distance(currentTarget, heard.Position) > _newSoundDistanceThreshold) { Agent.RequestReplan(Agent.ActiveGoal); }
        }

        // ───── Private methods ────────────────────────────────────────────────
        
        private bool TryFindAudibleEvent(out SoundEvent result)
        {
            float nearestDist = float.MaxValue;
            bool found = false;
            result = default;

            foreach (SoundEvent evt in _recentEvents)
            {
                float dist = Vector3.Distance(Agent.transform.position, evt.Position);
                
                if (dist > _hearingRange) { continue; }
                if (dist > evt.Radius) { continue; }
                
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    result = evt;
                    found = true;
                }
            }
            
            return found;
        }

        // ───── Helper methods ────────────────────────────────────────────────
        
        #if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (!_showHearingGizmos) { return; }
            
            GetAgent();
            if (Agent == null) { return; }
            
            Gizmos.color = _hearingGizmoColor;
            Gizmos.DrawWireSphere(Agent.transform.position, _hearingRange);
        }
        #endif
    }
}