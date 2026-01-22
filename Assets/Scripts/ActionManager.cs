//==============================================================================
// @Author: Gary Yang
// @File: ActionList.cs
// @brief: action list stuff for card game
// @copyright DigiPen(C) 2025
//==============================================================================


//===| Includes | =================================================================

using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;


//( action class to "list" )
//===| Action Class |==============================================================

#region Actions 

public abstract class ActionBase
{
    public float duration; // total duration of the action
    public float delay;    // delay before starting the action
    public bool isBlocking;// if true, blocks subsequent actions until complete
    public System.Action onComplete;
    public bool isComplete;
    public float timeScale = 1f;
    protected ActionManager manager;             // Reference to manager for global timescale
    protected bool isPartOfSimultaneous = false; // track if the action is part of a simultaneous action

    protected bool hasStarted = false; // track if the action started or not 
    protected float delayTimer = 0.0f; // delay timer 
    protected float elapsed = 0.0f;    // track elapsed time separately from delay
    protected float deltaTime => Time.deltaTime * timeScale * (manager != null ? manager.globalTimeScale : 1f);
    
    public void SetManager(ActionManager manager)
    {
        this.manager = manager;
    }
    
    public void SetSimultaneousFlag(bool isSimultaneous)
    {
        this.isPartOfSimultaneous = isSimultaneous;
    }
    
    public virtual void Start() // IMPORTANT: for the delay to work properly inherited need to call base.Start() 
    {                           //            if they override this class. 
        delayTimer = 0f;
        elapsed = 0f;
        hasStarted = false;
        isComplete = false; // Ensure we reset this when restarting
    }

    public virtual bool UpdateDelay()
    {
        if (hasStarted) return true; // delay is complete, return true
        
        delayTimer += deltaTime;
        
        if (delayTimer >= delay)
        {
            hasStarted = true;
            OnDelayComplete();
            return true;
        }
        return false;
    }

    public virtual void Update()
    {
        if (hasStarted)
        {
            elapsed += deltaTime;
            if (elapsed >= duration)
            {
                isComplete = true;
            }
        }
    }
    protected virtual void OnDelayComplete() { }

    public virtual string GetActionState()
    {
        return $"{GetType().Name} - Duration: {duration}, ElapsedTime: {deltaTime}, Delay: {delay}, " +
               $"ElapsedDelay: {delayTimer} Blocking: {isBlocking} ";
    }

}

/// <summary>
/// This action is to be used at the end of a sequence of actions to block the action list until the end of the sequence
/// time should be probably set to or more than the duration of the sequence including delays
/// </summary>
public class BlockAction : ActionBase
{
    public BlockAction(float duration)
    {
        this.duration = duration;
        this.isBlocking = true;
    }
    
    public override void Update()
    {
        duration -= deltaTime;
        isComplete = duration <= 0f;
    }
}

// Callback action to trigger a function when the action completes
public class CallBackAction : ActionBase
{
    public CallBackAction(float duration, bool isBlocking, System.Action callback)
    {
        this.duration = duration;
        this.onComplete = callback;
        this.isBlocking = isBlocking;
    }
    
}

//===| Game Object Level actions|=================================================
// GameObject-level actions inherit from ActionBase, designed to act on a GameObject
public abstract class GameObjectAction : ActionBase
{
    protected readonly GameObject targetObject;
    protected readonly Transform transform;     // Cached for performance 
    protected Vector3 startPosition;
    protected Quaternion startRotation;
    protected Vector3 startScale;

    protected GameObjectAction(GameObject target)
    {
        this.targetObject = target;
        this.transform = target.transform;
    }
    
    public GameObject GetTargetObject() => targetObject;
    
    public override void Start()
    {
        base.Start();
        // Cache the starting transform values
        CaptureCurrentTransform();
    }
    
    protected void CaptureCurrentTransform()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;
        startScale = transform.localScale;
    }
    
    // Override OnDelayComplete to recapture transform after delay
    protected override void OnDelayComplete()
    {
        // Recapture all transforms when delay completes
        CaptureCurrentTransform();
    }

    
}


//===| SRT Actions |====================================================================================================|
#region ScaleRotateTranslateActions
// README/ IMPORTANT: all these actions are based on absolute world position and rotation, not local 
//                   or relative position and rotation.

public delegate float EaseFunction(float t);

/// <summary>
/// Scale a game object based to a target size
/// </summary>
/// 
public class ScaleAction : GameObjectAction
{
    private Vector3 targetScale;
    private EaseFunction easeFunction;
    
    public Vector3 GetTargetScale() => targetScale;
    public EaseFunction GetEaseFunction() => easeFunction;

