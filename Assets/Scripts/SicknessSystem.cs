using System.Collections.Generic;
using UnityEngine;

public class SicknessSystem : MonoBehaviour
{
    // Define sicknesses and their symptoms
    private Dictionary<string, List<string>> sicknessMap = new Dictionary<string, List<string>>();

    // List of sickness names to map IDs to (index 0 = ID 0, etc.)
    private List<string> sicknessOrder = new List<string>();

    [Header("Current Sickness ID")]
    public int sicknessID = 0; // Set this in the Inspector or via code

    private FaceChangeColor faceChanger;
    private bool sicknessActive = false;

    void Awake()
    {
        faceChanger = GetComponent<FaceChangeColor>();

        // Initialize sicknesses
        sicknessMap = new Dictionary<string, List<string>>
        {
            { "Hypoglycemie", new List<string> { "Zweten", "Rillen", "Bleek", "Moeite met concentreren", "Wazig zicht", "Vermoeidheid" } },
            { "Delier", new List<string> { "Verwardheid", "Rusteloosheid", "Frequent urineren", "Constante plasdrang", "Pijn of branderig gevoel bij het plassen", "Weinig urinelozing" } },
            { "Sepsis", new List<string> { "Koorts", "Klamme huid", "Versnelde hartslag", "Versnelde ademhaling", "Verwardheid", "Lage bloeddruk" } },
        };

        // Maintain a fixed order for ID mapping
        sicknessOrder = new List<string> { "Hypoglycemie", "Delier", "Sepsis"};
    }

    private void Start()
    {
        ApplySickness(sicknessID);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && sicknessActive)
        {
            EndSickness();
        }
    }

    /// <summary>
    /// Applies a sickness based on its numeric ID.
    /// </summary>
    public void ApplySickness(int id)
    {
        if (id < 0 || id >= sicknessOrder.Count)
        {
            Debug.LogWarning($"Invalid sickness ID: {id}");
            return;
        }

        string sicknessName = sicknessOrder[id];

        if (sicknessMap.TryGetValue(sicknessName, out var symptoms))
        {
            // 🔸 Here you would trigger actual symptom effects
            foreach (var symptom in symptoms)
            {
                TriggerSymptom(symptom);
            }

            sicknessActive = true;
        }
    }

    /// <summary>
    /// Handles how each symptom manifests visually or behaviorally.
    /// </summary>
    private void TriggerSymptom(string symptom)
    {
        switch (symptom)
        {
            case "Koorts":
            case "Bleek":
                // Example: Change character skin color or heat effect
                if (faceChanger != null)
                {
                    faceChanger.ApplySymptomEffect(symptom);
                }
                break;
            default:
                
                break;
        }
    }

    /// <summary>
    /// Resets sickness effects (including face color).
    /// </summary>
    private void EndSickness()
    {
        sicknessActive = false;
        faceChanger?.ResetFace();
        Debug.Log("✅ Sickness ended, face color reset");
    }
}
