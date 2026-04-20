using System;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Represents a door in the game environment. Can be interacted with through SmartObject for AI or IInteractable for player.
///
/// Authorised Use Instructions:
///     - NavMeshObstacle must be disabled immediately upon close, enabled immidately upon open
///     - Hinge should be assigned for realistic door open motion
///     - Only the hinge gets rotated, so the door model should be a hierarchy CHILD of the hinge GameObject
/// </summary>
public sealed class Door : MonoBehaviour, IInteractable
{
    // -----Nested Type-----
    public enum DoorState
    {
        Closing,
        Closed,
        Opening,
        Open
    }

    // ───── Implementation ────────────────────────────────────────────────
    public string InteractionPrompt => _state == DoorState.Open ? "Close Door" : "Open Door";
    public bool CanInteract => _state == DoorState.Open || _state == DoorState.Closed;
    
    // -----Serialized properties-----
    [Header("References")] 
    [SerializeField] private Transform _hinge;
    [SerializeField] private NavMeshObstacle _navMeshObstacle;

    [Space(10.0f)] 
    
    [Header("Animation")] 
    [SerializeField] [Range(0.0f, 180.0f)] private float _openAngle = 90.0f;
    [SerializeField] [Range(0.0f, 10.0f)] private float _openDuration = 0.6f;
    [SerializeField] [Range(0.0f, 180.0f)] private float _closedAngle = 0.0f;
    [SerializeField] private AnimationCurve _openCurve = AnimationCurve.EaseInOut(0.0f, 0.0f, 1.0f, 1.0f);
    
    [Space(10.0f)]
    
    [Header("Gizmos")]
    [SerializeField] private bool _drawDoorGizmos = true;
    
    // -----Private properties-----
    [HideInInspector] [SerializeField] private DoorState _state = DoorState.Closed;
    
    private float _openTimer;
    private float _targetAngle;
    private float _closeTimer;
    
    // -----Public properties-----
    public DoorState State => _state;
    public bool IsOpen => _state == DoorState.Open;
    public bool IsClosed => _state == DoorState.Closed;
    
    public event Action OnDoorOpened;

    // ───── Implementation ────────────────────────────────────────────────
    public void Interact(GameObject interactor)
    {
        if (_state == DoorState.Closed)
            Open(interactor.transform.position);
        else if (_state == DoorState.Open)
            Close(interactor.transform.position);
    }
    
    // -----MonoBehaviour methods-----
    private void Awake()
    {
        Debug.Assert(_hinge != null, "[Door] Hinge not assigned", this);
        Debug.Assert(_navMeshObstacle != null, "[Door] NavMeshObstacle not assigned", this);
        
        if (_hinge != null)
        {
            Vector3 euler = _hinge.localEulerAngles;
            euler.y = _state == DoorState.Open ? _closedAngle + _openAngle : _closedAngle;
            _hinge.localEulerAngles = euler;
        }
        
        SyncDoorStates(_state);
    }

    private void Update()
    {
        if (_state == DoorState.Opening)
            TickOpening();
        else if (_state == DoorState.Closing)
            TickClosing();
    }
    
    // -----Public methods-----
    public void Open(Vector3 openerPosition)
    {
        if (_state != DoorState.Closed) { return; }
        
        _targetAngle = CalculateOpenAngle(openerPosition);
        _state = DoorState.Opening;
        _openTimer = 0.0f;
        
        if (_navMeshObstacle != null)
            _navMeshObstacle.enabled = false;
    }
    
    public void Close(Vector3 closerPosition)
    {
        if (_state != DoorState.Open) { return; }
        
        _state = DoorState.Closing;
        _closeTimer = 0.0f;
        
        _targetAngle = _closedAngle;
        
        if (_navMeshObstacle != null)
            _navMeshObstacle.enabled = false;
    }
    
    public void Open()
    {
        Open(transform.position - transform.forward);
    }
    
    public void ResetToClosed()
    {
        _state = DoorState.Closed;
        _openTimer = 0.0f;
        
        if (_hinge != null)
            _hinge.localEulerAngles = new Vector3( _hinge.localEulerAngles.x, _closedAngle, _hinge.localEulerAngles.z);
        
        SyncDoorStates(_state);
    }
    
    // -----Private methods-----
    private void TickOpening()
    {
        _openTimer += Time.deltaTime;
        
        float t = Mathf.Clamp01(_openTimer / _openDuration);
        float curveT =  _openCurve.Evaluate(t);
        
        _hinge.localEulerAngles = new Vector3( _hinge.localEulerAngles.x, Mathf.LerpAngle(_closedAngle, _targetAngle, curveT), _hinge.localEulerAngles.z);
        
        if (t >= 1.0f)
        {
            _state = DoorState.Open;
            SyncDoorStates(_state);
            OnDoorOpened?.Invoke();
        }
    }
    
    private void TickClosing()
    {
        _closeTimer += Time.deltaTime;
        
        float t = Mathf.Clamp01(_closeTimer / _openDuration);
        float curveT =  _openCurve.Evaluate(t);
        
        _hinge.localEulerAngles = new Vector3( _hinge.localEulerAngles.x, Mathf.LerpAngle(_targetAngle, _closedAngle, curveT), _hinge.localEulerAngles.z);
        
        if (t >= 1.0f)
        {
            _state = DoorState.Closed;
            SyncDoorStates(_state);
        }
    }
    
    private void SyncDoorStates(DoorState state)
    {
        switch (state)
        {
            case DoorState.Closing:
                if (_navMeshObstacle != null)
                    _navMeshObstacle.enabled = false;
                break;
            
            case DoorState.Closed:
                if (_navMeshObstacle != null)
                    _navMeshObstacle.enabled = false;
                break;
            
            case DoorState.Opening:
                if (_navMeshObstacle != null)
                    _navMeshObstacle.enabled = false;
                break;
            
            case DoorState.Open:
                if (_navMeshObstacle != null)
                    _navMeshObstacle.enabled = true;
                break;
        }
    }
    
    private float CalculateOpenAngle(Vector3 openerPosition)
    {
        Vector3 toOpener = (openerPosition - transform.position).normalized;
        float dot = Vector3.Dot(transform.right, toOpener);
        float direction = dot >= 0.0f ? -1.0f : 1.0f;
        
        return _closedAngle + (_openAngle * direction);
    }
    
    // -----Editor helper methods-----
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (_hinge == null || !_drawDoorGizmos) { return; }
        
        Gizmos.color = _state == DoorState.Closed ? Color.red : Color.green;
        
        Gizmos.DrawWireCube(_hinge.position, new Vector3(0.1f, 2.0f, 0.9f));
    }
#endif
}
