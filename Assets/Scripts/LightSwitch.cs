using UnityEngine;
using System.Collections.Generic;

public class LightSwitch : MonoBehaviour
{
    [Header("Lights to Control")]
    public List<Light> lights = new List<Light>();

    [Header("Materials")]
    public Material onMaterial;
    public Material offMaterial;

    [Header("Renderers for material swap")]
    public List<Renderer> renderers = new List<Renderer>();

    private bool lightsOn = true;

    // Toggle lights manually (can be called by UI button or trigger)
    public void ToggleLights()
    {
        lightsOn = !lightsOn;

        foreach(var light in lights)
        {
            if (light != null)
                light.enabled = lightsOn;
        }
        foreach(var rend in renderers)
        {
            if (rend != null)
                rend.material = lightsOn ? onMaterial : offMaterial;
        }
    }
}
