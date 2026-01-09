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

public class PlayerState {
    public List<GameObject> hand;
    public PlayerCurve curve;
    
    public PlayerState()
    {
        hand = new List<GameObject>();
        curve = null;
    }
    
    public PlayerState(List<GameObject> hand, PlayerCurve curve)
    {
        this.hand = hand;
        this.curve = curve;
    }

    public PlayerState(PlayerCurve c)
    {
        this.curve = c;
    }
    
    // I technically could have a solver for this since it would
    // just be copying pasting rumikub from data strucutures...
    // this is funny to me but outside of need for this project 
}



//==| Rules of Rummy |==================================================================================================|
/*
    Basic Rummy (Gin Rummy Variation)
    Number of Players: 2–6
    Deck: Standard 52-card deck 
    Starting Hand: Usually 10 cards per player
    Objective: Form sets (three or four of a kind) and runs (three or more consecutive cards of the same suit).
    Gameplay:
    ( original draw deck is face down )
    Players take turns drawing and discarding cards.
    The round ends when a player has formed valid sets and runs, or when the draw pile runs out.
    Scoring is based on the value of unmatched cards left in opponents' hands.

    In Gin Rummy (and most Rummy variations), the discarded cards go into a face-up discard pile, but only the top card
    is available to be picked up by the next player. Players cannot freely swap cards from a pool.
*/
//==| Game Manager |===================================================================================================|
public class GameManager : MonoBehaviour
{
    [Header("Action to the Scene Action Manager")]
    [Tooltip("In charge of all actions that happen within the scene")]
    [SerializeField] private ActionManager actionManager;
    [SerializeField] private GameObject cardPrefab;
    
    //==| Game Setting Properties |=====================================================================================|
    [Header("Game Settings")]
    [SerializeField] // NOTE: This can not be changed via the inspector in play mode
    [Range(2, 6)]    // We could have more players but if we do I'm going to have to make it 2+ decks 
    private int playerCount = 4;               // number of players at the table 
    
    [SerializeField]
    [Range(1, 3)]
    private int CardSize = 1;    // Base size of the card
    private const int CardSizeMin = 1;    // Base size of the card
    private const int CardSizeMax = 3;    // Max size of the 
    private bool cardSizeDirty = false;   // if the card size has been changed
    
    // Play Field Locations
    [Header ("Card Location References")]
    [SerializeField] private GameObject DrawDeckLocationReference; // where the draw deck is located
    [SerializeField] private GameObject DiscardDeckLocationReference; // where the discard deck is located
    
    
    // * * * Game Data * * * \\
    //==| Places to put cards |=========================================================================================|
    
    private List<GameObject> WholeDeck = new List<GameObject>();  // deck of ALL cards
    private List<GameObject> DrawDeck = new List<GameObject>();   // deck of cards to draw from
    private List<GameObject> SwapDeck = new List<GameObject>();   // deck of cards that have been discarded / swapped
    // private List<PlayerState> playerStates = new List<PlayState>();    // players at the table
    
    [SerializeField] private PlaySpace playSpace; // reference to the play space script in the scene (in charge of player positions)
    
    
    //==| Misc Game Data |==============================================================================================|
    [Header("Deck Spacing Properties")]
    [SerializeField] 
    [Tooltip("Spacing between cards in the draw deck (x, y, -z)")]
    private Vector3 DrawDeckSpacing = new Vector3(0.003f, 0.003f, -0.05f); 
    [SerializeField]
    [Tooltip("Spacing between cards in the swap deck hand (x, y, -z)")]
    private Vector3 DiscardDeckSpacing = new Vector3(0.00f, 0.001f, -0.05f); // spacing between cards in the discard deck
    
