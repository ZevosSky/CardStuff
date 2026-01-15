//-----------------------------------------------------------------
// File:   Player.cs
// Author: Gary Yang
// Date:   2/4/2025
// Desc:   Player Definitions
//-----------------------------------------------------------------


using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;
using Vector4 = UnityEngine.Vector4;


public class PlayerCurve
{
    //==| Structure |===================================================================================================|
    #region DataStruct
    //--| Curve Definitions |-----------------------------
    private Transform transform = null; // This will be composed with interpolated values to get the final position

    private float curveWidth = 0.5f; // width of bezier curve
    private float curveHeight = 0.4f; // height of bezier curve
    
    //--| Hand Behavior |--------------------------------
    private float interpolatedDistance = 0.1f; // the distance between cards (assuming that the hand is not crowded) 
    private float zOffset = 0.001f;            // the z offset between cards to give a layered look
    #endregion
    //==================================================================================================================|
    
    //==|Setters and Getters|===========================================================================================|
    public void SetCurveWidth(float width) { this.curveWidth = width; }
    public void SetCurveHeight(float height) { this.curveHeight = height; }
    public void SetZOffset(float offset) { this.zOffset = offset; }
    public void SetInterpolatedSpreadDistance(float distance) { this.interpolatedDistance = distance; }
    
    public float GetCurveWidth() { return this.curveWidth; }
    public float GetCurveHeight() { return this.curveHeight; }
    public float GetZOffset() { return this.zOffset; }
    public float GetInterpolatedSpreadDistance() { return this.interpolatedDistance; }
    //==================================================================================================================|
    
    
    //==| Constructor |=================================================================================================|
    public PlayerCurve(Transform Trans)
    {
        this.transform = Trans;
        // default values for curve width and height 
    }

    public PlayerCurve(Transform T, float width, float height, float zOffset)
    {
        this.transform = T;
        this.curveWidth = width;
        this.curveHeight = height;
        this.zOffset = zOffset;
        
    }

    //==| Functions |===================================================================================================|
    
    
    /// <summary>
    /// Calculate the positions of the cards in the player's hand 
    /// </summary>
    /// <param name="numberOfCards"> number of cards/points on the curve we are calculating for</param>
    /// <returns> (world position, world rotation) per card  </returns>
    public (Vector3, float)[] CalculateCardPositions(int numberOfCards)
    {
        if (numberOfCards <= 0) return null;
        (Vector3, float)[] output = new (Vector3, float)[numberOfCards];
        if (numberOfCards == 1) { // Single Card Case, just center it
            Vector2 localPos = PreTransformInterpolation(0.5f);
            Vector2 normalVector = GetNormalVector(0.5f);
            Vector3 worldPos = transform.TransformPoint(new Vector3(localPos.x, localPos.y, 0.0f));
            Vector3 worldNormal = transform.TransformDirection(normalVector.x, normalVector.y, 0.0f);
            float zAngle = CalculateRawZRotation(worldNormal);
            output[0] = (worldPos, zAngle);
            return output;
        }
        // We cant fit all the the cards in the curve with default spacing
        if (numberOfCards * interpolatedDistance > 1.0f) 
            return CalculateCardPositions(numberOfCards);
        
        float fanWidth = numberOfCards * interpolatedDistance;
        // Ensure we distribute cards evenly along the curve
        for (int i = 0; i < numberOfCards; i++)
        {
            float t = (0.5f - (fanWidth / 2.0f)) + (i * interpolatedDistance);
            
            // Calculate position and normal on curve
            Vector2 localPos = PreTransformInterpolation(t);
            Vector2 normalVector = GetNormalVector(t);
        
            // Transform to world space
            Vector3 worldPos = transform.TransformPoint(new Vector3(localPos.x, localPos.y, 0.0f - (i * zOffset)));
            Vector3 worldNormal = transform.TransformDirection(normalVector.x, normalVector.y, 0.0f);
        
            // Calculate raw Z-axis rotation angle based on normal
            float zAngle = CalculateRawZRotation(worldNormal);
        
            // Store the position and raw rotation angle for this card
            output[i] = (worldPos, zAngle);
        }
    
        return output;
    }  // End of CalculateCardPositions Method
    
    
    
    
    /// <summary>
    /// Calculate a point on the Bezier curve at parameter t
    /// </summary>
    private Vector2 PreTransformInterpolation(float t) 
    {
        // Clamp t to ensure it's between 0 and 1
        t = Mathf.Clamp01(t);
        
        Vector2 leftP = new Vector2(-curveWidth / 2.0f, 0.0f);   // the left control point for the bezier curve
        Vector2 rightP = new Vector2(curveWidth / 2.0f, 0.0f);   // the right control point for the bezier curve
        Vector2 centerControlP = new Vector2(0.0f, curveHeight); // the center control point for the bezier curve
    
        // calculate the bezier curve
        Vector2 leftCenter = Vector2.Lerp(leftP, centerControlP, t);   // Lines between leftp and centerControlP
        Vector2 centerRight = Vector2.Lerp(centerControlP, rightP, t); // Lines between centerControlP and rightP
    
        return Vector2.Lerp(leftCenter, centerRight, t); // calculate the final position
    }
    
    
    /// <summary>
    /// Calculate the normal vector to the bezier curve at parameter t 
    /// </summary>
    /// <param name="t">Parameter value (0-1) along the curve</param>
    /// <returns>Normalized vector perpendicular to the curve</returns>
    private Vector2 GetNormalVector(float t)
    {
        // Clamp t to ensure it's between 0 and 1
        t = Mathf.Clamp01(t);
        
        Vector2 leftP = new Vector2(-curveWidth / 2.0f, 0.0f);
        Vector2 rightP = new Vector2(curveWidth / 2.0f, 0.0f);
        Vector2 centerControlP = new Vector2(0.0f, curveHeight);
    
        // Calculate the tangent vector using the derivative of the quadratic Bezier curve
        // B'(t) = 2(1-t)(p1-p0) + 2t(p2-p1)
        Vector2 tangent = 2 * (1 - t) * (centerControlP - leftP) + 2 * t * (rightP - centerControlP);
    
        // Normalize the tangent
        tangent.Normalize();
    
        // The normal is perpendicular to the tangent (rotate 90 degrees counterclockwise)
        return new Vector2(-tangent.y, tangent.x);
    }
    
