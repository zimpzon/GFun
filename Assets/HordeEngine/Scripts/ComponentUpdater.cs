using System;
using System.Collections.Generic;
using UnityEngine;

namespace HordeEngine
{
    public interface IComponentUpdate
    {
        void ComponentUpdate(ComponentUpdatePass pass);
    }

    /// <summary>
    /// Component passes can be used not only for order of execution, but also for
    /// performance improvements by ensuring all components of the same type run in
    /// sequence. This is beneficial if they operate on the same small(-ish) data set.
    /// New passes can be added to enum ComponentUpdatePass without further code changes.
    /// Order of execution:
    ///  1) All passes before ComponentUpdatePass.Late
    ///  2) Unity Update()
    ///  3) ComponentUpdatePass.Late and all passes after
    ///  4) Unity LateUpdate()
    /// </summary>
    public enum ComponentUpdatePass
    {
        CollisionSetup, Early, Default, Physics,
        // Unity Update() happens here
        Late, Internal_DrawMeshes,
        // Unity LateUpdate() happens here
    }; 

    public class ComponentUpdater
    {
        const ComponentUpdatePass FirstInLateUpdate = ComponentUpdatePass.Late;

        Dictionary<ComponentUpdatePass, List<IComponentUpdate>> passes_ = new Dictionary<ComponentUpdatePass, List<IComponentUpdate>>();

        public ComponentUpdater()
        {
            var values = Enum.GetValues(typeof(ComponentUpdatePass));
            foreach(var value in values)
            {
                const int Capacity = 200;
                passes_[(ComponentUpdatePass)value] = new List<IComponentUpdate>(Capacity);
            }
        }

        public void RegisterForUpdate(IComponentUpdate component, params ComponentUpdatePass[] passes)
        {
            foreach (var pass in passes)
            {
                var list = passes_[pass];
                if (Application.isEditor)
                {
                    if (list.Contains(component))
                        Debug.LogErrorFormat("Component (hash {0}) is already added for priority {1}.", component.GetHashCode(), pass);

                }

                list.Add(component);
            }
        }

        public void UnregisterForUpdate(IComponentUpdate component, params ComponentUpdatePass[] passes)
        {
            foreach (var pass in passes)
            {
                var list = passes_[pass];
                if (Application.isEditor)
                {
                    if (!list.Contains(component))
                        Debug.LogErrorFormat("Component (hash {0}) was not found in priority {1}.", component.GetHashCode(), pass);
                }

                list.ReplaceRemove(component);
            }
        }

        public void DoUpdate()
        {
            foreach(var pair in passes_)
            {
                var pass = pair.Key;
                if (pass < FirstInLateUpdate)
                {
                    var list = pair.Value;
                    for (int i = 0; i < list.Count; ++i)
                    {
                        list[i].ComponentUpdate(pass);
                    }
                }
            }
        }

        public void DoLateUpdate()
        {
            foreach (var pair in passes_)
            {
                var pass = pair.Key;
                if (pass >= FirstInLateUpdate)
                {
                    var list = pair.Value;
                    for (int i = 0; i < list.Count; ++i)
                    {
                        list[i].ComponentUpdate(pass);
                    }
                }
            }
        }
    }
}
