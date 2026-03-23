using UnityEngine;

public class CharacterCombat : MonoBehaviour
{
    [SerializeField] private KeyCode attack01Key = KeyCode.LeftArrow;     // Attack 01
    [SerializeField] private KeyCode attack02Key = KeyCode.RightArrow;    // Attack 02
    [SerializeField] private KeyCode attack03Key = KeyCode.UpArrow;       // Attack 03
    [SerializeField] private KeyCode attack04Key = KeyCode.DownArrow;     // Attack 04
    [SerializeField] private KeyCode parryKey = KeyCode.Alpha0;           // Parry
    [SerializeField] private float attack01Duration = 1.0f;
    [SerializeField] private float attack02Duration = 1.0f;
    [SerializeField] private float attack03Duration = 1.2f;
    [SerializeField] private float attack04Duration = 1.2f;
    [SerializeField] private float parryDuration = 1.0f;

    public Animator animator;
    public bool useAnimator = true;

    private bool isAttacking = false;
    private float lastAttackTime = 0f;

    void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    void Update()
    {
        HandleAttackInput();
    }

    private void HandleAttackInput()
    {
        float timeSinceLastAttack = Time.time - lastAttackTime;

        // Parry-Input
        if (Input.GetKeyDown(parryKey) && timeSinceLastAttack >= parryDuration)
        {
            Debug.Log("Parry gedrückt");
            TriggerParry();
            return;
        }

        // Attack03-Input
        if (Input.GetKeyDown(attack03Key) && timeSinceLastAttack >= attack03Duration)
        {
            Debug.Log("Attack03 gedrückt");
            TriggerAttack03();
            return;
        }

        // Attack04-Input
        if (Input.GetKeyDown(attack04Key) && timeSinceLastAttack >= attack04Duration)
        {
            Debug.Log("Attack04 gedrückt");
            TriggerAttack04();
            return;
        }

        // Attack01-Input (Linke Pfeiltaste)
        if (Input.GetKeyDown(attack01Key) && timeSinceLastAttack >= attack01Duration)
        {
            Debug.Log("Attack01 gedrückt");
            TriggerAttack01();
            return;
        }

        // Attack02-Input (Rechte Pfeiltaste)
        if (Input.GetKeyDown(attack02Key) && timeSinceLastAttack >= attack02Duration)
        {
            Debug.Log("Attack02 gedrückt");
            TriggerAttack02();
            return;
        }
    }

    private void TriggerAttack01()
    {
        isAttacking = true;
        lastAttackTime = Time.time;
        if (useAnimator && animator != null)
            animator.SetTrigger("Attack01");
    }

    private void TriggerAttack02()
    {
        isAttacking = true;
        lastAttackTime = Time.time;
        if (useAnimator && animator != null)
            animator.SetTrigger("Attack02");
    }

    private void TriggerAttack03()
    {
        isAttacking = true;
        lastAttackTime = Time.time;
        if (useAnimator && animator != null)
            animator.SetTrigger("Attack03");
    }

    private void TriggerAttack04()
    {
        isAttacking = true;
        lastAttackTime = Time.time;
        if (useAnimator && animator != null)
            animator.SetTrigger("Attack04");
    }

    private void TriggerParry()
    {
        isAttacking = true;
        lastAttackTime = Time.time;
        if (useAnimator && animator != null)
            animator.SetTrigger("Parry");
    }

    // Combo-Fenster öffnen (per AnimationEvent)
    public void OnAttack01ComboWindow()
    {
        // Combo-System entfernt
    }

    private void CloseComboWindow()
    {
        // Combo-System entfernt
    }

    // Animation Events
    public void OnAttack01End()
    {
        isAttacking = false;
    }

    public void OnAttack02End()
    {
        isAttacking = false;
    }

    public void OnAttack03End()
    {
        isAttacking = false;
    }

    public void OnAttack04End()
    {
        isAttacking = false;
    }

    public void OnParryEnd()
    {
        isAttacking = false;
    }
}