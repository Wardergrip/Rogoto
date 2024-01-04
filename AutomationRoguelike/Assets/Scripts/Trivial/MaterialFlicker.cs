using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Trivial
{
    public class MaterialFlicker : MonoBehaviour
    {
        [SerializeField] private List<MeshRenderer> _meshRenderers;
        [SerializeField] private List<SkinnedMeshRenderer> _skinnedMeshRenderers;
        [SerializeField] private Material _matToFlickerTo;
        [SerializeField] private Material _originalMat;
        [SerializeField] private float _flickerLength = 0.5f;
        private Coroutine _resetMaterialCoroutine;
        private Coroutine _resetSkinnedMaterialCoroutine;

		private void Awake()
		{
			//stinky doodoo sloppy fix but it works sorry
			_originalMat = _skinnedMeshRenderers[0].material;
		}

		public void FlickerForSeconds(float length)
        {
			if (_resetMaterialCoroutine != null)
			{
				StopCoroutine(_resetMaterialCoroutine);
			}
			if (_resetSkinnedMaterialCoroutine != null)
			{
				StopCoroutine(_resetSkinnedMaterialCoroutine);
			}
			List<Tuple<MeshRenderer, Material>> originalMeshRendMats = new();
            foreach (MeshRenderer meshRenderer in _meshRenderers)
            {
                originalMeshRendMats.Add(new(meshRenderer, meshRenderer.material));
                meshRenderer.material = _matToFlickerTo;
            }
			List<Tuple<SkinnedMeshRenderer, Material>> originalSkinnedMeshRendMats = new();
			foreach (SkinnedMeshRenderer meshRenderer in _skinnedMeshRenderers)
			{
				originalSkinnedMeshRendMats.Add(new(meshRenderer, meshRenderer.material));
				meshRenderer.material = _matToFlickerTo;
			}
			_resetMaterialCoroutine = StartCoroutine(ResetMaterialCoroutine(originalMeshRendMats, length));
            _resetSkinnedMaterialCoroutine = StartCoroutine(ResetSkinnedMaterialCoroutine(originalSkinnedMeshRendMats, length));
        }

        public void Flicker()
        {
            FlickerForSeconds(_flickerLength);
        }

        private IEnumerator ResetMaterialCoroutine(List<Tuple<MeshRenderer, Material>> originalMeshRendMats, float length)
        {
            yield return new WaitForSeconds(length);
            foreach (var meshRendMat in originalMeshRendMats)
            {
                meshRendMat.Item1.material = meshRendMat.Item2;
            }
		}

		private IEnumerator ResetSkinnedMaterialCoroutine(List<Tuple<SkinnedMeshRenderer, Material>> originalMeshRendMats, float length)
		{
			yield return new WaitForSeconds(length);
			foreach (var meshRendMat in originalMeshRendMats)
			{
				meshRendMat.Item1.material = _originalMat;
			}
		}
	}
}