using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Minimap : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Camera _minimapCam; 
    [SerializeField] private GameObject _objToMove;
    [SerializeField] private RawImage _rawImage;

    void Start()
    {
        if (_objToMove == null)
        {
            _objToMove = Camera.main.gameObject;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_rawImage.rectTransform,
            eventData.pressPosition, eventData.pressEventCamera, out Vector2 cursor))
        {
            Texture texture = _rawImage.texture;
            Rect rect = _rawImage.rectTransform.rect;

            float coordX = Mathf.Clamp(0, (((cursor.x - rect.x) * texture.width) / rect.width), texture.width);
            float coordY = Mathf.Clamp(0, (((cursor.y - rect.y) * texture.height) / rect.height), texture.height);

            float calX = coordX / texture.width;
            float calY = coordY / texture.height;

            cursor = new Vector2(calX, calY);

            CastRayToWorld(cursor);
        }
    }
    private void CastRayToWorld(Vector2 vec)
    {
        Ray MapRay = _minimapCam.ScreenPointToRay(new Vector2(vec.x * _minimapCam.pixelWidth,
            vec.y * _minimapCam.pixelHeight));

        if (Physics.Raycast(MapRay, out RaycastHit miniMapHit, Mathf.Infinity))
        {
            float yPos = _objToMove.transform.position.y;
            _objToMove.transform.position = new Vector3(miniMapHit.point.x, yPos,miniMapHit.point.z);
        }

    }
}
