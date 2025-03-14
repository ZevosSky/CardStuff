// ---------------------------------------------------------------------------------------------------------------------
// File:          CardStack.cs
// Author:        Gary Yang
// Created:       2025-02-28
// Description:   A basic class to represent a stack of cards and various operations that can be performed on it.
// Requirements:  - Card.cs, ActionManager.cs 
// ---------------------------------------------------------------------------------------------------------------------
// Last Modified: 2025-03-01

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Base class for a card container going to split it into stack and hand options later 
public class CardStack : MonoBehaviour
{
    //===| Serialized Interface |=======================================================================================
    [SerializeField] private bool faceUpCards = false;
    [SerializeField] private Vector3 deckOffset;
    [SerializeField] private ActionManager actionManager;
    
    //===| Data Structure |=============================================================================================
    private List<GameObject> cards = new List<GameObject>();
    public int Count { get { return cards.Count; } }
    
    
    //===| Public Interface |===========================================================================================
    public void AddCardTop(GameObject card) 
    {
        cards.Add(card);
    }
    
    
    
    
    //===| Unity Events |===============================================================================================
    // NOT Needed for this class
}
