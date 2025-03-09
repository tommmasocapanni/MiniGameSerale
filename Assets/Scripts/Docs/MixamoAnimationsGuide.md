# Guida alle Animazioni Mixamo con NavMeshAgent

## Tipi di Animazioni Mixamo

Mixamo offre due tipi principali di animazioni di movimento:
- **In Place**: Il personaggio anima il movimento senza spostarsi dalla posizione di origine
- **Root Motion**: Il personaggio si sposta effettivamente nello spazio durante l'animazione

## Quale Scegliere per NavMeshAgent?

**Usa sempre le versioni "In Place" quando lavori con NavMeshAgent.**

### Perché usare "In Place"
1. NavMeshAgent gestisce già lo spostamento fisico del personaggio
2. Se usi animazioni con Root Motion, creerai un "doppio movimento":
   - Il movimento dell'animazione stessa
   - Il movimento imposto dal NavMeshAgent
3. Questo "doppio movimento" causa problemi come:
   - Scivolamento
   - Velocità irregolare
   - Desincronizzazione tra animazione e movimento reale

## Come Importare Correttamente da Mixamo

1. Su Mixamo, cerca le animazioni con "In Place" nel nome (es. "Walking In Place")
2. Scarica l'animazione in formato FBX
3. Importa in Unity
4. Nelle impostazioni di importazione:
   - Assicurati che "Loop Time" sia attivato per animazioni cicliche
   - Verifica che "Root Transform Position (Y)" sia impostato su "Original"
   - Disattiva "Root Motion" se appare l'opzione

## Configurazione in Unity Animator

1. Nel tuo Animator Controller, crea i parametri:
   - "Speed" (float)
   - "IsWalking" (bool)
2. Configura le transizioni basandoti su questi parametri:
   - Da Idle a Walk: quando Speed > 0.1 o IsWalking = true
   - Da Walk a Idle: quando Speed < 0.1 o IsWalking = false
3. Assicurati che le transizioni siano immediate (durata breve, senza Exit Time)

## Migliori Animazioni Mixamo per NPC

Per il tuo NPC, consiglio queste animazioni "In Place":
- Idle: "Idle" o "Breathing Idle"
- Camminata: "Walking In Place"
- Corsa: "Running In Place"
- Svolta: "Turn 90 Left/Right In Place"

## Risoluzione Problemi Comuni

- **Scivolamento quando fermo**: Assicurati che l'animazione Idle non contenga movimento root
- **Animazione che si ferma**: Verifica che il parametro IsWalking sia correttamente impostato a true nel tuo script
- **Movimenti innaturali**: Regola la velocità del NavMeshAgent per farla corrispondere alla velocità dell'animazione
