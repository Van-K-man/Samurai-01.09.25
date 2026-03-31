using UnityEngine;

/// <summary>
/// NPC Enemy AI – Echtzeit Nahkampf (3D Third-Person)
/// Benötigt: Animator, HealthBar, Collider auf demselben GameObject
/// </summary>
[RequireComponent(typeof(Animator))]
public class EnemyAI : MonoBehaviour
{
    private Rigidbody rb;
    [Header("Referenzen")]
    public Transform player;

    [Header("Erkennung")]
    public float sichtweite = 15f;
    public float angriffweite = 1.5f;  // Näher heran zum Treffen

    [Header("Angriff")]
    public float angriffSchaden = 15f;
    public float angriffCooldown = 1.5f;
    public float minAngriffsAbstand = 1.2f;  // Minimaler Abstand bevor NPC stoppt

    [Header("Bewegung")]
    public float patrouilleRadius = 8f;
    public float patrouilleWartezeit = 3f;
    public float laufgeschwindigkeit = 5f;
    public float verfolgungsgeschwindigkeit = 7f;  // Schneller beim Verfolgen

    [Header("Rotation")]
    public float rotationsgeschwindigkeit = 5f;

    private Animator anim;
    private EnemyHealth health;
    private Vector3 aktuelleGeschwindigkeit = Vector3.zero;
    private Vector3 zielPosition = Vector3.zero;

    private float angriffTimer = 0f;
    private float patrouilleTimer = 0f;
    private Vector3 startPosition;

    private enum Zustand { Patrouille, Verfolgen, Angreifen, Tot }
    private Zustand zustand = Zustand.Patrouille;

    private const string PARAM_SPEED    = "Speed";
    private const string PARAM_ANGRIFF  = "Attack01";
    private const string PARAM_TOT      = "Death";

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("[EnemyAI] Kein Rigidbody gefunden! Bitte Rigidbody-Komponente hinzufügen.");
        }
        anim         = GetComponent<Animator>();
        health       = GetComponent<EnemyHealth>();
        startPosition = transform.position;
        zielPosition = startPosition;

        // ✅ FindGameObjectWithTag nur wenn player nicht zugewiesen wurde
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
        }

        if (health != null)
            health.OnTod += HandleTod;
    }

    void Update()
    {
        if (zustand == Zustand.Tot) return;

        float distanzZumSpieler = player != null
            ? Vector3.Distance(transform.position, player.position)
            : float.MaxValue;

        if (distanzZumSpieler <= angriffweite)
            zustand = Zustand.Angreifen;
        else if (distanzZumSpieler <= sichtweite)
            zustand = Zustand.Verfolgen;
        else
            zustand = Zustand.Patrouille;

        switch (zustand)
        {
            case Zustand.Patrouille:   Patrouillieren(); break;
            case Zustand.Verfolgen:    Verfolgen();      break;
            case Zustand.Angreifen:    Angreifen();      break;
        }

        // Bewegung erfolgt jetzt in FixedUpdate via Rigidbody
        anim.SetFloat(PARAM_SPEED, aktuelleGeschwindigkeit.magnitude);
    }

    void FixedUpdate()
    {
        if (zustand == Zustand.Tot) return;
        if (rb != null)
        {
            // Nur bewegen, wenn Geschwindigkeit gesetzt ist
            if (aktuelleGeschwindigkeit.sqrMagnitude > 0.0001f)
            {
                Vector3 zielPos = rb.position + aktuelleGeschwindigkeit * Time.fixedDeltaTime;
                rb.MovePosition(zielPos);
            }
        }
    }

    void Patrouillieren()
    {
        patrouilleTimer += Time.deltaTime;

        // Wenn Zeit um oder Ziel erreicht: Neues Ziel suchen
        if (patrouilleTimer >= patrouilleWartezeit || Vector3.Distance(transform.position, zielPosition) < 0.5f)
        {
            patrouilleTimer = 0f;
            zielPosition = startPosition + (Vector3)(Random.insideUnitCircle * patrouilleRadius);
            zielPosition.y = transform.position.y;
        }

        // Bewege dich zum Ziel
        Vector3 richtung = (zielPosition - transform.position);
        if (richtung.magnitude > 0.01f)
        {
            richtung = richtung.normalized;
            aktuelleGeschwindigkeit = richtung * laufgeschwindigkeit;
            
            // Drehe dich zum Ziel
            Quaternion zielRotation = Quaternion.LookRotation(richtung);
            transform.rotation = Quaternion.Slerp(transform.rotation, zielRotation, Time.deltaTime * rotationsgeschwindigkeit);
        }
        else
        {
            aktuelleGeschwindigkeit = Vector3.zero;
        }
    }

    void Verfolgen()
    {
        if (player == null) return;
        
        // Berechne Richtung zum Spieler (direkt ohne Lerp!)
        Vector3 richtung = (player.position - transform.position);
        float distanz = richtung.magnitude;
        
        if (distanz > 0.01f)
        {
            richtung = richtung.normalized;
            
            // ✅ SCHNELLE BEWEGUNG: Setze direkt die Geschwindigkeit
            aktuelleGeschwindigkeit = richtung * verfolgungsgeschwindigkeit;
            
            // Drehe dich zum Spieler
            Vector3 rotRichtung = richtung;
            rotRichtung.y = 0;
            if (rotRichtung.sqrMagnitude > 0.01f)
            {
                Quaternion zielRotation = Quaternion.LookRotation(rotRichtung);
                transform.rotation = Quaternion.Slerp(transform.rotation, zielRotation, Time.deltaTime * rotationsgeschwindigkeit);
            }
        }
    }

    void Angreifen()
    {
        if (player == null) return;
        
        float distanzZumSpieler = Vector3.Distance(transform.position, player.position);
        
        // ✅ Laufe näher heran, wenn noch nicht ganz nah dran
        if (distanzZumSpieler > minAngriffsAbstand)
        {
            Vector3 richtung = (player.position - transform.position).normalized;
            aktuelleGeschwindigkeit = richtung * laufgeschwindigkeit;
        }
        else
        {
            // Stoppe, wenn nah genug
            aktuelleGeschwindigkeit = Vector3.zero;
        }

        // Drehe dich zum Spieler
        Vector3 rotRichtung = (player.position - transform.position).normalized;
        rotRichtung.y = 0;
        
        if (rotRichtung.sqrMagnitude > 0.01f)
        {
            Quaternion zielRotation = Quaternion.LookRotation(rotRichtung);
            transform.rotation = Quaternion.Slerp(transform.rotation, zielRotation, Time.deltaTime * rotationsgeschwindigkeit);
        }

        // ✅ Hit-Check: Nur Schaden machen, wenn wirklich nah genug
        angriffTimer += Time.deltaTime;
        if (angriffTimer >= angriffCooldown && distanzZumSpieler <= angriffweite + 0.3f)
        {
            angriffTimer = 0f;
            anim.SetTrigger(PARAM_ANGRIFF);

            PlayerHealth ph = player.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                ph.NimmSchaden(angriffSchaden);
                Debug.Log($"[EnemyAI] TREFFER! Distanz: {distanzZumSpieler:F2} <= {angriffweite + 0.3f:F2}");
            }
        }
    }

    void HandleTod()
    {
        zustand = Zustand.Tot;
        aktuelleGeschwindigkeit = Vector3.zero;

        if (anim != null)
            anim.SetTrigger(PARAM_TOT);

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // NPC bleibt in der Szene liegen und wird nicht zerstört
        // Destroy(gameObject, 3f);
    }


    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sichtweite);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, angriffweite);
    }
}