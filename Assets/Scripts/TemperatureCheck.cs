using System.Collections;
using TMPro;
using UnityEngine;

public class TemperatureCheck : MonoBehaviour
{
    public GameObject thermometer;
    public GameObject worldCanvas;
    public TextMeshProUGUI text;

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
        text.text = "Hmmm her temperature is only 35.2°C"; // Low temperature <36.1°C, normal = 36.1°C-37.2°C, high >37.2°C
        yield return new WaitForSeconds(5f);
        text.text = "";
        worldCanvas.SetActive(false);
    }
}
