﻿using System.Collections.Generic;
using ReadyPlayerMe.AvatarCreator;
using ReadyPlayerMe.AvatarLoader;
using UnityEngine;
using UnityEngine.UI;

namespace ReadyPlayerMe
{
    public class AvatarCreatorSelection : State
    {
        [SerializeField] private AssetTypeUICreator assetTypeUICreator;
        [SerializeField] private AssetButtonCreator assetButtonCreator;
        [SerializeField] private Button saveButton;
        [SerializeField] private AvatarConfig inCreatorConfig;
        [SerializeField] private RuntimeAnimatorController animator;

        private AvatarManager avatarManager;
        private GameObject avatar;
        private Quaternion lastRotation;

        public override StateType StateType => StateType.Editor;

        private void OnEnable()
        {
            saveButton.onClick.AddListener(OnSave);

            Loading.SetActive(true);
            LoadAssets();
            CreateDefaultModel();
        }

        private void OnDisable()
        {
            saveButton.onClick.RemoveListener(OnSave);
            
            if (avatar != null)
            {
                Destroy(avatar);
            }
            saveButton.gameObject.SetActive(false);
            assetTypeUICreator.ResetUI();
        }

        private void CreateUI(BodyType bodyType, Dictionary<string, AssetType> assets)
        {
            assetTypeUICreator.CreateUI(bodyType, AssetTypeHelper.GetAssetTypeList(bodyType));
            assetButtonCreator.CreateUI(assets, UpdateAvatar);
            saveButton.gameObject.SetActive(true);
        }

        private async void OnSave()
        {
            var startTime = Time.time;
            var avatarId = await avatarManager.Save();
            DataStore.AvatarId = avatarId;
            DebugPanel.AddLogWithDuration("Avatar saved", Time.time - startTime);
            StateMachine.SetState(StateType.End);
        }

        private async void LoadAssets()
        {
            var startTime = Time.time;

            var partnerAssetManager = new PartnerAssetsManager(
                DataStore.User.Token,
                DataStore.AvatarProperties.Partner,
                DataStore.AvatarProperties.BodyType,
                DataStore.AvatarProperties.Gender);
            var assetIconDownloadTasks = await partnerAssetManager.GetAllAssets();

            DebugPanel.AddLogWithDuration("Got all partner assets", Time.time - startTime);
            CreateUI(DataStore.AvatarProperties.BodyType, assetIconDownloadTasks);
            partnerAssetManager.DownloadAssetsIcon(assetButtonCreator.SetAssetIcons);
        }

        private async void CreateDefaultModel()
        {
            var startTime = Time.time;
            DataStore.AvatarProperties.Assets = DataStore.AvatarProperties.Gender == OutfitGender.Feminine
                ? AvatarPropertiesConstants.FemaleDefaultAssets
                : AvatarPropertiesConstants.MaleDefaultAssets;

            avatarManager = new AvatarManager(
                DataStore.User.Token,
                DataStore.AvatarProperties.BodyType,
                DataStore.AvatarProperties.Gender,
                inCreatorConfig);

            avatar = await avatarManager.Create(DataStore.AvatarProperties);

            var avatarLoadingTime = Time.time - startTime;
            DebugPanel.AddLogWithDuration("Avatar loaded", avatarLoadingTime);

            ProcessAvatar();
            Loading.SetActive(false);
        }

        private async void UpdateAvatar(string assetId, AssetType assetType)
        {
            var startTime = Time.time;

            var payload = new AvatarProperties
            {
                Assets = new Dictionary<AssetType, object>()
            };

            payload.Assets.Add(assetType, assetId);
            lastRotation = avatar.transform.rotation = lastRotation;
            avatar = await avatarManager.Update(assetId, assetType);
            ProcessAvatar();
            DebugPanel.AddLogWithDuration("Avatar updated", Time.time - startTime);
        }


        private void ProcessAvatar()
        {
            if (DataStore.AvatarProperties.BodyType == BodyType.FullBody)
            {
                avatar.GetComponent<Animator>().runtimeAnimatorController = animator;
            }
            avatar.transform.rotation = lastRotation;
            avatar.AddComponent<RotateAvatar>();
        }
    }
}
