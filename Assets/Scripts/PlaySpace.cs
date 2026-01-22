//==============================================================================
// @Author: Gary Yang
// @File: ActionList.cs
// @brief: action list stuff for card game
// @copyright DigiPen(C) 2025
//==============================================================================



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Serialization;


public class Ellipse 
{
    
    private float _radiusX;
    private float _radiusY;
    
    private Vector2 _center;

    /// <summary>
    /// Initializes a new instance of the Ellipse class.
    /// </summary>
    /// <param name="width">The width of the ellipse.</param>
    /// <param name="height">The height of the ellipse.</param>
    /// <param name="center">The XY coordinate of the center of the ellipse.</param>
    public Ellipse(float width, float height, Vector2 center)
    {
        _radiusX = width / 2.0f;
        _radiusY = height / 2.0f;
        _center = center;
    }

    /// <summary>
    /// Gets or sets the width of the ellipse.
    /// </summary>
    public float Width
    {
        get { return _radiusX * 2.0f; }
        set { _radiusX = value / 2.0f; }
    }

    /// <summary>
    /// Gets or sets the height of the ellipse.
    /// </summary>
    public float Height
    {
        get { return _radiusY * 2.0f; }
        set { _radiusY = value / 2.0f; }
    }

    /// <summary>
    /// Gets or sets the X coordinate of the center of the ellipse.
    /// </summary>
    public Vector2 _Center
    {
        get { return _center; }
        set { _center = value; }
    }

    /// <summary>
    /// Calculates a point on the ellipse based on a parameter t (0 to 1).
    /// </summary>
    /// <param name="t">The parameter t, where 0 ≤ t ≤ 1. 
    /// A value of 0 or 1 corresponds to the rightmost point of the ellipse,
    /// 0.25 to the bottommost point, 0.5 to the leftmost point, and 0.75 to the topmost point.</param>
    /// <returns>A tuple representing the (x, y) coordinates of the interpolated point.</returns>
    public Vector2 GetPointAt(double t)
    {
        // Convert the parameter t (0 to 1) to angle in radians (0 to 2π)
        double angle = t * 2.0 * Math.PI;
        
        // Calculate the point on the ellipse
        float x = (float)(_center.x + _radiusX * Math.Cos(angle));
        float y = (float) (_center.y + _radiusY * Math.Sin(angle));
        
        return new Vector2(x, y);
    }

    /// <summary>
    /// Gets an array of points along the ellipse perimeter.
    /// </summary>
    /// <param name="numberOfPoints">The number of points to calculate.</param>
    /// <returns>An array of (x, y) coordinate tuples representing points on the ellipse.</returns>
    public Vector2[] GetPoints(int numberOfPoints)
    {
        if (numberOfPoints <= 0)
        {
            throw new ArgumentException("Number of points must be positive.", nameof(numberOfPoints));
        }

        var points = new Vector2[numberOfPoints];
        
        for (int i = 0; i < numberOfPoints; i++)
        {   // We are offseting the t value by 0.5 to start at the top of the ellipse
            float t = ((float)i / numberOfPoints) - 0.25f; 
            points[i] = GetPointAt(t);
        }
        
        return points;
    }
}

public class PlaySpace : MonoBehaviour
{
    private const int MAX_PLAYERS = 6;
    private const int MIN_PLAYERS = 2;
    
    [SerializeField] private GameObject _GameSpace; // the center of the game space
    [SerializeField] private float _Width; // the width of the game space
    [SerializeField] private float _Height; // the height of the game space
    [FormerlySerializedAs("_Players")] [SerializeField] [Range(MIN_PLAYERS, MAX_PLAYERS)] private int _PlayerCount; // "Segments" of the ellipse
    [SerializeField] GameObject _PlayerPrefab; // the player prefab
    [SerializeField] public GameObject playZoneReference;  
    
    
    
