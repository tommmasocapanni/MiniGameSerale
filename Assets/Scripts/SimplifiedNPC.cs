using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class SimplifiedNPC : MonoBehaviour
{
    [Header("Movimento")]
    public float velocitaMovimento = 3.5f;
    public float raggioMovimento = 10f;
    public float tempoFermo = 3.0f;
    public float distanzaArresto = 0.1f;
    public bool muovitiAllAvvio = true;

    [Header("Animazione")]
    public string parametroWalking = "IsWalking";
    public float velocitaAnimazione = 1.0f;
    
    [Header("Anti-Blocco")]
    public bool correzioneAutomatica = true;
    public float tempoRilevazioneBloccato = 2.0f;
    public float distanzaMinimaSpostamento = 0.3f;
    
    [Header("Sistema Esplorazione")]
    public bool esplorazioneAttiva = true;          // Attiva il nuovo sistema di esplorazione
    public float distanzaRilevaOstacoli = 2.0f;     // Distanza per rilevare ostacoli davanti
    public float angoloScansioneOstacoli = 60.0f;   // Angolo di scansione per gli ostacoli
    public int numeroRaggi = 5;                     // Numero di raggi per il rilevamento
    public LayerMask layerOstacoli;                 // Layer degli ostacoli da evitare
    public bool disegnaSensori = false;             // Disegna i raggi di rilevamento

    [Header("Debug")]
    public bool mostraDebug = false;
    
    // Componenti
    private NavMeshAgent agente;
    private Animator animatore;
    private Transform miaTrasformata;
    
    // Stato
    private bool inAttesa = false;
    private float timerAttesa = 0f;
    private Vector3 ultimaPosizione;
    private float timerRilevazioneBloccato = 0f;
    private int contatoreBloccaggi = 0;
    private bool isGestendoBlocco = false;
    
    private void Awake()
    {
        // Ottieni riferimenti ai componenti
        agente = GetComponent<NavMeshAgent>();
        animatore = GetComponent<Animator>();
        miaTrasformata = transform;
        ultimaPosizione = miaTrasformata.position;
        
        // Verifica che i componenti necessari siano presenti
        if (agente == null || animatore == null)
        {
            Debug.LogError("SimplifiedNPC richiede NavMeshAgent e Animator!");
            enabled = false;
            return;
        }
        
        // Configura i parametri base
        ConfiguraParametriBase();
    }
    
    private void ConfiguraParametriBase()
    {
        // Configurazione NavMeshAgent
        agente.speed = velocitaMovimento;
        agente.stoppingDistance = distanzaArresto;
        agente.acceleration = 8.0f;
        agente.angularSpeed = 120.0f;
        agente.autoBraking = true;
        
        // Configurazione Animator
        animatore.applyRootMotion = false;
        animatore.speed = velocitaAnimazione;
    }
    
    private void Start()
    {
        // Verifica che l'NPC sia sulla NavMesh
        if (!agente.isOnNavMesh)
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(miaTrasformata.position, out hit, 5.0f, NavMesh.AllAreas))
            {
                miaTrasformata.position = hit.position;
                if (mostraDebug) Debug.Log("NPC posizionato sulla NavMesh più vicina");
            }
            else
            {
                Debug.LogError("NPC non trovato sulla NavMesh e non è possibile posizionarlo!");
                enabled = false;
                return;
            }
        }
        
        // Inizia il movimento se richiesto
        if (muovitiAllAvvio)
        {
            ImpostaNuovaDestinazione();
        }
        else
        {
            inAttesa = true;
            timerAttesa = tempoFermo;
        }
    }
    
    private void Update()
    {
        if (isGestendoBlocco) return;
        
        // Gestisci lo stato corrente
        if (inAttesa)
        {
            GestisciStatoAttesa();
        }
        else
        {
            GestisciStatoMovimento();
            
            // Sistema di rilevamento ostacoli ed esplorazione
            if (esplorazioneAttiva && !isGestendoBlocco)
            {
                ScansionaEEvitaOstacoli();
            }
            
            // Sistema anti-blocco tradizionale (ora meno aggressivo)
            if (correzioneAutomatica)
            {
                ControllaBloccaggio();
            }
        }
    }
    
    private void GestisciStatoAttesa()
    {
        timerAttesa -= Time.deltaTime;
        
        if (timerAttesa <= 0)
        {
            ImpostaNuovaDestinazione();
        }
    }
    
    private void GestisciStatoMovimento()
    {
        // Controlla se è arrivato a destinazione
        if (!agente.pathPending && agente.remainingDistance <= agente.stoppingDistance)
        {
            IniziaAttesa();
        }
    }
    
    private void ControllaBloccaggio()
    {
        // Controlla solo se l'NPC si sta muovendo
        if (agente.hasPath && agente.remainingDistance > agente.stoppingDistance)
        {
            timerRilevazioneBloccato += Time.deltaTime;
            
            // Controlla solo periodicamente, ora con un tempo più lungo
            if (timerRilevazioneBloccato >= tempoRilevazioneBloccato)
            {
                // Calcola quanto si è spostato
                float distanzaPercorsa = Vector3.Distance(miaTrasformata.position, ultimaPosizione);
                
                // Si è spostato abbastanza?
                if (distanzaPercorsa < distanzaMinimaSpostamento)
                {
                    contatoreBloccaggi++;
                    
                    // Ora solo se il blocco è persistente, intervieni
                    if (contatoreBloccaggi >= 2) 
                    {
                        StartCoroutine(GestisciBlocco());
                    }
                }
                else
                {
                    // Reset del contatore per blocchi non consecutivi
                    contatoreBloccaggi = 0;
                }
                
                // Reset per il prossimo controllo
                ultimaPosizione = miaTrasformata.position;
                timerRilevazioneBloccato = 0;
            }
        }
        else
        {
            timerRilevazioneBloccato = 0;
            ultimaPosizione = miaTrasformata.position;
            contatoreBloccaggi = 0;
        }
    }
    
    private void ImpostaNuovaDestinazione()
    {
        // Reset stato
        inAttesa = false;
        
        // Cerca una nuova destinazione casuale - aumentiamo la casualità
        Vector3 direzionePreferita = Random.insideUnitSphere.normalized;
        direzionePreferita.y = 0;  // Manteniamo lo stesso piano
        
        for (int i = 0; i < 30; i++)
        {
            // Maggiore casualità nella distanza
            float distanzaCasuale = Random.Range(5f, raggioMovimento);
            Vector3 posizioneRandom = miaTrasformata.position + direzionePreferita * distanzaCasuale;
            
            NavMeshHit hit;
            if (NavMesh.SamplePosition(posizioneRandom, out hit, raggioMovimento, NavMesh.AllAreas))
            {
                // Prima ferma e resetta l'agente
                agente.isStopped = true;
                agente.ResetPath();
                
                // Poi imposta la nuova destinazione
                agente.isStopped = false;
                agente.SetDestination(hit.position);
                
                // Attiva l'animazione di camminata
                animatore.SetBool(parametroWalking, true);
                
                if (mostraDebug)
                    Debug.Log($"Nuova destinazione esplorativa: {hit.position}");
                
                return;
            }
            
            // Cambia direzione ad ogni tentativo fallito
            direzionePreferita = Random.insideUnitSphere.normalized;
            direzionePreferita.y = 0;
        }
        
        // Se non riesce a trovare una destinazione valida, attende
        IniziaAttesa();
    }
    
    private void IniziaAttesa()
    {
        inAttesa = true;
        timerAttesa = tempoFermo;
        
        // Ferma l'agente
        agente.isStopped = true;
        agente.ResetPath();
        
        // Disattiva l'animazione di camminata
        animatore.SetBool(parametroWalking, false);
    }
    
    private IEnumerator GestisciBlocco()
    {
        isGestendoBlocco = true;
        
        if (mostraDebug)
            Debug.Log($"Rilevato blocco #{contatoreBloccaggi}. Tentativo di sblocco...");
        
        // Ferma brevemente per resettare
        agente.isStopped = true;
        animatore.SetBool(parametroWalking, false);
        
        yield return new WaitForSeconds(0.2f);
        
        // Se è bloccato ripetutamente, teletrasporta
        if (contatoreBloccaggi >= 3)
        {
            TeletrasportaNPCInAvanti();
            contatoreBloccaggi = 0;
        }
        else
        {
            // Altrimenti ricalcola solo il percorso
            RicalcolaPercorso();
        }
        
        // Riattiva l'NPC
        agente.isStopped = false;
        animatore.SetBool(parametroWalking, true);
        
        isGestendoBlocco = false;
    }
    
    private void RicalcolaPercorso()
    {
        // Prova con una leggera variazione della destinazione
        if (agente.destination != null)
        {
            Vector3 nuovaDest = agente.destination + Random.insideUnitSphere * 1.0f;
            nuovaDest.y = agente.destination.y;
            
            agente.ResetPath();
            agente.SetDestination(nuovaDest);
        }
        else
        {
            // Se non c'è una destinazione, imposta una nuova
            ImpostaNuovaDestinazione();
        }
    }
    
    private void TeletrasportaNPCInAvanti()
    {
        // Cerca un punto valido davanti all'NPC
        Vector3 direzioneAvanti = miaTrasformata.forward * 2.0f;
        Vector3 nuovaPosizione = miaTrasformata.position + direzioneAvanti;
        
        NavMeshHit hit;
        if (NavMesh.SamplePosition(nuovaPosizione, out hit, 3.0f, NavMesh.AllAreas))
        {
            agente.ResetPath();
            miaTrasformata.position = hit.position;
            
            if (mostraDebug)
                Debug.Log($"NPC teletrasportato in avanti a {hit.position}");
            
            // Dopo il teletrasporto, imposta una nuova destinazione
            Invoke("ImpostaNuovaDestinazione", 0.5f);
        }
        else
        {
            // Se non trova un punto valido davanti, prova in altre direzioni
            for (int i = 0; i < 4; i++)
            {
                Vector3 direzioneCasuale = Quaternion.Euler(0, 90 * i, 0) * Vector3.forward * 2.0f;
                nuovaPosizione = miaTrasformata.position + direzioneCasuale;
                
                if (NavMesh.SamplePosition(nuovaPosizione, out hit, 3.0f, NavMesh.AllAreas))
                {
                    agente.ResetPath();
                    miaTrasformata.position = hit.position;
                    
                    if (mostraDebug)
                        Debug.Log($"NPC teletrasportato in direzione alternativa a {hit.position}");
                    
                    Invoke("ImpostaNuovaDestinazione", 0.5f);
                    return;
                }
            }
            
            // Se tutto fallisce, prova a impostare una nuova destinazione da dove si trova
            ImpostaNuovaDestinazione();
        }
    }
    
    // Nuovo metodo per rilevare ed evitare ostacoli in modo più naturale
    private void ScansionaEEvitaOstacoli()
    {
        if (!agente.hasPath) return;
        
        bool ostacoloRilevato = false;
        Vector3 direzioneEvitamento = Vector3.zero;
        float distanzaOstacoloRilevato = float.MaxValue;
        
        // Raggio centrale - ostacolo direttamente davanti
        if (Physics.Raycast(miaTrasformata.position + Vector3.up * 0.5f, miaTrasformata.forward, 
                           out RaycastHit hitInfo, distanzaRilevaOstacoli, layerOstacoli))
        {
            ostacoloRilevato = true;
            distanzaOstacoloRilevato = hitInfo.distance;
            direzioneEvitamento = -miaTrasformata.forward; // Inizialmente allontanati
            
            if (disegnaSensori)
                Debug.DrawRay(miaTrasformata.position + Vector3.up * 0.5f, miaTrasformata.forward * hitInfo.distance, Color.red);
        } 
        else if (disegnaSensori)
        {
            Debug.DrawRay(miaTrasformata.position + Vector3.up * 0.5f, miaTrasformata.forward * distanzaRilevaOstacoli, Color.green);
        }
        
        // Raggi laterali per trovare percorsi alternativi
        if (numeroRaggi > 1)
        {
            float incrementoAngolo = angoloScansioneOstacoli / (numeroRaggi - 1);
            
            // Controlla a destra e sinistra per trovare percorsi liberi
            for (int i = 0; i < numeroRaggi; i++)
            {
                if (i == numeroRaggi / 2) continue; // Salta la direzione centrale (già controllata)
                
                float angolo = -angoloScansioneOstacoli / 2 + i * incrementoAngolo;
                Vector3 direzione = Quaternion.Euler(0, angolo, 0) * miaTrasformata.forward;
                
                if (Physics.Raycast(miaTrasformata.position + Vector3.up * 0.5f, direzione, 
                                  out RaycastHit lateralHit, distanzaRilevaOstacoli, layerOstacoli))
                {
                    if (disegnaSensori)
                        Debug.DrawRay(miaTrasformata.position + Vector3.up * 0.5f, direzione * lateralHit.distance, Color.yellow);
                }
                else
                {
                    // Direzione libera trovata - considera come possibile via di fuga
                    if (ostacoloRilevato)
                    {
                        direzioneEvitamento += direzione;
                        
                        if (disegnaSensori)
                            Debug.DrawRay(miaTrasformata.position + Vector3.up * 0.5f, direzione * distanzaRilevaOstacoli, Color.blue);
                    }
                    else if (disegnaSensori)
                    {
                        Debug.DrawRay(miaTrasformata.position + Vector3.up * 0.5f, direzione * distanzaRilevaOstacoli, Color.green);
                    }
                }
            }
        }
        
        // Se abbiamo rilevato un ostacolo, cambia direzione
        if (ostacoloRilevato)
        {
            if (distanzaOstacoloRilevato < 1.0f) // Ostacolo molto vicino
            {
                // Normalizza la direzione di evitamento
                if (direzioneEvitamento != Vector3.zero)
                {
                    direzioneEvitamento.Normalize();
                    TrovaNuovaDestinazioneDirezionale(direzioneEvitamento);
                }
                else
                {
                    // Se non abbiamo una direzione chiara, prova una rotazione casuale
                    float angoloRandom = Random.Range(-120f, 120f);
                    Vector3 nuovaDirezione = Quaternion.Euler(0, angoloRandom, 0) * miaTrasformata.forward;
                    TrovaNuovaDestinazioneDirezionale(nuovaDirezione);
                }
            }
        }
    }
    
    // Nuovo metodo per trovare una destinazione in una direzione specifica
    private void TrovaNuovaDestinazioneDirezionale(Vector3 direzione)
    {
        Vector3 posizioneTarget = miaTrasformata.position + direzione * 5f;
        
        NavMeshHit hit;
        float raggioRicerca = 8f;
        
        // Trova un punto valido sulla NavMesh nella direzione specificata
        if (NavMesh.SamplePosition(posizioneTarget, out hit, raggioRicerca, NavMesh.AllAreas))
        {
            // Reset del percorso attuale
            agente.ResetPath();
            
            // Imposta la nuova destinazione
            agente.SetDestination(hit.position);
            
            if (mostraDebug)
                Debug.Log($"Nuova direzione per evitare ostacolo: {hit.position}");
        }
        else
        {
            // Se non troviamo un punto valido, proviamo con una destinazione completamente nuova
            ImpostaNuovaDestinazione();
        }
    }
    
    // Metodi pubblici per controllo esterno
    
    [ContextMenu("Muovi NPC")]
    public void ForzaMovimento()
    {
        if (!isGestendoBlocco)
        {
            ImpostaNuovaDestinazione();
        }
    }
    
    [ContextMenu("Ferma NPC")]
    public void ForzaArresto()
    {
        if (!isGestendoBlocco)
        {
            IniziaAttesa();
        }
    }
    
    // Metodo per visualizzare il raggio di movimento in editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 0, 0.3f);
        Gizmos.DrawWireSphere(transform.position, raggioMovimento);
        
        if (agente != null && agente.hasPath)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(agente.destination, 0.3f);
        }
    }

