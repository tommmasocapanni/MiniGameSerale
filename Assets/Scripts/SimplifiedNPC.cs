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

    [Header("Sistema Vita")]
    public float vitaMassima = 100f;
    public float vitaAttuale;
    public bool isDead = false;
    public bool showHealthBar = true;
    public GameObject bloodEffectPrefab; // Effetto sangue opzionale quando colpito

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
        
        // Inizializza la vita
        vitaAttuale = vitaMassima;
        
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
        if (isDead) return; // Skip all updates if dead
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

    // Metodo per ricevere danno
    public void RiceviDanno(float quantitaDanno, Vector3 puntoImpatto)
    {
        if (isDead) return; // Ignora il danno se già morto
        
        // Applica il danno
        vitaAttuale -= quantitaDanno;
        
        // Verifica se l'NPC è morto
        if (vitaAttuale <= 0)
        {
            Muori(puntoImpatto); // Passa il punto d'impatto al metodo Muori
        }
    }

    // Metodo per gestire la morte
    private void Muori(Vector3 puntoImpatto)
    {
        isDead = true;
        vitaAttuale = 0;
        
        // Attiva il trigger di morte nell'animatore
        animatore.SetTrigger("IsDead");
        
        // Mostra effetto sangue solo al momento della morte
        if (bloodEffectPrefab != null)
        {
            GameObject blood = Instantiate(bloodEffectPrefab, puntoImpatto, 
                             Quaternion.LookRotation(puntoImpatto - transform.position));
            
            // Distruggi l'effetto sangue dopo un tempo ragionevole
            Destroy(blood, 2f);
        }
        
        // Ferma il movimento
        if (agente != null)
        {
            agente.isStopped = true;
            agente.ResetPath();
            agente.enabled = false; // Disabilita il NavMeshAgent completamente
        }
        
        // Disattiva il movimento dell'NPC
        this.enabled = false;
        
        // Gestione appropriata del collider e della fisica
        Collider collider = GetComponent<Collider>();
        Rigidbody rb = GetComponent<Rigidbody>();
        
        if (collider != null)
        {
            // NON trasformare in trigger
            // collider.isTrigger = true; <- Rimuovi/commenta questa linea
            
            // Opzionale: puoi regolare il centro del collider per adattarlo alla posizione del corpo caduto
            if (collider is CapsuleCollider capsule)
            {
                // Adatta il collider alla posizione del corpo a terra
                capsule.center = new Vector3(capsule.center.x, capsule.center.y / 2, capsule.center.z);
                capsule.height = capsule.height / 2;
            }
        }
        
        // Gestione del Rigidbody per mantenere il corpo sul terreno
        if (rb != null)
        {
            // Opzione 1: Mantieni il rigidbody ma blocca la posizione dopo un breve periodo
            StartCoroutine(BloccaCorpoDopoCaduta());
        }
        else 
        {
            // Se non c'è già un Rigidbody, ne aggiungi uno per gestire la caduta correttamente
            rb = gameObject.AddComponent<Rigidbody>();
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.isKinematic = false;
            StartCoroutine(BloccaCorpoDopoCaduta());
        }
        
        // Programma la rimozione del cadavere dopo un po' (opzionale)
        // Invoke("RimuoviCadavere", 10f);
    }

    // Aggiorna anche questa versione senza parametri per retrocompatibilità
    private void Muori()
    {
        // Usa la posizione della testa dell'NPC come punto d'impatto predefinito
        Vector3 puntoImpatto = transform.position + Vector3.up * 1.5f;
        Muori(puntoImpatto);
    }

    // Nuovo metodo per bloccare il corpo dopo che è caduto a terra
    private IEnumerator BloccaCorpoDopoCaduta()
    {
        // Aspetta che il corpo cada a terra e si stabilizzi
        yield return new WaitForSeconds(2f);
        
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Blocca il corpo in posizione una volta caduto
            rb.isKinematic = true;
            
            // Opzionale: puoi anche bloccare la rotazione
            rb.freezeRotation = true;
        }
    }

    // Opzionale: metodo per rimuovere il cadavere
    private void RimuoviCadavere()
    {
        // Fade out e distruggi
        StartCoroutine(FadeOutAndDestroy());
    }

    private IEnumerator FadeOutAndDestroy()
    {
        // Trova tutti i renderer nell'NPC
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        
        // Fade out graduale
        float duration = 2.0f;
        float elapsed = 0;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / duration;
            
            // Applica fade a tutti i materiali
            foreach (Renderer renderer in renderers)
            {
                Color color = renderer.material.color;
                color.a = 1.0f - normalizedTime;
                renderer.material.color = color;
            }
            
            yield return null;
        }
        
        // Distruggi l'oggetto
        Destroy(gameObject);
    }

    // Per visualizzare la barra della salute sopra l'NPC
    private void OnGUI()
    {
        if (!showHealthBar || isDead || vitaAttuale >= vitaMassima) return;
        
        // Converti la posizione del mondo in posizione dello schermo
        Vector3 posizioneMondo = transform.position + Vector3.up * 2.2f; // Sopra la testa
        Vector3 posizioneSchermo = Camera.main.WorldToScreenPoint(posizioneMondo);
        
        if (posizioneSchermo.z <= 0) return; // Ignora se dietro la camera
        
        // Calcola la larghezza della barra in base alla distanza
        float distanza = Vector3.Distance(Camera.main.transform.position, transform.position);
        float larghezza = Mathf.Clamp(100 - distanza * 2, 40, 100);
        
        // Disegna la barra di salute
        GUI.color = Color.red;
        GUI.DrawTexture(new Rect(posizioneSchermo.x - larghezza/2, Screen.height - posizioneSchermo.y, larghezza, 7), Texture2D.whiteTexture);
        
        GUI.color = Color.green;
        float healthWidth = (vitaAttuale / vitaMassima) * larghezza;
        GUI.DrawTexture(new Rect(posizioneSchermo.x - larghezza/2, Screen.height - posizioneSchermo.y, healthWidth, 7), Texture2D.whiteTexture);
    }
}