    public int Players() { return _PlayerCount; }
    
    
    private Vector3 _CenterSpace;
    private Vector3[] _PlayerPositions = new Vector3[MAX_PLAYERS];
    private GameObject[] _PlayerObjectReferences = new GameObject[MAX_PLAYERS];

    public GameObject[] AddPlayer() {
        if (_PlayerCount < MAX_PLAYERS) {_PlayerCount++;}
        SetupPlayerPositions(); // recalculate the player positions and curves
        return _PlayerObjectReferences;
    }

    public GameObject[] RemovePlayer() {
        if (_PlayerCount > MIN_PLAYERS) { _PlayerCount--; }
        SetupPlayerPositions(); // recalculate the player positions and curves
        return _PlayerObjectReferences;
    }
    
    // I wish i could make the player object references read only but unity won't let me 
    public  GameObject[] GetPlayerObjectReferences() { return _PlayerObjectReferences; }
    
   
    /// <returns>the number of players in the game (NOT ZERO INDEXED)</returns>
    public int GetPlayerCount() { return _PlayerCount; } 
    
    private void SetupPlayerPositions()
    {
        // Calculate the player positions
        float t = 0.75f; // Start at the bottom of the ellipse
        Ellipse e = new Ellipse(_Width, _Height, _CenterSpace);
        
        for (int i = 0; i < _PlayerCount; i++)
        {
            // Use the ellipse class to calculate the player positions
            _PlayerPositions[i] = e.GetPointAt(t);
    
            // Calculate direction to center
            Vector3 dirToCenter = (_CenterSpace - _PlayerPositions[i]).normalized;
    
            // Calculate rotation angle to make objects tilt toward center
            float angle = Mathf.Atan2(dirToCenter.y, dirToCenter.x) * Mathf.Rad2Deg;
            // Subtract 90 degrees to make the "up" of the player face the center
            Quaternion rotation = Quaternion.Euler(0, 0, angle - 90);
    
            if (_PlayerObjectReferences[i] == null)
            {
                _PlayerObjectReferences[i] = Instantiate(_PlayerPrefab, _PlayerPositions[i], rotation);
            }
            else 
            {
                _PlayerObjectReferences[i].SetActive(true);
                _PlayerObjectReferences[i].transform.position = _PlayerPositions[i];
                _PlayerObjectReferences[i].transform.rotation = rotation;
            }
    
            t += 1.0f / _PlayerCount;
            t = t % 1.0f;
        }
        
        // Deactivate any unused player objects
        for (int i = _PlayerCount; i < MAX_PLAYERS; ++i)
        {
            if (_PlayerObjectReferences[i] != null)
                _PlayerObjectReferences[i].SetActive(false);
        }

    } 
    
    private Vector3 VectorToCenter(Vector3 position)
    {
        return (_CenterSpace - position).normalized;
    }

    #region UnityUpdates
    
    
    // Start is called before the first frame update

    private void Awake()
    {
        if (_GameSpace == null ) 
            _CenterSpace = GetComponent<Transform>().position;
        else
            _CenterSpace = _GameSpace.GetComponent<Transform>().position;
        
        SetupPlayerPositions();
    }

    void Start()
    {
      
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    
    #endregion

    #region DebugGizmos
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        
        if (_GameSpace == null) return;
        Vector3 pos3D = _GameSpace.GetComponent<Transform>().position;
        Vector2 pos2D = new Vector2(pos3D.x, pos3D.y); // Since we are in the XY plane, we can ignore the Z coordinate
        
        DrawEllipse(pos2D, _Width, _Height, _PlayerCount);
    }
    
    
    void DrawEllipse(Vector2 center, float width, float height, int segments = 20)
    {
        Ellipse ellipse = new Ellipse(width, height, center);
        Vector2[] points = ellipse.GetPoints(segments);
        
        for (int i = 0; i < points.Length - 1; i++)
        {
            Gizmos.DrawLine(points[i], points[i + 1]);
        }
        
        Gizmos.DrawLine(points[points.Length - 1], points[0]);
    }
    #endregion
}
