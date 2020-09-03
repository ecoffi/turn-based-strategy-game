using UnityEngine;

// Only one of this object persists across the entire game
// From Jon McElroy's Unity course https://learn.unity.com/tutorial/writting-the-gamemanager?courseId=5d0f5954edbc2a4dd39f1e4b&projectId=5d0cd2a3edbc2a00212b99f1#5d0c21d5edbc2a00205671a0
namespace Utils
{
    public class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        private static T instance;

        public static T Instance => instance;

        public static bool isInitialized => instance != null;

        protected virtual void Awake()
        {
            if (instance != null)
            {
                Debug.Log("Trying to instantiate second instance of singleton class");
            }
            else
            {
                instance = (T) this;
            }
        }

        protected virtual void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
    }
}
