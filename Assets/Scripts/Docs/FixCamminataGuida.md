# Guida Risoluzione Problemi Camminata NPC

Se il tuo NPC non cammina correttamente nonostante tutte le modifiche, segui questa guida passo-passo.

## Passo 1: Usa il Controller Legacy Semplificato

1. **Disabilita tutti gli script di movimento** esistenti sull'NPC:
   - NPCMovementController
   - NPCMovement
   - NPCAnimationHelper
   - NPCSpeedSynchronizer

2. **Aggiungi lo script NPCLegacyController** al tuo NPC
   
3. **Configura i parametri base**:
   - Velocita Agente: 2.0-3.0 (inizia con un valore basso)
   - Velocita Animazione: 1.0 (valore standard)
   - Parametro Walking: "IsWalking" (o il nome del tuo parametro Boolean nell'Animator)

4. **Verifica che l'Animator Controller** abbia:
   - Un parametro booleano chiamato "IsWalking"
   - Uno stato Idle e uno stato Walking
   - Transizioni corrette tra gli stati (basate su IsWalking)

## Passo 2: Verifica la Configurazione di Base

1. **Posizione dell'NPC**:
   - L'NPC deve essere posizionato esattamente sulla NavMesh
   - Verifica aprendo la finestra Navigation (Window > AI > Navigation)
   - Attiva la visualizzazione della NavMesh e conferma che l'NPC sia sopra l'area blu

2. **Componente Animator**:
   - Apply Root Motion: DISATTIVATO
   - Update Mode: Normal
   - Culling Mode: Always Animate

3. **Componente NavMeshAgent**:
   - Speed: 2.0-3.0
   - Angular Speed: 120
   - Acceleration: 8
   - Stopping Distance: 0.1
   - Auto Braking: ATTIVATO
   - Update Rotation: ATTIVATO

4. **Componente Rigidbody**:
   - Is Kinematic: ATTIVATO
   - Use Gravity: ATTIVATO
   - Interpolate: Interpolate
   - Collision Detection: Continuous

## Passo 3: Semplifica per Isolare il Problema

1. **Test con animazioni standard**:
   - Temporaneamente, usa animazioni semplici come quelle del package standard "Humanoid"
   - Questo aiuta a determinare se il problema è nelle animazioni Mixamo

2. **Controlla l'importazione dell'animazione**:
   - Seleziona il file FBX dell'animazione di camminata
   - Nella finestra Inspector, vai alla tab "Animation"
   - Verifica che "Loop Time" sia attivato
   - Controlla che "Root Transform Position (Y)" sia su "Original"
   - Disattiva "Root Motion" se presente

3. **Prova una camminata manuale**:
   - Nel NPCLegacyController, usa il menu contestuale "Muovi NPC Ora"
   - Osserva attentamente l'NPC e registra cosa succede

## Problemi e Soluzioni Specifiche

### L'NPC non si muove affatto
- Verifica che non ci sia collisione con altri oggetti
- Controlla che l'NPC sia sulla NavMesh
- Assicurati che i layer di collisione siano configurati correttamente

### L'NPC si muove ma l'animazione non parte
- Verifica che il nome del parametro nell'Animator sia esattamente "IsWalking"
- Controlla che le transizioni nell'Animator non abbiano condizioni aggiuntive
- Rimuovi "Exit Time" dalle transizioni

### L'NPC si muove troppo lentamente
- Aumenta "velocitaAgente" a valori più alti (3-5)
- Prova valori più bassi di "velocitaAnimazione" (0.8-0.9)

### L'NPC scivola o pattina
- Riduce "velocitaAgente" a valori più bassi (1-2)
- Aumenta "velocitaAnimazione" (1.1-1.2)

### L'NPC si muove a scatti
- Verifica che il frame rate sia stabile
- Controlla che non ci siano altri script pesanti che influenzano le prestazioni
