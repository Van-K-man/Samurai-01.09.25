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
    [Range(10f, 180f)] public float angriffWinkel = 100f; // Trefferkegel vor dem NPC
    public Transform angriffUrsprung; // Optional: Brust/Waffe als Ursprung

    [Header("Bewegung")]
    public float patrouilleRadius = 8f;
    public float patrouilleWartezeit = 3f;
    public float laufgeschwindigkeit = 5f;
    public float verfolgungsgeschwindigkeit = 7f;  // Schneller beim Verfolgen

    [Header("Rotation")]
    public float rotationsgeschwindigkeit = 5f;

    [Header("Separation")]
    public float separationsRadius = 1.5f;
    public float separationsStaerke = 3f;

    private Animator anim;
    private EnemyHealth health;
    private Vector3 aktuelleGeschwindigkeit = Vector3.zero;
    private Vector3 zielPosition = Vector3.zero;

    private float angriffTimer = 0f;
    private float patrouilleTimer = 0f;
    private Vector3 startPosition;
    private bool attackEventArmed = false;

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
        else
        {
            // Nur Y-Rotation erlauben, X und Z einfrieren– nhält NPCs aufrecht
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }
        anim         = GetComponent<Animator>();
        health       = GetComponent<EnemyHealth>();
        startPosition = transform.position;
        zielPosition = startPosition;

        if (angriffUrsprung == null)
            angriffUrsprung = transform;

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

        // Separation: Abstand zu anderen NPCs einhalten
        aktuelleGeschwindigkeit += BerechneSeperationskraft();

        // Bewegung erfolgt jetzt in FixedUpdate via Rigidbody
        anim.SetFloat(PARAM_SPEED, aktuelleGeschwindigkeit.magnitude);
    }

    void FixedUpdate()
    {
        if (zustand == Zustand.Tot) return;
        if (rb != null)
        {
            // angularVelocity nullen – Rigidbody soll sich nicht drehen
            rb.angularVelocity = Vector3.zero;

            // Verhindere, dass NPCs auf andere Objekte hochklettern
            Vector3 vel = rb.linearVelocity;
            if (vel.y > 0.1f)
            {
                vel.y = 0f;
                rb.linearVelocity = vel;
            }

            // Nur horizontal bewegen – Y-Position bleibt durch Physik bestimmt
            if (aktuelleGeschwindigkeit.sqrMagnitude > 0.0001f)
            {
                Vector3 zielPos = rb.position + aktuelleGeschwindigkeit * Time.fixedDeltaTime;
                zielPos.y = rb.position.y;
                rb.MovePosition(zielPos);
            }
        }
    }

    Vector3 BerechneSeperationskraft()
    {
        Vector3 separation = Vector3.zero;
        Collider[] nachbarn = Physics.OverlapSphere(transform.position, separationsRadius);
        foreach (Collider nachbar in nachbarn)
        {
            if (nachbar.gameObject == gameObject) continue;
            if (!nachbar.TryGetComponent<EnemyAI>(out _)) continue;

            Vector3 wegVektor = transform.position - nachbar.transform.position;
            wegVektor.y = 0f; // Nur horizontal abstoßen, nicht nach oben/unten
            float distanz = wegVektor.magnitude;
            if (distanz > 0.001f)
                separation += wegVektor.normalized * (separationsRadius - distanz) / separationsRadius;
        }
        return separation * separationsStaerke;
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

        // Angriff starten; Schaden wird im Animation Event ausgelöst
        angriffTimer += Time.deltaTime;
        if (angriffTimer >= angriffCooldown && distanzZumSpieler <= angriffweite + 0.3f)
        {
            angriffTimer = 0f;
            attackEventArmed = true;
            anim.SetTrigger(PARAM_ANGRIFF);
        }
    }

    // Wird vom Animation Event aufgerufen
    public void ApplyCurrentAttackDamage()
    {
        if (!attackEventArmed || zustand == Zustand.Tot)
            return;

        attackEventArmed = false;

        if (player == null)
            return;

        float distanzZumSpieler = Vector3.Distance(transform.position, player.position);
        Vector3 origin = angriffUrsprung != null ? angriffUrsprung.position : transform.position;
        Vector3 toPlayer = player.position - origin;
        toPlayer.y = 0f;
        float angleToPlayer = toPlayer.sqrMagnitude > 0.0001f
            ? Vector3.Angle(transform.forward, toPlayer.normalized)
            : 180f;
        bool playerImTrefferkegel = angleToPlayer <= angriffWinkel * 0.5f;

        if (distanzZumSpieler > angriffweite + 0.3f || !playerImTrefferkegel)
            return;

        PlayerHealth ph = player.GetComponent<PlayerHealth>();
        if (ph != null)
        {
            ph.NimmSchaden(angriffSchaden);
            Debug.Log($"[EnemyAI] TREFFER! Distanz: {distanzZumSpieler:F2} | Winkel: {angleToPlayer:F1} <= {angriffWinkel * 0.5f:F1}");
        }
    }

    void HandleTod()
    {
        if (zustand == Zustand.Tot) return; // Doppelaufruf verhindern
        zustand = Zustand.Tot;
        aktuelleGeschwindigkeit = Vector3.zero;

        // Root Motion aktivieren – OnAnimatorMove übernimmt die Y-Kontrolle
        if (anim != null)
        {
            anim.applyRootMotion = true;
            anim.SetTrigger(PARAM_TOT);
        }

        // Velocity nullen, kinematisch machen
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        // NPC bleibt in der Szene liegen und wird nicht zerstört
        // Destroy(gameObject, 3f);
    }

    // Übernimmt Root Motion manuell: XZ-Bewegung erlaubt, Y wird auf den Boden geklemt
    void OnAnimatorMove()
    {
        if (anim == null) return;

        if (zustand == Zustand.Tot)
        {
            Vector3 pos = anim.rootPosition;
            // Y via Raycast auf die tatsächliche Bodenoberfläche setzen
            if (Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, out RaycastHit hit, 3f))
                pos.y = hit.point.y;
            else
                pos.y = transform.position.y;

            transform.position = pos;
            transform.rotation = anim.rootRotation;
        }
        // Im lebenden Zustand: kein Root Motion – Bewegung läuft über FixedUpdate
    }


    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sichtweite);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, angriffweite);
    }
}