    public ScaleAction( GameObject entity,
                        Vector3 targetScale, 
                        float duration, 
                        float delay = 0f, 
                        EaseFunction easeFunction = null,
                        bool blocking = true ) 
        : base(entity)
    {
        this.targetScale = targetScale;
        this.duration = duration;
        this.delay = delay;
        this.isBlocking = blocking;
        this.easeFunction = easeFunction ?? Easing.Linear;
    }
    
    public override void Start()
    {
        base.Start();
        
        if (!isPartOfSimultaneous)
        {
            onComplete = () => targetObject.transform.localScale = targetScale;
        }
    }


    public override void Update()
    {
        base.Update(); // Call base update to handle elapsed time and completion
        
        // Only update if we've passed the delay
        if (!hasStarted) return;
        
        // Use elapsed from base class
        float t = Mathf.Clamp01(elapsed / duration);
        float easedT = easeFunction(t);
        
        if (t >= 1.0f)
        {
            targetObject.transform.localScale = targetScale; // Ensure we reach exactly the target
            return;
        }

        // Use startScale from the base class
        targetObject.transform.localScale = Vector3.LerpUnclamped(startScale, targetScale, easedT);
    }
} // end of ScaleAction


/// <summary>
/// Translate a game object to a target position with optional easing, duration, and delay
/// </summary>
public class TranslateAction : GameObjectAction
{
    
    private Vector3 targetPos;
    private EaseFunction easeFunction;
    
    public Vector3 GetTargetPosition() => targetPos;
    public EaseFunction GetEaseFunction() => easeFunction;
    
    // Constructor to create a TranslateAction
    public TranslateAction(GameObject entity, Vector3 target, float duration, float delay = 0f, EaseFunction easeFunction = null, bool blocking = true) 
        : base(entity)
    {
        this.targetPos = target;
        this.duration = duration;
        this.delay = delay;
        this.isBlocking = blocking;
        this.easeFunction = easeFunction ?? Easing.Linear;
    }
    public override void Start()
    {
        base.Start();
        
        if (!isPartOfSimultaneous)
        {
            onComplete = () => targetObject.transform.position = targetPos;
        }
    }
    public override void Update()
    {
        base.Update();
        
        if (!hasStarted) return;
        
        float t = Mathf.Clamp01(elapsed / duration);
        float easedT = easeFunction(t);
        
        if (t >= 1.0f)
        {
            targetObject.transform.position = targetPos;
            return;
        }

        // Use startPosition from the base class
        targetObject.transform.position = Vector3.LerpUnclamped(startPosition, targetPos, easedT);
    }
    
    // protected override void OnDelayComplete()
    // {
    //     // Recapture start position when delay completes
    //     // This ensures we start from current position after delay
    //     startPos = targetObject.transform.position;
    // }


} // end of TranslateAction

/// <summary>
/// Specify a target rotation for a game object
/// NOTE: this is an absolute rotation, not a relative rotation this is the target rotation
/// </summary>
public class RotateAction : GameObjectAction
{
    private Quaternion targetRotation;
    private EaseFunction easeFunction;
    private Vector3 targetEulerAngles;
    
    public Vector3 GetTargetRotation() => targetEulerAngles;
    public EaseFunction GetEaseFunction() => easeFunction;

    public RotateAction( GameObject entity,
                         Vector3 targetEulerAngles, 
                         float duration, 
                         float delay = 0f, 
                         EaseFunction easeFunction = null, 
                         bool blocking = true ) 
        : base(entity)
    {
        this.targetEulerAngles = targetEulerAngles;     
        this.targetRotation = Quaternion.Euler(targetEulerAngles);
        this.duration = duration;
        this.delay = delay;
        this.isBlocking = blocking;
        this.easeFunction = easeFunction ?? Easing.Linear;
    }
    
    public override void Start()
    {
        base.Start();
        
        if (!isPartOfSimultaneous)
        {
            onComplete = () => targetObject.transform.rotation = targetRotation;
        }
    }

    public override void Update()
    {
        base.Update(); // Call base update to handle elapsed time and completion
        
        // Only update if we've passed the delay
        if (!hasStarted) return;
        
        // Use elapsed from base class - NOT incrementing here
        float t = Mathf.Clamp01(elapsed / duration);
        float easedT = easeFunction(t);
        
        if (t >= 1.0f)
        {
            targetObject.transform.rotation = targetRotation; // Ensure we reach exactly the target
            return;
        }

        targetObject.transform.rotation = Quaternion.LerpUnclamped(startRotation, targetRotation, easedT);
    }
    
    
}

