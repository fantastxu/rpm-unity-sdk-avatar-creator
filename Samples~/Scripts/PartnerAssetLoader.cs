﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AvatarCreator;
using ReadyPlayerMe.AvatarLoader;
using UnityEngine;

namespace AvatarCreatorExample
{
    public class PartnerAssetLoader : MonoBehaviour
    {
        [SerializeField] private DataStore dataStore;
        [SerializeField] private AvatarCreatorSelection avatarCreatorSelection;

        private void OnEnable()
        {
            avatarCreatorSelection.Show += Show;
        }

        private void OnDisable()
        {
            avatarCreatorSelection.Show -= Show;
        }

        private async void Show()
        {
            await GetAllAssets();
        }

        private async Task GetAllAssets()
        {
            var startTime = Time.time;
            var assets = await PartnerAssetsRequests.Get(dataStore.User.Token, dataStore.AvatarProperties.Partner);
            assets = assets.Where(FilterAssets).ToArray();
            DebugPanel.AddLogWithDuration("Got all partner assets", Time.time - startTime);

            var assetIconDownloadTasks = new Dictionary<PartnerAsset, Task<Texture>>();

            foreach (var asset in assets)
            {
                var iconDownloadTask = PartnerAssetsRequests.GetAssetIcon(
                    dataStore.User.Token,
                    asset.AssetType == AssetType.EyeColor ? asset.Mask + "?w=256" : asset.Icon);
                assetIconDownloadTasks.Add(asset, iconDownloadTask);
            }

            avatarCreatorSelection.AddAllAssetButtons(dataStore.AvatarProperties.BodyType, assetIconDownloadTasks);
            await Task.Yield();
        }

        private bool FilterAssets(PartnerAsset asset)
        {
            // Outfit is only for full body and are gender specific.
            // Shirt is only for half body.
            if (dataStore.AvatarProperties.BodyType != BodyType.FullBody)
                return asset.AssetType != AssetType.Outfit;

            if (asset.AssetType == AssetType.Outfit)
                return asset.Gender == dataStore.AvatarProperties.Gender;

            return asset.AssetType != AssetType.Shirt;
        }
    }
}