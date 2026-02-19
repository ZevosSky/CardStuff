

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class GameManager
{
    // Card has been played, move it to the play area
    private void PlayCard(int playerIndex, GameObject cardObject, GameObject playZone) 
    {        // Update state, move card to play area 
        
        Debug.Log("Player " + playerIndex + " played a card.");
        // remove card from players hand
        int cardIndex = CardIndexInHand(cardObject, playerIndex); // find which card in hand
        _playerHands[playerIndex].RemoveAt(cardIndex);
        
        // add it to play zone

        // Move card to play zone position 
        Vector3 calculatedPosition = playZone.transform.position;

        const float xOffsetPerCard = 0.5f;
        calculatedPosition.x += _discardDeck.Count * xOffsetPerCard; // slight offset for stacking effect
        calculatedPosition.z -= _discardDeck.Count * 0.1f + 0.01f;

        _discardDeck.Add(cardObject);
        
        // Update card state based on player
        Card cardComponent = cardObject.GetComponentInChildren<Card>();
        if (cardComponent != null)
        {
            // AI players (not player 0) play cards face-down, player 0 plays face-up
            if (playerIndex == 0)
                cardComponent.faceUp = true;  // Player 0's card is face-up
            else
                cardComponent.faceUp = false; // AI players' cards are face-down
        }
        
        // AI players (not player 0) play cards face-down, player 0 plays face-up
        bool shouldBeFaceDown = (playerIndex != 0);
        AnimateCardToPosition(cardObject, calculatedPosition, 0, shouldBeFaceDown);
        actionManager.AddAction( new BlockAction(0.5f));
        
        // Realign remaining cards in hand
        AlignHandCards(playerIndex);
    }

    
    
    private int CardIndexInHand(GameObject cardPrefab, int playerIndex)
    {
        for (int i = 0; i < _playerHands[playerIndex].Count; i++)
        {
            if (_playerHands[playerIndex][i] == cardPrefab)
                return i;
        }

        return -1; // not found 
    }

    // if a card is selected, in the player's hand, play it (player is always index 0) 
    private void CheckIfCardSelected()
    {
        // get player hand 
        var playerPositions = playSpace.GetPlayerObjectReferences();
        playerCount = playSpace.GetPlayerCount();

        if (_playerHands[0].Count >= 1)
        {
            foreach (var card in _playerHands[0])
            {
                Card cardComponent = card.GetComponentInChildren<Card>();
                BoxCollider boxCollider = card.GetComponentInChildren<BoxCollider>();
                Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
                
                if (Physics.Raycast(r, out RaycastHit hit) &&
                    hit.collider == boxCollider &&
                    Input.GetMouseButtonDown(0))
                {
                    PlayCard(0, card, playSpace.playZoneReference);
                    _turn = _turn + 1; // advance turn (let it reach playerCount to trigger end of round)
                    break;
                }
            }
        }
        
        // Update player hand & re center cards

    }

   
    
    // recenter player hand cards after a card left the hand or something else happend
    void AlignHandCards(int playerIndex)
    {
        if (_playerHands[playerIndex].Count == 0) return;
        if (playerIndex < 0 || playerIndex >= playerCount) return;
        
        // Get the number of cards in the player's hand
        int cardCount = _playerHands[playerIndex].Count;
        
        // Calculate new positions for all cards using the stored PlayerCurve
        (Vector3, float)[] handPositions = _playerCurves[playerIndex].CalculateCardPositions(cardCount);
        
        // Animate each card to its new position
        for (int i = 0; i < cardCount; i++)
        {
            GameObject card = _playerHands[playerIndex][i];
            Card cardComponent = card.GetComponentInChildren<Card>();
            
            // Determine if card should be flipped (player 0 sees their cards face-up)
            bool isFlipped = (playerIndex != 0);
            
            // Animate card to new position
            AnimateCardToPositionNoBlock(card, handPositions[i].Item1, handPositions[i].Item2, isFlipped);
        }
    }
    
    
    
    // if it's an AI player's turn, have them play a card
    private void AITurn()
    {
        // Only execute once per turn number
        if (_lastExecutedTurn == _turn) return;
        
        // Check if current AI player has cards to play
        if (_playerHands[_turn].Count == 0)
        {
            _turn = _turn + 1; // Skip to next player if no cards
            return;
        }
        
        // Mark this turn as executed
        _lastExecutedTurn = _turn;
        
        // Block player interaction while AI is playing
        BlockInteraction(2.0f); // Block for full duration of AI turn
        
        // Select a random card from the AI's hand
        int randomCardIndex = Random.Range(0, _playerHands[_turn].Count);
        GameObject cardToPlay = _playerHands[_turn][randomCardIndex];
        
        // Add a small delay before AI plays (makes it more natural)
        actionManager.AddAction(new BlockAction(0.5f));
        
        // Play the selected card
        PlayCard(_turn, cardToPlay, playSpace.playZoneReference);
        
        // Add another delay after playing for visual clarity, then advance turn
        actionManager.AddAction(new CallBackAction(
            0.5f, 
            false, 
            () => {
                _turn = _turn + 1; // Advance to next player's turn (let it reach playerCount to trigger end of round)
            }
        ));
    }
}