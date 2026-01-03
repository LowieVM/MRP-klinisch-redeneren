using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TimerController : MonoBehaviour
{
    public static TimerController Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private bool startOnAwake = true;
    [Tooltip("If true the TimerController GameObject will persist between scene loads.")]
    [SerializeField] private bool persistAcrossScenes = true;
    [Tooltip("If true the timer will be reset to 00:00:00 when a new scene loads.")]
    [SerializeField] private bool resetOnSceneLoad = true;

    private float elapsed;
    private bool running;

    void Awake()
    {
        // Singleton handling
        if (Instance == null)
        {
            Instance = this;
            if (persistAcrossScenes)
                DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Try to use a TMP on the same GameObject if not assigned
        if (timerText == null)
            timerText = GetComponent<TextMeshProUGUI>();

        // Subscribe to scene load to (re)attach UI or reset as required
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;

        // Initialize display using current elapsed (don't force reset here)
        UpdateText(elapsed);
    }

    void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        // Only run start logic for the singleton instance
        if (Instance != this) return;

        // If configured to start on awake and not already running:
        if (startOnAwake && !running)
        {
            // Start and reset only if elapsed == 0, otherwise resume current elapsed.
            if (elapsed <= 0f)
                StartTimerInternal(reset: true);
            else
                StartTimerInternal(reset: false);
        }
    }

    void Update()
    {
        if (!running) return;

        elapsed += Time.deltaTime;
        UpdateText(elapsed);
    }

    // Internal start implementation with optional reset.
    private void StartTimerInternal(bool reset)
    {
        if (reset)
            elapsed = 0f;
        running = true;
        UpdateText(elapsed);
    }

    // Public API kept for other scripts (e.g. VRDoor)
    // Call StartTimer() to reset+start (backwards compatible).
    public void StartTimer()
    {
        StartTimer(reset: true);
    }

    // New API: allow starting without resetting elapsed (useful when you want timer to continue across scenes)
    public void StartTimer(bool reset)
    {
        StartTimerInternal(reset);
    }

    // Stop/pause the timer
    public void StopTimer()
    {
        running = false;
    }

    // Reset the timer to zero (won't start unless StartTimer(true) is called)
    public void ResetTimer()
    {
        elapsed = 0f;
        running = false;
        UpdateText(elapsed);
    }

    // Attach a TMP label from the current scene to display the time.
    // Call this from other scripts (e.g. scene UI initializers) if automatic finding doesn't work.
    public void AttachText(TextMeshProUGUI text)
    {
        timerText = text;
        UpdateText(elapsed);
    }

    // Called when a new scene finishes loading. Attempt to attach a TextMeshProUGUI if none assigned.
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // If the controller persists but should reset on scene load, reset it now before updating/attaching UI.
        if (resetOnSceneLoad)
        {
            elapsed = 0f;
            running = false;
            PlayerPrefs.SetInt("SpeedScore", 0);
        }
    }

    private void UpdateText(float seconds)
    {
        var t = TimeSpan.FromSeconds(seconds);
        int hours = (int)t.TotalHours;
        string text = string.Format("{0:D2}:{1:D2}:{2:D2}", hours, t.Minutes, t.Seconds);

        if (timerText != null)
            timerText.text = text;
    }
}
