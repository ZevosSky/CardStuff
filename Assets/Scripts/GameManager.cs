//==============================================================================
// @Author: Gary Yang
// @File: GameManager.cs
// @brief: Game Logic for Rummy game, includes deck and player setup
// @copyright DigiPen(C) 2025
//==============================================================================

using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEditor;
using UnityEngine.Playables;
using UnityEngine.Serialization;


//==| Highest Card Wins |==================================================================================================|
/*
    
*/
//==| Game Manager |===================================================================================================|
public partial class GameManager : MonoBehaviour
{
    [Header("Action to the Scene Action Manager")]
    [Tooltip("In charge of all actions that happen within the scene")]
    [SerializeField]
    private ActionManager actionManager;
    
    [Tooltip("Dedicated action manager for hover animations - keeps hover responsive and independent from game logic")]
    [SerializeField]
    private ActionManager hoverActionManager;

    [SerializeField] private GameObject cardPrefab;

    //==| Game Setting Properties |=====================================================================================|
    [Header("Game Settings")]
    [SerializeField] // NOTE: This can not be changed via the inspector in play mode
    [Range(2, 6)]
    // We could have more players but if we do I'm going to have to make it 2+ decks 
    private int playerCount = 4; // number of players at the table 

    [SerializeField] [Range(1, 3)] private float CardSize = 1; // Base size of the card
    private const float CardSizeMin = 0.5f; // Base size of the card
    private const float CardSizeMax = 2.5f; // Max size of the 
    private bool cardSizeDirty = false; // if the card size has been changed
    [SerializeField] private HoverManager hoverManager; 
    

    // Play Field Locations
    [Header("Card Location References")] [SerializeField]
    private GameObject DrawDeckLocationReference; // where the draw deck is located

    [SerializeField] private GameObject DiscardDeckLocationReference; // where the discard deck is located


    // * * * Game Data * * * \\
    //==| Places to put cards |=========================================================================================|

    private List<GameObject> WholeDeck = new List<GameObject>(); // deck of ALL cards
    private List<GameObject> DrawDeck = new List<GameObject>(); // deck of cards to draw from
    private List<GameObject> SwapDeck = new List<GameObject>(); // deck of cards that have been discarded / swapped
    // private List<PlayerState> playerStates = new List<PlayState>();    // players at the table

    [SerializeField]
    private PlaySpace playSpace; // reference to the play space script in the scene (in charge of player positions)

    [DoNotSerialize]
    public List<GameObject> playSpaceCards = new List<GameObject>(); // cards currently in the play space (on the table)

    //==| Misc Game Data |==============================================================================================|
    [Header("Deck Spacing Properties")] [SerializeField] [Tooltip("Spacing between cards in the draw deck (x, y, -z)")]
    private Vector3 DrawDeckSpacing = new Vector3(0.003f, 0.003f, -0.05f);

    [SerializeField] [Tooltip("Spacing between cards in the swap deck hand (x, y, -z)")]
    private Vector3
        DiscardDeckSpacing = new Vector3(0.00f, 0.001f, -0.05f); // spacing between cards in the discard deck

    // settings (config from pause menu) 
    
 
    
    
    //==| Game State |==================================================================================================|
    [DoNotSerialize] public int _turn;
    [DoNotSerialize] public bool _isPaused = false;
    [DoNotSerialize] public bool _allowInteraction;
    [DoNotSerialize] private int _lastExecutedTurn = -1; // Track which turn was last executed
    [DoNotSerialize] private bool _roundInProgress = false; // Track if end of round is being processed
    [DoNotSerialize] private List<List<GameObject>> _playerHands = new List<List<GameObject>>(); // Each player's hand of cards
    [DoNotSerialize] private List<GameObject> _drawDeck = new List<GameObject>(); // The draw deck of cards
    [DoNotSerialize] private List<GameObject> _discardDeck = new List<GameObject>(); // The discard deck of cards
    [DoNotSerialize] private List<PlayerCurve> _playerCurves = new List<PlayerCurve>();
    //==| Unity Functions |=============================================================================================|
    bool GetIsPaused() { return _isPaused; }
    
    #region UnityFunctions
    IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(1.1f);  // Small delay
    
