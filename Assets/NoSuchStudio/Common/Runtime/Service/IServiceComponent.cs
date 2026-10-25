using UnityEngine;

namespace NoSuchStudio.Common.Service
{
    
    
    
    
    
    
    
    
    
    
    
    
    public interface IServiceComponent<T> where T : Service<T>
    {
        MonoBehaviour mono
        {
            get;
        }
        bool IsConnected<ST>() where ST : T;
        void Connect<ST>() where ST : T;
        void Disconnect<ST>() where ST : T;
    }
}
