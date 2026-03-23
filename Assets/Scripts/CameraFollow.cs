using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target; // The character
    
    [Header("Follow Settings")]
    public Vector3 offset = new Vector3(0, 5, -7); // Camera offset
    public float followSpeed = 2f;
    
    void LateUpdate()
    {
        if (target == null) return;
        
        // Calculate target position (character's position + offset)
        Vector3 targetPosition = target.position + offset;
        
        // Move camera smoothly to target position
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
        
        // DO NOT change camera rotation - it remains fixed
    }
}