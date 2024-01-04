using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Trivial;
using UnityEngine.UI;

public class RandomLoadingIcon : MonoBehaviour
{
    [SerializeField] private List<Sprite> _sprites;
    [SerializeField] private Image _image;
    [SerializeField] private LoadingScreen _loadingscreen;

    private void Awake()
    {
        _loadingscreen.OnVisible.AddListener(UpdateSprite);
    }

    private void OnDestroy()
    {
        _loadingscreen.OnVisible.RemoveListener(UpdateSprite);
    }
    void UpdateSprite()
    {
        _image.sprite = _sprites.GetRandomValue();

    }
}
