using UnityEngine;
using System;
using spaar;

//Called immediately after initializing and every time the Script is activated again
public void Awake(){

}

//Called after $Awake, but only once, not on every activation
public void Start(){

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

//Called when the player starts/stops the Simulation
public void OnSimulationToggle(bool simulating){
	
}

//Called when the player finished the level
public void OnLevelWon(){
	
}