using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

namespace UI
{
    /// <summary>
    /// Simple pause menu.
    /// </summary>
    public sealed class PauseMenu : MonoBehaviour
    {
        [SerializeField] private GameObject _pausePanel;

        private PlayerInputActions _input;
        private bool _isPaused;

        private void Awake()
        {
            // TODO: Move to dedicated script, have UI settings
            // Application.targetFrameRate = 60;
            // QualitySettings.vSyncCount  = 0;
            
            _input = new PlayerInputActions();

            Debug.Assert(_pausePanel != null,"[PauseMenu] Pause panel not assigned.", this);

            _pausePanel.SetActive(false);
        }

        private void OnEnable()
        {
            _input.Player.Enable();
            _input.Player.Pause.performed += OnPausePerformed;
        }

        private void OnDisable()
        {
            _input.Player.Pause.performed -= OnPausePerformed;
            _input.Player.Disable();
        }

        private void OnPausePerformed(InputAction.CallbackContext ctx)
        {
            if (_isPaused) Resume(); else Pause();
        }

        public void Resume()
        {
            _pausePanel.SetActive(false);
            Time.timeScale      = 1.0f;
            Cursor.lockState    = CursorLockMode.Locked;
            Cursor.visible      = false;
            _isPaused           = false;
        }
        
        public void Restart()
        {
            Time.timeScale      = 1.0f;
            Cursor.lockState    = CursorLockMode.Locked;
            Cursor.visible      = false;
            _isPaused           = false;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void Quit()
        {
            Time.timeScale = 1.0f;
            Application.Quit();

    #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
    #endif
        }

        private void Pause()
        {
            _pausePanel.SetActive(true);
            Time.timeScale      = 0.0f;
            Cursor.lockState    = CursorLockMode.None;
            Cursor.visible      = true;
            _isPaused           = true;
        }
    }
}
