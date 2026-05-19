using NoSuchStudio.Common;
using NoSuchStudio.Common.Service;
using NoSuchStudio.Variables;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NoSuchStudio.DataStorage {

    [ExecuteAlways]
    public class DataStore : NoSuchMonoBehaviour, ISerializationCallbackReceiver, IVariableSource {

        public enum KnownType {
            None = 0,
            Int = 1,
            Float = 2,
            String = 3
        }

        // For compatibility with Unity Inspector
        [Serializable]
        public struct IntDataEntry {
            public IntDataEntry(string n, int v, bool syncPrefs, bool syncVariables) {
                this.name = n;
                this.value = v;
                this.syncPrefs = syncPrefs;
                this.syncVariables = syncVariables;
            }
            public string name;
            public int value;
            public bool syncPrefs;
            public bool syncVariables;
        }
        [Serializable]
        public struct FloatDataEntry {
            public FloatDataEntry(string n, float v, bool syncPrefs, bool syncVariables) {
                this.name = n;
                this.value = v;

                this.syncPrefs = syncPrefs;
                this.syncVariables = syncVariables;
            }
            public string name;
            public float value;
            public bool syncPrefs;
            public bool syncVariables;
        }
        [Serializable]
        public struct StringDataEntry {
            public StringDataEntry(string n, string v, bool syncPrefs, bool syncVariables) {
                this.name = n;
                this.value = v;
                this.syncPrefs = syncPrefs;
                this.syncVariables = syncVariables;
            }
            public string name;
            public string value;
            public bool syncPrefs;
            public bool syncVariables;
        }

        [Serializable]
        public struct DataEntry {
            public KnownType type;
            public object value;
            public bool syncToPrefs;
            public bool syncToVariables;

            public DataEntry(KnownType type, object value, bool syncToPrefs = true, bool syncToVariables = false) {
                this.type = type;
                this.value = value;
                this.syncToPrefs = syncToPrefs;
                this.syncToVariables = syncToVariables;
            }

            public override string ToString() {
                string vstr = "none";
                switch (type) {
                    case KnownType.Int:
                        vstr = ((int)value).ToString();
                        break;
                    case KnownType.Float:
                        vstr = ((float)value).ToString();
                        break;
                    case KnownType.String:
                        vstr = (string)value;
                        break;
                }
                return string.Format("DataEntry({0}, {1})", type, vstr);
            }

            public object GetValue() {
                return value;
            }

            public string GetStringValue() {
                return value.ToString();
            }

            public int GetIntValue() {
                switch (type) {
                    case KnownType.Int:
                        return (int)value;
                    case KnownType.Float:
                        return (int)(float)value;
                    case KnownType.String:
                        int retInt;
                        int.TryParse((string)value, out retInt);
                        return retInt;
                    default:
                        return default(int);

                }
            }

            public float GetFloatValue() {
                switch (type) {
                    case KnownType.Int:
                        return (int)value;
                    case KnownType.Float:
                        return (float)value;
                    case KnownType.String:
                        float retFloat;
                        float.TryParse((string)value, out retFloat);
                        return retFloat;
                    default:
                        return default(int);

                }
            }

            public bool GetBoolValue() {
                switch (type) {
                    case KnownType.Int:
                        return (int)value != 0;
                    case KnownType.Float:
                        return (float)value != 0;
                    case KnownType.String:
                        bool retBool;
                        bool.TryParse((string)value, out retBool);
                        return retBool;
                    default:
                        return default(bool);

                }
            }


            public bool SetValue(object v) {
                Type t = v.GetType();
                object oldValue = value;
                value = null;
                switch (type) {
                    case KnownType.Int:
                        if (t == typeof(int) || t == typeof(float) || t == typeof(short) || t == typeof(long) || t == typeof(double)) {
                            value = (int)v;
                        } else if (t == typeof(bool)) {
                            value = (bool)v ? 1 : 0;
                        } else if (t == typeof(string)) {
                            value = int.Parse((string)v);
                        } else {
                            throw new IllegalStateException(string.Format("dataentry is int and not compatible with {0}.", t));
                        }
                        break;

                    case KnownType.Float:
                        if (t == typeof(int) || t == typeof(float) || t == typeof(short) || t == typeof(long) || t == typeof(double)) {
                            value = (float)v;
                        } else if (t == typeof(string)) {
                            value = float.Parse(v.ToString());
                        } else {
                            throw new IllegalStateException(string.Format("dataentry is float and not compatible with {0}.", t));
                        }
                        break;

                    case KnownType.String:
                        if (t == typeof(string)) {
                            value = v;
                        } else if (t == typeof(int) || t == typeof(float) || t == typeof(short) || t == typeof(long) || t == typeof(double)) {
                            value = v.ToString();
                        } else {
                            throw new IllegalStateException(string.Format("dataentry is string and not compatible with {0}.", t));
                        }
                        break;
                }
                bool valueChanged = !Equals(oldValue, value);
                return valueChanged;
            }
        }

        [SerializeField] private Scope _scope;
        public Scope scope {
            get { return _scope; }
            set {
                Disconnect<VariablesService>();
                _scope = value;
                if (!_disableSyncWithPrefs) {
                    SyncToPrefs();
                }
                Connect<VariablesService>();
            }
        }

        [SerializeField] private bool _readOnly;
        public bool readOnly {
            get { return _readOnly; }
            set {
                _readOnly = value;
            }
        }

        [SerializeField] private bool _disableSyncWithPrefs;
        public bool disableSyncWithPrefs {
            get { return _disableSyncWithPrefs; }
            set {
                _disableSyncWithPrefs = value;
                if (!_disableSyncWithPrefs) {
                    SyncToPrefs();
                }
            }
        }

        [SerializeField] private bool _disableSyncWithVariables;
        public bool disableSyncWithVariables {
            get { return _disableSyncWithVariables; }
            set {
                _disableSyncWithVariables = value;
                if (_disableSyncWithVariables) {
                    Disconnect<VariablesService>();
                } else {
                    if (IsConnected<VariablesService>()) {
                        SyncToVariables();
                    }
                }
            }
        }

        [SerializeField] List<IntDataEntry> _unityIntData = new List<IntDataEntry>();
        [SerializeField] List<FloatDataEntry> _unityFloatData = new List<FloatDataEntry>();
        [SerializeField] List<StringDataEntry> _unityStringData = new List<StringDataEntry>();
        Dictionary<string, DataEntry> _data = new Dictionary<string, DataEntry>();
#if UNITY_EDITOR && NEWTONSOFTJSON_PRESENT
        public Dictionary<string, DataEntry> dataEditor {
            get { return _data; }
        }

        public void LoadFromJson(string jsonStr) {
            _data = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, DataEntry>>(jsonStr);
            _data.Keys.ToList().ForEach(k => {
                var entry = _data[k];
                var newEntry = entry;
                switch (newEntry.type) {
                    case KnownType.None:
                        break;
                    case KnownType.Int:
                        newEntry.value = Convert.ToInt32(newEntry.value);
                        break;
                    case KnownType.Float:
                        newEntry.value = Convert.ToSingle(newEntry.value);
                        break;
                    case KnownType.String:
                        newEntry.value = Convert.ToString(newEntry.value);
                        break;
                }
                _data[k] = newEntry;
            });
        }

        public string ExportAsJson() {
            return Newtonsoft.Json.JsonConvert.SerializeObject(_data);
        }
#endif

        public DataEntry AddDataEntryLocal(string localKey, KnownType type, object val, bool syncToPrefs = true, bool syncToVariables = false) {
            bool exists = _data.ContainsKey(localKey);
            if (exists) throw new ApplicationException($"data entry for key {name} already exists.");
            var newEntry = new DataEntry(type, val, syncToPrefs, syncToVariables);
            _data[localKey] = newEntry;
            if (syncToPrefs) {
                SyncToPrefs(localKey);
            }
            if (syncToVariables) {
                if (IsConnected<VariablesService>()) {
                    SyncToVariables(localKey);
                }
            }
            return newEntry;
        }

        public DataEntry AddDataEntry(string key, KnownType type, object val, bool syncToPrefs = true, bool syncToVariables = false) {
            var localKey = GlobalToLocalKey(key);
            return AddDataEntryLocal(localKey, type, val, syncToPrefs, syncToVariables);
        }

        public DataEntry AddIntDataEntry(string key, int val, bool syncToPrefs = true, bool syncToVariables = false) {
            return AddDataEntry(key, KnownType.Int, val, syncToPrefs, syncToVariables);
        }

        public DataEntry AddFloatDataEntry(string key, float val, bool syncToPrefs = true, bool syncToVariables = false) {
            return AddDataEntry(key, KnownType.Float, val, syncToPrefs, syncToVariables);
        }

        public DataEntry AddStringDataEntry(string key, string val, bool syncToPrefs = true, bool syncToVariables = false) {
            return AddDataEntry(key, KnownType.String, val, syncToPrefs, syncToVariables);
        }

        private bool _initialized;
        public bool IsInitialized {
            get { return _initialized; }
        }

        protected Dictionary<Type, bool> _serviceConnections;

        void OnEnable() {
            Connect<VariablesService>();
        }
        void OnDisable() {
            Disconnect<VariablesService>();
        }

        void Awake() {
            Init();
        }

        bool shouldSyncPrefs {
            get {
                return _initialized && EditorUtilities.IsInMainStage(gameObject);
            }
        }

        private void Init() {
            _serviceConnections = _serviceConnections ?? new Dictionary<Type, bool>();

            if (_initialized) return;
            _initialized = true;

            if (shouldSyncPrefs && !_disableSyncWithPrefs) {
                if (!CommonHelpers.IsEditMode) { // prevent runtime saved pref values to leak back in edit mode
                    SyncFromPrefs();
                }
            }
        }

        private void SyncToVariables(string dn = null) {
            if (dn != null) {
                string prefKey = _scope.Apply(dn);
                var e = _data[dn];
                LogLogFormat("sync {0} to variables... {1}", dn, e);
                if (!e.syncToVariables) {
                    LogLog("not syncing due to entry prop.");
                    return;
                }
                VariablesService.AddVariableSource(prefKey, this);
                LogLog($"synced {dn} to variables as {prefKey} -> {e.value}");
            } else { // all values
                foreach (string k in _data.Keys) {
                    SyncToVariables(k);
                }
            }
        }

        private void SyncChangeToVariables(string dn) {
            if (!IsConnected<VariablesService>()) return;
            if (dn != null) {
                string prefKey = _scope.Apply(dn);
                var e = _data[dn];
                LogLog($"sync {dn} change to variables... {e}");
                if (!e.syncToVariables) {
                    LogLog("not syncing due to entry prop.");
                    return;
                }
                VariablesService.SetVariableValueChanged(prefKey, this);
                LogLog($"synced {dn} change to variables as {prefKey} -> {e.value}");
            };
        }

        private void UnsyncToVariables(string lk = null) {
            if (lk != null) {
                string prefKey = _scope.Apply(lk);
                var e = _data[lk];
                LogLog($"unsync {lk} to variables... {e}");
                if (!e.syncToVariables) {
                    LogLog($"not unsyncing due to entry prop.");
                    return;
                }
                VariablesService.RemoveVariableSource(prefKey, this);
                LogLog($"unsynced {lk} to variables as {prefKey} -> {e.value}");
            } else { // all values
                foreach (string localKey in _data.Keys) {
                    UnsyncToVariables(localKey);
                }
            }
        }

        public void SyncToPrefs(string lk = null) {
            if (lk != null) {
                string prefKey = _scope.Apply(lk);
                var e = GetLocalEntry(lk);
                LogLog($"sync {lk} to pref... {e}");
                if (!e.syncToPrefs) {
                    LogLog($"sync {lk} -> {e} to pref: not prop option not set.");
                    return;
                }
                switch (e.type) {
                    case KnownType.None:
                        break;
                    case KnownType.Int:
                        PlayerPrefs.SetInt(prefKey, e.GetIntValue());
                        break;
                    case KnownType.Float:
                        PlayerPrefs.SetFloat(prefKey, e.GetFloatValue());
                        break;
                    case KnownType.String:
                        PlayerPrefs.SetString(prefKey, e.GetStringValue());
                        break;
                }
                PlayerPrefs.Save();
                LogLog($"synced {lk} to prefs {e.value}");
            } else { // all values
                foreach (string localKey in _data.Keys) {
                    SyncToPrefs(localKey);
                }
            }
        }

        public bool SyncFromPrefs(string lk = null) {
            if (lk != null) { // single value
                DataEntry de = _data[lk];
                LogLog($"sync {lk} from pref... {de}");
                string prefKey = _scope.Apply(lk);
                if (!PlayerPrefs.HasKey(prefKey) || !_data.Keys.Contains(lk) || !de.syncToPrefs) {
                    LogLog($"NOT syncing {lk} from pref {prefKey}.");
                    return false;
                }
                object prefValue = null;
                switch (de.type) {
                    case KnownType.Int:
                        prefValue = PlayerPrefs.GetInt(prefKey, 0);
                        break;
                    case KnownType.Float:
                        prefValue = PlayerPrefs.GetFloat(prefKey, 0f);
                        break;
                    case KnownType.String:
                        prefValue = PlayerPrefs.GetString(prefKey, "");
                        break;
                }
                SetLocal(lk, prefValue);
                LogLog($"set var from pref '{lk}': {_data[lk].value}");
                return true;
            } else { // all values
                foreach (string localKey in _data.Keys.ToList()) {
                    if (!SyncFromPrefs(localKey)) {
                        // missing value, write
                        SyncToPrefs(localKey);
                    }
                }
                return true;
            }
        }

        /*public float SetDataIfAbsent(string dn, float v) {
            if (!_floatData.Contains(dn)) { 
            }
            if (!PlayerPrefs.HasKey(varName)) {
                Set(varName, val);
            }
            return Get(varName);
        }*/

        public void Set(string k, object v) {
            string localKey = GlobalToLocalKey(k);
            SetLocal(localKey, v);
        }

        public void SetLocal(string lk, object v) {
            if (_readOnly) {
                return;
            }

            DataEntry de = GetLocalEntry(lk);
            bool valueChanged = de.SetValue(v);
            if (valueChanged) {
                _data[lk] = de;
                if (isActiveAndEnabled) {
                    if (!_disableSyncWithPrefs) {
                        SyncToPrefs(lk);
                    }
                    if (!_disableSyncWithVariables) {
                        SyncChangeToVariables(lk);
                    }
                }
            }
        }

        private string GlobalToLocalKey(string key) {
            if (!_scope.Match(key)) {
                throw new IllegalStateException(string.Format("key {0} does not match scope {1} in data store {2}", key, _scope, name));
            }
            return _scope.Unapply(key);
        }

        public object Get(string k) {
            string localKey = GlobalToLocalKey(k);
            return GetLocal(localKey);
        }

        public int GetInt(string k) {
            string localKey = GlobalToLocalKey(k);
            return GetIntLocal(localKey);
        }

        public float GetFloat(string k) {
            string localKey = GlobalToLocalKey(k);
            return GetFloatLocal(localKey);
        }

        public string GetString(string k) {
            string localKey = GlobalToLocalKey(k);
            return GetStringLocal(localKey);
        }

        public object GetLocal(string lk) {
            return GetLocalEntry(lk).value;
        }

        public int GetIntLocal(string lk) {
            return GetLocalEntry(lk).GetIntValue();
        }

        public float GetFloatLocal(string lk) {
            return GetLocalEntry(lk).GetFloatValue();
        }

        public string GetStringLocal(string lk) {
            return GetLocalEntry(lk).GetStringValue();
        }

        public bool GetBoolLocal(string lk) {
            return GetLocalEntry(lk).GetBoolValue();
        }

        public DataEntry GetLocalEntry(string lk) {
            if (!_data.Keys.Contains(lk)) {
                throw new IllegalStateException(string.Format("key {0} does not exist in data store {1}", lk, name));
            }
            return _data[lk];
        }

        public DataEntry GetEntry(string k) {
            string localKey = GlobalToLocalKey(k);
            return GetLocalEntry(localKey);
        }

        public bool Delete(string k) {
            var lk = GlobalToLocalKey(k);
            return DeleteLocal(lk);
        }

        public bool DeleteLocal(string lk) {
            if (!_data.ContainsKey(lk)) return false;
            if (!_disableSyncWithPrefs) {
                PlayerPrefs.DeleteKey(lk);
                PlayerPrefs.Save();
            }
            if (IsConnected<VariablesService>()) {
                UnsyncToVariables(lk);
            }
            return _data.Remove(lk);
        }

        public bool ContainsKey(string k) {
            return _data.ContainsKey(k);
        }

        public void Clear() {
            var localKeys = _data.Keys.ToList();
            foreach (var lk in localKeys) {
                DeleteLocal(lk);
            }
            LogLog("cleared.");
        }

        #region ISerializationCallbackReceiver
        // ISerializationCallbackReceiver
        public void OnBeforeSerialize() {
            _unityIntData.Clear();
            _unityFloatData.Clear();
            _unityStringData.Clear();
            foreach (var e in _data) {
                switch (e.Value.type) {
                    case KnownType.Int:
                        _unityIntData.Add(new IntDataEntry(e.Key, (int)e.Value.value, e.Value.syncToPrefs, e.Value.syncToVariables));
                        break;
                    case KnownType.Float:
                        _unityFloatData.Add(new FloatDataEntry(e.Key, (float)e.Value.value, e.Value.syncToPrefs, e.Value.syncToVariables));
                        break;
                    case KnownType.String:
                        _unityStringData.Add(new StringDataEntry(e.Key, (string)e.Value.value, e.Value.syncToPrefs, e.Value.syncToVariables));
                        break;
                }
            }
        }

        public void OnAfterDeserialize() {
            _data.Clear();
            foreach (var e in _unityIntData) {
                // if (string.IsNullOrEmpty(e.name)) continue;
                string tryName = e.name;
                int li = 0;
                while (_data.ContainsKey(tryName)) {
                    tryName = e.name + "_" + (li++);
                }
                _data[tryName] = new DataEntry(KnownType.Int, e.value, e.syncPrefs, e.syncVariables);
            }
            foreach (var e in _unityFloatData) {
                // if (string.IsNullOrEmpty(e.name)) continue;
                string tryName = e.name;
                int li = 0;
                while (_data.ContainsKey(tryName)) {
                    tryName = e.name + "_" + (li++);
                }
                _data[tryName] = new DataEntry(KnownType.Float, e.value, e.syncPrefs, e.syncVariables);
            }
            foreach (var e in _unityStringData) {
                // if (string.IsNullOrEmpty(e.name)) continue;
                string tryName = e.name;
                int li = 0;
                while (_data.ContainsKey(tryName)) {
                    tryName = e.name + "_" + (li++);
                }
                _data[tryName] = new DataEntry(KnownType.String, e.value, e.syncPrefs, e.syncVariables);
            }
        }
        #endregion
        public bool IsConnected<ST>() where ST : Service<ST> {
            Init();
            return _serviceConnections.ContainsKey(typeof(ST)) ? _serviceConnections[typeof(ST)] : false;
        }

        public void Connect<ST>() where ST : Service<ST> {
            Init();
            if (!shouldSyncPrefs) return;
            if (!isActiveAndEnabled) return;
            if (_disableSyncWithVariables) return;
            if (!Service<ST>.IsReady) return;
            if (typeof(ST) != typeof(VariablesService)) return;
            if (IsConnected<ST>()) Disconnect<ST>();
            SyncToVariables();
            _serviceConnections[typeof(ST)] = true;
        }

        public void Disconnect<ST>() where ST : Service<ST> {
            Init();
            // if (!_initialized) return;
            // if (_disableSyncWithVariables) return;
            if (!Service<ST>.IsReady) return;
            if (typeof(ST) != typeof(VariablesService)) return;
            UnsyncToVariables();
            _serviceConnections[typeof(ST)] = false;
        }

        #region IVariableSource
        public MonoBehaviour mono => this;

        public string GetVariable(string variable) {
            return GetString(variable);
        }

        public bool SetVariable(string variable, string value) {
            Set(variable, value);
            return true;
        }

        void IServiceComponent<VariablesService>.Connect<ST>() {
            Connect<VariablesService>();
        }

        void IServiceComponent<VariablesService>.Disconnect<ST>() {
            Disconnect<VariablesService>();
        }

        bool IServiceComponent<VariablesService>.IsConnected<ST>() {
            return IsConnected<VariablesService>();
        }
        #endregion

#if UNITY_EDITOR
        private void Reset() {
            Clear();
            scope = Scope.Global;
        }

        private void OnValidate() {
            Connect<VariablesService>();
        }
#endif
    }
}
