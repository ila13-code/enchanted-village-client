namespace Unical.Demacs.EnchantedVillage
{
    using UnityEngine;

    public class CameraController : MonoBehaviour
    {
        [SerializeField] private Camera _camera = null;
        [SerializeField] private float _moveSpeed = 50f;
        [SerializeField] private float _zoomSpeed = 5f;

        private InputControls _inputs = null;
        private bool _zooming = false;
        private bool _moving = false;

        private Vector3 _center = Vector3.zero;
        private float _right = 10f;
        private float _left = 10f;
        private float _up = 10f;
        private float _down = 10f;
        private float _angle = 45f;
        private float _zoom = 5f;

        private float _zoomMin = 15f;
        private float _zoomMax = 1f;
        private Vector2 _zoomPositionOnScreen = Vector2.zero;
        private Vector3 _zoomPositionInWorld = Vector3.zero;
        private float _zoomBaseValue = 0;
        private float _zoomBaseDistance = 0;

        private Transform _root = null;
        private Transform _pivot = null;
        private Transform _target = null;

        private void Awake()
        {
            _inputs = new InputControls();
            _root = new GameObject("CameraRoot").transform;
            _pivot = new GameObject("CameraPivot").transform;
            _target = new GameObject("CameraTarget").transform;
            _camera.orthographic = true;
            _camera.nearClipPlane = 0f;
        }

        private void Start()
        {
            Initialize(Vector3.zero, 40, 40, 40, 40, 45, 5, 10, 20);
        }

        private void Initialize(Vector3 center, float right, float left, float up, float down, float angle, float zoom, float zoomMin, float zoomMax)
        {
            _center = center;
            _right = right;
            _left = left;
            _up = up;
            _down = down;
            _angle = angle;
            _zoom = zoom;
            _zoomMin = zoomMin;
            _zoomMax = zoomMax;

            _camera.orthographicSize = _zoom;

            _zooming = false;
            _moving = false;
            _pivot.SetParent(_root);
            _target.SetParent(_pivot);

            _root.position = center;
            _root.localEulerAngles = Vector3.zero;

            _pivot.localPosition = Vector3.zero;
            _pivot.localEulerAngles = new Vector3(_angle, 0, 0);

            _target.localPosition = new Vector3(0, 0, -100);
            _target.localEulerAngles = Vector3.zero;
        }

        private void OnEnable()
        {
            _inputs.Enable();
            _inputs.MainMap.Move.started += ctx => MovePressed();
            _inputs.MainMap.Move.canceled += ctx => MoveStopped();
            _inputs.MainMap.TouchZoom.started += ctx => ZoomPressed();
            _inputs.MainMap.TouchZoom.canceled += ctx => ZoomStopped();
            _inputs.MainMap.MouseScroll.started += ctx => ZoomPressed();
            _inputs.MainMap.MouseScroll.canceled += ctx => ZoomStopped();
        }

        private void OnDisable()
        {
            _inputs.MainMap.Move.started -= ctx => MovePressed();
            _inputs.MainMap.Move.canceled -= ctx => MoveStopped();
            _inputs.MainMap.TouchZoom.started -= ctx => ZoomPressed();
            _inputs.MainMap.TouchZoom.canceled -= ctx => ZoomStopped();
            _inputs.MainMap.MouseScroll.started -= ctx => ZoomPressed();
            _inputs.MainMap.MouseScroll.canceled -= ctx => ZoomStopped();
            _inputs.Disable();
        }

        private void MovePressed()
        {
            _moving = true;
        }
        private void MoveStopped()
        {
            _moving = false;
        }

        private void ZoomPressed()
        {
            _zooming = true;
            if (!Input.touchSupported)
            {
                float mouseScroll = _inputs.MainMap.MouseScroll.ReadValue<float>();
                if (mouseScroll > 0)
                {
                    _zoom -= _zoomSpeed * Time.deltaTime;
                }
                else if (mouseScroll < 0)
                {
                    _zoom += _zoomSpeed * Time.deltaTime;
                }
                _zoom = Mathf.Clamp(_zoom, _zoomMax, _zoomMin);
            }
            else
            {
                Vector2 touch0 = _inputs.MainMap.TouchPosition0.ReadValue<Vector2>();
                Vector2 touch1 = _inputs.MainMap.TouchPosition1.ReadValue<Vector2>();
                _zoomPositionOnScreen = Vector2.Lerp(touch0, touch1, 0.5f);
                _zoomPositionInWorld = CameraPositionToMapPosition(_zoomPositionOnScreen);
                _zoomBaseValue = _zoom;

                touch0.x /= Screen.width;
                touch1.x /= Screen.width;
                touch0.y /= Screen.height;
                touch1.y /= Screen.height;

                _zoomBaseDistance = Vector2.Distance(touch0, touch1);
            }
        }

        private void ZoomStopped()
        {
            _zooming = false;
        }

        private void Update()
        {
            if (_zooming)
            {
                if (!Input.touchSupported)
                {
                    float mouseScroll = _inputs.MainMap.MouseScroll.ReadValue<float>();
                    if (mouseScroll != 0)
                    {
                        _zoom -= mouseScroll * _zoomSpeed * Time.deltaTime;
                        _zoom = Mathf.Clamp(_zoom, _zoomMax, _zoomMin);
                    }
                }
                else
                {
                    Vector2 touch0 = _inputs.MainMap.TouchPosition0.ReadValue<Vector2>();
                    Vector2 touch1 = _inputs.MainMap.TouchPosition1.ReadValue<Vector2>();

                    touch0.x /= Screen.width;
                    touch1.x /= Screen.width;
                    touch0.y /= Screen.height;
                    touch1.y /= Screen.height;

                    float currentDistance = Vector2.Distance(touch0, touch1);
                    float deltaDistance = currentDistance - _zoomBaseDistance;
                    _zoom = _zoomBaseValue - (deltaDistance * _zoomSpeed);
                    _zoom = Mathf.Clamp(_zoom, _zoomMax, _zoomMin);

                    Vector3 zoomCenter = CameraPositionToMapPosition(_zoomPositionOnScreen);
                    _root.position += (_zoomPositionInWorld - zoomCenter);
                }
            }

            if (_moving)
            {
                Vector2 move = _inputs.MainMap.MoveDelta.ReadValue<Vector2>();
                if (move != Vector2.zero)
                {
                    move.x /= Screen.width;
                    move.y /= Screen.height;
                    _root.position -= _root.right.normalized * move.x * _moveSpeed;
                    _root.position -= _root.forward.normalized * move.y * _moveSpeed;
                }
            }

            AdjustBounds();

            if (_camera.orthographicSize != _zoom)
            {
                _camera.orthographicSize = Mathf.Lerp(_camera.orthographicSize, _zoom, _zoomSpeed * Time.deltaTime);
            }
            if (_camera.transform.position != _target.position)
            {
                _camera.transform.position = Vector3.Lerp(_camera.transform.position, _target.position, _zoomSpeed * Time.deltaTime);
            }
            if (_camera.transform.rotation != _target.rotation)
            {
                _camera.transform.rotation = _target.rotation;
            }
        }

        private Vector3 CameraPositionToWorldPosition(Vector2 screenPosition)
        {
            float h = _camera.orthographicSize * 2f;
            float w = _camera.aspect * h;

            Vector3 anchor = _camera.transform.position - (_camera.transform.right.normalized * w / 2f) - (_camera.transform.up.normalized * h / 2f);

            return anchor + (_camera.transform.right.normalized * screenPosition.x / Screen.width * w) + (_camera.transform.up.normalized * screenPosition.y / Screen.height * h);
        }

        private Vector3 CameraPositionToMapPosition(Vector2 screenPosition)
        {
            Vector3 point = CameraPositionToWorldPosition(screenPosition);
            float h = point.y - _root.position.y;
            float x = h / Mathf.Sin(_angle * Mathf.Deg2Rad);
            return point + _camera.transform.forward.normalized * x;
        }

        private void AdjustBounds()
        {
            if (_zoom < _zoomMin)
                _zoom = _zoomMin;
            if (_zoom > _zoomMax)
                _zoom = _zoomMax;

            float h = GetPlaneOrthographicSize();
            float w = h * _camera.aspect;

            if (h > (_up + _down) / 2f)
            {
                float n = (_up + _down) / 2f;
                _zoom = n * Mathf.Sin(_angle * Mathf.Deg2Rad);
            }
            if (w > (_right + _left) / 2f)
            {
                float n = (_right + _left) / 2f;
                _zoom = n * Mathf.Sin(_angle * Mathf.Deg2Rad) / _camera.aspect;
            }

            h = GetPlaneOrthographicSize();
            w = h * _camera.aspect;

            Vector3 tr = _root.position + _root.right.normalized * w + _root.forward.normalized * h; // top_right
            Vector3 tl = _root.position - _root.right.normalized * w + _root.forward.normalized * h; // top_left
            Vector3 dl = _root.position - _root.right.normalized * w - _root.forward.normalized * h; // down_left

            if (tr.x > _center.x + _right)
                _root.position += Vector3.left * Mathf.Abs(tr.x - (_center.x + _right));
            if (tl.x < _center.x - _left)
                _root.position += Vector3.right * Mathf.Abs((_center.x - _left) - tl.x);
            if (tr.z > _center.z + _up)
                _root.position += Vector3.back * Mathf.Abs(tr.z - (_center.z + _up));
            if (dl.z < _center.z - _down)
                _root.position += Vector3.forward * Mathf.Abs((_center.z - _down) - dl.z);
        }

        private float GetPlaneOrthographicSize()
        {
            float h = _zoom * 2f;
            return h / Mathf.Sin(_angle * Mathf.Deg2Rad) / 2f;
        }
    }
}
