using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace NoSuchStudio.Common {

    public class Events : MonoBehaviour {
        public static Events gInstance;
        public delegate void EventsDelegate(string eventName, params object[] eventParams);
        public event EventsDelegate gEvent;

        private bool _alreadyRaised = false;
        private Queue<Tuple<string, object[]>> _eventQueue = new Queue<Tuple<string, object[]>>();

        
        void Awake() {
            
            if (gInstance == null) gInstance = this;
            
            if (gInstance != this)
                throw new ApplicationException("Only one events object.");
        }

        public void RaiseEventImmediate(string eventName, params object[] eventParams) {
            gEvent(eventName, eventParams);
        }

        public void RaiseEvent(string eventName, params object[] eventParams) {
            _eventQueue.Enqueue(Tuple.Create(eventName, eventParams));
            if (!_alreadyRaised) {
                StartCoroutine(RaiseEventInternal());
            }
        }

        private IEnumerator RaiseEventInternal() {
            _alreadyRaised = true;
            yield return null; 
            _alreadyRaised = false;
            Queue<Tuple<string, object[]>> curEventBatch = _eventQueue;
            _eventQueue = new Queue<Tuple<string, object[]>>();
            foreach (Tuple<string, object[]> t in curEventBatch) {
                gEvent(t.Item1, t.Item2);
            }
        }

    }
}