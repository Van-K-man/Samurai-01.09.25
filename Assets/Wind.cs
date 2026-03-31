using UnityEngine;

public class GrassWindAnimator : MonoBehaviour
{
    public Material grassMat;
    
    [Header("Wind Settings")]
    public float baseStrength = 0.5f;
    public float waveSpeed = 1f;
    public float waveAmplitude = 0.2f;
    
    [Header("Turbulence")]
    public float turbulenceStrength = 0.1f;
    public float turbulenceSpeed = 2f;
    public Vector2 turbulenceScale = new Vector2(1f, 1f);
    
    [Header("Direction")]
    public Vector2 windDirection = Vector2.right;
    
    private Vector2 turbulenceOffset;
    private float time;

    void Awake()
    {
        // Normalize wind direction
        windDirection = windDirection.normalized;
        turbulenceOffset = Random.insideUnitCircle * 100f; // Random starting offset
    }

    void Update()
    {
        time += Time.deltaTime;
        
        // Calculate main wind wave
        float mainWind = baseStrength + Mathf.Sin(time * waveSpeed) * waveAmplitude;
        
        // Calculate turbulence using Perlin noise
        float turbX = Mathf.PerlinNoise(
            time * turbulenceSpeed + turbulenceOffset.x, 
            turbulenceOffset.y) * 2f - 1f;
        float turbY = Mathf.PerlinNoise(
            turbulenceOffset.x, 
            time * turbulenceSpeed + turbulenceOffset.y) * 2f - 1f;
        
        Vector2 turbulence = new Vector2(turbX, turbY) * turbulenceStrength;
        
        // Combine wind and turbulence
        Vector2 finalWind = (windDirection * mainWind) + (turbulence * turbulenceScale);
        
        // Set shader parameters
        grassMat.SetFloat("_WindStrength", finalWind.magnitude);
        grassMat.SetVector("_WindDirection", new Vector4(finalWind.x, finalWind.y, 0, 0));
        
        // Optional: Set parameters for point grass system if it exists
        Shader.SetGlobalVector("_PG_VectorA", new Vector4(finalWind.x, 0, finalWind.y, mainWind));
        Shader.SetGlobalFloat("_PG_ValueA", turbulenceStrength);
    }

    private void OnValidate()
    {
        windDirection = windDirection.normalized;
    }
}