    //==| Game State |==================================================================================================|
    [DoNotSerialize] public int turn;
    [DoNotSerialize] public bool IsPaused;
    [DoNotSerialize] public bool AllowInput;
    //==| Unity Functions |=============================================================================================|
    bool GetIsPaused() { return IsPaused; }
    
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
        
        
        for (int i = 0; i < playerCount; ++i) {
            Debug.Log($"Player {i} rotation: {playerPositions[i].transform.eulerAngles}");
            PlayerCurve playerCurve = new PlayerCurve(playerPositions[i].transform, 3, 1, 0.01f);
            //playerStates.Add(new PlayerState(playerCurve));
            (Vector3, float)[] handPositions = playerCurve.CalculateCardPositions(5);

            for (int j = 0; j < 5; ++j)
            {
                GameObject card = DrawDeck[DrawDeck.Count - 1];
                DrawDeck.RemoveAt(DrawDeck.Count - 1);
                var cc = card.GetComponentInChildren<Card>();

                //Debug.Log($"Player {i} transform position: {playerPositions[i].transform.position}, rotation: {playerPositions[i].transform.rotation.eulerAngles}, scale: {playerPositions[i].transform.localScale}");
                // Apply Z rotation while preserving X rotation (flipped state)
                if (i == 0) cc.faceUp = false;
                else cc.faceUp = true;
                AnimateCardToPosition(card, handPositions[j].Item1, handPositions[j].Item2, (i == 0) ? false : true);
            }
        } // end for loop
        
        
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
        
        // if there are no cards in the draw deck, end the game
        if (DrawDeck.Count > 0)
        {
            // calculate who won the game
            // display a message
            // tell the player to reset from the pause menu 
        }
        
        if (turn == 0 && !AutoPlay) // player turn 
        {
            // Wait for player action 
            
            // phase 1...
            
            // phase 2...
            
        }
        else // computer turn, play randomly 
        {
            // get the "player" we are playing as
            var playerPositions = playSpace.GetPlayerObjectReferences();
            GameObject player = playerPositions[turn];
            PlayerCurve playerCurve = new PlayerCurve(player.transform, 3, 1, 0.01f);
            
            // phase 1: draw a card from ether the draw deck or the swap deck
            
            
            
            switch (Random.Range(0, 2)) // 0 or 1
            {
                case 0: // draw from draw deck
                    if (DrawDeck.Count > 0)
                    {
                        GameObject card = DrawDeck[DrawDeck.Count - 1];
                        DrawDeck.RemoveAt(DrawDeck.Count - 1);
                        AddCardToSwapDeck(card);
                    }
                    break;
                case 1: // draw from swap deck
                    if (SwapDeck.Count > 0)
                    {
                        GameObject card = SwapDeck[SwapDeck.Count - 1];
                        SwapDeck.RemoveAt(SwapDeck.Count - 1);
                        AddCardToSwapDeck(card);
                    }
                    break;
                
            }
            
            


            turn = (turn + 1) % (playerCount - 1); // increment turn
        }



    }
    #endregion // UnityFunctions
    
    
    
    #region DeckFunctions
    // Spawn a deck of cards 
    void SpawnDeck()
    {
        for (int i = 0; i < 52; i++)
        {
            WholeDeck.Add(Instantiate(cardPrefab));
            Card current = WholeDeck[i].GetComponentInChildren<Card>();
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
            AddCardToDrawDeck(cards[i]);
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
            eulerAngles,  // Euler angles, already correctly formatted for your RotateAction
            0.5f,         // Duration
            0.0f,         // Delay
            easeFunction: Easing.EaseOutCubic,
            false         // Not blocking
        ));
    
        // Add to action manager
        actionManager.AddAction(simultaneous);
    }
    
    #endregion
    
    #region EnableDisableFunctions

    void RenableInputAfterActions() 
    {
        CallBackBlockAction enableInputAfterActions = new CallBackBlockAction(
            0.1f, () => { AllowInput = true; }, false);
        actionManager.AddAction(enableInputAfterActions);
    }
    
    
    #endregion
    
}

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

