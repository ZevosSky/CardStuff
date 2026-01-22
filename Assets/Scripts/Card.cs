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
using UnityEngine.UIElements;


public class Card : MonoBehaviour
{
    [SerializeField] private SpriteRenderer frontRenderer;
    [SerializeField] private SpriteRenderer backRenderer;
    [SerializeField] private BoxCollider hoverCollider; 
    
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
    
    [HideInInspector] public ActionManager actionManager; // reference to action manager in play space 
    [HideInInspector] public bool isHoverAble; // control if hover does anything
    [HideInInspector] public bool isHovered; 
    [HideInInspector] public GameManager gameManager;
    
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
        UpdateCardSprites(); // update the sprite to the correct one when initialized
        
    }
    
    void OnHoverEnter()
    {
        // Debug.Log("OnHoverEnter");  
        if (actionManager == null) return;
        
      
        ScaleAction sa = new ScaleAction(this.gameObject, 
            (new Vector3(1.4f, 1.4f, 1.4f)),
            0.5f,
            0.0f, 
            easeFunction: Easing.EaseOutElastic,
            false);
    
        actionManager.AddAction(sa);
        isHovered = true;
    }

    void OnHoverExit()
    {
        if (actionManager == null) return;
        
        isHovered = false;
        
        ScaleAction sa = new ScaleAction(this.gameObject, 
            (new Vector3(1.0f, 1.0f, 1.0f)),
            0.5f,
            0.0f, 
            easeFunction: Easing.EaseOutElastic,
            false);
        actionManager.AddAction(sa);
        
    }
    
    void Update()
    {
        if (gameManager._isPaused || gameManager._allowInteraction == false) return; 
        
        Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);

        // if (Physics.Raycast(r, out RaycastHit hit1))
        // {
        //     Debug.Log("Hit: " + hit1.collider.gameObject.name);
        // }
        
        if (Physics.Raycast(r, out RaycastHit hit) &&
            hit.collider == hoverCollider)
        {
            if (!isHovered)
            {
                isHovered = true;
                
                OnHoverEnter();
            }
        }
        else if (isHovered)
        {
            isHovered = false;
            OnHoverExit();
        }
        
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

