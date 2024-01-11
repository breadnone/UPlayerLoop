using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace UPlayerLoop
{
    /// <summary>
    /// The main interface to be used on classes.
    /// </summary>
    public interface ICustomLoop
    {
        public int GetId
        {
            get
            {
                return this.GetHashCode();
            }
        }
        /// <summary>
        /// Executes before all Monobehavior's Updates.
        /// </summary>
        public void BeforeScriptUpdate();
        /// <summary>
        /// Executes after all Monobehavior's Updates.
        /// </summary>
        public void AfterScriptUpdate(){}
        /// <summary>
        /// Executes before all Monobehavior's FixedUpdates.
        /// </summary>
        public void BeforeScriptFixedUpdate(){}
        /// <summary>
        /// Executes after all Monobehavior's FixedUpdates.
        /// </summary>
        public void AfterScriptFixedUpdate(){}
        /// <summary>
        /// Executes before all Monobehavior's PreLateUpdates.
        /// </summary>
        public void BeforeEndOfFrame(){}
        /// <summary>
        /// Executes after all Monobehavior's PreLateUpdates.
        /// </summary>
        public void AfterEndOfFrame(){}
    }
}