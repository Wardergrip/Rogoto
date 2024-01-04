using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneLoaderProxy : MonoBehaviour
{
    [SerializeField] private SceneLoader.SceneRef _scene;

    public void LoadSceneFromVariable()
    {
        LoadScene(_scene);
    }

    public void LoadScene(SceneLoader.SceneRef scene)
    {
        SceneLoader.Instance.LoadScene(scene);
    }
}
