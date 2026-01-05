using System.Collections;
using TMPro;
using UnityEngine;

public class BloodPressureCheck : MonoBehaviour
{
    public GameObject sphygmomanometer;
    public GameObject worldCanvas;
    public TextMeshProUGUI text;
    public int patientCase;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == sphygmomanometer)
        {
            StartCoroutine(DisplayText());
        }
    }

    public IEnumerator DisplayText()
    {
        worldCanvas.SetActive(true);
        switch(patientCase)
        {
            case 1:
                text.text = "Hmmm his blood pressure is 150/95 mmHg"; // High blood pressure
                break;
            case 2:
                text.text = "Hmmm her blood pressure is 120/80 mmHg"; // Normal blood pressure
                break;
            case 3:
                text.text = "Hmmm her blood pressure is 85/55 mmHg"; // Low blood pressure
                break;
        }
        yield return new WaitForSeconds(5f);
        text.text = "";
        worldCanvas.SetActive(false);
    }
}
