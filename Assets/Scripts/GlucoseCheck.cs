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
        text.text = "Her glucose is 107"; // normal glucose 70-130 mg/dL before meals
        yield return new WaitForSeconds(5f);
        text.text = "";
        worldCanvas.SetActive(false);
    }

    public void DisplayText()
    {
        StartCoroutine(DisplayTextCoroutine());
    }
}
