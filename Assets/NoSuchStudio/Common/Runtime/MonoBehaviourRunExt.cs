using System;
using System.Collections;

using UnityEngine;

namespace NoSuchStudio.Common {


    /// <summary>
    /// Utility class for UnityEngine.Object subclasses (MonoBehaviour, Component, Editor, etc.) that want to use the extended logging capabilities below:
    /// <ul>
    /// <li>Option to log ThreadId, class name, object name, game time or other common info to log messages.</li>
    /// <li>Configure the info PER CLASS. Useful for debugging specific classes.</li>
    /// </ul>
    /// </summary>
    /// <remarks>
    /// This class keeps track of all types that use it and creates a <see cref="UnityEngine.Logger"/> for each. 
    /// Any messages logged through the extension methods will have the info based on the LoggerConfig for that type prepended to the message.
    /// <code>
    /// MyClass myObj = new MyClass(); // MyClass extends UnityEngine.Object (i.e. MonoBehaviour, Editor, Component, ...)
    /// myObj.LogLog("Hello World!"); 
    /// // will print "[1][4.56](MyClass)(myObjName) Hello World!"
    /// </code>
    /// Using sample code like below, you can filter your logs by class.
    /// <code>
    /// UnityObjectLoggerExt.GetLoggerByType&lt;MyClass&gt;().logger.filterLogType = LogType.Error;
    /// </code>
    /// Using sample code like below, you can change the logging config for each class.
    /// <code>
    /// UnityObjectLoggerExt.GetLoggerByType&lt;MyClass&gt;().loggerConfig.logGameTime = false;
    /// </code>
    /// </remarks>
    public static class MonoBehaviourRunExt {

        public static IEnumerator PeriodicCoroutine(object ie, Action a) {
            while (true) {
                a();
                yield return ie;
            }
        }

        public static IEnumerator PredicatedCoroutine(object ie, Func<bool> p, Action a) {
            while (p()) {
                a();
                yield return ie;
            }
        }

        public static IEnumerator DelayedCoroutine(object ie, Action a) {
            yield return ie;
            a();
        }

        public static Coroutine RunDelayed(this MonoBehaviour mono, float delay, Action a) {
            if (mono == null || !mono.isActiveAndEnabled || !mono.gameObject.activeInHierarchy) return null;
            return mono.StartCoroutine(DelayedCoroutine(new WaitForSeconds(delay), a));
        }

        public static Coroutine RunDelayedRealtime(this MonoBehaviour mono, float delay, Action a) {
            if (mono == null || !mono.isActiveAndEnabled || !mono.gameObject.activeInHierarchy) return null;
            return mono.StartCoroutine(DelayedCoroutine(new WaitForSecondsRealtime(delay), a));
        }

        public static Coroutine RunPeriodic(this MonoBehaviour mono, float timestep, Action a) {
            if (mono == null || !mono.isActiveAndEnabled || !mono.gameObject.activeInHierarchy) return null;
            return mono.StartCoroutine(PeriodicCoroutine(new WaitForSeconds(timestep), a));
        }

        public static Coroutine RunPeriodicRealtime(this MonoBehaviour mono, float timestep, Action a) {
            if (mono == null || !mono.isActiveAndEnabled || !mono.gameObject.activeInHierarchy) return null;
            return mono.StartCoroutine(PeriodicCoroutine(new WaitForSecondsRealtime(timestep), a));
        }

        public static Coroutine RunWhile(this MonoBehaviour mono, float timestep, Func<bool> p, Action a) {
            if (mono == null || !mono.isActiveAndEnabled || !mono.gameObject.activeInHierarchy) return null;
            return mono.StartCoroutine(PredicatedCoroutine(new WaitForSeconds(timestep), p, a));
        }

        public static Coroutine RunWhileRealtime(this MonoBehaviour mono, float timestep, Func<bool> p, Action a) {
            if (mono == null || !mono.isActiveAndEnabled || !mono.gameObject.activeInHierarchy) return null;
            return mono.StartCoroutine(PredicatedCoroutine(new WaitForSecondsRealtime(timestep), p, a));
        }
    }
}