using System;
using Attributes;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class RTSCamera : CustomBehaviour {
    [Header("General")]
    [SerializeField] private LayerMask _mask;
    [SerializeField] private Transform _boundsRoot;
    [SerializeField] private Vector2 _boundsSize;

    [Header("Zoom")]
    [SerializeField] private float _zoomSpeed = 1f;
    [SerializeField] private float _zoomTargetSpeed = 1f;
    [SerializeField] private Vector2 _zoomLimits;
    
    [Header("Rotation")]
    [SerializeField] private float _rotationSpeed = 1f;
    
    [Header("Debug")]
    [SerializeField] private bool _debugViewport;
    [SerializeField] private bool _debugBoundries;

    [Require] private Camera _camera;
    private Viewport _viewport;
    private RaycastHit _hit;
    private Rect _boundsRect;
    private Vector3 _newPosition;

    // panning
    private Vector3 _panningOrigin;
    private bool _panning;
    
    // zooming
    private float _zoomDistance;
    
    // rotating
    private bool _rotating;
    private Vector3 _rotationOffset;
    
    // following
    private Transform _target;
    private Vector3 _followTarget;
    private bool _hasTarget;
    private Vector3 _followDirection;
    private float _followDistance;
    private float _hitDistance;

    private void OnDrawGizmos() {
        if (_debugViewport) {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireCube(new Vector3(_viewport.X + _viewport.Width / 2, 0, _viewport.Z + _viewport.Depth / 2), new Vector3(_viewport.Width, 0, _viewport.Depth));
        }

        if (_debugBoundries) {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(_boundsRoot.position, new Vector3(_boundsSize.x, 0, _boundsSize.y));
        }
    }

    private void Start() {
        Vector3 pos = _boundsRoot.position;

        _boundsRect = new Rect(
            pos.x - _boundsSize.x / 2,
            pos.z - _boundsSize.y / 2,
            _boundsSize.x,
            _boundsSize.y);
        
        GetZoomDistance(0);
    }

    private void Update() {
        _viewport = CalculateViewport();
        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out _hit, Mathf.Infinity, _mask)) {
            if (_hasTarget) {
                CalculateTarget();
                Vector3 dir = _followTarget - transform.position;
                dir.y = 0;
                _newPosition = dir;
            }
            
            Rotate();
            Pan();
            Zoom();
        
            transform.position += _newPosition;
            _newPosition = Vector3.zero;
        } else {
            _hit = new RaycastHit();
        }
    }

    private void Pan() {
        if (Input.GetMouseButtonDown(2)) {
            _panningOrigin = _hit.point;
            _panning = true;
            
            SetTarget(null);
        } else if (Input.GetMouseButtonUp(2)){
            _panning = false;
        }

        if (!_panning) {
            return;
        }
        
        Vector3 newPosition = _panningOrigin - _hit.point;
        _viewport = CalculateViewport(newPosition);

        float x = _viewport.X;
        float z = _viewport.Z;
        
        if (x <= _boundsRect.x || x + _viewport.Width >= _boundsRect.x + _boundsRect.width) {
            newPosition.x = 0;
        }
                
        if (z <= _boundsRect.y || z + _viewport.Depth >= _boundsRect.y + _boundsRect.height) {
            newPosition.z = 0;
        }
        
        _newPosition += newPosition;
    }

    private void Zoom() {
        float scroll = Input.mouseScrollDelta.y;

        if (Math.Abs(scroll) < 0.01f) {
            return;
        }

        if (!GetZoomDistance(scroll)) {
            return;
        }

        // scroll and keep the mouse position in the same spot
        if (_target) {
            Vector3 dir = transform.forward * (scroll * _zoomTargetSpeed);
            Physics.Raycast(transform.position + dir, transform.forward, out RaycastHit hitA, Mathf.Infinity, _mask);
        
            if (hitA.distance < _zoomLimits.x || hitA.distance > _zoomLimits.y) {
                return;
            }

            _viewport = CalculateViewport(dir);
            _newPosition += dir;
            return;
        }
        
        Vector3 center = (_hit.point - transform.position) / 2;
        Vector3 newPosition = center * (scroll * _zoomSpeed);
        
        Physics.Raycast(transform.position + newPosition, transform.forward, out RaycastHit hit, Mathf.Infinity, _mask);
        
        if (hit.distance < _zoomLimits.x || hit.distance > _zoomLimits.y) {
            return;
        }

        _viewport = CalculateViewport(newPosition);
        _newPosition += newPosition;
    }

    private bool GetZoomDistance(float scroll) {
        Physics.Raycast(transform.position + transform.forward * scroll, transform.forward, out RaycastHit hit, Mathf.Infinity, _mask);

        if (hit.distance < _zoomLimits.x || hit.distance > _zoomLimits.y) {
            return false;
        }
        
        _zoomDistance = hit.distance;
        return true;
    }

    private void Rotate() {
        if (Input.GetMouseButtonDown(1)) {
            _rotating = true;
        } else if (Input.GetMouseButtonUp(1)) {
            _rotating = false;
        }
    
        if (_rotating) {
            Physics.Raycast(transform.position, transform.forward, out RaycastHit centerHit, Mathf.Infinity, _mask);
            transform.RotateAround(centerHit.point, Vector3.up, Input.GetAxis("Mouse X") * _rotationSpeed);
        }
    }

    private Viewport CalculateViewport(Vector3 offset = new Vector3()) {
        Vector3 posOffset = transform.position + offset;
        Physics.Raycast(posOffset, transform.forward, out RaycastHit centerHit, Mathf.Infinity, _mask);
        Vector3 center = centerHit.point;
        
        float halfFOV = _camera.fieldOfView * 0.5f * Mathf.Deg2Rad;
        float halfHeight = _zoomDistance * Mathf.Tan(halfFOV);
        float halfWidth = _camera.aspect * halfHeight;
        float horizontalMin = -halfWidth;
        float horizontalMax = halfWidth;
        float verticalMin = -halfHeight;
        float verticalMax = halfHeight;
    
        return new Viewport(center.x + horizontalMin, center.z + verticalMin, horizontalMax * 2, verticalMax * 2);
    }

    public void SetTarget(Transform target) {
        if (target == _target) {
            return;
        }
        
        _target = target;
        _newPosition = Vector3.zero;
        _hasTarget = false;
        
        if (target != null) {
            CalculateTarget();
            transform.position = _followTarget;
        
            _hasTarget = true;
        }
    }

    private void CalculateTarget() {
        Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, Mathf.Infinity, _mask);
        _hitDistance = hit.distance;
        _followTarget = _target.position + Vector3.up * _hitDistance;

        Physics.Raycast(_followTarget, transform.forward, out hit, Mathf.Infinity, _mask);
        _followDirection = -Vector3.ProjectOnPlane(hit.point - _target.position, transform.right).normalized;
        _followDistance = Vector3.Distance(hit.point, _target.position);

        _followTarget += _followDirection * _followDistance;
    }
}