#if UNITY_EDITOR
    // Questo metodo verrà chiamato automaticamente quando i valori vengono modificati nell'Inspector
    private void OnValidate()
    {
        // Non fare nulla se siamo in modalità play
        if (!Application.isPlaying || !enabled)
            return;
        
        // Aggiorna le impostazioni con i valori dell'Inspector
        if (agente != null)
        {
            agente.speed = velocitaMovimento;
            agente.stoppingDistance = distanzaArresto;
        }
        
        if (animatore != null)
        {
            animatore.speed = velocitaAnimazione;
        }
    }
#endif

    // Questo metodo può essere chiamato da altri script o eventi per cambiare la velocità
    public void AggiornaSoloVelocita(float nuovaVelocita)
    {
        velocitaMovimento = nuovaVelocita;
        
        if (agente != null)
        {
            agente.speed = velocitaMovimento;
        }
    }
    
    // Questo metodo applica tutte le impostazioni di movimento
    [ContextMenu("Applica Impostazioni Movimento")]
    public void ApplicaImpostazioniMovimento()
    {
        if (agente != null)
        {
            agente.speed = velocitaMovimento;
            agente.stoppingDistance = distanzaArresto;
            agente.acceleration = 8.0f;
            agente.angularSpeed = 120.0f;
        }
        
        if (animatore != null)
        {
            animatore.speed = velocitaAnimazione;
        }
    }
}
