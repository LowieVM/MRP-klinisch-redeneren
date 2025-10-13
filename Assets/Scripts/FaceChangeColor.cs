using UnityEngine;

public class FaceChangeColor : MonoBehaviour
{
    [Header("Material settings")]
    public Material faceMaterial;
    public string defaultHex = "F1E5E2";

    [Header("Sweat settings")]
    public string sweatHex = "F9BAAB";
    [HideInInspector] public bool isSweating = false;

    [Header("Pale Settings")]
    public string paleHex = "FFFFFF";
    [HideInInspector] public bool isPale = false;

    private Color defaultColor;
    private Color sweatColor;
    private Color paleColor;

    private void Start()
    {

        if (faceMaterial == null)
        {
            Debug.LogError("Face material is not assigned.");
            return;
        }

        if (!ColorUtility.TryParseHtmlString("#" + defaultHex, out defaultColor))
            defaultColor = Color.white;

        if (!ColorUtility.TryParseHtmlString("#" + sweatHex, out sweatColor))
            sweatColor = Color.white;

        if (!ColorUtility.TryParseHtmlString("#" + paleHex, out paleColor))
            paleColor = Color.white;

        UpdateFaceColor();
    }

    public void UpdateFaceColor()
    {
        if (faceMaterial == null) return;

        Color targetColor = defaultColor;

        if (isSweating)
            targetColor = sweatColor;
        else if (isPale)
            targetColor = paleColor;

        if (faceMaterial.HasProperty("_BaseColor"))
            faceMaterial.SetColor("_BaseColor", targetColor);
        else if (faceMaterial.HasProperty("_Color"))
            faceMaterial.SetColor("_Color", targetColor);
    }

    /// <summary>
    /// Enables a symptom-related face color change.
    /// </summary>
    public void ApplySymptomEffect(string symptom)
    {
        switch (symptom)
        {
            case "Koorts":
                isSweating = true;
                isPale = false;
                break;

            case "Bleek":
                isPale = true;
                isSweating = false;
                break;

            default:
                // Reset to default if unrelated
                isSweating = false;
                isPale = false;
                break;
        }

        UpdateFaceColor();
    }

    /// <summary>
    /// Resets all face color effects.
    /// </summary>
    public void ResetFace()
    {
        isSweating = false;
        isPale = false;
        UpdateFaceColor();
    }
}
