using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Minimap : MonoBehaviour
{
    [SerializeField] Camera _camera;
    [SerializeField] SpriteRenderer _icon;
    [SerializeField] RawImage _mapImage;
    [SerializeField] Transform _followTarget;

    public void Initialize(Transform follow)
    {
        _followTarget = follow;
        _icon.gameObject.SetActive(true);
    }
    // Start is called before the first frame update
    void Awake()
    {
        _camera = GetComponentInChildren<Camera>();
        _icon = _camera.GetComponentInChildren<SpriteRenderer>(true);
        _mapImage = GetComponentInChildren<RawImage>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!_followTarget) return;
        Vector3 newPos = _followTarget.position;
        newPos.y = 90f;
        _camera.transform.position = newPos;
    }
}
