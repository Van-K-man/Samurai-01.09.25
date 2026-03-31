using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;
    private float currentHealth;

    [Header("UI")]
    public Slider healthBar;

    [Header("Billboard")]
    public bool useBillboard = true;
    public Vector3 healthBarOffset = new Vector3(0, 2f, 0); // Position über dem Kopf
    private Canvas healthBarCanvas;
    private Vector3 canvasRelativePos; // Speichert die relative Position beim Start

    [Header("Damage Feedback")]
    public Color damageFlashColor = Color.red;
    public float flashDuration = 0.3f;
    public float fadeDuration = 0.5f;
    
    private Color originalSliderColor;
    private Coroutine flashCoroutine;

    public event Action OnTod;

    void Start()
    {
        currentHealth = maxHealth;
        
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
            healthBarCanvas = healthBar.GetComponentInParent<Canvas>();
            
            // Speichere die originale Slider-Farbe
            Image sliderFill = healthBar.fillRect.GetComponent<Image>();
            if (sliderFill != null)
            {
                originalSliderColor = sliderFill.color;
            }
            
            // Speichere die relative Position des Canvas zum NPC beim Start
            if (healthBarCanvas != null)
            {
                canvasRelativePos = healthBarCanvas.transform.position - transform.position;
            }
            
            if (healthBarCanvas == null)
            {
                Debug.LogError("[HealthBar] Canvas für Slider nicht gefunden! Stelle sicher, dass der Slider ein Kind eines Canvas ist.");
            }
        }
        else
        {
            Debug.LogError("[HealthBar] Slider (healthBar) nicht im Inspector zugewiesen!");
        }
    }

    void LateUpdate()
    {
        if (healthBarCanvas != null)
        {
            // Position: Canvas behält die relative Position zum NPC bei (wie beim Start positioniert)
            healthBarCanvas.transform.position = transform.position + canvasRelativePos;
            
            // Debug: Zeige wo die Healthbar hingeht
            Debug.DrawLine(transform.position, healthBarCanvas.transform.position, Color.red);
            
            // Rotation: Canvas bleibt aufrecht, folgt der Kamera (nur Y-Achse)
            if (useBillboard && Camera.main != null)
            {
                float camY = Camera.main.transform.eulerAngles.y;
                healthBarCanvas.transform.rotation = Quaternion.Euler(0f, camY, 0f);
            }
            else
            {
                // Keine Rotation, komplett aufrecht
                healthBarCanvas.transform.rotation = Quaternion.identity;
            }
        }
    }

    public void NimmSchaden(float schaden)
    {
        currentHealth -= schaden;
        Debug.Log($"[HealthBar] {gameObject.name} nimmt {schaden} Schaden | HP: {currentHealth}/{maxHealth}");
        
        if (healthBar != null)
        {
            healthBar.value = currentHealth;
            
            // Starte visuellen Feedback-Effekt
            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine);
            }
            flashCoroutine = StartCoroutine(FlashDamageEffect());
        }

        if (currentHealth <= 0)
            Sterben();
    }

    private IEnumerator FlashDamageEffect()
    {
        Image sliderFill = healthBar.fillRect.GetComponent<Image>();
        if (sliderFill == null)
            yield break;

        // Phase 1: Schnelles Aufblitzen in Damage-Farbe
        float elapsed = 0f;
        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / flashDuration;
            sliderFill.color = Color.Lerp(originalSliderColor, damageFlashColor, t);
            yield return null;
        }

        // Phase 2: Sanftes Ausblenden zurück zur Originalfarbe
        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            sliderFill.color = Color.Lerp(damageFlashColor, originalSliderColor, t);
            yield return null;
        }

        // Stelle sicher, dass wir genau die Originalfarbe haben
        sliderFill.color = originalSliderColor;
    }

    private void Sterben()
    {
        Debug.Log($"[HealthBar] {gameObject.name} ist tot!");
        OnTod?.Invoke();
    }

    public float GetHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;
}