using UnityEngine;

public class TurretRange : MonoBehaviour
{
    [SerializeField] private GameObject _visual;
    [SerializeField] private LayerMask _mask;
    [SerializeField] private Material _bottomQuadInvalid;
    [SerializeField] private Material _bottomQuadObstructed;
    [SerializeField] private Material _bottomQuadValid;
    [SerializeField] private Material _sideQuadInvalid;
    [SerializeField] private Material _sideQuadObstructed;
    [SerializeField] private Material _sideQuadValid;
    [SerializeField] private Renderer _bottomQuadRenderer;
    [SerializeField] private Renderer[] _sideQuadsRenderers;
    private void Start()
    {

        UpdateVisual(true, true);
    }
    public void UpdateVisual(bool validPosition, bool turretGrounded)
    {
        if (CheckGround())
        {
            _visual.transform.localPosition = Vector3.up;
        }
        else
        {
            _visual.transform.localPosition = Vector3.zero;
        }
        Material bottomMat;
        Material sideMat;
        if (!validPosition||!turretGrounded)
        {
            bottomMat = _bottomQuadInvalid;
            sideMat = _sideQuadInvalid;
        }
        else if (CheckGround())
        {
            bottomMat = _bottomQuadObstructed;
            sideMat = _sideQuadObstructed;
        }
        else
        {
            bottomMat = _bottomQuadValid;
            sideMat = _sideQuadValid;
        }
        _bottomQuadRenderer.material = bottomMat;
        foreach (Renderer renderer in _sideQuadsRenderers)
        {
            renderer.material = sideMat;
        }
    }
    private bool CheckGround()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, out hit, 0.6f, _mask))
        {          
            return true;
        }
        else
        {
            return false;
        }
    }
}
