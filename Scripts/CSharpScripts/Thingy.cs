using UnityEngine;
using System;
//Called immediately after initializing and every time the Script is activated again
public void Awake(){
}
//Called after $Awake, but only once, not on every activation
public void Start(){
    //gameObject.GetComponent(typeof(Renderer)).Color = Color.green;
    
    Debug.Log("Brooke is awesome");
}
//Used to Draw GUIs on the screen, called every frame
public void OnGUI(){
}
//Called every 'tick', depends on fps
public void Update(){
}
//Called after $Update
public void LateUpdate(){
}
//Called on a fixed rate, independent from fps
public void FixedUpdate(){
}
//Called when a level was loaded
public void OnLevelWasLoaded(int level){
}
//Called when the script is destroyed, e.g. when you close Besiege
public void OnDestroy(){
}