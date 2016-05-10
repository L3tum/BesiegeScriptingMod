#Unlike the other languages this mod supports, you have to care about indentation in Python.

from UnityEngine import *
from System import *
from spaar import *

#Called immediately after initializing and every time the Script is activated again
def Awake(self):
  pass #Remove this when you add code to this method

#Called after $Awake, but only once, not on every activation
def Start(self):
  pass #Remove this when you add code to this method

#Used to Draw GUIs on the screen, called every frame
def OnGUI(self):
  pass #Remove this when you add code to this method

#Called every 'tick', depends on fps
def Update(self):
  pass #Remove this when you add code to this method

#Called after $Update
def LateUpdate(self):
  pass #Remove this when you add code to this method

#Called on a fixed rate, independent from fps
def FixedUpdate(self):
  pass #Remove this when you add code to this method

#Called when a level was loaded
def OnLevelWasLoaded(self, level):
  pass #Remove this when you add code to this method

#Called when the script is destroyed, e.g. when you close Besiege
def OnDestroy(self):
  pass #Remove this when you add code to this method

