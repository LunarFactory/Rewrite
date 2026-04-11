using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI
{
    public class SceneLoadTrigger : MonoBehaviour
    {
        public void LoadScene(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName)) return;
            SceneManager.LoadScene(sceneName);
        }
    }
}
