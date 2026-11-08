using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathPopup : MonoBehaviour
{
    public void Show()
    {
        gameObject.SetActive(true);
    }
    public void OnRestart()
    {
        SceneManager.LoadScene("Arena");
    }
    public void OnTitle()
    {
        SceneManager.LoadScene("Title");
    }
}
