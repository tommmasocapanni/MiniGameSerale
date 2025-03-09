# Guida al Controllo del NavMeshAgent

## Script che Modificano i Parametri NavMeshAgent

I seguenti script sovrascrivono i valori del NavMeshAgent durante l'esecuzione. Puoi disabilitarli o modificarli per mantenere il controllo dall'Inspector.

### 1. NPCLegacyController

**Posizione:** `Assets/Scripts/NPCLegacyController.cs`

**Modifica automatica:**
```csharp
agente.speed = velocitaAgente;  // In Awake()
```

**Come risolvere:**
- Disabilita questo script se non ti serve
- Oppure imposta `velocitaAgente` allo stesso valore che hai nell'Inspector
- Oppure modifica lo script per eliminare questa riga

### 2. NPCUnstuckHelper

**Posizione:** `Assets/Scripts/NPCUnstuckHelper.cs`

**Modifica automatica:**
```csharp
agente.radius = agente.radius * 0.9f;  // In Start()
agente.acceleration = 12f;
agente.stoppingDistance = 0.1f;
agente.avoidancePriority = 40;
```

**Come risolvere:**
- Disabilita questo script se non hai problemi di "blocco" dell'NPC
- Oppure imposta `correzioneAutomatica = false` nell'Inspector
- Oppure commenta le righe che modificano i parametri

### 3. NPCSpeedSynchronizer

**Posizione:** `Assets/Scripts/NPCSpeedSynchronizer.cs`

**Modifica automatica:**
```csharp
agent.speed = agentSpeed;  // In ApplySpeedSettings()
```

**Come risolvere:**
- Disabilita questo script se non ti serve la sincronizzazione
- Oppure imposta `agentSpeed` allo stesso valore che hai nell'Inspector
- Oppure aggiungi un'opzione per disabilitare la sincronizzazione

### 4. NPCMovementController

**Posizione:** `Assets/Scripts/NPCMovementController.cs`

**Modifica automatica:**
```csharp
agent.speed = movementSpeed;  // In Start()
```

**Come risolvere:**
- Disabilita questo script se usi NPCLegacyController
- Oppure imposta `movementSpeed` allo stesso valore che hai nell'Inspector
- Oppure modifica lo script per rispettare i valori dell'Inspector

## Soluzione Consigliata

1. **Aggiungi NPCNavMeshPriority** al tuo NPC
2. **Imposta preservaValoriInspector = true**
3. **Configura i tuoi valori NavMeshAgent** come preferisci nell'Inspector
4. **Lascia attivi gli altri script** che ti servono, NPCNavMeshPriority si assicurerà che i tuoi valori vengano preservati
