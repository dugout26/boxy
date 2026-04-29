using System;
using UnityEngine.SceneManagement;

namespace Mound.Core.Scenes
{
    public interface ISceneLoader
    {
        void LoadScene(string sceneName);
        void LoadSceneAdditive(string sceneName);
        void UnloadScene(string sceneName);
        bool IsLoaded(string sceneName);
        string ActiveSceneName { get; }
    }

    public sealed class SceneLoader : ISceneLoader
    {
        public string ActiveSceneName => SceneManager.GetActiveScene().name;

        public void LoadScene(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
                throw new ArgumentException("sceneName empty", nameof(sceneName));
            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }

        public void LoadSceneAdditive(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
                throw new ArgumentException("sceneName empty", nameof(sceneName));
            SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
        }

        public void UnloadScene(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName) || !IsLoaded(sceneName)) return;
            // 비동기 결과 무시 — fire-and-forget. Unity AsyncOperation은 Task 아니므로 _ = 표기 불필요
            SceneManager.UnloadSceneAsync(sceneName);
        }

        public bool IsLoaded(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName)) return false;
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                if (SceneManager.GetSceneAt(i).name == sceneName) return true;
            }
            return false;
        }
    }

}
