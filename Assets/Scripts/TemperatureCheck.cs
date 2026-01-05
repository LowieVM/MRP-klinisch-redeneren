using System.Collections;
using TMPro;
using UnityEngine;

public class TemperatureCheck : MonoBehaviour
{
    public GameObject thermometer;
    public GameObject worldCanvas;
    public TextMeshProUGUI text;
    public int patientCase;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == thermometer)
        {
            StartCoroutine(DisplayText());
        }
    }

    public IEnumerator DisplayText()
    {
        worldCanvas.SetActive(true);
        switch (patientCase)
        {
            case 1:
                text.text = "Hmmm his temperature is 38.5°C"; // High temperature
                break;
            case 2:
                text.text = "Hmmm her temperature is 36.8°C"; // normal temperature
                break;
            case 3:
                text.text = "Hmmm her temperature is only 35.2°C"; // Low temperature
                break;
        }
        yield return new WaitForSeconds(5f);
        text.text = "";
        worldCanvas.SetActive(false);
    }
}
