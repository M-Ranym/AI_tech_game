using System;
using System.Collections.Generic;

using UnityEngine;

namespace NoSuchStudio.Localization {
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    [Serializable]
    public class LocalizedAssetData<T> {
        [SerializeField][LocaleProperty(false, LocalePropertyDrawMode.MenuDropdown)] private string _locale;
        public string locale {
            get { return _locale; }
        }
        [SerializeField] private T _data;
        public T data {
            get { return _data; }
        }
        public LocalizedAssetData(string name, T data) {
            this._locale = name;
            this._data = data;
        }
    }

    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    public abstract class AssetMapComponentLocalizer<LT, CT, AT, LAD> : ComponentLocalizer<LT, CT>
        where LT : AssetMapComponentLocalizer<LT, CT, AT, LAD>
        where CT : Component
        where LAD : LocalizedAssetData<AT> {
        [SerializeField] protected AT _defaultAsset;
        [SerializeField] protected List<LAD> _assetList;
        [NonSerialized] protected Dictionary<string, AT> _assets;

        protected override void Init() {
            base.Init();
            _assets = _assets ?? new Dictionary<string, AT>();
            _assets.Clear();
            if (_assetList != null) {
                _assetList.ForEach(ld => {
                    if (string.IsNullOrEmpty(ld.locale)) return;
                    _assets[ld.locale] = ld.data;
                });
            }
        }
    }
}
