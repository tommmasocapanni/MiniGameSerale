# Guida alla Sincronizzazione della Velocità con l'Animazione

## Problema
Quando un NPC si muove con il NavMeshAgent e usa un'animazione "Walking In Place", spesso la velocità di movimento non coincide con la velocità del passo animato. Questo può causare:
- NPC che sembrano scivolare (movimento troppo veloce rispetto all'animazione)
- NPC che sembrano camminare sul posto (movimento troppo lento rispetto all'animazione)

## Soluzione

### Metodo 1: Usa NPCSpeedSynchronizer
1. Aggiungi il componente NPCSpeedSynchronizer al tuo NPC
2. Regola i parametri:
   - **Agent Speed**: Velocità di movimento del NavMeshAgent (consigliato: 3.0-4.0)
   - **Animation Speed**: Velocità di riproduzione dell'animazione (consigliato: 0.8-1.2)
3. Usa i pulsanti contestuali per applicare preset predefiniti

### Metodo 2: Regolazione Manuale
1. Seleziona il NavMeshAgent dell'NPC
2. Imposta **Speed** a un valore appropriato (3.0-4.0)
3. Seleziona il componente Animator
4. Imposta **Speed** a un valore appropriato (0.8-1.2)

## Valori Consigliati per Diversi Tipi di Movimento

| Tipo di Movimento | NavMeshAgent Speed | Animator Speed |
|-------------------|-------------------|----------------|
| Camminata lenta   | 1.5-2.0           | 0.7-0.9        |
| Camminata normale | 3.0-4.0           | 1.0            |
| Camminata veloce  | 4.5-5.5           | 1.1-1.3        |
| Corsa             | 7.0-9.0           | 1.2-1.5        |

## Note Aggiuntive
- Se l'animazione ha già un passo veloce, diminuisci l'Animator Speed e aumenta il NavMeshAgent Speed
- Se l'animazione ha un passo lento, aumenta l'Animator Speed e diminuisci il NavMeshAgent Speed
- Ricorda che la velocità di rotazione può influenzare la percezione del movimento
