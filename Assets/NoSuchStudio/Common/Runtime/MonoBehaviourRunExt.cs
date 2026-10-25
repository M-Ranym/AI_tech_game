using System;
using System.Collections;

using UnityEngine;

namespace NoSuchStudio.Common {


    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
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