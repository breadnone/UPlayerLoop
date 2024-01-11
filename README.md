**Custom Update, FixedUpdate and PreLateUpdate for Unity3D**  
This lib/utility is meant to make a custom Update, FixedUpdate and PreLateUpdate(EndOfFrame) with pretty much ease.  
Thanks to the undocumented PlayerLoop api, we can inject our own custom updates on the managed side.  

**Use cases:**  
- Sync your async/awaits with the mainthread.
- Custom execution order.
- Interpolator function for tweening engine without having to rely on coroutines or the default Update.
- Poking external threads and queue them back to mainthread, on a multithreaded game.
  
Note: Be wise when using it and don't instantiate too many of these. Just make your own pool then queue your delegates in it.

```

var newUpdate = new CustomUpdate();
newUpdate.OnBeforeScriptUpdate += ()=> {Debug.Log("Executed before the default Update");};
newUpdate.OnAfterScriptUpdate += ()=> {Debug.Log("Executed after the default Update");};

var newFixedUpdate = new CustomFixedUpdate();
newFixedUpdate.OnBeforeScriptFixedUpdate += ()=> {Debug.Log("Executed before the default FixedUpdate");};
newFixedUpdate.OnAfterScriptFixedUpdate += ()=> {Debug.Log("Executed before the default FixedUpdate");};

var newEndOfFrame = new CustomEndOfFrame();
newEndOfFrame.OnBeforeScriptEndOfFrameUpdate += ()=> {Debug.Log("Executed before the default EndOfFrame");};
newEndOfFrame.OnAfterScriptEndOfFrameUpdate +=()=> {Debug.Log("Executed before the default EndOfFrame");};

//IMPORTANT!
//Make sure to do .Destroy on the instance if instantiated in a MonoBehavior when the gameObeject gets destroyed or it may cause leaks.
//Otherwise, make a monobehavior script then attach it to an empty gameObject and tag it as DontDestroyOnLoad so it won't get lost during scene changes.


```
  
Note: 
- Runtime only.
- Make a custom class and inherit the CustomPlayerLoop for more control.   
