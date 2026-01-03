using UnityEngine;

public class ResetPlayerPrefs : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PlayerPrefs.SetInt("SpeedScore", 0);
    }
}