#region  Quick_Flip
public class FlipAction : RotateAction
{
    public FlipAction(GameObject entity, float duration, float delay = 0f, bool blocking = false) 
        : base(entity, new Vector3(180, 0, 0), duration, delay, Easing.Linear, blocking) { }
}

public class FlipActionBouncy : RotateAction
{
    public FlipActionBouncy(GameObject entity, float duration, float delay = 0f, bool blocking = false) 
        : base(entity, new Vector3(180, 0, 0), duration, delay, Easing.EaseOutElastic, blocking) { }
}

public class Flip2DPreserveY : RotateAction
{
    public Flip2DPreserveY(GameObject entity,
                           float duration,
                           float delay = 0f,
                           bool blocking = false,
                           EaseFunction easeFunction = null) 
        : base(entity,
            new Vector3(180, 
                        entity.transform.rotation.eulerAngles.y,
                        entity.transform.rotation.eulerAngles.z), 
            duration,
            delay,
            (easeFunction == null) ? Easing.Linear : easeFunction) 
    { }
}




#endregion
public class SimultaneousTransformActions : GameObjectAction
{
    private List<GameObjectAction> actions = new List<GameObjectAction>();
    
    // Track which properties we're actually transforming
    private bool hasTranslateAction = false;
    private bool hasRotateAction = false;
    private bool hasScaleAction = false;
    
    private Vector3 finalPosition;
    private Quaternion finalRotation;
    private Vector3 finalScale;

    // Constructor
    public SimultaneousTransformActions(GameObject target, float delay = 0f, bool blocking = true) 
        : base(target)
    {
        this.delay = delay;
        this.isBlocking = blocking;
        this.duration = 0f;
        
        // Initialize final values to prevent null reference
        finalPosition = Vector3.zero;
        finalRotation = Quaternion.identity;
        finalScale = Vector3.one;
    }

    // Add an action to this simultaneous group
    public void AddAction(GameObjectAction action)
    {
        if (action.GetTargetObject() != targetObject)
        {
            Debug.LogError("Cannot add action with different target object to SimultaneousTransformActions");
            return;
        }
        
        action.SetSimultaneousFlag(true);
        actions.Add(action);
        
        // Update the duration based on this action
        duration = Mathf.Max(duration, action.duration + action.delay);
        
        // Store target values based on action type
        if (action is TranslateAction translateAction)
        {
            finalPosition = translateAction.GetTargetPosition();
            hasTranslateAction = true;
            //Debug.Log($"Added TranslateAction to {targetObject.name}: Target={finalPosition}");
        }
        else if (action is RotateAction rotateAction)
        {
            // Store the target rotation
            finalRotation = Quaternion.Euler(rotateAction.GetTargetRotation());
            hasRotateAction = true;
            //Debug.Log($"Added RotateAction to {targetObject.name}: Target={rotateAction.GetTargetRotation()}");
        }
        else if (action is ScaleAction scaleAction)
        {
            finalScale = scaleAction.GetTargetScale();
            hasScaleAction = true;
            //Debug.Log($"Added ScaleAction to {targetObject.name}: Target={finalScale}");
        }
    }

    public override void Start()
    {
        base.Start();
        
        // Capture current transform state
        startPosition = transform.position;
        startRotation = transform.rotation;
        startScale = transform.localScale;
        
        // For properties without explicit actions, use current values as final values
        if (!hasTranslateAction) finalPosition = startPosition;
        if (!hasRotateAction) finalRotation = startRotation;
        if (!hasScaleAction) finalScale = startScale;
        /*
        // Debug info
        Debug.Log($"SimultaneousTransformActions Start for {targetObject.name}:");
        Debug.Log($"  Duration={duration}, Delay={delay}");
        Debug.Log($"  hasTranslateAction={hasTranslateAction}, hasRotateAction={hasRotateAction}, hasScaleAction={hasScaleAction}");
        Debug.Log($"  Start Position: {startPosition}, Final Position: {finalPosition}");
        Debug.Log($"  Start Rotation: {startRotation.eulerAngles}, Final Rotation: {finalRotation.eulerAngles}");
        Debug.Log($"  Start Scale: {startScale}, Final Scale: {finalScale}");
        */
    }

