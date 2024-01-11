
using System;

namespace UPlayerLoop
{
    /// <summary>
    /// Custom update base class. 
    /// </summary>
    public class CustomPlayerLoop : ICustomLoop
    {
        public CustomPlayerLoop()
        {
            Init();
        }
        void Init()
        {
            CustomUpdateManager.RegisterPool(this);
        }
        public virtual void AfterEndOfFrame(){}
        public virtual void AfterScriptFixedUpdate(){}
        public virtual void AfterScriptUpdate(){}
        public virtual void BeforeEndOfFrame(){}
        public virtual void BeforeScriptFixedUpdate(){}
        public virtual void BeforeScriptUpdate(){}
        public void Destroy()
        {
            CustomUpdateManager.UnregisterPool(this);
        }
    }
    /// <summary>
    /// Custom Update.
    /// </summary>
    public class CustomUpdate : CustomPlayerLoop
    {
        public Action OnBeforeScriptUpdate;
        public Action OnAfterScriptUpdate;
        public override void AfterScriptUpdate()
        {
            OnAfterScriptUpdate?.Invoke();
        }
        public override void BeforeScriptUpdate()
        {
            OnBeforeScriptUpdate?.Invoke();
        }
    }
    /// <summary>
    /// Custom FixedUpdate.
    /// </summary>
    public class CustomFixedUpdate : CustomPlayerLoop
    {
        public Action OnBeforeScriptFixedUpdate;
        public Action OnAfterScriptFixedUpdate;
        public override void AfterScriptUpdate()
        {
            OnAfterScriptFixedUpdate?.Invoke();
        }
        public override void BeforeScriptUpdate()
        {
            OnBeforeScriptFixedUpdate?.Invoke();
        }
    }
    /// <summary>
    /// Custom EndOfFrame.
    /// </summary>
    public class CustomEndOfFrame : CustomPlayerLoop
    {
        public Action OnBeforeScriptEndOfFrameUpdate;
        public Action OnAfterScriptEndOfFrameUpdate;
        public override void AfterScriptUpdate()
        {
             OnAfterScriptEndOfFrameUpdate?.Invoke();
        }
        public override void BeforeScriptUpdate()
        {
             OnBeforeScriptEndOfFrameUpdate?.Invoke();
        }
    }
}