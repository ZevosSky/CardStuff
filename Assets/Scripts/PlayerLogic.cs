

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
        AnimateCardToPosition(cardObject, calculatedPosition, 0, false);
        actionManager.AddAction( new BlockAction(0.5f));
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
                    _turn = (_turn + 1) % playerCount; // advance turn
                    break;
                }
            }
        }
        
        // Update player hand & re center cards

    }

   
    
    
    
    
    
    // if it's an AI player's turn, have them play a card
    private void AITurn()
    {
        
    }
}