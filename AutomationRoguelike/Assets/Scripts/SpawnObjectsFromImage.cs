using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SpawnObjectsFromImage : MonoBehaviour
{
    [Header("Image")]
    [Tooltip("The PNG image texture.")]
    [SerializeField] private Texture2D _imageTexture;
    public Texture2D ImageTexture { get => _imageTexture; set => _imageTexture = value; }
    [Tooltip("This color is interpreted as an object that has to spawn.")]
    [SerializeField] private Color _colorOfInterest = Color.black;
    public Color ColorOfInterest { get => _colorOfInterest; set => _colorOfInterest = value; }
    [Header("Objects")]
    [Tooltip("The GameObject to spawn.")]
    [SerializeField] private GameObject _objectToSpawn;
    public GameObject ObjectToSpawn { get => _objectToSpawn; set => _objectToSpawn = value; }
    [Tooltip("Size of each pixel.")]
    [SerializeField] private float _gridSize = 1.0f;
    [Tooltip("Set height offset.")]
    [SerializeField] private float _height = -1.0f;
    public float Height { get => _height; set => _height = value; }
    [Header("Misc")]
    [SerializeField] private bool _parseOnStart = true;
    public bool ParseOnStart { get => _parseOnStart; }
    public List<GameObject> SpawnedObjects { get; private set; } = new();

    public UnityEvent OnObjectsSpawned= new();

    void Start()
    {
        if (_parseOnStart) ParseAndSpawn();
    }

    public void ParseAndSpawn()
    {
        if (_imageTexture == null)
        {
            Debug.LogError("Please assign an image texture to the script.");
            return;
        }
        // Get the width and height of the image.
        int width = _imageTexture.width;
        int height = _imageTexture.height;

        // Calculate the center position of the parent GameObject.
        Vector3 centerPosition = new(-width * _gridSize * .5f + _gridSize * .5f, 0, -height * _gridSize * .5f + _gridSize * .5f);

        // Loop through each pixel in the image.
        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                // Get the color of the pixel at (x, y).
                Color pixelColor = _imageTexture.GetPixel(x, y);

                // Check if the pixel is black.
                if (pixelColor == _colorOfInterest)
                {
                    // Calculate the position to spawn the object relative to the center.
                    Vector3 spawnPosition = centerPosition + new Vector3(x * _gridSize, _height, y * _gridSize);

                    // Spawn the specified object as a child of the parent.
                    GameObject spawnedObject = Instantiate(_objectToSpawn, transform);
                    spawnedObject.transform.localPosition = spawnPosition;
                    spawnedObject.transform.localScale = new Vector3(_gridSize, 1, _gridSize);
                    SpawnedObjects.Add(spawnedObject);
                }
            }
        }
        OnObjectsSpawned?.Invoke();
    }
}
