using UnityEngine;

namespace TRKGeneric
{
    ///<summary>
    /// This class acts as an addition to a monobehaviour class, making it a singleton
    ///<para> use protected override void Init() instead of Awake(), Start() can be used normally </para>
    ///</summary>
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        private static T _instance;
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject(typeof(T).ToString());
                    go.AddComponent<T>();
                }
                return _instance;
            }
        }
        private void Awake()
        {
            _instance = this as T;
            Init();
        }
        protected virtual void Init()
        {
            //optional to override
        }
    }
}
