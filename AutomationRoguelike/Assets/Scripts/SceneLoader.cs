using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine;

public class SceneLoader : MonoSingleton<SceneLoader>
{
    public enum SceneRef : int 
    {
        MainMenu = 0,
        MainGame = 1
    }

    private readonly float _fadeSpeed = 1.0f;

    public void LoadScene(SceneRef scene)
    {
        StartCoroutine(LoadCoroutine(scene));
    }

    private IEnumerator LoadCoroutine(SceneRef newScene)
    {
        LoadingScreen loadScreen = LoadingScreen.Instance;
		for (float alpha = 0.0f;  alpha <= 1.0f; alpha += _fadeSpeed * Time.deltaTime) 
        {
			loadScreen.SetAlpha(alpha);
            yield return null;
        }

		AsyncOperation asyncOperation = SceneManager.LoadSceneAsync((int)newScene, LoadSceneMode.Single);
        yield return new WaitUntil(() => asyncOperation.isDone);

		for (float alpha = 1.0f; alpha >= 0.0f; alpha -= _fadeSpeed * Time.deltaTime)
		{
			loadScreen.SetAlpha(alpha);
			yield return null;
		}
        LoadingScreen.Instance.SetAlpha(0.0f);
	}
}
