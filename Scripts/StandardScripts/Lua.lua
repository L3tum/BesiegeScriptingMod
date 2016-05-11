import ('System')
import ('UnityEngine')
import ('spaar')

--Called immediately after initializing and every time the Script is activated again
function Awake()

end

--Called after $Awake, but only once, not on every activation
function Start()

end

--Used to Draw GUIs on the screen, called every frame
function OnGUI()

end

--Called every 'tick', depends on fps
function Update()

end

--Called after $Update
function LateUpdate()

end

--Called on a fixed rate, independent from fps
function FixedUpdate()

end

--Called when a level was loaded
function OnLevelWasLoaded(level)

end

--Called when the script is destroyed, e.g. when you close Besiege
function OnDestroy()

end

--Called when the player starts or stops the Simulation
function OnSimulationToggle(simulating)

end

--Called when the player finished the level
function OnLevelWon()

end