        // Now try to access playSpace
        var playerPositions = playSpace.GetPlayerObjectReferences();
        playerCount = playSpace.GetPlayerCount();
    
        // Continue with your card dealing code
    }
    void Start()
    {
        #region Examples_I_Made

        /*
        // Card Instantiation Example
        GameObject newCard = Instantiate(cardPrefab);
        Card newCardComponent = newCard.GetComponentInChildren<Card>();
        newCardComponent.SetCard(Card.Suit.Hearts, Card.Rank.Ace, 0);

        Vector3 targetPosition = new Vector3(5f, 2f, 0f);
        Vector3 targetPosition1 = new Vector3(5f, 0f, 0f);
        */
        /*
        TranslateAction moveCard = new TranslateAction(
            newCard,                                                         // target object
            targetPosition,                                                  // where to move
            duration: 2.0f,                                                  // how long it takes
            easeFunction: (x) => Easing.EaseOutElastic(                 // easing function
                x,
                decayRate: 6f,         // Higher = faster decay of bounces
                oscillationSpeed: 6f,  // Higher = faster bounces
                frequency: 4f          // Higher = more bounces
            ),
            blocking: true                                                   // wait for completion
                                                                             // before next action
        );
        */
        /*  Example of multiple actions in sequence
        TranslateAction moveCard = new TranslateAction(
            newCard,                                                         // target object
            targetPosition,                                                  // where to move
            duration: 2.0f,                                                  // how long it takes
            easeFunction: Easing.EaseOutElastic,                             // easing function
            delay: 0.0f,                                                     // delay before starting
            blocking: false                                                   // wait for completion
        );
        TranslateAction moveCard2 = new TranslateAction(
            newCard,                                                         // target object
            targetPosition1,                                                  // where to move
            duration: 2.0f,                                                  // how long it takes
            easeFunction: Easing.EaseOutElastic,                             // easing function
            delay: 5.0f,                                                     // delay before starting
            blocking: false                                                   // wait for completion
        );

        RotateAction spinCard = new RotateAction(
            newCard,                                                          // target object
            new Vector3(0, 0, 180),                                           // where to rotate
            duration: 2.0f,                                                   // how long it takes
            easeFunction: Easing.EaseOutElastic,                              // easing function
            blocking: false                                                    // wait for completion
        );

        RotateAction flipCard = new RotateAction(
            newCard,                                                          // target object
            new Vector3(180, 0, 0),                                           // where to rotate
            duration: 1.0f,                                                   // how long it takes
            easeFunction: Easing.Linear,                                      // easing function
            blocking: false                                                    // wait for completion
        );

        var simultaneous = new SimultaneousTransformActions(newCard);
        simultaneous.AddAction(moveCard);
        simultaneous.AddAction(spinCard);
        simultaneous.AddAction(flipCard);

        actionManager.AddAction(simultaneous);
        actionManager.AddAction(moveCard2);
        */

        #endregion
        
        // * * * Game Setup * * * \\
        
        // Spawn a deck of cards, debug show all the cards in a spread
        // Step 1: Spawn and prepare deck
        SpawnDeck();
        LayerDeck();
        FlipAllCards();

        // Step 2: Add a blocking action to ensure Step 1 is complete
        actionManager.AddAction(new BlockAction(0.1f));

        // Step 3: Add cards to draw deck with animations
        AddCardsToDrawDeck(WholeDeck);

        // Step 4: Shuffle deck with animations
        ShuffleDeck(DrawDeck, DrawDeckLocationReference.transform, DrawDeckSpacing);
        
        // Step 5: Deal cards to players
        
        var playerPositions = playSpace.GetPlayerObjectReferences();
        playerCount = playSpace.GetPlayerCount();
        // add playerCount number of player states
        
        
        for (int i = 0; i < playerCount; ++i) { // for each player... 
            // Debug.Log($"Player {i} rotation: {playerPositions[i].transform.eulerAngles}");
            PlayerCurve playerCurve = new PlayerCurve(playerPositions[i].transform, 3, 1, 0.01f);
            _playerCurves.Add(playerCurve); // Store the curve for later use
            _playerHands.Add(new List<GameObject>());
            (Vector3, float)[] handPositions = playerCurve.CalculateCardPositions(5);
            
            for (int j = 0; j < 5; ++j) // for each card in their starting hand
            {
                GameObject card = DrawDeck[DrawDeck.Count - 1];
                DrawDeck.RemoveAt(DrawDeck.Count - 1);
                var cc = card.GetComponentInChildren<Card>();
                
                // Set card state to match what the animation will show
                if (i == 0) cc.faceUp = true;  // player 0 is the human player, deal face up cards
                else cc.faceUp = false;        // AI players get face down cards 
                
                AnimateCardToPosition(card, handPositions[j].Item1, handPositions[j].Item2, (i == 0) ? false : true);
                _playerHands[i].Add(card);
            }
        } // end for loop

        BlockInteraction(0.1f); // block interaction until dealing is done
        
        // register all cards to action manager
        for (int i = 0; i < WholeDeck.Count; ++i)
        {
            RegisterCardToActionManager(WholeDeck[i]);
        }
        
        
        
    }

    // temp testing hand debug game objects, remove in final build 
    [SerializeField] private GameObject DebugSphere;
    [SerializeField] private bool AutoPlay;
    
    void Update()
    {
        // TODO: add pause menu functionality 
        //  * Player needs to be able to pause the game
        //  * pauses the ActionManger of game actions 
        //  * spins up a new temp action list for pause menu actions 
        //  * size & player count have to be configureable in there 
        
        
        EndOfGameCheck(); // check if the game should end & what to do if it does
        
        if (_turn == 0 && !AutoPlay) // player turn 
        {
            CheckIfCardSelected(); // wait for player input
            
        }
        else if (_turn >= playerCount && !_roundInProgress)
        {
            // round over, move the play cards to the center, winner draws a card
            _roundInProgress = true; // Prevent triggering again until round is complete
            EndOfRoundLogic();
            Debug.Log("Round Over Reached");
        }
        else if (_turn < playerCount) // computer turn, play randomly
        {
            AITurn();
        } // computer turn



    }
    #endregion // UnityFunctions

    void ClearGame()
    {
        for (int i = 0; i < WholeDeck.Count; ++i) { Destroy(WholeDeck[i]); }
        WholeDeck.Clear();
        DrawDeck.Clear();
        SwapDeck.Clear();
    }


    void RestartGame()
    {
        ClearGame();
        Start(); // restart the game setup
    }
    
    
    #region DeckFunctions
    // Spawn a deck of cards 
    void SpawnDeck()
    {
        for (int i = 0; i < 52; i++)
        {
            WholeDeck.Add(Instantiate(cardPrefab));
            Card current = WholeDeck[i].GetComponentInChildren<Card>();
            current.actionManager = actionManager;
            current.SetCard((Card.Suit)(i % 4), (Card.Rank)((i % 13) ), 0);
            // Debug.Log("Card: " + current.suit + " " + current.rank + "Faceup? " + current.faceUp);
        }
    }
    void SpawnDeck(int decks) // if the game requires more than multiple decks (more than 6 players) 
    {
        for (int i = 0; i < (52 * decks); ++i)
        {
            int wrap = i % 52;
            WholeDeck.Add(Instantiate(cardPrefab));
            Card current = WholeDeck[i].GetComponentInChildren<Card>();
            current.SetCard((Card.Suit)(wrap % 4), (Card.Rank)((wrap % 13) ), 0);
        }
    }
    
    public Vector3 GetFlippedCardEulerAngles(float zRotation)
    {
        return new Vector3(180, 0, -zRotation);
    }

    // Add cards to the draw deck
    void AddCardToDrawDeck(GameObject card)
    {
        // Calculate position BEFORE adding to DrawDeck
        int deckIndex = DrawDeck.Count;
        Vector3 targetPosition = DrawDeckLocationReference.transform.position + DrawDeckSpacing * deckIndex;
    
        // Now add the card to the deck
        DrawDeck.Add(card);
    
        // Create the translate action with the pre-calculated position
        actionManager.AddAction(new
            TranslateAction(
                card,
                targetPosition,
                0.5f,
                0.0f,
                easeFunction: Easing.EaseInOutCubic,
                false)
        );
    
        Card c = card.GetComponentInChildren<Card>();
        if (c.faceUp == true)
        {
            c.faceUp = !c.faceUp;
            actionManager.AddAction(new
                FlipAction(
                    card,
                    0.5f,
                    0.0f,
                    false)
            );
        }
    } 
    
    // Shuffle Deck w/ animations
    void ShuffleDeck(List<GameObject> deck, Transform transform, Vector3 deckSpacing)
    {
        // first shuffle their orders in the deck data structure
        deck.Shuffle(); // Fisher-Yates shuffle algorithm, List extension in Interpolation_easing.cs
        
        // now animate them to their correct positiions
        Vector3 deckLocation = transform.position;
        Quaternion deckRotation = transform.rotation;
        for (int i = 0; i < deck.Count; ++i)
        {
            int direction = Random.Range(0, 2);
            const float shuffleOffset = 0.5f;
            // First movement - to side (non-blocking)
            actionManager.AddAction(new TranslateAction(
                deck[i],
                new Vector3(
                    deckLocation.x + (direction == 0 ? shuffleOffset : -shuffleOffset), 
                    deckLocation.y,
                    deckLocation.z
                ),
                0.5f,
                0.0f,
                easeFunction: Easing.EaseOutElastic,
                false // NOT blocking - all cards move at once
            ));
            // then move them to their correct position
            actionManager.AddAction(new TranslateAction(
                deck[i],
                new Vector3(
                    deckLocation.x + deckSpacing.x * i, 
                    deckLocation.y + deckSpacing.y * i,
                    deckLocation.z + deckSpacing.z * i
                ),
                0.7f,
                0.7f + (0.01f * i), // Delay
                easeFunction: Easing.EaseOutElastic,
                false // NOT blocking - all cards move at once
            ));

        } // end for loop
        actionManager.AddAction(new BlockAction(2.45f));
    }
    
    void AddCardsToDrawDeck(List<GameObject> cards)
    {
        for (int i = 0; i < cards.Count; ++i)
        {
            AddCardToDrawDeck(cards[i]); // add each card to the draw deck (animation) 
            
        }
        actionManager.AddAction(new BlockAction(.5f));
    }
    
    void AddCardToSwapDeck(GameObject card)
    {
        SwapDeck.Add(card);
    }
    
    void ClearAllCards() // Clear all cards from the table
    {
        for (int i = 0; i < WholeDeck.Count; ++i) { Destroy(WholeDeck[i]); }
        WholeDeck.Clear();
    }
    
    void FlipAllCardsDown()
    {
        if (WholeDeck.Count == 0) return;
        
        for (int i = 0; i < WholeDeck.Count; ++i)
        {
            GameObject currentG = WholeDeck[i];
            Card currentC = WholeDeck[i].GetComponentInChildren<Card>();

            if (currentC.faceUp == false) // flip the cards face down 
            {
                // flip the card 
                actionManager.AddAction(new
                        FlipAction(
                                    currentG,
                            0.1f,  // Duration
                              0.0f,  // No delay
                            false) // Not blocking
                );
                currentC.faceUp = false;
            } 
        } // end for loop
    }
    // Debug function
    void FullDeckDisplay() // Display the deck fully spread out 
    {
        const float downSpread = 1.5f;
        const float rightSpread = 0.80f;
        Vector2 topRight = new Vector2((-rightSpread * 13) / 2.0f, (rightSpread * 4) / 2.0f);
        
        for (int i = 0; i < WholeDeck.Count; ++i)
        {
            GameObject currentG = WholeDeck[i];
            Card currentC = WholeDeck[i].GetComponentInChildren<Card>();
            
            
            actionManager.AddAction( new 
                TranslateAction(
                    currentG, 
                    new Vector3(topRight.x + rightSpread * (i % 13), topRight.y - downSpread * (i % 4), 0),
                    1.0f, 
                    0.0f,
                    easeFunction: Easing.EaseOutCubic,
                    false)
                );
            
            if (currentC.faceUp == false) // if the card is face down 
            {
                // flip the card 
                actionManager.AddAction( new 
                        FlipAction(currentG, 
                            0.5f,     // Duration
                            0.0f,       // No delay
                            false)    // Not blocking
                );
                currentC.faceUp = true;
            }
        }
        actionManager.AddAction(new BlockAction(0.01f));
    }
    
    // Layer deck: keep z order correct based on index
    void LayerDeck()
    {
        for (int i = 0; i < WholeDeck.Count; ++i)
        {
            // Mild offset to left and right to make the card look like a "3D" deck
            WholeDeck[i].transform.position = new Vector3(0.003f * i, 0.003f * i, -0.05f * i);
        }
    }
    // overload with custom position and offsets
    void LayerDeck(Vector3 position, List<GameObject> effectedCards, float xOffset, float yOffset)
    {
        for (int i = 0; i < effectedCards.Count; ++i)
        {
            effectedCards[i].transform.position = position;
        }
    }
    
    void FlipAllCards()
    {
        for (int i = 0; i < WholeDeck.Count; i++) 
        {
            WholeDeck[i].transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x + 180, 0, 0);
            Card current = WholeDeck[i].GetComponentInChildren<Card>();
            current.faceUp = !current.faceUp;
        }
    }
    
    void FlipCard(GameObject card)
    {
        Card current = card.GetComponentInChildren<Card>();
        Quaternion currentRotation = card.transform.rotation;
        current.faceUp = !current.faceUp;
        // just flip the card, maintain tilt (y quaternion)
        actionManager.AddAction(new
            RotateAction(
                card,
                new Vector3(currentRotation.eulerAngles.x + 180, currentRotation.eulerAngles.y, currentRotation.eulerAngles.z),
                0.5f,
                0.0f,
                easeFunction: Easing.EaseOutElastic,
                true
                )
        );
        
    }
    
    #endregion // DeckFunctions

    #region TestFunctionsStuff
    
    void TestSimultaneousActions(GameObject targetObject)
    {
        
        // EXAMPLE 1: Simple simultaneous rotation and scale
        // This makes the object rotate and scale at the same time
        var simpleCombo = new SimultaneousTransformActions(targetObject, delay: 0f, blocking: true);
        
        // Add a rotation that spins the object 360 degrees around the Y axis
        simpleCombo.AddAction(new RotateAction(
            targetObject,
            new Vector3(0, 360, 0),
            duration: 1.0f,
            easeFunction: Easing.EaseInOutQuad
        ));
        
        // At the same time, make it pulse larger then back to normal size
        simpleCombo.AddAction(new ScaleAction(
            targetObject,
            new Vector3(1.5f, 1.5f, 1.5f),
            duration: 1.0f,
            easeFunction: Easing.EaseOutBack
        ));
        
        // Add the combined action to the manager
        actionManager.AddAction(simpleCombo);
        
        // EXAMPLE 2: Card flip with movement
        // This creates a more complex motion combining translation, rotation, and scale
        var cardFlip = new SimultaneousTransformActions(targetObject, delay: 1.5f, blocking: true);
        
        // Move the card up slightly while flipping
        cardFlip.AddAction(new TranslateAction(
            targetObject,
            targetObject.transform.position + new Vector3(0, 0.5f, 0),
            duration: 0.8f,
            easeFunction: Easing.EaseOutQuad
        ));
        
        // Flip the card (180 degrees around X axis)
        cardFlip.AddAction(new RotateAction(
            targetObject,
            new Vector3(180, 0, 0),
            duration: 0.8f,
            easeFunction: Easing.EaseInOutQuad
        ));
        
        // Slightly scale down while flipping (like cards do)
        cardFlip.AddAction(new ScaleAction(
            targetObject,
            new Vector3(0.9f, 0.9f, 0.9f),
            duration: 0.8f,
            easeFunction: Easing.EaseInOutQuad
        ));
        
        // Add the card flip to the action queue
        actionManager.AddAction(cardFlip);
        
        // EXAMPLE 3: Bounce and spin effect
        // Creates a bouncing movement with rotation
        var bounceAndSpin = new SimultaneousTransformActions(targetObject, delay: 1.0f, blocking: true);
        
        // Make it bounce up and down
        bounceAndSpin.AddAction(new TranslateAction(
            targetObject,
            targetObject.transform.position + new Vector3(0, 0, 0), // Returns to original position
            duration: 1.2f,
            easeFunction: Easing.EaseOutBounce
        ));
        
        // Spin around the Z axis while bouncing
        bounceAndSpin.AddAction(new RotateAction(
            targetObject,
            new Vector3(0, 0, 360),
            duration: 1.2f,
            easeFunction: Easing.EaseInOutCubic
        ));
        
        // Add the bounce and spin effect
        actionManager.AddAction(bounceAndSpin);
        
        // Add a small delay between action sequences for clarity
        actionManager.AddAction(new BlockAction(0.5f));
    }
        

    #endregion
    
    #region HandFunctions
    private void AnimateCardToPosition(GameObject card, Vector3 targetPosition, float zRotation, bool isFlipped)
    {
        // Create simultaneous action
        var simultaneous = new SimultaneousTransformActions(card);
    
        // Add translate action
        simultaneous.AddAction(new TranslateAction(
            card,
            targetPosition,
            0.5f,  // Duration
            0.0f,  // Delay
            easeFunction: Easing.EaseOutCubic,
            false  // Not blocking
        ));
    
        // Get the proper Euler angles based on whether the card is flipped
        Vector3 eulerAngles = isFlipped 
            ? GetFlippedCardEulerAngles(zRotation)  // For flipped cards
            : new Vector3(0, 0, zRotation);         // For face-up cards
    
        // Add rotate action
        simultaneous.AddAction(new RotateAction(
            card,
            eulerAngles,  
            0.5f,         // Duration
            0.0f,         // Delay
            easeFunction: Easing.EaseOutCubic,
            false         // Not blocking
        ));
    
        // Add to action manager
        actionManager.AddAction(simultaneous);
    }
    
    private void AnimateCardToPositionNoBlock(GameObject card, Vector3 targetPosition, float zRotation, bool isFlipped)
    {
        // actionManager.AddAction(new ScaleAction(
        //     card,
        //     new Vector3(1)));
        
        // Create simultaneous action
        var simultaneous = new SimultaneousTransformActions(card);
        simultaneous.isBlocking = false;
    
        // Add translate action
        simultaneous.AddAction(new TranslateAction(
            card,
            targetPosition,
            0.5f,  // Duration
            0.0f,  // Delay
            easeFunction: Easing.EaseOutCubic,
            false  // Not blocking
        ));
    
        // Get the proper Euler angles based on whether the card is flipped
        Vector3 eulerAngles = isFlipped 
            ? GetFlippedCardEulerAngles(zRotation)  // For flipped cards
            : new Vector3(0, 0, zRotation);         // For face-up cards
    
        // Add rotate action
        simultaneous.AddAction(new RotateAction(
            card,
            eulerAngles,  
            0.5f,         // Duration
            0.0f,         // Delay
            easeFunction: Easing.EaseOutCubic,
            false         // Not blocking
        ));
    
        // Add to action manager
        actionManager.AddAction(simultaneous);
    }
    
    #endregion
    
    
    
    void EndOfGameCheck()
    {
        if (DrawDeck.Count == 0)
        {
            Debug.Log("Game Over: Draw Deck is empty!");
            // Additional end-of-game logic can be added here
            RestartGame();
        }
    }
    
    
    
    void RegisterCardToActionManager(GameObject card)
    {
        Card cardComponent = card.GetComponentInChildren<Card>();
        if (cardComponent != null)
        {
            cardComponent.actionManager = actionManager;
            cardComponent.hoverActionManager = hoverActionManager; // Register hover action manager
            cardComponent.gameManager = this;
        }
        else
        {
            Debug.LogError("RegisterCardToActionManager: Card component not found on the provided GameObject.");
        }
    }
    
    void BlockInteraction(float duration)
    {
        _allowInteraction = false;
        actionManager.AddAction(new CallBackAction(duration,
            false,
            () => { _allowInteraction = true; })
        );
    }
    [Header("Discard Field Reference")]
    [SerializeField] GameObject DiscardFieldReference; // where the cards in the play space go when they are discarded, set in the inspector
    void EndOfRoundLogic()
    {
        // reveal all cards, make the card that win pop up(scale up), do a small jiggle rotate left and right and then scale down down, make the player who won draw a card, then start the next round 
        
        // Block player interaction during end of round animations
        BlockInteraction(2.0f * playerCount); // Block for a duration based on number of players (adjust as needed)
        
        // Step 1: Reveal all cards in the discard deck (flip face-down cards to face-up)
        int cardsToFlip = 0;
        for (int i = 0; i < _discardDeck.Count; i++)
        {
            GameObject cardObj = _discardDeck[i];
            Card cardComponent = cardObj.GetComponentInChildren<Card>();
            
            Debug.Log($"Card {i} - faceUp state: {cardComponent?.faceUp}");
            
            // Only flip cards that are currently face-down (AI players' cards)
            if (cardComponent != null && !cardComponent.faceUp)
            {
                Debug.Log($"Flipping card {i} to reveal it");
                
                // Get current rotation and add 180 to X axis (same as FlipCard method)
                Quaternion currentRotation = cardObj.transform.rotation;
                
                // Add rotate action with staggered delay (non-blocking)
                actionManager.AddAction(new RotateAction(
                    cardObj,
                    new Vector3(currentRotation.eulerAngles.x + 180, currentRotation.eulerAngles.y, currentRotation.eulerAngles.z),
                    0.5f,  // Duration
                    cardsToFlip * 0.1f,  // Stagger the reveals
                    easeFunction: Easing.EaseOutElastic,
                    false  // Non-blocking
                ));
                
                // Update faceUp state immediately so logic knows it's been flipped
                cardComponent.faceUp = true;
                cardsToFlip++;
            }
        }
        
        Debug.Log($"Total cards to flip: {cardsToFlip}");
        
        // Wait for ALL flip animations to complete
        if (cardsToFlip > 0)
        {
            float totalFlipTime = 0.5f + ((cardsToFlip - 1) * 0.1f); // Duration + last stagger delay
            actionManager.AddAction(new BlockAction(totalFlipTime));
        }
        
        // Step 2-6: After reveals complete, do everything else in sequence
        // Disable hover on all cards in discard to prevent interference
        for (int i = 0; i < _discardDeck.Count; i++)
        {
            Card cardComponent = _discardDeck[i].GetComponentInChildren<Card>();
            if (cardComponent != null)
            {
                cardComponent.isHoverAble = false; // Disable hover
            }
        }
        
        Debug.Log("Starting winner determination after card reveals");
        
        // Step 2: Determine the winning card (highest rank)
        int winningIndex = -1;
        int highestRank = -1;
        
        for (int i = 0; i < _discardDeck.Count; i++)
        {
            Card cardComponent = _discardDeck[i].GetComponentInChildren<Card>();
            if (cardComponent != null)
            {
                int rankValue = (int)cardComponent.rank;
                Debug.Log($"Card {i}: Rank = {cardComponent.rank} ({rankValue})");
                if (rankValue > highestRank)
                {
                    highestRank = rankValue;
                    winningIndex = i;
                }
            }
        }
        
        Debug.Log($"Winner: Card {winningIndex} with rank {highestRank}");
        
        // Step 3: Animate the winning card
        if (winningIndex >= 0)
        {
            GameObject winningCardParent = _discardDeck[winningIndex];
            Card winningCardComponent = winningCardParent.GetComponentInChildren<Card>();
            
            if (winningCardComponent != null)
            {
                GameObject winningCardGameObject = winningCardComponent.gameObject;
                
                // Log current scales for both parent and child
                Debug.Log($"BEFORE - Parent scale: {winningCardParent.transform.localScale}, Child scale: {winningCardGameObject.transform.localScale}");
                
                // Reset scale to 1.0 on the CHILD (the actual card visual)
                winningCardGameObject.transform.localScale = Vector3.one;
                Debug.Log($"AFTER reset - Child scale: {winningCardGameObject.transform.localScale}");
                
                Debug.Log("Scaling up winning card (child GameObject)");
                // Pop up animation - scale up the CHILD GameObject
                actionManager.AddAction(new ScaleAction(
                    winningCardGameObject,  // ← Scale the CHILD, not the parent!
                    new Vector3(1.5f, 1.5f, 1.5f),
                    0.5f,  // Duration
                    0.0f,  // Delay
                    easeFunction: Easing.EaseOutBack,
                    true   // Blocking
                ));
                
                // Jiggle animation - rotate the CHILD (where the card visual is after flip)
                // Capture the starting rotation of the CHILD after it's been flipped
                Vector3 childStartRotation = winningCardGameObject.transform.eulerAngles;
                Debug.Log($"Starting jiggle from CHILD rotation: {childStartRotation}");
                
                // Jiggle right (+15 degrees on Z)
                actionManager.AddAction(new RotateAction(
                    winningCardGameObject,  // Rotate the CHILD, not parent!
                    new Vector3(childStartRotation.x, childStartRotation.y, childStartRotation.z + 15),
                    0.15f,
                    0.0f,
                    easeFunction: Easing.EaseInOutCubic,
                    true
                ));
                
                // Jiggle left (-15 degrees from center)
                actionManager.AddAction(new RotateAction(
                    winningCardGameObject,  // Rotate the CHILD, not parent!
                    new Vector3(childStartRotation.x, childStartRotation.y, childStartRotation.z - 15),
                    0.15f,
                    0.0f,
                    easeFunction: Easing.EaseInOutCubic,
                    true
                ));
                
                // Return to center
                actionManager.AddAction(new RotateAction(
                    winningCardGameObject,  // Rotate the CHILD, not parent!
                    childStartRotation,  // Back to flipped rotation
                    0.15f,
                    0.0f,
                    easeFunction: Easing.EaseInOutCubic,
                    true
                ));
                
                // Scale back down on the CHILD GameObject
                actionManager.AddAction(new ScaleAction(
                    winningCardGameObject,  // ← Scale the CHILD back down!
                    new Vector3(1.0f, 1.0f, 1.0f),
                    0.5f,
                    0.0f,
                    easeFunction: Easing.EaseInBack,
                    true
                ));
            }
        }
        
        // Step 4: Move all cards to the discard field
        if (DiscardFieldReference != null)
        {
            for (int i = 0; i < _discardDeck.Count; i++)
            {
                Vector3 discardPosition = DiscardFieldReference.transform.position + DiscardDeckSpacing * i;
                actionManager.AddAction(new TranslateAction(
                    _discardDeck[i],
                    discardPosition,
                    0.7f,  // Duration
                    i * 0.05f,  // Slight stagger
                    easeFunction: Easing.EaseInOutCubic,
                    false
                ));
            }
            actionManager.AddAction(new BlockAction(0.7f + (_discardDeck.Count * 0.05f)));
        }
        
        // Step 5: Winner draws a card from the draw deck
        if (winningIndex >= 0 && DrawDeck.Count > 0)
        {
            int winningPlayerIndex = winningIndex; // The index matches the player who played that card
            GameObject drawnCard = DrawDeck[DrawDeck.Count - 1];
            DrawDeck.RemoveAt(DrawDeck.Count - 1);
            
            // Get the position for the new card in the winner's hand
            PlayerCurve winnerCurve = _playerCurves[winningPlayerIndex];
            (Vector3, float)[] handPositions = winnerCurve.CalculateCardPositions(_playerHands[winningPlayerIndex].Count + 1);
            
            // Animate card to winner's hand
            Card drawnCardComponent = drawnCard.GetComponentInChildren<Card>();
            bool shouldBeFlipped = (winningPlayerIndex != 0); // Player 0 sees face up, others face down
            
            AnimateCardToPosition(drawnCard, handPositions[handPositions.Length - 1].Item1, 
                                handPositions[handPositions.Length - 1].Item2, shouldBeFlipped);
            
            _playerHands[winningPlayerIndex].Add(drawnCard);
            
            // Realign the winner's hand
            actionManager.AddAction(new CallBackAction(0.5f, false, () => {
                AlignHandCards(winningPlayerIndex);
            }));
        }
            
        // Step 6: Clear the discard deck and reset for next round
        actionManager.AddAction(new CallBackAction(1.0f, false, () => {
            _discardDeck.Clear();
            _turn = 0; // Reset to first player
            _lastExecutedTurn = -1; // Reset turn tracker
            _roundInProgress = false; // Allow next round to start
        }));
    }
  
    
    
    
    
    
    
    
} // end GameManager class





#region HelperClasses_and_Structs

public static class ListExtensions
{
    // Fisher-Yates shuffle algorithm - works with any List<T>
    public static void Shuffle<T>(this List<T> list)
    {
        System.Random random = new System.Random();
        int n = list.Count;
        
        for (int i = n - 1; i > 0; i--)
        {
            // Pick a random index from 0 to i
            int j = random.Next(0, i + 1);
            
            // Swap elements at positions i and j
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}

#endregion // HelperClasses_and_Structs

