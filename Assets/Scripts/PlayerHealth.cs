using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

/// <summary>
/// Kombiniertes Spieler-Gesundheitssystem mit UI-Bar und visuellen Effekten.
/// Verwaltet Leben, UI-Anzeige, Billboard-Funktionalität und Schadenseffekte.
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    [Header("Leben")]
    public float maxLeben = 100f;
    public float aktuellesLeben { get; private set; }

    [Header("UI")]
    public Slider healthBar;

    [Header("Billboard")]
    public bool useBillboard = true;
    public Vector3 healthBarOffset = new Vector3(0, 2f, 0);
    private Canvas healthBarCanvas;
    private Vector3 canvasRelativePos;

    [Header("Schadenseffekte")]
    public Color damageFlashColor = Color.red;
    public float flashDuration = 0.3f;
    public float fadeDuration = 0.5f;
    
    private Color originalSliderColor;
    private Coroutine flashCoroutine;
    private Animator animator;

    /// <summary>Wird ausgelöst wenn der Spieler stirbt.</summary>
    public event Action OnTod;

    private bool istTot = false;

    // ─────────────────────────────────────────────────────────────────────────
    void Start()
    {
        aktuellesLeben = maxLeben;
        animator = GetComponent<Animator>();
        
        if (healthBar != null)
        {
            healthBar.maxValue = maxLeben;
            healthBar.value = aktuellesLeben;
            healthBarCanvas = healthBar.GetComponentInParent<Canvas>();
            
            Image sliderFill = healthBar.fillRect.GetComponent<Image>();
            if (sliderFill != null)
            {
                originalSliderColor = sliderFill.color;
            }
            
            if (healthBarCanvas != null)
            {
                canvasRelativePos = healthBarCanvas.transform.position - transform.position;
            }
            
            if (healthBarCanvas == null)
            {
                Debug.LogError("[PlayerHealth] Canvas für Slider nicht gefunden!");
            }
        }
        else
        {
            Debug.LogWarning("[PlayerHealth] Slider (healthBar) nicht im Inspector zugewiesen!");
        }
    }

    void LateUpdate()
    {
        if (healthBarCanvas != null)
        {
            healthBarCanvas.transform.position = transform.position + canvasRelativePos;
            
            if (useBillboard && Camera.main != null)
            {
                float camY = Camera.main.transform.eulerAngles.y;
                healthBarCanvas.transform.rotation = Quaternion.Euler(0f, camY, 0f);
            }
            else
            {
                healthBarCanvas.transform.rotation = Quaternion.identity;
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    /// <summary>Wird vom NPC aufgerufen wenn er den Spieler trifft.</summary>
    public void NimmSchaden(float schaden)
    {
        if (istTot) return;

        aktuellesLeben -= schaden;
        aktuellesLeben = Mathf.Clamp(aktuellesLeben, 0, maxLeben);

        Debug.Log($"Spieler nimmt {schaden} Schaden. Leben: {aktuellesLeben}/{maxLeben}");

        // UI aktualisieren
        if (healthBar != null)
        {
            healthBar.value = aktuellesLeben;
            
            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine);
            }
            flashCoroutine = StartCoroutine(SchadenEffekt());
        }

        if (aktuellesLeben <= 0)
            Sterben();
    }

    // ─────────────────────────────────────────────────────────────────────────
    /// <summary>Heilt den Spieler.</summary>
    public void Heilen(float betrag)
    {
        if (istTot) return;
        aktuellesLeben = Mathf.Clamp(aktuellesLeben + betrag, 0, maxLeben);
        
        if (healthBar != null)
            healthBar.value = aktuellesLeben;
    }

    // ─────────────────────────────────────────────────────────────────────────
    private IEnumerator SchadenEffekt()
    {
        Image sliderFill = healthBar.fillRect.GetComponent<Image>();
        if (sliderFill == null)
            yield break;

        float elapsed = 0f;
        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / flashDuration;
            sliderFill.color = Color.Lerp(originalSliderColor, damageFlashColor, t);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            sliderFill.color = Color.Lerp(damageFlashColor, originalSliderColor, t);
            yield return null;
        }

        sliderFill.color = originalSliderColor;
    }

    private void Sterben()
    {
        if (istTot) return;
        istTot = true;

        Debug.Log("Spieler ist gestorben!");
        
        // Triggere Death Animation
        if (animator != null)
            animator.SetTrigger("Death");
        
        OnTod?.Invoke();

        // Hier: Game-Over-Screen, Respawn etc. einbauen
    }

    // ─────────────────────────────────────────────────────────────────────────
    public float LebensProzent() => aktuellesLeben / maxLeben;
    public float GetHealth() => aktuellesLeben;
    public float GetMaxHealth() => maxLeben;
}
