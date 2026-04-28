using UnityEngine;
                                                                                                                                                                                                                                  
  namespace Player
  {
      /// <summary>
      /// Responsible for handling the player lean functionality.
      /// </summary>
      public sealed class PlayerLeanController : MonoBehaviour
      {
          // ───── Serialized properties ────────────────────────────────────────────────
          
          [Header("References")]
          [SerializeField] private FPSController _controller;
          [SerializeField] private Transform _cameraTransform;
   
          [Space(10.0f)]                                                                                                                                                                                                          
                  
          [Header("Lean")]
          [SerializeField] [Range(0.1f, 0.6f)] private float _maxLeanDistance = 0.35f;
          [SerializeField] [Range(1.0f, 15.0f)] private float _maxLeanRoll = 5.0f;                                                                                                                                             
          [SerializeField] [Range(1.0f, 20.0f)] private float _leanSpeed = 10.0f;                                                                                                                                            
                                                                                                                                                                                                                                  
          [Space(10.0f)]                                                                                                                                                                                                          
                  
          [Header("Wall Check")]                                                                                                                                                                                                  
          [SerializeField] private float _wallCheckRadius = 0.12f;
          [SerializeField] private LayerMask _wallMask    = ~0;

          // ───── Public properties ────────────────────────────────────────────────
          
          public Vector3 WorldLeanOffset
          {
              get
              {
                  Vector3 right = Vector3.ProjectOnPlane(_cameraTransform.right, Vector3.up).normalized;
                  return right * (SmoothedLean * _maxLeanDistance);
              }
          } 
          
          public float SmoothedLean { get; private set; }
          public float LeanRoll => SmoothedLean * -_maxLeanRoll;

          // ───── Private properties ────────────────────────────────────────────────
          
          private PlayerCrouchSettings _crouchSettings;
          
          private float _rawInput;

          // ───── Public methods ────────────────────────────────────────────────
          
          public void SetLeanInput(float value) => _rawInput = Mathf.Clamp(value, -1.0f, 1.0f);

          // ───── Lifecycle methods ────────────────────────────────────────────────
          
          private void Awake()                                                                                                                                                                                                    
          {      
              Debug.Assert(_controller != null,"[PlayerLeanController] FPSController not assigned.", this);
              _crouchSettings = _controller._crouchSettings;
              
              Debug.Assert(_crouchSettings != null,"[PlayerLeanController] FPSController's CrouchSettings not assigned.", this);                                                                                                                  
          }
                                                                                                                                                                                                                                  
          private void Update()
          {
              float target = _rawInput;                                                                                                                                                                                                   
                                                                                                                                                                                                                                  
              if (Mathf.Abs(target) > 0.01f)                                                                                                                                                                                              
              {
                  float eyeHeight = _controller.IsCrouching ? _crouchSettings.CrouchingEyeHeight : _crouchSettings.StandingEyeHeight;                                                                                                     
                                                                                                                                                                                                                                          
                  Vector3 leanRight = Vector3.ProjectOnPlane(_cameraTransform.right, Vector3.up).normalized;                                                                                                                              
                  Vector3 origin = _controller.transform.position + Vector3.up * eyeHeight;                                                                                                                                            
                  Vector3 dir = leanRight * Mathf.Sign(target);                                                                                                                                                                     
                          
                  if (Physics.SphereCast(origin, _wallCheckRadius, dir, out RaycastHit hit, _maxLeanDistance, _wallMask))                                                                                                                 
                  {
                      float maxFraction = Mathf.Max(0.0f, hit.distance - _wallCheckRadius) / _maxLeanDistance;                                                                                                                            
                      target = Mathf.Sign(target) * Mathf.Min(Mathf.Abs(target), maxFraction);                                                                                                                                            
                  }
              }                                                                                                                                                                                                                           
                          
              SmoothedLean = Mathf.Lerp(SmoothedLean, target, 1.0f - Mathf.Pow(0.1f, Time.deltaTime * _leanSpeed));                                                                                                                    
          }
      }                                                                                                                                                                                                                           
  }    