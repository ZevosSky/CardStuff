using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class HoverManager : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private ActionManager hoverActionManager; // Dedicated ActionManager for hover animations
    [SerializeField] private LayerMask hoverMask;
    [SerializeField] private float maxDistance = 100f;

    private Card hovered;
    
    void Update()
    {
        if (gameManager == null) return;
        
        if (!gameManager._allowInteraction || gameManager.IsPaused())
        {
            ForceUnhover();
            return;
        }
        
        Card next = RaycastCard();
        if (next == hovered) return;

        if (hovered != null)
        {
            Debug.Log("Unhovering " + hovered.name);
            hovered.HoverExit();
        }
        hovered = next;
        if (hovered != null)
        {
            // Assign the hover action manager to the card
            if (hoverActionManager != null)
            {
                hovered.hoverActionManager = hoverActionManager;
            }
            
            Debug.Log("hovering" + hovered.name);
            hovered.HoverEnter();
        }
    }

    private void ForceUnhover()
    {
        if (hovered != null)
        {
            hovered.HoverExit();
            hovered = null;
        }
    }

    private Card RaycastCard()
    {
        if (Camera.main == null)
        {
            Debug.LogError("Camera.main is null! Make sure your camera is tagged as 'MainCamera'");
            return null;
        }

        Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
        //Debug.DrawRay(r.origin, r.direction * maxDistance, Color.red, 0.1f);

        if (Physics.Raycast(r, out RaycastHit hit, maxDistance, hoverMask, QueryTriggerInteraction.Ignore))
        {
            // Debug.Log($"Raycast hit: {hit.collider.gameObject.name} on layer {LayerMask.LayerToName(hit.collider.gameObject.layer)} at distance {hit.distance}");
            
            // Changed from GetComponentInParent to GetComponentInChildren to match PlayerLogic
            GameObject parent = hit.collider.transform.parent.gameObject;
            Card card = parent.GetComponentInChildren<Card>();
            
            if (card == null)
            {
                Debug.LogWarning($"Hit object {hit.collider.gameObject.name} but couldn't find Card component in children");
            }
            return card;
        }

        return null;
    }

}
