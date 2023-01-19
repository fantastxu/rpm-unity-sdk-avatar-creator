﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NativeAvatarCreator;
using UnityEngine;
using UnityEngine.UI;

namespace AvatarCreatorExample
{
    public class AvatarCreatorSelection : SelectionPanel
    {
        [SerializeField] private GameObject assetButtonPrefab;
        [SerializeField] private GameObject clearAssetSelectionButton;
        [SerializeField] private AssetTypeUICreator assetTypeUICreator;

        public Action Show;
        public Action Hide;

        private Dictionary<AssetType, AssetButton> selectedAssetByTypeMap;

        private void OnEnable()
        {
            Loading.SetActive(true);
            Show?.Invoke();
        }

        private void OnDisable()
        {
            assetTypeUICreator.ResetUI();
            Hide?.Invoke();
        }

        public void AddAllAssetButtons(Dictionary<PartnerAsset, Task<Texture>> assets, Action<string, AssetType> onClick)
        {
            selectedAssetByTypeMap = new Dictionary<AssetType, AssetButton>();
            var distinctAssetType = assets.Keys.Select(x => x.AssetType).Distinct();
            assetTypeUICreator.CreateUI(AssetTypeHelper.GetAssetTypeList().Where(x => distinctAssetType.Contains(x)));

            foreach (var asset in assets)
            {
                var parent = assetTypeUICreator.AssetTypePanelsMap[asset.Key.AssetType];
                AddAssetButton(asset.Key.Id, parent, asset.Key.AssetType, onClick, asset.Value);
            }

            foreach (var assetTypePanelMap in assetTypeUICreator.AssetTypePanelsMap)
            {
                var assetType = assetTypePanelMap.Key;
                var assetTypePanel = assetTypePanelMap.Value;
                if (assetType != AssetType.Outfit && assetType != AssetType.Shirt)
                {
                    AddAssetSelectionClearButton(assetTypePanel, assetType, onClick);
                }
            }

            Loading.SetActive(false);
        }

        private async void AddAssetButton(string assetId, Transform parent, AssetType assetType, Action<string, AssetType> onClick,
            Task<Texture> iconDownloadTask)
        {
            var assetButtonGameObject = Instantiate(assetButtonPrefab, parent.GetComponent<ScrollRect>().content);
            var assetButton = assetButtonGameObject.GetComponent<AssetButton>();
            assetButton.AddListener(() =>
            {
                if (selectedAssetByTypeMap.ContainsKey(assetType))
                {
                    selectedAssetByTypeMap[assetType].SetSelect(false);
                    selectedAssetByTypeMap[assetType] = assetButton;
                }
                else
                {
                    selectedAssetByTypeMap.Add(assetType, assetButton);
                }

                assetButton.SetSelect(true);
                onClick?.Invoke(assetId, assetType);
            });
            assetButton.SetIcon(await iconDownloadTask);
        }

        private void AddAssetSelectionClearButton(Transform parent, AssetType assetType, Action<string, AssetType> onClick)
        {
            var assetButtonGameObject = Instantiate(clearAssetSelectionButton, parent.GetComponent<ScrollRect>().content);
            assetButtonGameObject.transform.SetAsFirstSibling();
            assetButtonGameObject.GetComponent<Button>().onClick.AddListener(() =>
            {
                if (selectedAssetByTypeMap.ContainsKey(assetType))
                {
                    selectedAssetByTypeMap[assetType].SetSelect(false);
                }

                onClick?.Invoke(string.Empty, assetType);
            });
        }
    }
}
