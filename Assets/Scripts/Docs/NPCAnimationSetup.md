# Configurazione dell'Animator per NPC

Per correggere i problemi di animazione e scivolamento nel tuo NPC, segui questi passaggi per configurare l'Animator controller:

## 1. Parametri dell'Animator

Aggiungi/modifica i seguenti parametri nell'Animator:

- **Speed** (Float): Velocità di movimento dell'NPC
- **IsWalking** (Bool): Stato di camminata on/off

## 2. Stati dell'Animator

Assicurati di avere questi stati configurati:

- **Idle**: Animazione quando l'NPC è fermo
- **Walk**: Animazione quando l'NPC cammina

## 3. Transizioni

Configura le transizioni come segue:

### Da Idle a Walk:
- Condizione: `Speed > 0.1`
- Durata transizione: 0.15 secondi
- Exit Time: 0
- Interrompibile: Sì

### Da Walk a Idle:
- Condizione: `Speed < 0.1`
- Durata transizione: 0.25 secondi
- Exit Time: 0
- Interrompibile: Sì

## 4. Root Motion

Per risolvere il problema dello scivolamento:

1. Seleziona l'animazione Idle nell'Editor Animation
2. Assicurati che non ci siano movimenti nella posizione root (X, Y, Z dovrebbero essere costanti)
3. In alternativa, disabilita "Apply Root Motion" nel componente Animator dell'NPC

## 5. Rigidbody e Collider

Se lo scivolamento persiste:

1. Assicurati che `isKinematic` sia impostato su `true` nel Rigidbody
2. Verifica che il Collider non interferisca con la superficie
3. Controlla che il NavMeshAgent abbia valori appropriati per:
   - Stopping Distance: 0.1
   - Auto Braking: true
   - Base Offset: valore appropriato per il modello (solitamente 0)

## 6. Sostituzione dello Script

Sostituisci NPCMovement con NPCMovementController per una gestione più precisa del movimento.
