using UnityEngine;

public class BulletDamage : MonoBehaviour
{
    [Header("Damage Settings")]
    public float danno = 20f;
    public float raggioDanno = 0.0f;  // 0 = solo impatto diretto, > 0 = danno ad area
    public LayerMask npcLayer;        // Layer degli NPC (imposta da inspector)
    
    [Header("Damage Modifiers")]
    public bool dannoVariabile = false;      // Se abilitato, il danno varia in base alla distanza
    public float dannoMassimo = 40f;         // Danno a distanza ravvicinata
    public float dannoMinimo = 10f;          // Danno a lunga distanza
    public float distanzaMassimaDanno = 50f; // Distanza oltre la quale il danno è minimo
    
    private Rigidbody rb;
    private Vector3 ultimaVelocita;
    
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    
    private void Update()
    {
        // Memorizza la velocità ad ogni frame per il calcolo della direzione d'impatto
        if (rb != null)
        {
            ultimaVelocita = rb.linearVelocity;
        }
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        // Calcola il punto di impatto
        Vector3 puntoImpatto = collision.contacts[0].point;
        
        // Se il colpito è un NPC, infliggi danno
        SimplifiedNPC npc = collision.gameObject.GetComponent<SimplifiedNPC>();
        if (npc != null)
        {
            // Calcola il danno effettivo
            float dannoEffettivo = danno;
            
            // Se il danno è variabile in base alla distanza
            if (dannoVariabile && transform.parent != null) // transform.parent = arma che spara
            {
                float distanza = Vector3.Distance(transform.parent.position, puntoImpatto);
                dannoEffettivo = Mathf.Lerp(dannoMassimo, dannoMinimo, distanza / distanzaMassimaDanno);
            }
            
            // Infliggi danno
            npc.RiceviDanno(dannoEffettivo, puntoImpatto);
        }
        
        // Se c'è danno ad area, cerca altri NPC vicini
        if (raggioDanno > 0)
        {
            Collider[] colliders = Physics.OverlapSphere(puntoImpatto, raggioDanno, npcLayer);
            
            foreach (Collider nearbyObject in colliders)
            {
                // Salta l'oggetto già colpito direttamente
                if (nearbyObject.gameObject == collision.gameObject) continue;
                
                // Cerca componente NPC
                SimplifiedNPC nearbyNPC = nearbyObject.GetComponent<SimplifiedNPC>();
                
                if (nearbyNPC != null)
                {
                    // Calcola danno in base alla distanza dall'esplosione
                    float distanza = Vector3.Distance(puntoImpatto, nearbyObject.transform.position);
                    float percentualeDanno = 1.0f - (distanza / raggioDanno);
                    float dannoProssimita = danno * percentualeDanno;
                    
                    // Infliggi danno
                    nearbyNPC.RiceviDanno(dannoProssimita, nearbyObject.transform.position);
                }
            }
        }
        
        // Non distruggere il proiettile qui, lascia che lo faccia lo script Bullet
    }
}
