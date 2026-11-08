using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleMenu : MonoBehaviour
{
    public void OnStart()
    {
        SceneManager.LoadScene("Arena");
    }
}
