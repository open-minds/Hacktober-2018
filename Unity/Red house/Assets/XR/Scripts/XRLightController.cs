using UnityEngine;
using System;

// Controller for Unity Light objects, adjusting the illumination based on observations from the scene.
public class XRLightController : MonoBehaviour {
  private XRController xr;
  private Light sceneLight;

  void Start() {
    sceneLight = GetComponent<Light>();
    xr = GameObject.FindWithTag("XRController").GetComponent<XRController>();
  }

  void Update () {
    if (xr.DisabledInEditor()) {
      return;
    }
    // Update the light exposure.
    float exposure = xr.GetLightExposure();
    float temperature = xr.GetLightTemperature();

    // Exposure ranges from -1 to 1 in XR, adjust to 0-2 for Unity.
    sceneLight.intensity = exposure + 1.0f;
    sceneLight.colorTemperature = temperature;
    RenderSettings.ambientIntensity = exposure + 1.0f;
    RenderSettings.ambientLight = tempToColor(temperature);
  }

  float trunc(double color) {
    return color < 0.0f ? 0.0f : (color > 255.0f ? 255.0f : (float)color);
  }

  Color tempToColor(float temp) {
    temp = temp < 0.0f ? 0.0f : temp / 100.0f;
    float red = temp <= 66.0f
      ? 255.0f
      : trunc(329.698727446 * Math.Pow(temp - 60.0, -0.1332047592));
    float green = temp <= 0.0f
      ? 255.0f
      : (temp < 66.0f
        ? trunc(99.4708025861 * Math.Log(temp) - 161.1195681661)
        : trunc(288.1221695283 * Math.Pow(temp - 60.0, -0.0755148492)));
    float blue = temp >= 66.0f
      ? 255.0f
      : (temp <= 10.0f
        ? 0.0f
        : trunc(138.5177312231 * Math.Log(temp - 10.0) - 305.0447927307));
    return new Color(red / 255.0f, green / 255.0f, blue / 255.0f);
  }
}