    protected override void OnDelayComplete()
    {
        base.OnDelayComplete();
        
        // Recapture transforms in case they changed during the delay
        Vector3 oldStartPosition = startPosition;
        Quaternion oldStartRotation = startRotation;
        Vector3 oldStartScale = startScale;
        
        startPosition = transform.position;
        startRotation = transform.rotation;
        startScale = transform.localScale;
        
        // For properties without explicit actions, update final values to match new current values
        if (!hasTranslateAction) finalPosition = startPosition;
        if (!hasRotateAction) finalRotation = startRotation;
        if (!hasScaleAction) finalScale = startScale;
        
        /*
        // Debug info for transform updates
        Debug.Log($"SimultaneousTransformActions Delay Complete for {targetObject.name}:");
        if (oldStartPosition != startPosition)
            Debug.Log($"  Position changed during delay: {oldStartPosition} -> {startPosition}");
        if (oldStartRotation != startRotation)
            Debug.Log($"  Rotation changed during delay: {oldStartRotation.eulerAngles} -> {startRotation.eulerAngles}");
        if (oldStartScale != startScale)
            Debug.Log($"  Scale changed during delay: {oldStartScale} -> {startScale}");
        */
    }

    public override void Update()
    {
        base.Update(); // Call base update for proper timing
        
        // Only update if we've passed the delay
        if (!hasStarted) return;
        
        // Calculate normalized progress (0-1)
        float t = Mathf.Clamp01(elapsed / duration);
        
        // Get easing functions for each transform property
        EaseFunction translateEase = Easing.Linear;
        EaseFunction rotateEase = Easing.Linear;
        EaseFunction scaleEase = Easing.Linear;
        
        // Find easing functions from added actions
        foreach (var action in actions)
        {
            if (action is TranslateAction transAction)
                translateEase = transAction.GetEaseFunction();
            else if (action is RotateAction rotAction)
                rotateEase = rotAction.GetEaseFunction();
            else if (action is ScaleAction scaleAction)
                scaleEase = scaleAction.GetEaseFunction();
        }
        
        // Apply transform changes ONLY for actions that were explicitly added
        if (hasTranslateAction)
        {
            float translationT = translateEase(t);
            transform.position = Vector3.LerpUnclamped(startPosition, finalPosition, translationT);
        }
        
        if (hasRotateAction)
        {
            float rotationT = rotateEase(t);
            transform.rotation = Quaternion.SlerpUnclamped(startRotation, finalRotation, rotationT);
        }
        
        if (hasScaleAction)
        {
            float scaleT = scaleEase(t);
            transform.localScale = Vector3.LerpUnclamped(startScale, finalScale, scaleT);
        }
        
        // Handle completion
        if (isComplete)
        {
            // Ensure we end exactly at the target transforms, but ONLY for properties we're modifying
            if (hasTranslateAction) transform.position = finalPosition;
            if (hasRotateAction) transform.rotation = finalRotation;
            if (hasScaleAction) transform.localScale = finalScale;
            
            //Debug.Log($"SimultaneousTransformActions Complete for {targetObject.name}");
        }
    }
} // end of SimultaneousTransformActions






#endregion // End of Actions Region 

#endregion

// CallBack action 





// ( "Action List" ) 
//===| Action Manager |============================================================
public class ActionManager : MonoBehaviour
{
    private Queue<ActionBase> actionQueue = new Queue<ActionBase>();
    private List<ActionBase> activeActions = new List<ActionBase>();
    private bool islistBlocked = false;
    public float globalTimeScale = 1f;
    
    [SerializeField] private TextMeshProUGUI debugText; // Debug text for showing active actions  
    [SerializeField] private bool debugMode = false;

   void Update()
    {
        if (debugMode)
        {
            debugText.text = $"Active Actions: {activeActions.Count}" + "\n";
        }
        // Process active actions
        for (int i = activeActions.Count - 1; i >= 0; i--) 
        {
            var action = activeActions[i];
            
            if (debugMode) debugText.text += action.GetActionState() + "\n";
        
            // Check if action should start (after delay)
            if (action.UpdateDelay()) action.Update();
        
            if (action.isComplete) 
            {   
                action.onComplete?.Invoke();                
                activeActions.RemoveAt(i);
            
                // Only unblock if this was a blocking action
                if (action.isBlocking) islistBlocked = false;
            }
        }

        // Add new actions if not blocked
        // Keep adding actions until we hit a blocking one or empty the queue
        while (!islistBlocked && actionQueue.Count > 0)
        {
            ActionBase action = actionQueue.Dequeue();
            activeActions.Add(action);
            action.SetManager(this);
            action.Start();
        
            // If we hit a blocking action, stop adding more until it completes
            if (action.isBlocking) 
            { 
                islistBlocked = true;
                break;
            }
        }
    }
    
    public void AddAction(ActionBase action)
    {
        action.SetManager(this);        // action looks at the manager for global time scale
        actionQueue.Enqueue(action);
    }
} // end of ActionManager



