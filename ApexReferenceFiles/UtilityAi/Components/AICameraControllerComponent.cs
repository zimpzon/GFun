#pragma warning disable 1591

namespace Apex.Examples.AI.Game
{
    using System;
    using UnityEngine;

    public sealed class AICameraControllerComponent : MonoBehaviour
    {
        public CameraMode cameraMode = CameraMode.Isometric;
        
        public float isometricLength = 30f;
        public float topDownY = 40f;
        public float staticY = 175f;

        public KeyCode changeModeKey = KeyCode.C;

        private GameObject _player;
        private Camera _camera;
        private CameraMode _lastCameraMode;

        private void Awake()
        {
            _camera = Camera.main;
            if (_camera == null)
            {
                throw new ArgumentNullException("_camera", this.ToString() + " could not find a Camera tagged 'MainCamera'");
            }

            _player = GameObject.FindGameObjectWithTag("Player");
            if (_player == null)
            {
                throw new ArgumentNullException("_player", this.ToString() + " could not find a GameObject tagged 'Player'");
            }

            this.transform.rotation = _player.transform.rotation;
            this.transform.SetParent(_player.transform);

            var values = Enum.GetValues(typeof(CameraMode));
            _lastCameraMode = (CameraMode)values.GetValue(values.Length - 1);
        }

        private void Update()
        {
            if (_player == null || _camera == null)
            {
                return;
            }

            if (Input.GetKeyUp(this.changeModeKey))
            {
                this.cameraMode += 1;
                if (this.cameraMode > _lastCameraMode)
                {
                    this.cameraMode = 0;
                }
            }
        }

        private void FixedUpdate()
        {
            if (_player == null || _camera == null)
            {
                return;
            }

            if (this.cameraMode == CameraMode.TopDown)
            {
                if (this.transform.parent == null)
                {
                    this.transform.SetParent(_player.transform);
                }

                this.transform.localPosition = new Vector3(0f, this.topDownY, 0f);
                this.transform.eulerAngles = new Vector3(90f, 0f, 0f);
            }
            else if (this.cameraMode == CameraMode.Isometric)
            {
                if (this.transform.parent != null)
                {
                    this.transform.SetParent(null);
                }

                this.transform.position = _player.transform.position + new Vector3(0f, this.isometricLength, this.isometricLength * -0.5f);
                this.transform.eulerAngles = new Vector3(60f, 0f, 0f);
            }
            else
            {
                if (this.transform.parent != null)
                {
                    this.transform.SetParent(null);
                }

                this.transform.position = new Vector3(0f, this.staticY, 0f);
                this.transform.eulerAngles = new Vector3(90f, 0f, 0f);
            }
        }
    }
}