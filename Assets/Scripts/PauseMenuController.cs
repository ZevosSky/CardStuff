using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable] 
public struct GameObjectPair
{
    public GameObject objectToTransform;
    public GameObject whereTheyGo;
}

public class PauseMenuController : MonoBehaviour
{
    
    
    // This holds the objects that need to be toggled (droped down and fade in when the pause menu is activated, and then put back when the pause menu is deactivated)
    [Header("Pair object with where it should animate to when the pause menu is activated")]
    [SerializeField] private List<GameObjectPair> _objectsToToggle = new List<GameObjectPair>();
    
    [DoNotSerialize] private List<Transform> _originalPositions = new List<Transform>();

    [DoNotSerialize] private bool _activated = false; 
    
    [DoNotSerialize] private bool _isAnimating = false;

    [SerializeField] private ActionManager _actionManager; 
    
    
    // Start is called before the first frame update
    void Start()
    {
        // for each object pair cache the original position of the object transform so we can 
        // put it back when the pause menu is deactivated
        foreach (GameObjectPair pair in _objectsToToggle) {
            _originalPositions.Add(pair.objectToTransform.transform);
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        if (_isAnimating) { return; } // if we're in the middle of an animation, ignore input to prevent spamming the pause menu

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_activated) {
                DeactivatePauseMenu();
                 
            } else {
                ActivatePauseMenu();
            }
        }
    }

    private void ActivatePauseMenu()
    {
        _activated = true; 
        _isAnimating = true;

        foreach (GameObjectPair pair in _objectsToToggle)
        {
            _actionManager.AddAction(new TranslateAction(
                pair.objectToTransform,
                pair.whereTheyGo.transform.position,
                0.5f,
                0.0f,
                Easing.EaseInBounce,
                false
            ));
        }
    }

    private void DeactivatePauseMenu()
    {
        _activated = false;
        _isAnimating = true;
        
    }
}
