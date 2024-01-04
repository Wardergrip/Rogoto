using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConveyerBelt : MonoBehaviour
{
    [SerializeField] private Renderer _renderer;
    private Material _baseMaterial;
    [SerializeField] private Animator _animator;

    public Animator Animator { get => _animator; }

    public void SetUp(GameObject child)
    {
        _renderer = child.GetComponentInChildren<Renderer>();
        _baseMaterial = _renderer.material;
    }
    public void SetMaterial(Material mat)
    {
        if (_renderer == null)
        {
            return;
        }
        _renderer.material = mat;
    }
    public void ResetMaterial()
    {
        if (_renderer == null)
        {
            return;
        }
        _renderer.material = _baseMaterial;
    }
}
