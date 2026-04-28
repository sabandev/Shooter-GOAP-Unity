using UnityEngine;
using Weapons;

namespace Player
{
    /// <summary>
    /// Applies procedural weapon bob to ViewModelRoot.
    /// Oscillates with movement speed.
    /// Different settings for standing vs crouching.
    /// </summary>
    public sealed class ViewModelBob : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private FPSController _controller;
        [SerializeField] private WeaponHolder  _weaponHolder;

        [Header("Standing")]
        [SerializeField] private float _standFrequency  = 1.8f;
        [SerializeField] private float _standVertical   = 0.008f;
        [SerializeField] private float _standHorizontal = 0.004f;

        [Header("Crouching")]
        [SerializeField] private float _crouchFrequency  = 1.2f;
        [SerializeField] private float _crouchVertical   = 0.005f;
        [SerializeField] private float _crouchHorizontal = 0.003f;

        [SerializeField] [Range(1.0f, 10.0f)]
        private float _blendSpeed = 5.0f;

        private float   _bobTimer;
        private float   _bobWeight;
        private Vector3 _bobOffset;
        private Vector3 _basePosition;

        private void Awake() =>
            _basePosition = transform.localPosition;

        private void Update()
        {
            bool  crouch    = _controller.IsCrouching;
            float speed     = _controller.NormalisedSpeed;
            bool  grounded  = _controller.IsGrounded;

            float freq = crouch ? _crouchFrequency  : _standFrequency;
            float vert = crouch ? _crouchVertical   : _standVertical;
            float horiz = crouch ? _crouchHorizontal : _standHorizontal;

            float scale = _weaponHolder.HasWeapon ? 1.0f : 0.5f;

            float targetWeight = speed > 0.05f && grounded
                ? speed * scale : 0.0f;

            _bobWeight = Mathf.Lerp(
                _bobWeight, targetWeight,
                1.0f - Mathf.Pow(0.1f, Time.deltaTime * _blendSpeed));

            if (_bobWeight > 0.001f)
            {
                _bobTimer += Time.deltaTime * freq * speed;

                _bobOffset = new Vector3(
                    Mathf.Cos(_bobTimer * Mathf.PI) * horiz * _bobWeight,
                    Mathf.Sin(_bobTimer * Mathf.PI * 2.0f) * vert * _bobWeight,
                    0.0f);
            }
            else
            {
                _bobOffset = Vector3.Lerp(
                    _bobOffset, Vector3.zero,
                    1.0f - Mathf.Pow(0.1f, Time.deltaTime * 8.0f));
            }

            transform.localPosition = _basePosition + _bobOffset;
        }
    }
}