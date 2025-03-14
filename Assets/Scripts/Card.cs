//==============================================================================
// @Author: Gary Yang
// @File: Card.cs
// @brief: Card Script for basic poker cards, comes with functions loadable into
//         an action list
// @copyright DigiPen(C) 2025
//==============================================================================

// Card.cs - Sprite control for card sprites, also keeps a little bit of card state data for the card game
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Serialization;


public class Card : MonoBehaviour
{
    [SerializeField] private SpriteRenderer frontRenderer;
    [SerializeField] private SpriteRenderer backRenderer;
    
    // Enums for card properties
    public enum Suit { Hearts, Diamonds, Clubs, Spades }
    public enum Rank { Ace, Two, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King }
    public enum CardBack { Red, Blue, Black }
    
    [FormerlySerializedAs("faceup")] public bool  faceUp = true; 
                            // WARNING: & IMPORTANT: THIS value is not defaulting properly, it is getting hard set in
                            // awake, if you want the logic default to be different it has to be changed there. I'm
                            // not sure why this is, but I'm here to make it work not wonder in mystic awe. 
    
    [Header("Card Info")]
    public Suit suit;
    public Rank rank;
    public int backStyle; // 0-3 for different back designs
    
    
    
    // Get the SpriteRenderers for the front and back of the card
    void Awake() 
    {
        faceUp = true; // default to face up
        frontRenderer = transform.Find("CardFront").GetComponent<SpriteRenderer>();
        backRenderer = transform.Find("CardBack").GetComponent<SpriteRenderer>();
        
            
        if (frontRenderer == null || backRenderer == null)
            Debug.LogError("Card renderers not found!");
    }
    

    private void Start()
    {
        
   
        UpdateCardSprites();
    }
    
    void Update()
    {

    }
    
    public void UpdateCardSprites()
    {
        // Debug.Log($"Card Index: {((int)suit * 13) + (int)rank}");
        frontRenderer.sprite = GetSprite(((int)suit * 13) + (int)rank);
        backRenderer.sprite = GetBackSprite(backStyle);
    }
    
    private Sprite GetSprite(int index)
    {
        // Load from sprite sheet asset
        Sprite[] sprites = Resources.LoadAll<Sprite>("Playing_Cards");
        return sprites[index];
    }

    private Sprite GetBackSprite(int style)
    {
        Sprite[] sprites = Resources.LoadAll<Sprite>("Playing_Cards");
        return sprites[52 + 13 + style];
    }


    
    
    // Method to set card values and update sprites
    public void SetCard(Suit newSuit, Rank newRank, int newBackStyle)
    {
        suit = newSuit;
        rank = newRank;
        backStyle = Mathf.Clamp(newBackStyle, 0, 3);
        UpdateCardSprites();
    }
    public void SetCard(Suit newSuit, Rank newRank, CardBack newBackStyle)
    {
        suit = newSuit;
        rank = newRank;
        backStyle = Mathf.Clamp((int)newBackStyle, 0, 3);
        UpdateCardSprites();
    }
    
}

