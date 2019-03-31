using System;
using System.Collections.Generic;

namespace HordeEngine
{
    public class ReusableObject<T> where T : new()
    {
        List<T> objects_ = new List<T>();
        Action<T> initializeMethod_;
        int idx_;

        public ReusableObject(Action<T> initializeMethod)
        {
            initializeMethod_ = initializeMethod;
        }

        public T GetObject()
        {
            if (idx_ >= objects_.Count)
                CreateNew();

            return objects_[idx_++];
        }

        public void ReturnObject(T obj)
        {
            objects_[--idx_] = obj;
        }

        void CreateNew()
        {
            var newObject = new T();
            initializeMethod_(newObject);
            objects_.Add(newObject);
        }
    }
}
