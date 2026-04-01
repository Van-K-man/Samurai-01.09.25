using UnityEngine;
using UnityEngine.InputSystem;

public enum DodgeDirection { Forward = 1, Backward = 2, None = -1 }

public class CharacterCombat : MonoBehaviour
{
    [SerializeField] private Key attack01Key = Key.LeftArrow;     // Attack 01
    [SerializeField] private Key attack02Key = Key.RightArrow;    // Attack 02
    [SerializeField] private Key attack03Key = Key.UpArrow;       // Attack 03
    [SerializeField] private Key attack04Key = Key.DownArrow;     // Attack 04
    [SerializeField] private Key parryKey = Key.Digit0;                 // Parry
    [SerializeField] private float attack01Duration = 1.0f;
    [SerializeField] private float attack02Duration = 1.0f;
    [SerializeField] private float attack03Duration = 1.2f;
    [SerializeField] private float attack04Duration = 1.2f;
    [SerializeField] private float parryDuration = 1.0f;

    [Header("Ausweichrolle")]
    public Key dodgeKey = Key.RightCtrl;      // Ausweichrolle
    [SerializeField] private float dodgeDuration = 0.6f;       // Animation
    [SerializeField] private float dodgeCooldown = 1.0f;       // Cooldown
    [SerializeField] private float dodgeSpeed = 8f;            // Bewegungsgeschwindigkeit

    [Header("Schaden")]
    [SerializeField] private float attack01Damage = 15f;
    [SerializeField] private float attack02Damage = 20f;
    [SerializeField] private float attack03Damage = 25f;
    [SerializeField] private float attack04Damage = 30f;
    [SerializeField] private float attackRange = 2.5f;  // Reichweite des Angriffs

    private Animator animator;
    private bool useAnimator = true;

    private float lastAttackTime = 0f;
    private float attackEndTime = 0f;  // Zeitpunkt, wann der Angriff endet
    private string currentAttackType = "";  // Wird genutzt zur Schaden-Berechnung

    private bool isDodging = false;
    private float dodgeEndTime = 0f;
    private float lastDodgeTime = -10f;
    private Vector3 dodgeDirection = Vector3.zero;
    private DodgeDirection currentDodgeDirection = DodgeDirection.None;

