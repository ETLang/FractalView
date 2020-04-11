using FractalView;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;

public class Controls : MonoBehaviour
{
    public float smoothing = 0.9f;

    public TheEffect effect;
    private Fractal _fractal;

    // Start is called before the first frame update
    void Start()
    {
        _fractal = effect.Fractal;
        _targetPosition = _fractal.ViewCenter;
        _targetScale = _fractal.ViewScale;
    }

    // Update is called once per frame
    void Update()
    {
        if (EventSystem.current.currentSelectedGameObject != null) return;

        if (_cam == null)
        {
            _cam = GetComponentInParent<Camera>();
            if (_cam == null) return;
        }

        /*
         * Rules:
         * 
         * Left button down: Pan
         * Wheel: Zoom
         * Ctrl+Left: Zoombox?
         * Alt+Left: Rotate?
         */

        bool leftDown = Input.GetMouseButton(0);
        bool ctrlDown = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        bool altDown = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
        var mousePos = Input.mousePosition;

        if (leftDown && !_mouseLeft)
            _lastPosition = mousePos;

        var mouseDelta = mousePos - _lastPosition;

        var isRotating = leftDown && altDown && !ctrlDown;
        var isPanning = leftDown && !altDown && !ctrlDown;

        if (!isRotating)
            _rotateAxis = Vector3.zero;

        if (isRotating)
        {
            if (_rotateAxis == Vector3.zero)
            {
                _rotateAxis = mousePos - new Vector3(_cam.pixelWidth / 8, _cam.pixelHeight / 8, 0);
                _rotateAxis.z = 0;
                _rotateAxis = new Vector3(_rotateAxis.y, -_rotateAxis.x, 0);
                _rotateAxis.Normalize();
            }
            else
            {
                Rotate(Vector3.Dot(mouseDelta, _rotateAxis) * 0.3f);
            }
        }

        if (isPanning && _mouseLeft)
            Pan(mouseDelta);

        if (!isPanning && !isRotating)
        {
            float scrollPos = Input.GetAxis("Mouse ScrollWheel");

            if (scrollPos < 0)
                ZoomOut();
            else if (scrollPos > 0)
                ZoomIn();
        }

        _lastPosition = mousePos;
            _mouseLeft = leftDown;

        InterpolateTransform();
    }

    double2 rot0 = new double2(1, 0);
    double2 rot1 = new double2(0, 1);

    private void Pan(Vector3 amount)
    {
        var dpan = new double2(amount.x, amount.y);
        //dpan.x /= _cam.pixelHeight;
        //dpan.y /= _cam.pixelHeight;
        dpan *= _cam.orthographicSize / _cam.pixelHeight;

        dpan *= 2 * _targetScale;
        dpan = rot0 * dpan.x + rot1 * dpan.y;
        _targetPosition -= dpan;
    }

    private void Rotate(float amount)
    {
        _targetAngle += amount;

        var q = Quaternion.AngleAxis(_targetAngle, Vector3.back);
        var srot0 = q * new Vector3(1, 0, 0);
        var srot1 = q * new Vector3(0, 1, 0);

        rot0 = new double2(srot0.x, srot1.x);
        rot1 = new double2(srot0.y, srot1.y);
    }

    private void ZoomIn()
    {
        _targetScale /= 1.2f;
    }

    private void ZoomOut()
    {
        _targetScale *= 1.2f;
    }

    double MagnitudeSq(double2 v)
    {
        return v.x * v.x + v.y * v.y;
    }

    double Magnitude(double2 v)
    {
        return Math.Sqrt(v.x * v.x + v.y * v.y);
    }

    private void InterpolateTransform()
    {
        var u = 1-Math.Exp(Math.Log(smoothing) * Time.deltaTime);

        var currentScale = _fractal.ViewScale;
        var scaleK = _targetScale / currentScale;
        var scaleU = Math.Pow(scaleK, u);
        _fractal.ViewScale *= scaleU;
        //effect.viewScale = _targetScale;

        var rotation = transform.localRotation;
        var euler = rotation.eulerAngles;
        var currentAngle = euler.z;

        while (currentAngle - _targetAngle > 180)
            currentAngle -= 360;

        while (_targetAngle - currentAngle > 180)
            currentAngle += 360;

        var newAngle = Mathf.Lerp(currentAngle, _targetAngle, (float)u);

        euler.z = newAngle;
        rotation.eulerAngles = euler;

        transform.localRotation = rotation;
        _fractal.ViewCenter += (_targetPosition - _fractal.ViewCenter) * u;
        effect.isMoving =
            //Mathf.Abs(currentAngle - _targetAngle) > 0.001f ||
            Math.Abs(1 - currentScale / _targetScale) > 0.001;
            //MagnitudeSq(effect.ViewCenter - _targetPosition) > 0.00001;
    }

    Camera _cam;

    Vector3 _rotateAxis;
    Vector3 _lastPosition;

    bool _isPanning;
    bool _isRotating;

    bool _mouseLeft;
    bool _mouseRight;

    double _targetScale;
    float _targetAngle;
    public double2 _targetPosition;
}
