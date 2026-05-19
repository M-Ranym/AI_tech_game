using UnityEngine;
using NoSuchStudio.Common.Service;
using NoSuchStudio.Common;
using System;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.Events;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("NoSuchStudio-Networking-Editor")]

namespace NoSuchStudio.Networking {
    [ExecuteAlways]
    public class InternetConnectionWatcher : Service<InternetConnectionWatcher> {

        [Serializable]
        public class InternetConnectivityEvent : UnityEvent<bool> { }
        
        [Header("Check Properties")]
        [SerializeField, Range(1, 15)] int _checkTimeout;
        public int checkTimeout {
            get { return _checkTimeout; }
            set {
                _checkTimeout = value;
                _checkInterval = Mathf.Max(_checkInterval, _checkTimeout);
            }
        }

        [SerializeField] string _checkURL; // "https://www.google.com"

        [Header("Periodic Checks")]
        [SerializeField] bool _checkPeriodically;
        public bool checkPeriodically {
            get { return _checkPeriodically; }
            set {
                _checkPeriodically = value;
                SyncPeriodicCheck();
            }
        }

        [SerializeField, Range(1f, 5f*60f)] float _checkInterval;
        public float checkInterval {
            get { return _checkInterval; }
            set {
                _checkInterval = value;
                _checkTimeout = Math.Min(_checkTimeout, (int)_checkInterval);
            }
        }

        [SerializeField, HideInInspector] private bool _isConnected;
        public bool isConnected {
            get { return _isConnected; }
        }

        [SerializeField, HideInInspector] private float _lastCheckTime;
        public float lastCheckTime {
            get { return _lastCheckTime; }
        }

        [Header("Events")]
        public InternetConnectivityEvent connectivityChangeEvent; // raised when connectivity state changes
        public InternetConnectivityEvent connectivityCheckEvent; // raised for every completed check

        private UnityWebRequestAsyncOperation _checkOperation;

        private Coroutine _periodicCheckCoroutine;

        private void CancelPeriodicCoroutine() {
            if (_periodicCheckCoroutine != null) {
                StopCoroutine(_periodicCheckCoroutine);
                _periodicCheckCoroutine = null;
            }
        }

        public void SyncPeriodicCheck() {
            CancelPeriodicCoroutine();
            if (_checkPeriodically) {
                _periodicCheckCoroutine = StartCoroutine(PeriodicCheck());
            }
        }

        IEnumerator PeriodicCheck() {
            while (true) {
                CheckInternetConnectivity();
                yield return new WaitForSecondsRealtime(_checkInterval);
            }
        }

        protected override void OnEnable() {
            if (EditorUtilities.IsInMainStage(gameObject) && isActiveAndEnabled) SyncPeriodicCheck();
        }

        protected override void OnDisable() {
            base.OnDisable();
        }

        private void OnValidate() {
            if (EditorUtilities.IsInMainStage(gameObject) && isActiveAndEnabled) SyncPeriodicCheck();
        }

        public void CancelCurrentCheck() {
            if (_checkOperation != null) {
                _checkOperation.completed -= CheckInternetConnectivityCompleted;
                _checkOperation = null;
            }
        }

        public void CheckInternetConnectivity() {
            CancelCurrentCheck();

            UnityWebRequest request = new UnityWebRequest(_checkURL);
            request.timeout = _checkTimeout;
            _checkOperation = request.SendWebRequest();
            _checkOperation.completed += CheckInternetConnectivityCompleted;
        }

        private void CheckInternetConnectivityCompleted(AsyncOperation obj) {
            if (this == null) return;
            var httpRes = _checkOperation.webRequest;
            CancelCurrentCheck();
            if (!string.IsNullOrEmpty(httpRes.error)) {
                LogWarn($"connection error: {httpRes.error}");
                OnConnectivityUpdate(false);
            } else {
                LogLog("connection successful!");
                OnConnectivityUpdate(true);
            }
        }

        void OnConnectivityUpdate(bool isConnected) {
            bool connectivityChanged = isConnected != _isConnected;
            if (isConnected) {
                _isConnected = true;
                _lastCheckTime = Time.realtimeSinceStartup;
            } else {
                _isConnected = false;
                _lastCheckTime = Time.realtimeSinceStartup;
            }
            connectivityCheckEvent.Invoke(isConnected);
            if (connectivityChanged) {
                connectivityChangeEvent.Invoke(isConnected);
            }
        }
    }
}
