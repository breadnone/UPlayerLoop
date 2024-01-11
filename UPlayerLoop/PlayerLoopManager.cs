using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

namespace UPlayerLoop
{
    public interface IUTManager
    {
        public void ExecuteOnAfterUpdate();
        public void ExecuteOnBeforeUpdate();
        public void ExecuteOnAfterFixedUpdate();
        public void ExecuteOnBeforeFixedUpdate();
        public void ExecuteOnAfterEndOfFrame();
        public void ExecuteOnBeforeEndOfFrame();
    }
    public class CustomUpdateManager : IUTManager
    {
        public static Dictionary<int, ICustomLoop> uthreads = new();
        static QueueLoop OnUpdate;
        static QueueLoop OnBeforeUpdate;
        static QueueLoop OnFixedUpdate;
        static QueueLoop OnBeforeFixedUpdate;
        static QueueLoop OnEndOfFrame;
        static QueueLoop OnBeforeEndOfFrame;
        static bool wasOverriden(ICustomLoop instance, string methodstring)
        {
            var type = instance.GetType();
            var method = type.GetMethod(methodstring, BindingFlags.Public | BindingFlags.Instance);
            return method.GetMethodBody().GetILAsByteArray().Length > 2;
        }
        /// <summary>
        /// Subscribe to the pool
        /// </summary>
        /// <param name="instance">The instance.</param>
        public static void RegisterPool(ICustomLoop instance)
        {
            if (uthreads.TryAdd(instance.GetId, instance))
            {
                if (wasOverriden(instance, "AfterScriptUpdate"))
                {
                    OnUpdate += instance.AfterScriptUpdate;
                }

                if (wasOverriden(instance, "BeforeScriptUpdate"))
                {
                    OnBeforeUpdate += instance.BeforeScriptUpdate;
                }

                if (wasOverriden(instance, "AfterScriptFixedUpdate"))
                {
                    OnFixedUpdate += instance.AfterScriptFixedUpdate;
                }

                if (wasOverriden(instance, "BeforeScriptFixedUpdate"))
                {
                    OnBeforeFixedUpdate += instance.BeforeScriptFixedUpdate;
                }

                if (wasOverriden(instance, "AfterEndOfFrame"))
                {
                    OnEndOfFrame += instance.AfterEndOfFrame;
                }

                if (wasOverriden(instance, "BeforeEndOfFrame"))
                {
                    OnBeforeEndOfFrame += instance.BeforeEndOfFrame;
                }
            }
        }

        /// <summary>
        /// Usubscribe to the pool
        /// </summary>
        /// <param name="instance"></param>
        public static void UnregisterPool(ICustomLoop instance)
        {
            if (uthreads.TryGetValue(instance.GetId, out var ins))
            {
                OnUpdate -= ins.AfterScriptUpdate;
                OnBeforeUpdate -= ins.BeforeScriptUpdate;
                OnFixedUpdate -= ins.AfterScriptFixedUpdate;
                OnBeforeFixedUpdate -= ins.BeforeScriptFixedUpdate;
                OnEndOfFrame -= ins.AfterEndOfFrame;
                OnBeforeEndOfFrame -= instance.BeforeEndOfFrame;
            }
        }
        public static void ClearRegisters()
        {
            if (uthreads.Count == 0)
            {
                return;
            }

            foreach (var ins in uthreads.Values)
            {
                UnregisterPool(ins);
            }
        }

        void IUTManager.ExecuteOnAfterUpdate()
        {
            OnUpdate?.Invoke();
        }

        void IUTManager.ExecuteOnBeforeUpdate()
        {
            OnBeforeUpdate?.Invoke();
        }

        void IUTManager.ExecuteOnAfterFixedUpdate()
        {
            OnFixedUpdate?.Invoke();
        }

        void IUTManager.ExecuteOnBeforeFixedUpdate()
        {
            OnBeforeFixedUpdate?.Invoke();
        }

        void IUTManager.ExecuteOnAfterEndOfFrame()
        {
            OnEndOfFrame?.Invoke();
        }

        void IUTManager.ExecuteOnBeforeEndOfFrame()
        {
            OnBeforeEndOfFrame?.Invoke();
        }
    }
}