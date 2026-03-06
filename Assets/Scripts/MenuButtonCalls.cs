using UnityEngine;

public class MenuActions : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;

    public void StartGame()
    {
        Debug.Log("Start Game clicked");
    }

    public void QuitGame()
    {
        Debug.Log("Quit clicked");
        Application.Quit();
    }
    
    public void ChangePlaySpeed(float newSpeed)
    {
        Debug.Log("Play speed changed to: " + newSpeed);
        if (gameManager != null) gameManager.SetPlaySpeed(newSpeed);
    }
    
    public void ChangeCardSize(float newSize)
    {
        Debug.Log("Card size changed to: " + newSize);
        if (gameManager != null) gameManager.SetCardSize(newSize);
    }
}