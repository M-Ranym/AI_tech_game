using System;

using UnityEngine;

namespace NoSuchStudio.Common {
    /// <summary>
    /// Base class for MonoBehaviours that have helper functions from <see cref="UnityObjectLoggerExt"/> and <see cref="MonoBehaviourRunDelayedExt"/>
    /// included in them.
    /// </summary>
    public abstract class NoSuchMonoBehaviour : MonoBehaviour {

        public Logger logger {
            get {
                Type thisType = GetType();
                return UnityObjectLoggerExt.GetLoggerByType(thisType).logger;
            }
        }
        public LoggerConfig loggerConfig {
            get {
                Type thisType = GetType();
                return UnityObjectLoggerExt.GetLoggerByType(thisType).loggerConfig;
            }
        }

        protected void LogLogFormat(string format, params object[] args) {
            UnityObjectLoggerExt.LogLogFormat(this, format, args);
        }
        protected void LogLog(string msg) {
            UnityObjectLoggerExt.LogLog(this, msg);
        }
        protected void LogWarnFormat(string format, params object[] args) {
            UnityObjectLoggerExt.LogWarnFormat(this, format, args);
        }
        protected void LogWarn(string log) {
            UnityObjectLoggerExt.LogWarn(this, log);
        }
        protected void LogErrorFormat(string format, params object[] args) {
            UnityObjectLoggerExt.LogErrorFormat(this, format, args);
        }
        protected void LogError(string log) {
            UnityObjectLoggerExt.LogError(this, log);
        }

        public static void LogLogFormat<T>(string format, params object[] args) {
            UnityObjectLoggerExt.LogLogFormat<T>(format, args);
        }
        public static void LogLog<T>(string log) {
            UnityObjectLoggerExt.LogLog<T>(log);
        }
        public static void LogWarnFormat<T>(string format, params object[] args) {
            UnityObjectLoggerExt.LogWarnFormat<T>(format, args);
        }
        public static void LogWarn<T>(string log) {
            UnityObjectLoggerExt.LogWarn<T>(log);
        }
        public static void LogErrorFormat<T>(string format, params object[] args) {
            UnityObjectLoggerExt.LogErrorFormat<T>(format, args);
        }
        public static void LogError<T>(string log) {
            UnityObjectLoggerExt.LogError<T>(log);
        }

        public static void LogLogFormat<T>(UnityEngine.Object unityObj, string format, params object[] args) {
            UnityObjectLoggerExt.LogLogFormat<T>(unityObj, format, args);
        }
        public static void LogLog<T>(UnityEngine.Object unityObj, string log) {
            UnityObjectLoggerExt.LogLog<T>(unityObj, log);
        }
        public static void LogWarnFormat<T>(UnityEngine.Object unityObj, string format, params object[] args) {
            UnityObjectLoggerExt.LogWarnFormat<T>(unityObj, format, args);
        }
        public static void LogWarn<T>(UnityEngine.Object unityObj, string log) {
            UnityObjectLoggerExt.LogWarn<T>(unityObj, log);
        }
        public static void LogErrorFormat<T>(UnityEngine.Object unityObj, string format, params object[] args) {
            UnityObjectLoggerExt.LogErrorFormat<T>(unityObj, format, args);
        }
        public static void LogError<T>(UnityEngine.Object unityObj, string log) {
            UnityObjectLoggerExt.LogError<T>(unityObj, log);
        }

        protected Coroutine RunDelayed(float delay, Action a) {
            return MonoBehaviourRunExt.RunDelayed(this, delay, a);
        }

        protected Coroutine RunDelayedRealtime(float delay, Action a) {
            return MonoBehaviourRunExt.RunDelayedRealtime(this, delay, a);
        }

        protected Coroutine RunPeriodic(float timestep, Action a) {
            return MonoBehaviourRunExt.RunPeriodic(this, timestep, a);
        }

        protected Coroutine RunPeriodicRealtime(float timestep, Action a) {
            return MonoBehaviourRunExt.RunPeriodicRealtime(this, timestep, a);
        }

        protected Coroutine RunWhile(float timestep, Func<bool> p, Action a) {
            return MonoBehaviourRunExt.RunWhile(this, timestep, p, a);
        }

        protected Coroutine RunWhileRealtime(float timestep, Func<bool> p, Action a) {
            return MonoBehaviourRunExt.RunWhileRealtime(this, timestep, p, a);
        }
    }
}