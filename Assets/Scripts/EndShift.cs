using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndShift : MonoBehaviour
{
    [Header("Level Selection")]
    [Tooltip("If true the trigger will load the first-level scene. If false it will load the end scene.")]
    public bool isFirstLevel = true;

    [Header("Filtering")]

    [Tooltip("If true the trigger will activate only once")]
    public bool singleUse = true;

    [Header("Fade")]
    [Tooltip("Duration of the fade to white in seconds")]
    public float fadeDuration = 0.75f;
    [Tooltip("Color to fade to (white for a white-out)")]
    public Color fadeColor = Color.white;

    [Header("Debug")]
    public bool debugLogging = true;

    private readonly string _sceneName = "FullOldHospital";
    private readonly string _firstLevelSceneName = "FullOldHospital";
    private readonly string _endSceneName = "GameEndScreen";
    private readonly string _requiredTag = "Player";

    private bool _activated = false;
    private bool _isFading = false;

    // Called when another collider enters this trigger (requires this GameObject to have a Collider with Is Trigger = true)
    private void OnTriggerEnter(Collider other)
    {
        TryActivate(other.gameObject);
    }

    // Fallback for non-trigger colliders (in case you use collisions)
    private void OnCollisionEnter(Collision collision)
    {
        TryActivate(collision.gameObject);
    }

    // Public method so you can call activation from other scripts (e.g., XR interaction event)
    public void Activate()
    {
        if (_activated && singleUse) return;

        string target = ResolveTargetScene();
        if (debugLogging) Debug.Log($"[EndShift] Activate() called. Preparing to load scene '{target}' (fade to white).");
        _activated = true;
        StartCoroutine(FadeAndLoad(target));
    }

    private void TryActivate(GameObject activator)
    {
        if (_activated && singleUse) return;

        if (activator == null) return;

        // Accept if tag matches (if set) OR if activator (or parent) has a CharacterController (common in XR rigs)
        bool tagOk = string.IsNullOrEmpty(_requiredTag) || activator.CompareTag(_requiredTag);
        bool hasCharController = activator.GetComponent<CharacterController>() != null || activator.GetComponentInParent<CharacterController>() != null;
        bool hasRigidbody = activator.GetComponent<Rigidbody>() != null || activator.GetComponentInParent<Rigidbody>() != null;

        if (!tagOk && !hasCharController && !hasRigidbody)
        {
            if (debugLogging)
                Debug.Log($"[EndShift] Ignored '{activator.name}'. Tag='{activator.tag}', hasCharController={hasCharController}, hasRigidbody={hasRigidbody}. Expecting tag '{_requiredTag}' or a CharacterController/Rigidbody.");
            return;
        }

        string target = ResolveTargetScene();
        if (debugLogging)
            Debug.Log($"[EndShift] Triggered by '{activator.name}'. Preparing to load scene '{target}' (fade to white).");

        _activated = true;
        StartCoroutine(FadeAndLoad(target));
    }

    private string ResolveTargetScene()
    {
        if (isFirstLevel)
        {
            if (!string.IsNullOrEmpty(_firstLevelSceneName))
                return _firstLevelSceneName;
            // fallback
            return _sceneName;
        }
        else
        {
            if (!string.IsNullOrEmpty(_endSceneName))
                return _endSceneName;
            // fallback
            return _sceneName;
        }
    }

    private IEnumerator FadeAndLoad(string targetScene)
    {
        if (_isFading) yield break;
        _isFading = true;

        // Create a simple full-screen UI Canvas + Image to perform the fade
        var canvasGO = new GameObject("ScreenFader");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;

        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        var imageGO = new GameObject("FadeImage");
        imageGO.transform.SetParent(canvasGO.transform, false);
        var image = imageGO.AddComponent<Image>();
        image.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);

        var rt = image.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsed / fadeDuration);
            image.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
            yield return null;
        }

        // Ensure fully opaque
        image.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1f);

        // Allow one frame so the final color is rendered before scene change
        yield return null;

        // Load the target scene
        SceneManager.LoadScene(targetScene);
    }
}