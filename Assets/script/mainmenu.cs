using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Mainmenu : MonoBehaviour
{
    
    [Header("Settings")]
    public GameObject open;
    public GameObject play;
    public GameObject close; 
 
    
    public void PlayGame()
    {
       SceneManager.LoadScene("SampleScene");
    }


    public void OpenSettings()
    {
         if (open != null)
            open.SetActive(true);
         play.gameObject.SetActive(false);
        close.gameObject.SetActive(true);
        open.gameObject.SetActive(false);

    }


    public void CloseSettings()
    {
         if (open != null)
            open.SetActive(false);
         close.gameObject.SetActive(false);
        play.gameObject.SetActive(true);
        open.gameObject.SetActive(true);

    }
   
} 