    /// <summary>
    /// Debugger method to visualize the curve and card positions
    /// </summary>
    /// <param name="debugSphere">Prefab to use for debug visualization</param>
    /// <param name="numberOfPoints">Number of points to sample along the curve</param>
    /// <param name="parent">Parent transform for the debug objects</param>
    public void DebugDrawCurve(GameObject debugSphere, int numberOfPoints, Transform parent)
    {
        if (debugSphere == null || numberOfPoints <= 0) return;
        
        // Create debug spheres along the curve
        for (int i = 0; i < numberOfPoints; i++)
        {
            float t = (float)i / (numberOfPoints - 1);
            Vector2 localPos = PreTransformInterpolation(t);
            Vector3 worldPos = transform.TransformPoint(new Vector3(localPos.x, localPos.y, 0.0f));
            
            GameObject sphere = GameObject.Instantiate(debugSphere, worldPos, Quaternion.identity, parent);
            sphere.name = $"CurvePoint_{i}_t{t:F2}";
            sphere.transform.localScale = Vector3.one * 0.05f;  // Small visual size
            
            // Also visualize the normal
            Vector2 normalVector = GetNormalVector(t);
            Vector3 worldNormal = transform.TransformDirection(normalVector.x, normalVector.y, 0.0f);
            
            GameObject normalLine = new GameObject($"Normal_{i}");
            normalLine.transform.parent = parent;
            
            LineRenderer lineRenderer = normalLine.AddComponent<LineRenderer>();
            lineRenderer.startWidth = 0.01f;
            lineRenderer.endWidth = 0.01f;
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, worldPos);
            lineRenderer.SetPosition(1, worldPos + worldNormal * 0.2f);
            
            // Color based on t value
            Color lineColor = Color.Lerp(Color.green, Color.red, t);
            lineRenderer.startColor = lineColor;
            lineRenderer.endColor = lineColor;
        }
    }
    
    
    /// <summary>
    /// Calculate the positions of the cards in the player's hand with raw rotation values
    /// </summary>
    /// <param name="numberOfCards">Number of cards/points on the curve we are calculating for</param>
    /// <returns>(world position, raw z rotation angle in degrees) per card</returns>
    public (Vector3, float)[] CalculateCardPositionsRaw(int numberOfCards)
    {
        if (numberOfCards <= 0) return null;

        (Vector3, float)[] output = new (Vector3, float)[numberOfCards];
    
        // Ensure we distribute cards evenly along the curve
        for (int i = 0; i < numberOfCards; i++)
        {
            // Calculate t value to distribute cards evenly
            float t;
            if (numberOfCards == 1)
                t = 0.5f; // Center the single card
            else
                t = (float)i / (float)(numberOfCards - 1); // Distribute from left to right (0 to 1)
            
            // Calculate position and normal on curve
            Vector2 localPos = PreTransformInterpolation(t);
            Vector2 normalVector = GetNormalVector(t);
        
            // Transform to world space
            Vector3 worldPos = transform.TransformPoint(new Vector3(localPos.x, localPos.y, 0.0f - (i * zOffset)));
            Vector3 worldNormal = transform.TransformDirection(normalVector.x, normalVector.y, 0.0f);
        
            // Calculate raw Z-axis rotation angle based on normal
            float zAngle = CalculateRawZRotation(worldNormal);
        
            // Store the position and raw rotation angle for this card
            output[i] = (worldPos, zAngle);
        }
    
        return output;
    }
    
    

    
    /// <summary>
    /// Calculate the raw Z-axis rotation angle based on normal vector </summary>
    /// <note> This method assumes that the x-axis is the piviot for flipping the card </note>
    /// <param name="normal">The normal vector at the card's position on the curve</param>
    /// <returns> Raw Z-axis rotation angle in degrees </returns>
    private float CalculateRawZRotation(Vector3 normal)
    {
        // The key issue might be the coordinate system transformation
        // Let's try a more direct approach
    
        // Get the angle between the up vector and the normal in world space
        Vector3 worldUp = Vector3.up;
        float angle = Vector3.SignedAngle(worldUp, normal, Vector3.forward);
    
        // Debug this value to see what's happening
        Debug.Log($"Normal: {normal}, Calculated angle: {angle}");
    
        return angle;
    }
    

    
} // End of Player Curve Class 


public class Player : MonoBehaviour
{
    public enum PlayerType
    {
        HUMAN,
        AI
    }
    
    // I'm making this get instantiated by the GameManager so these need to be public so I can assign them there
    
    public PlayerCurve handCurve;       // ref to the player's hand curve
    public ActionManager actionManager; // 
    public PlaySpace playSpace;
    public PlayerType playerType;       // is this a human or AI player
    
    
    void Start()
    {
           
        
    }

    void Update()
    {
        if (playerType == PlayerType.HUMAN)
        {
            
            
        }
    }

}





