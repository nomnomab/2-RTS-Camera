using System;
using Attributes;
using UnityEngine;
using UnityEngine.EventSystems;

public class Target: MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    [SerializeField] private float _clickTime = 0.1f;
    [SerializeField] private RTSCamera _camera;

    private float _time = -1;
    [SerializeField, ReadOnly] private bool _hovering;

    private void OnMouseOver() {
        if (Input.GetMouseButtonDown(0)) {
            if (_time > 0) {
                // check for double-click
                if (Time.time - _time <= _clickTime) {
                    _time = -1;
                    _camera.SetTarget(transform);
                    return;
                }
                
                _time = -1;
                return;
            }
            
            _time = Time.time;
        }
    }

    private void OnMouseExit() {
        _time = -1;
    }

    public void OnPointerEnter(PointerEventData eventData) {
        _hovering = true;
    }

    public void OnPointerExit(PointerEventData eventData) {
        _hovering = false;
    }
}