    void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (animator != null)
            animator.SetInteger("DodgeDirection", (int)DodgeDirection.None);
    }

    void Update()
    {
        HandleAttackInput();
        
        // Angriff beenden, falls Zeit abgelaufen
        if (attackEndTime > 0 && Time.time >= attackEndTime)
        {
            attackEndTime = 0f;
        }

        // Während Ausweichrolle bewegen
        if (isDodging && Time.time < dodgeEndTime)
        {
            transform.Translate(dodgeDirection * dodgeSpeed * Time.deltaTime, Space.World);
        }
        else if (isDodging && Time.time >= dodgeEndTime)
        {
            isDodging = false;
            currentDodgeDirection = DodgeDirection.None;
            if (useAnimator && animator != null)
                animator.SetInteger("DodgeDirection", (int)DodgeDirection.None);
        }
    }

    private bool IsKeyPressed(Key key, bool wasPressedThisFrame)
    {
        if (Keyboard.current == null)
            return false;

        var keyButton = Keyboard.current[key];
        if (keyButton == null)
            return false;

        return wasPressedThisFrame ? keyButton.wasPressedThisFrame : keyButton.isPressed;
    }

    private void HandleAttackInput()
    {
        float timeSinceLastAttack = Time.time - lastAttackTime;
        bool isAttackActive = attackEndTime > 0 && Time.time < attackEndTime;

        // ✅ Dodge-Input (BLOCKIERT wenn Angriff läuft)
        if (!isAttackActive && IsKeyPressed(dodgeKey, true) && (Time.time - lastDodgeTime) >= dodgeCooldown && !isDodging)
        {
            DodgeDirection direction = GetDodgeDirection();
            // Kein Richtungsinput: Standard rückwärts ausweichen
            if (direction == DodgeDirection.None)
                direction = DodgeDirection.Backward;

            Debug.Log($"Dodge gedrückt: {direction}");
            TriggerDodge(direction);
            return;
        }

        // Diagnose-Log: Warum wird Dodge blockiert?
        if (IsKeyPressed(dodgeKey, true))
        {
            Debug.Log($"[Dodge BLOCKIERT] isAttackActive={isAttackActive} | Cooldown verbleibend={dodgeCooldown - (Time.time - lastDodgeTime):F2}s | isDodging={isDodging} | animator={animator != null} | useAnimator={useAnimator}");
        }

        // Wenn Angriff aktiv: keine neuen Angriffe möglich
        if (isAttackActive)
            return;

        // Parry-Input
        if (IsKeyPressed(parryKey, true) && timeSinceLastAttack >= parryDuration)
        {
            Debug.Log("Parry gedrückt");
            TriggerParry();
            return;
        }

        // Attack03-Input
        if (IsKeyPressed(attack03Key, true) && timeSinceLastAttack >= attack03Duration)
        {
            Debug.Log("Attack03 gedrückt");
            TriggerAttack03();
            return;
        }

        // Attack04-Input
        if (IsKeyPressed(attack04Key, true) && timeSinceLastAttack >= attack04Duration)
        {
            Debug.Log("Attack04 gedrückt");
            TriggerAttack04();
            return;
        }

        // Attack01-Input (Linke Pfeiltaste)
        if (IsKeyPressed(attack01Key, true) && timeSinceLastAttack >= attack01Duration)
        {
            Debug.Log("Attack01 gedrückt");
            TriggerAttack01();
            return;
        }

        // Attack02-Input (Rechte Pfeiltaste)
        if (IsKeyPressed(attack02Key, true) && timeSinceLastAttack >= attack02Duration)
        {
            Debug.Log("Attack02 gedrückt");
            TriggerAttack02();
            return;
        }
    }

    private void TriggerAttack01()
    {
        lastAttackTime = Time.time;
        attackEndTime = Time.time + attack01Duration;  // ✅ Setze Ende des Angriffs
        currentAttackType = "Attack01";
        
        if (useAnimator && animator != null)
        {
            animator.SetTrigger("Attack01");
        }
        
        // Verursache Schaden auf Gegner im Bereich
        ApplyAttackDamage(attack01Damage);
    }

    private void TriggerAttack02()
    {
        lastAttackTime = Time.time;
        attackEndTime = Time.time + attack02Duration;  // ✅ Setze Ende des Angriffs
        currentAttackType = "Attack02";
        
        if (useAnimator && animator != null)
        {
            animator.SetTrigger("Attack02");
        }
        
        // Verursache Schaden auf Gegner im Bereich
        ApplyAttackDamage(attack02Damage);
    }

    private void TriggerAttack03()
    {
        lastAttackTime = Time.time;
        attackEndTime = Time.time + attack03Duration;  // ✅ Setze Ende des Angriffs
        currentAttackType = "Attack03";
        
        if (useAnimator && animator != null)
        {
            animator.SetTrigger("Attack03");
        }
        
        // Verursache Schaden auf Gegner im Bereich
        ApplyAttackDamage(attack03Damage);
    }

    private void TriggerAttack04()
    {
        lastAttackTime = Time.time;
        attackEndTime = Time.time + attack04Duration;  // ✅ Setze Ende des Angriffs
        currentAttackType = "Attack04";
        
        if (useAnimator && animator != null)
        {
            animator.SetTrigger("Attack04");
        }
        
        // Verursache Schaden auf Gegner im Bereich
        ApplyAttackDamage(attack04Damage);
    }

    private void TriggerParry()
    {
        lastAttackTime = Time.time;
        attackEndTime = Time.time + parryDuration;  // ✅ Setze Ende der Pariere
        if (useAnimator && animator != null)
        {
            animator.SetTrigger("Parry");
        }
    }

    private DodgeDirection GetDodgeDirection()
    {
        if (Keyboard.current == null)
            return DodgeDirection.None;

        bool wPressed = Keyboard.current.wKey.isPressed;
        bool aPressed = Keyboard.current.aKey.isPressed;
        bool sPressed = Keyboard.current.sKey.isPressed;
        bool dPressed = Keyboard.current.dKey.isPressed;

        // WASD alle → Forward, keine → None (wird zu Backward)
        if (wPressed || aPressed || sPressed || dPressed)
            return DodgeDirection.Forward;

        return DodgeDirection.None;
    }

    private void TriggerDodge(DodgeDirection direction)
    {
        isDodging = true;
        lastDodgeTime = Time.time;
        dodgeEndTime = Time.time + dodgeDuration;
        currentDodgeDirection = direction;

        // Bewegungsrichtung basierend auf Enum
        dodgeDirection = direction switch
        {
            DodgeDirection.Forward => Vector3.forward,
            DodgeDirection.Backward => Vector3.back,
            _ => Vector3.zero
        };

        if (useAnimator && animator != null)
        {
            animator.SetInteger("DodgeDirection", (int)direction);
            animator.SetTrigger("DodgeTrigger");
        }
    }

    // Combo-Fenster �ffnen (per AnimationEvent)
    public void OnAttack01ComboWindow()
    {
        // Combo-System entfernt
    }

    private void CloseComboWindow()
    {
        // Combo-System entfernt
    }

    // Animation Events (können für zukünftige Logik verwendet werden)
    public void OnAttack01End() { }
    public void OnAttack02End() { }
    public void OnAttack03End() { }
    public void OnAttack04End() { }
    public void OnParryEnd() { }

    // Verursache Schaden auf alle Gegner im Angriffs-Bereich
    private void ApplyAttackDamage(float damage)
    {
        Collider[] hitsInRange = Physics.OverlapSphere(transform.position, attackRange);
        
        foreach (Collider hit in hitsInRange)
        {
            if (hit.gameObject == gameObject)
                continue;

            // ✅ Kein Schaden wenn gerade am Dodgen
            if (isDodging)
                continue;

            EnemyHealth enemyHealth = hit.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.NimmSchaden(damage);
                Debug.Log($"[CharacterCombat] {currentAttackType} TREFFER auf {hit.gameObject.name}! Schaden: {damage}");
                continue;
            }

            PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.NimmSchaden(damage);
                Debug.Log($"[CharacterCombat] {currentAttackType} TREFFER auf {hit.gameObject.name}! Schaden: {damage}");
            }
        }
    }
}