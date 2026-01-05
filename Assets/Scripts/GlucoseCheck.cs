using System.Collections;
using TMPro;
using UnityEngine;

public class GlucoseCheck : MonoBehaviour
{
    public GameObject testStrip;
    public GameObject lancet;
    public GameObject glass;

    public GameObject worldCanvas;
    public TextMeshProUGUI text;
    public int patientCase;

    private void Start()
    {
        testStrip.SetActive(false);
        glass.SetActive(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == lancet)
        {
            testStrip.SetActive(true);
            glass.SetActive(false);
        }
    }

    public IEnumerator DisplayTextCoroutine()
    {
        worldCanvas.SetActive(true);
        switch(patientCase)
        {
            case 1:
                text.text = "His glucose is 180"; // High glucose
                break;
            case 2:
                text.text = "Her glucose is 107"; // Normal glucose
                break;
            case 3:
                text.text = "Her glucose is 65"; // Low glucose
                break;
        }
        yield return new WaitForSeconds(5f);
        text.text = "";
        worldCanvas.SetActive(false);
    }

    public void DisplayText()
    {
        StartCoroutine(DisplayTextCoroutine());
    }
}
