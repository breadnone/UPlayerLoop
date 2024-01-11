using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
using System;
using System.Linq;
using System.Threading;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UPlayerLoop
{
    public delegate void QueueLoop ();
    //Update dummy class
    sealed class AwaitUpdate {}
    //Update dummy class
    sealed class AwaitEndOfFrame {}
    //Update dummy class
    sealed class AwaitFixedUpdate {}
    //A utility to inject the custom update loop.
    public sealed class UPlayerLoop
    {
        PlayerLoopSystem playerLoop;
        public static UPlayerLoop playerLoopUtil { get; set; }
        static event QueueLoop BeforeUpdate;
        static event QueueLoop Update;
        static event QueueLoop BeforeFixedUpdate;
        static event QueueLoop FixedUpdate;
        static event QueueLoop BeforeEndOfFrame;
        static event QueueLoop EndOfFrame;
        public static IUTManager uman;

        public void RegisterUpdate(QueueLoop before, QueueLoop after, QueueLoopType type)
        {
            if(type == QueueLoopType.Update)
            {
                BeforeUpdate += before;
                Update += after;
            }
            else if(type == QueueLoopType.FixedUpdate)
            {
                BeforeFixedUpdate += before;
                FixedUpdate += after;
            }
            else
            {
                BeforeEndOfFrame += before;
                EndOfFrame += after;
            }
        }
        public void UnregisterUpdate(QueueLoop before, QueueLoop after, QueueLoopType type)
        {
            if(type == QueueLoopType.Update)
            {
                BeforeUpdate -= before;
                Update -= after;
            }
            else if(type == QueueLoopType.FixedUpdate)
            {
                BeforeFixedUpdate -= before;
                FixedUpdate -= after;
            }
            else
            {
                BeforeEndOfFrame -= before;
                EndOfFrame -= after;
            }
        }

        public UPlayerLoop()
        {
            if (playerLoopUtil == null)
            {
                Application.wantsToQuit += OnQuit;
                playerLoopUtil = this;
                AssignPlayerLoop(true);
            }
        }
        void AssignPlayerLoop(bool addElseRemove)
        {
            playerLoop = PlayerLoop.GetDefaultPlayerLoop();

            if (addElseRemove)
            {
                var copy = InjectCustomUpdate(ref playerLoop, QueueLoopType.Update, true);
                var end = InjectCustomUpdate(ref copy, QueueLoopType.EndOfFrame, true);
                var fixedupdate = InjectCustomUpdate(ref end, QueueLoopType.FixedUpdate, true);
                PlayerLoop.SetPlayerLoop(fixedupdate);
            }
            else
            {
                var copy = InjectCustomUpdate(ref playerLoop, QueueLoopType.Update, false);
                var end = InjectCustomUpdate(ref copy, QueueLoopType.EndOfFrame, false);
                var fixedupdate = InjectCustomUpdate(ref end, QueueLoopType.FixedUpdate, false);
                PlayerLoop.SetPlayerLoop(fixedupdate);
            }
        }
        bool OnQuit()
        {
            AssignPlayerLoop(false);
            return true;
        }

        (PlayerLoopSystem before, PlayerLoopSystem after) CreateLoopSystem(QueueLoopType type)
        {
            PlayerLoopSystem before = default;
            PlayerLoopSystem after = default;

            if(type == QueueLoopType.Update)
            {
                before = new PlayerLoopSystem()
                {
                    updateDelegate = BeforeUpdateRun,
                    type = typeof(AwaitUpdate)
                };
                after = new PlayerLoopSystem()
                {
                    updateDelegate = UpdateRun,
                    type = typeof(AwaitUpdate)
                };
            }
            else if(type == QueueLoopType.FixedUpdate)
            {
                before = new PlayerLoopSystem()
                {
                    updateDelegate = BeforeFixedUpdateRun,
                    type = typeof(AwaitFixedUpdate)
                };
                after = new PlayerLoopSystem()
                {
                    updateDelegate = FixedUpdateRun,
                    type = typeof(AwaitFixedUpdate)
                };
            }
            else
            {
                before = new PlayerLoopSystem()
                {
                    updateDelegate = BeforeEndFrameUpdateRun,
                    type = typeof(AwaitEndOfFrame)
                };
                after = new PlayerLoopSystem()
                {
                    updateDelegate = EndFrameUpdateRun,
                    type = typeof(AwaitEndOfFrame)
                };
            }

            return (before, after);
        }

        PlayerLoopSystem InjectCustomUpdate(ref PlayerLoopSystem root, QueueLoopType type, bool addCustomUpdateElseClear)
        {
            var lis = root.subSystemList.ToList();
            int? index = null;

            for (int i = 0; i < root.subSystemList.Length; i++)
            {
                Type t = typeof(PreLateUpdate);

                if(type == QueueLoopType.Update)
                {
                    t = typeof(Update);
                }
                else if(type == QueueLoopType.FixedUpdate)
                {
                    t = typeof(FixedUpdate);
                }

                if (lis[i].type == t)
                {
                    index = i;
                    break;
                }
            }

            if (index.HasValue)
            {
                var tmp = root.subSystemList[index.Value].subSystemList.ToList();

                for (int i = tmp.Count; i-- > 0;)
                {
                    Type t = typeof(AwaitEndOfFrame);

                    if(type == QueueLoopType.Update)
                    {
                        t = typeof(AwaitUpdate);
                    }
                    else if(type == QueueLoopType.FixedUpdate)
                    {
                        t = typeof(AwaitFixedUpdate);
                    }

                    if (tmp[i].type == t)
                    {
                        tmp.Remove(tmp[i]);
                    }
                }

                if (addCustomUpdateElseClear)
                {
                    var sys = CreateLoopSystem(type);
                    int idx = 0;

                    if(type == QueueLoopType.Update)
                    {
                        idx = tmp.FindIndex(x=> x.type == typeof(Update.ScriptRunBehaviourUpdate));
                    }
                    else if(type == QueueLoopType.FixedUpdate)
                    {
                        idx = tmp.FindIndex(x=> x.type == typeof(FixedUpdate.ScriptRunBehaviourFixedUpdate));
                    }
                    else
                    {
                        idx = tmp.FindIndex(x=> x.type == typeof(PreLateUpdate.ScriptRunBehaviourLateUpdate));
                    }
                    
                    var beforeIndex = idx--;
                    var afterIndex = idx++;
                    
                    if(idx == 0)
                    {
                        beforeIndex = 0;
                        afterIndex = 2;
                    }

                    tmp.Insert(beforeIndex, sys.before);
                    tmp.Insert(afterIndex, sys.after);
                }

                root.subSystemList[index.Value].subSystemList = tmp.ToArray();
            }

            return root;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Init()
        {
            if (playerLoopUtil == null)
            {
                playerLoopUtil = new UPlayerLoop();
                uman = new CustomUpdateManager();
                Application.wantsToQuit -= ClearPools;
                Application.wantsToQuit += ClearPools;
            }
        }
        static bool ClearPools()
        {
            CustomUpdateManager.ClearRegisters();
            return true;
        }
        void BeforeEndFrameUpdateRun()
        {
            uman?.ExecuteOnBeforeEndOfFrame();
        }
        void EndFrameUpdateRun()
        {
            uman?.ExecuteOnAfterEndOfFrame();
        }
        void BeforeFixedUpdateRun()
        {
            uman?.ExecuteOnBeforeFixedUpdate();
        }
        void FixedUpdateRun()
        {
            uman?.ExecuteOnAfterFixedUpdate();
        }
        void BeforeUpdateRun()
        {
            uman?.ExecuteOnBeforeUpdate();
        }
        void UpdateRun()
        {
            uman?.ExecuteOnAfterUpdate();
        }

        /// <summary>
        /// Finds subsystem.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="def"></param>
        /// <returns></returns>
        private static PlayerLoopSystem FindSubSystem<T>(PlayerLoopSystem def)
        {
            if (def.type == typeof(T))
            {
                return def;
            }
            if (def.subSystemList != null)
            {
                foreach (var s in def.subSystemList)
                {
                    var system = FindSubSystem<Update.ScriptRunBehaviourUpdate>(s);
                    if (system.type == typeof(T))
                    {
                        return system;
                    }
                }
            }
            return default(PlayerLoopSystem);
        }
    }

    public enum QueueLoopType
    {
        Update = 2,
        FixedUpdate = 4,
        EndOfFrame = 8
    }
}