using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;

public class Telemetry : MonoBehaviour
{
    
    // Needs to log game data, such as hands won and lost per player 
    private struct GameStats
    {
        public int winnerIndex;
        public int roundsToWin; 
    }
    
    private List<GameStats> gameStatsList = new List<GameStats>(); // each game is a new entry in the list

    private struct FPS_Stats
    {
        public float meanFPS;
        public float worstFPS;
        public float HighestFPS;
        
        public FPS_Stats(float mean, float worst, float highest)
        {
            meanFPS = mean;
            worstFPS = worst;
            HighestFPS = highest;
        }
    }
    
    
    // Needs to log fps: the mean median and worst frame rate every 10-30 seconds
    [Range(10,40)]
    [SerializeField] private float fpsLogingIntervalSecs = 10f; 
    private List<FPS_Stats> FPS_Log = new List<FPS_Stats>();
    private FPS_Stats CurrentFPSIntervalStats = new FPS_Stats(0f, float.MaxValue, 0f);
    
    // Needs to log how many times the pause menu was activated and deactivated as well as how often each option was changed (timescale and card size)
    // At the end of the game, needs to save all this data to a csv file for later analysis
    
    [DoNotSerialize] private float FPS_Accumulator = 0f;

    public void LogRoundEndData(int winnerIndex, int roundsToWin)
    {
        gameStatsList.Add(new GameStats { winnerIndex = winnerIndex, roundsToWin = roundsToWin });
    }
    
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        FPSLogging(); 
    }

    private void OnDestroy() 
    {
        LogTelemetry();
    }

    private void FPSLogging()
    {
        float dt = Time.deltaTime;
        float fps = 1f / dt; 
        
        FPS_Accumulator += dt;

        if (FPS_Accumulator >= fpsLogingIntervalSecs)
        {
            // log current fps stats
            FPS_Log.Add(CurrentFPSIntervalStats);
            // reset for next interval
            FPS_Accumulator = 0f;
            CurrentFPSIntervalStats = new FPS_Stats(0f, float.MaxValue, 0f);
        }
        else
        {
            // update current interval stats
            CurrentFPSIntervalStats.meanFPS += fps * dt; // weighted average
            if (fps < CurrentFPSIntervalStats.worstFPS)
                CurrentFPSIntervalStats.worstFPS = fps;
            if (fps > CurrentFPSIntervalStats.HighestFPS)
                CurrentFPSIntervalStats.HighestFPS = fps;
        }
    }
    

    void LogTelemetry()
    {
        StringBuilder sb = new StringBuilder();

        // --- Table 1: FPS Stats ---
        sb.AppendLine("FPS Stats");
        sb.AppendLine("Interval,Mean FPS,Worst FPS,Best FPS");
        for (int i = 0; i < FPS_Log.Count; i++)
        {
            FPS_Stats s = FPS_Log[i];
            sb.AppendLine($"{i + 1},{s.meanFPS:F2},{s.worstFPS:F2},{s.HighestFPS:F2}");
        }

        sb.AppendLine(); // blank line between tables

        // --- Table 2: Game Stats ---
        sb.AppendLine("Game Stats");
        sb.AppendLine("Game,Winner Player Index,Rounds To Win");
        for (int i = 0; i < gameStatsList.Count; i++)
        {
            GameStats g = gameStatsList[i];
            sb.AppendLine($"{i + 1},{g.winnerIndex},{g.roundsToWin}");
        }

        string path = Path.Combine(Application.persistentDataPath, "telemetry.csv");
        File.WriteAllText(path, sb.ToString());
        Debug.Log($"Telemetry saved to: {path}");
    }
    
}
