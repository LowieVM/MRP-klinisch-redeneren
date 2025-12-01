using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimerController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private bool startOnAwake = true;

    private float elapsed;
    private bool running;

    void Awake()
    {
        // If no Text assigned, try to use a Text on the same GameObject
        if (timerText == null)
            timerText = GetComponent<TextMeshProUGUI>();

        UpdateText(0f);
    }

    // Will start automatically if startOnAwake is true
    void Start()
    {
        if (startOnAwake)
            StartTimerInternal();
    }

    void Update()
    {
        if (!running) return;

        elapsed += Time.deltaTime;
        UpdateText(elapsed);
    }

    // Start the timer (resets elapsed to 0)
    public void StartTimerInternal()
    {
        elapsed = 0f;
        running = true;
        UpdateText(elapsed);
    }

    // Stop/pause the timer
    public void StopTimer()
    {
        running = false;
    }

    // Reset the timer to zero (won't start unless StartTimerInternal is called)
    public void ResetTimer()
    {
        elapsed = 0f;
        UpdateText(elapsed);
    }

    // Public helper to be called from other scripts (e.g. VRDoor) when the door opens.
    // Keeps the TimerController API intent-clear.
    public void StartTimer()
    {
        StartTimerInternal();
    }

    private void UpdateText(float seconds)
    {
        var t = TimeSpan.FromSeconds(seconds);
        string text;

        if (t.TotalHours >= 1)
            text = string.Format("{0:D2}:{1:D2}:{2:D2}", (int)t.TotalHours, t.Minutes, t.Seconds);
        else
            text = string.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);

        if (timerText != null)
            timerText.text = text;
    }
}
