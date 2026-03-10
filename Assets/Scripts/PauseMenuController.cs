using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable] 
public class GameObjectPair
{
    public GameObject objectToTransform;
    public Vector3 targetPosition;       // where it animates TO (set via gizmo handle)
    [HideInInspector] public Vector3 originalPosition; // cached at Start, set automatically
}

public class PauseMenuController : MonoBehaviour
{
    
    
    // This holds the objects that need to be toggled (droped down and fade in when the pause menu is activated, and then put back when the pause menu is deactivated)
    [Header("Pair object with where it should animate to when the pause menu is activated")]
    public List<GameObjectPair> _objectsToToggle = new List<GameObjectPair>();
    

    public bool IsAnimating() { return _isAnimating; }
    public bool IsActivated() { return _activated; }



    [Header("Reference to the action manager to add the pause menu animations to")]
    [SerializeField] public ActionManager _actionManager;
    
    

    [DoNotSerialize] private bool _activated = false; 
    
    [DoNotSerialize] private bool _isAnimating = false;
    
    [Header("Sliders for size & timescale options")] 
    [SerializeField] private Slider _timeScaleSlider;
    public float GetTimeScale() { return _timeScaleSlider.value; }
    
    [SerializeField] private Slider _cardSizeSlider;
    public float GetCardSize() { return _cardSizeSlider.value; }
    
    
    // Start is called before the first frame update
    void Start()
    {
        // Cache the original position of each object so we can return to it on deactivate
        foreach (GameObjectPair pair in _objectsToToggle)
        {
            if (pair.objectToTransform != null)
                pair.originalPosition = pair.objectToTransform.transform.position;
            
            
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
        const float animationDuration = 1.5f;
        foreach (GameObjectPair pair in _objectsToToggle)
        {
            _actionManager.AddAction(new TranslateAction(
                pair.objectToTransform,
                pair.targetPosition,
                animationDuration,
                0.0f,
                Easing.EaseInBounce,
                false
            ));
            _actionManager.AddAction(new FadeAction(
                pair.objectToTransform,
                1.0f,
                animationDuration,
                0.0f,
                Easing.EaseInCirc,
                false
            ));
        }
            
        _actionManager.AddAction(new CallBackAction(
            animationDuration,
            true,
            () => { _isAnimating = false; }
        ));
    }

    private void DeactivatePauseMenu()
    {
        _activated = false;
        _isAnimating = true;

        foreach (GameObjectPair pair in _objectsToToggle)
        {
            _actionManager.AddAction(new TranslateAction(
                pair.objectToTransform,
                pair.originalPosition,
                1.5f,
                0.0f,
                Easing.EaseOutBounce,
                false
            ));
            _actionManager.AddAction(new FadeAction(
                pair.objectToTransform,
                0.0f,
                1.5f,
                0.0f,
                Easing.EaseInCirc,
                false
            ));
        }
        
        _actionManager.AddAction(new CallBackAction(
            1.5f,
            true,
            () => { _isAnimating = false; }
        ));
        
    }
    
    // Call this from a UI button to resume — same behaviour as pressing Escape while paused
    public void Resume()
    {
        if (_isAnimating || !_activated) return;
        DeactivatePauseMenu();
    }
    
    // Public method to programmatically activate pause menu (for stress testing)
    public void Activate()
    {
        if (_isAnimating || _activated) return;
        ActivatePauseMenu();
    }
    
    // Public method to programmatically deactivate pause menu (for stress testing)
    public void Deactivate()
    {
        if (_isAnimating || !_activated) return;
        DeactivatePauseMenu();
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (_objectsToToggle == null) return;

        foreach (GameObjectPair pair in _objectsToToggle)
        {
            if (pair.objectToTransform == null) continue;

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(pair.targetPosition, 0.3f);

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(pair.objectToTransform.transform.position, pair.targetPosition);
        }
    }
#endif
}
