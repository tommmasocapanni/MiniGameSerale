# Guida all'Ottimizzazione della NavMesh per NPC

## Problemi Comuni di Navigazione

### 1. NPC che cammina sul posto
Quando un NPC sembra muoversi ma rimane bloccato nella stessa posizione, questo può essere causato da:
- Ostacoli invisibili o collider che non sono visibili
- Impostazioni della NavMesh non ottimali
- NPC che cerca di raggiungere una posizione non accessibile
- Problemi nella geometria della NavMesh

### 2. NPC che si incastra
Gli NPC possono rimanere bloccati in:
- Angoli o spigoli della geometria
- Zone tra due collider ravvicinati
- Aree dove la NavMesh non è generata correttamente

## Soluzioni e Ottimizzazioni

### Ottimizzazione dei Parametri NavMeshAgent
```
// Valori consigliati
agente.radius = 0.4f;             // Leggermente minore della larghezza reale del personaggio
agente.height = 1.8f;             // Altezza realistica
agente.baseOffset = 0.0f;         // Dipende dal pivot del modello
agente.acceleration = 12.0f;      // Accelerazione elevata previene blocchi
agente.angularSpeed = 180.0f;     // Rotazione veloce
agente.speed = 3.5f;              // Velocità naturale
agente.stoppingDistance = 0.1f;   // Valore piccolo per evitare oscillazioni
agente.autoBraking = true;        // Rallenta in prossimità della destinazione
agente.avoidancePriority = 50;    // 0-99, valore più basso ha priorità maggiore
```

### Ottimizzazione della Generazione NavMesh

1. **Risoluzione della NavMesh**:
   - Nella finestra Navigation > Bake
   - Ridurre "Voxel Size" (es. 0.1) per una mesh più precisa
   - Aumentare "Min Region Area" per eliminare piccole aree staccate

2. **Gestione degli Spigoli**:
   - Nella finestra Navigation > Bake > Advanced
   - Aumentare "Agent Radius" (es. 0.5) per arrotondare gli angoli
   - Ridurre "Agent Height" se gli NPC si bloccano sotto ostacoli

3. **Impostazioni di Area**:
   - Nella finestra Navigation > Areas
   - Definire aree con costi diversi per personalizzare il routing

## Script di Utilità

1. **NPCUnstuckHelper**:
   - Rileva quando un NPC è bloccato (cammina sul posto)
   - Tenta prima correzioni leggere (ricalcolo percorso)
   - In caso di blocchi ripetuti, teletrasporta l'NPC in una posizione sicura

2. **Gizmos Diagnostici**:
   - Abilita la visualizzazione dei percorsi per vedere dove gli NPC stanno tentando di andare
   - Mostra le posizioni di blocco frequente per identificare problemi della NavMesh

## Consigli Pratici

1. **Spazi di Movimento**:
   - Assicurati che corridoi e passaggi siano almeno 1.5x più larghi del raggio dell'agente
   - Evita angoli a 90 gradi nella geometria navigabile

2. **Test Sistematici**:
   - Posiziona l'NPC in diverse aree e osserva il comportamento
   - Identifica e correggi le "zone problematiche" della NavMesh

3. **Debugging in Tempo Reale**:
   - Usa NavMeshAgent.pathStatus per verificare lo stato del percorso
   - Controlla NavMeshAgent.isOnNavMesh per assicurarti che l'NPC sia sempre sulla NavMesh
