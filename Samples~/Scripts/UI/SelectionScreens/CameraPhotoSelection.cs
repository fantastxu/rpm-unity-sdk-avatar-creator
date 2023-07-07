using System;
using UnityEngine;
using UnityEngine.UI;

namespace ReadyPlayerMe
{
    public class CameraPhotoSelection : State
    {
        [SerializeField] private RawImage rawImage;
        [SerializeField] private Button cameraButton;

        public override StateType StateType => StateType.CameraPhoto;
        public override StateType NextState => StateType.Editor;

        private WebCamTexture camTexture;
        
        public override void ActivateState()
        {
            cameraButton.onClick.AddListener(OnCameraButton);
            OpenCamera();
        }

        public override void DeactivateState()
        {
            cameraButton.onClick.RemoveListener(OnCameraButton);
            CloseCamera();
        }

        private void OpenCamera()
        {
            var devices = WebCamTexture.devices;
            if (devices.Length == 0)
            {
                return;
            }

            rawImage.color = Color.white;
            foreach (var device in devices)
            {
#if UNITY_EDITOR || !UNITY_WEBGL // WebGL builds do not support front facing camera
                if (!device.isFrontFacing)
                {
                    continue;
                }
#endif
                
                var size = rawImage.rectTransform.sizeDelta;
                camTexture = new WebCamTexture(device.name, (int) size.x, (int) size.y);
                camTexture.Play();
                rawImage.texture = camTexture;
                rawImage.SizeToParent();
                return;
            }
        }

        private void CloseCamera()
        {
            if (camTexture != null && camTexture.isPlaying)
            {
                camTexture.Stop();
            }
        }

        private void OnCameraButton()
        {
            if (camTexture == null || !camTexture.isPlaying)
            {
                LoadingManager.EnableLoading("Camera is not available.", LoadingManager.LoadingType.Popup, false);
                return;
            }

            var texture = new Texture2D(rawImage.texture.width, rawImage.texture.height, TextureFormat.ARGB32, false);
            texture.SetPixels(camTexture.GetPixels());
            texture.Apply();

            var bytes = texture.EncodeToPNG();
            
            AvatarCreatorData.AvatarProperties.Id = string.Empty;
            AvatarCreatorData.AvatarProperties.Base64Image = Convert.ToBase64String(bytes);
            AvatarCreatorData.IsExistingAvatar = false;
            
            StateMachine.SetState(StateType.Editor);
        }
    }
}
