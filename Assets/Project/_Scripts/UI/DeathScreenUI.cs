using System.Collections;
using Player;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace UI
{
    /// <summary>
    /// Handles the displaying of the death screen UI.
    /// </summary>
    public sealed class DeathScreenUI : MonoBehaviour
    {
        // ───── Serialized properties ────────────────────────────────────────────────
        
        [SerializeField] private PlayerHealth _playerHealth;
        [SerializeField] private Image _fadeOverlay;
        [SerializeField] private GameObject _deathPanel;
        [SerializeField] [Min(0.01f)] private float _fadeDuration = 1.5f;

        // ───── Lifecycle methods ────────────────────────────────────────────────

        private void Awake()
        {
            _fadeOverlay.color = new Color(0.0f, 0.0f, 0.0f, 0.0f); // start clear
            _deathPanel.SetActive(false);
        }

        private void OnEnable() => _playerHealth.OnDeath += Show;
        private void OnDisable() => _playerHealth.OnDeath -= Show;
        
        // ───── Public methods ────────────────────────────────────────────────
        
        public void Show() => StartCoroutine(FadeAndShow());
        
        public void Restart() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        
        public void Quit()
        {
            Application.Quit();
            
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #endif
        }

        // ───── Private methods ────────────────────────────────────────────────
        
        private IEnumerator FadeAndShow()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            float elapsed = 0.0f;
            while (elapsed < _fadeDuration)
            {
                elapsed += Time.deltaTime;
                _fadeOverlay.color = new Color(0.0f, 0.0f, 0.0f, elapsed / _fadeDuration);
                yield return null;
            }
            
            _fadeOverlay.color = new Color(0.0f, 0.0f, 0.0f, 1.0f);
            _deathPanel.SetActive(true);
        }
    }
}