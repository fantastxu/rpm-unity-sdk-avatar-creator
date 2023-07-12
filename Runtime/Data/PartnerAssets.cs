﻿using System.Collections.Generic;
using Newtonsoft.Json;
using ReadyPlayerMe.AvatarLoader;

namespace ReadyPlayerMe.AvatarCreator
{
    public class PartnerAsset
    {
        public string Id;
        [JsonConverter(typeof(AssetTypeConverter))]
        public AssetType AssetType;
        [JsonConverter(typeof(GenderConverter))]
        public OutfitGender Gender;
        public string Icon;
        public string Mask;
        public LockedCategories[] LockedCategories;
    }

    public class LockedCategories
    {
        public string Name;
        public KeyValuePair<string, string>[] CustomizationCategories;
    }
}
