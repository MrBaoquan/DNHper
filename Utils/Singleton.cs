using System;
using System.Collections;
using System.Collections.Generic;


namespace DNHper
{
    public class Singleton<T> where T : class, new()
    {
        
        private static T _instance;

        private static readonly object _lock = new object();
        
        
        public static T Instance
        {
            get
            {
                // 双重检查锁定
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new T();
                        }
                    }
                }
                return _instance;
            }
        }
    }
}
