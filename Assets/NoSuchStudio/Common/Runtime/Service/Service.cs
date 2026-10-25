using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NoSuchStudio.Common.Service {
    
    
    
    
    
    
    
    
    
    
    
    
    
    public abstract class Service<T> : NoSuchMonoBehaviour
        where T : Service<T> {

        private static bool alreadyLogged;
        
        
        
        
        protected static T gInstance;
        
        
        
        
        
        public static T Instance {
            get {
                if (gInstance == null) {
                    if (!alreadyLogged) LogLog<T>("[Static] instance not set, searching...");
                    alreadyLogged = true;
                    List<T> tmpList = FindObjectsOfType<T>().ToList();
                    tmpList.Find(tmp => {
                        if (tmp == null) {
                            if (!alreadyLogged) LogLog<T>(tmp, $"[Static] no instances found. Create one and enable it to use this service.");
                            return false;
                        } else if (!EditorUtilities.IsInMainStage(tmp.gameObject)) {
                            if (!alreadyLogged) LogLog<T>(tmp, $"[Static] instance {tmp.name} not in scene.");
                            return false;
                        } else if (!tmp.InstanceReady) {
                            if (!alreadyLogged) LogLog<T>(tmp, $"[Static] found `{tmp.name}` but not ready (is it enabled and active?), check it in editor and fix any setup problems.");
                            return false;
                        } else {
                            LogLog<T>(tmp, $"[Static] instance found: `{tmp.name}`.");
                            alreadyLogged = false;
                            Instance = tmp;
                            return true;
                        }
                    });
                }
                return gInstance;
            }
            protected set {
                alreadyLogged = false;
                if (value != null && !EditorUtilities.IsInMainStage(value.gameObject)) {
                    LogWarn<T>(gInstance, "[Static] new instance isn't in scene. Not registering.");
                    return;
                }

                if (gInstance != null) {
                    LogLog<T>(gInstance, "[Static] replacing previous instance.");
                    gInstance.OnServiceUnregister();
                }

                gInstance = value;

                if (gInstance == null) {
                    LogLog<T>("[Static] global instance set to null.");
                } else {
                    LogLog<T>(gInstance, $"[Static] registering global instance to {gInstance.name}.");
                    gInstance.OnServiceRegister();
                }
            }
        }

        [NonSerialized] public bool needsEditorRepaint; 

        protected bool _instanceReady; 
        public bool InstanceReady {
            get { return _instanceReady; }
        }

        protected virtual void OnEnable() {
            _instanceReady = true;
            RegisterInstance((T)this);
        }

        protected virtual void OnDisable() {
            UnregisterInstance((T)this);
            _instanceReady = false;
        }

        
        
        
        
        
        public static bool IsReady {
            get { return Instance != null; }
        }

        
        
        
        public virtual void OnServiceRegister() { }

        
        
        
        public virtual void OnServiceUnregister() { }

        
        
        
        public bool IsInstance {
            get { return Instance == this; }
        }

        
        
        
        protected static void RegisterInstance(T instance) {
            Instance = instance;
        }

        
        
        
        protected static void UnregisterInstance(T instance) {
            if (gInstance == instance) {
                Instance = null;
            } else {
                LogWarn<T>("instance different than current, NOP.");
            }
        }

        
        
        
        public void ReRegisterService() {
            if (_instanceReady) {
                UnregisterInstance((T)this);
                RegisterInstance((T)this);
            }
        }

        private static T CreateInstance() {
            GameObject instance = new GameObject(typeof(T).Name);
            return instance.AddComponent<T>();
        }
    }
}
