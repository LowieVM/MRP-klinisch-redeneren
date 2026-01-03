using System.Collections;
using TMPro;
using UnityEngine;

public class BloodPressureCheck : MonoBehaviour
{
    public GameObject sphygmomanometer;
    public GameObject worldCanvas;
    public TextMeshProUGUI text;

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
        text.text = "Hmmm his blood pressure is 130/80 mmHg"; // high blood pressure 130/80 <, normal = 120/80, low 90/60 >
        yield return new WaitForSeconds(5f);
        text.text = "";
        worldCanvas.SetActive(false);
    }
}
