/// <title>Elliptical Path</title>
/// <summary>
/// 
using UnityEngine;

public class EllipticalPath : MonoBehaviour
{
    [Header("Path Settings")]
    [SerializeField] private float width = 10f;
    [SerializeField] private float height = 5f;
    [SerializeField] private bool showGizmos = true;
    
    [Header("Player Placement")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private int playerCount = 8;
    
    /// <summary>
    /// Gets a point on the elliptical path at the specified normalized position (0-1)
    /// The path starts at the bottom center and goes clockwise.
    /// </summary>
    public Vector3 GetPointOnPath(float t)
    {
        // Ensure t is between 0 and 1
        t = Mathf.Repeat(t, 1f);
        
        // Convert t to angle (starting from bottom center, going clockwise)
        // Bottom center is at 270 degrees (or -90 degrees)
        float angle = (t * 360f) - 90f;
        float angleRadians = angle * Mathf.Deg2Rad;
        
        // Calculate position on ellipse
        float x = width * 0.5f * Mathf.Cos(angleRadians);
        float y = height * 0.5f * Mathf.Sin(angleRadians);
        
        return new Vector3(x, y, 0f);
    }
    
    /// <summary>
    /// Gets the tangent direction at the specified point on the path
    /// </summary>
    public Vector3 GetTangentOnPath(float t)
    {
        float delta = 0.01f;
        Vector3 current = GetPointOnPath(t);
        Vector3 next = GetPointOnPath(t + delta);
        
        return (next - current).normalized;
    }
    
    /// <summary>
    /// Places players evenly around the elliptical path
    /// </summary>
    public void PlacePlayers()
    {
        if (playerPrefab == null || playerCount <= 0) return;
        
        // Remove any existing players
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        
        // Place new players evenly around the path
        for (int i = 0; i < playerCount; i++)
        {
            float t = i / (float)playerCount;
            Vector3 position = transform.position + GetPointOnPath(t);
            
            // Get the direction the player should face (tangent to the path)
            Vector3 tangent = GetTangentOnPath(t);
            Quaternion rotation = Quaternion.LookRotation(tangent);
            
            // Create the player facing the direction of the path
            Instantiate(playerPrefab, position, rotation, transform);
        }
    }
    
    /// <summary>
    /// Draws debug visualization in the Scene view
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!showGizmos) return;
        
        // Draw the elliptical path
        int segments = 60;
        Vector3 previousPoint = transform.position + GetPointOnPath(0);
        
        for (int i = 1; i <= segments; i++)
        {
            float t = i / (float)segments;
            Vector3 currentPoint = transform.position + GetPointOnPath(t);
            
            // Draw path segment
            Gizmos.color = Color.red;
            Gizmos.DrawLine(previousPoint, currentPoint);
            
            previousPoint = currentPoint;
        }
        
        // Draw the starting point (bottom center)
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(transform.position + GetPointOnPath(0), 0.2f);
        
        // Draw some sample points for player positions
        Gizmos.color = Color.gray;
        for (int i = 1; i < 8; i++)
        {
            float t = i / 8f;
            Gizmos.DrawSphere(transform.position + GetPointOnPath(t), 0.15f);
        }
    }
    
    /// <summary>
    /// Updates player positions - can be called from UI button or other script
    /// </summary>
    public void UpdatePath()
    {
        PlacePlayers();
    }
}