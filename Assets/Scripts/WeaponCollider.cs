using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Weapon / Hitbox für Spieler-Angriffe
/// Triggert automatisch während Angriffs-Animationen und verursacht Schaden
/// </summary>
public class WeaponCollider : MonoBehaviour
{
    [SerializeField] private float attackDamage = 15f;
    [SerializeField] private bool debugMode = true;

    private Collider weaponCollider;
    private HashSet<Collider> hitTargetsThisAttack = new HashSet<Collider>(); // Verhindert mehrfache Treffer pro Angriff
    private bool isAttacking = false;

    void Start()
    {
        weaponCollider = GetComponent<Collider>();
        if (weaponCollider == null)
        {
            Debug.LogError("[WeaponCollider] Kein Collider auf dieser Waffe gefunden!");
            return;
        }

        // Stelle sicher, dass Collider als Trigger eingestellt ist
        if (!weaponCollider.isTrigger)
        {
            weaponCollider.isTrigger = true;
            Debug.Log("[WeaponCollider] Collider wurde auf Trigger eingestellt.");
        }

        // Deaktiviere den Collider am Anfang (wird bei Angriff aktiviert)
        weaponCollider.enabled = false;
    }

    // Wird von AnimationEvent aufgerufen, wenn der Angriff aktiv wird
    public void OnAttackStart()
    {
        if (weaponCollider != null)
        {
            isAttacking = true;
            weaponCollider.enabled = true;
            hitTargetsThisAttack.Clear(); // Neue Angriffs-Runde
            
            if (debugMode)
                Debug.Log("[WeaponCollider] Angriff gestartet - Hitbox aktiv");
        }
    }

    // Wird von AnimationEvent aufgerufen, wenn der Angriff endet
    public void OnAttackEnd()
    {
        if (weaponCollider != null)
        {
            isAttacking = false;
            weaponCollider.enabled = false;
            
            if (debugMode)
                Debug.Log("[WeaponCollider] Angriff beendet - Hitbox deaktiviert");
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (!isAttacking || weaponCollider == null) 
            return;

        // Prüfe, ob wir diesen Gegner bereits in diesem Angriff getroffen haben
        if (hitTargetsThisAttack.Contains(other))
            return;

        // Versuche Schaden auf Gegner zu verursachen
        EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.NimmSchaden(attackDamage);
            hitTargetsThisAttack.Add(other);
            if (debugMode)
                Debug.Log($"[WeaponCollider] TREFFER auf {other.gameObject.name}! Schaden: {attackDamage}");
            return;
        }

        // Versuche Schaden auf den Spieler zu verursachen
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.NimmSchaden(attackDamage);
            hitTargetsThisAttack.Add(other);
            if (debugMode)
                Debug.Log($"[WeaponCollider] TREFFER auf {other.gameObject.name}! Schaden: {attackDamage}");
        }
    }

    // Optional: Setze Schaden-Wert
    public void SetAttackDamage(float newDamage)
    {
        attackDamage = newDamage;
    }
}
