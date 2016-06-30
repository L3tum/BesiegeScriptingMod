#v.0.1.4 Enhancements
- New Languages can now be added by using the [LanguageExtension class](https://github.com/L3tum/BesiegeScriptingMod/blob/master/BesiegeScriptingMod/Extensions/LanguageExtension.cs) and by naming them `scriptextension.dll` at the end, docs will follow
- A [SettingsGUI](https://github.com/L3tum/BesiegeScriptingMod/blob/master/BesiegeScriptingMod/SettingsGUI.cs) is now available by pressing **Left Alt** and **L** which can be changed in the KeyMapper of the Modloader
- A basic version of an [API](https://github.com/L3tum/BesiegeScriptingMod/blob/master/BesiegeScriptingMod/LibrariesForScripts/Besiege.cs) is now available, docs will follow
- All Keys are now changeable in the KeyMapper of the Modloader
- The Selection, Deselection and Attachment of GameObjects is now faster and uses less computing power
- Added Settings being saved now
- Refined Project Structure
- First steps for real Wiki, planned is additionally a whole tab about GUI stuff
- See [Merge](https://github.com/L3tum/BesiegeScriptingMod/pull/16)

#v.0.1.3 Fixes and Update to Besiege v.0.3
- Fixed standard Python script errors
- Rearranged GUI a bit, still WIP
- Popup when Brainfuck or Ook wants a keyboard input
- Updated to Besiege v.0.3
- Added TrumpScript
- Fixed Bug where saved Scripts are saved wrong
- Known Bug: Standard Ook script is using way too many keyboard inputs, consider writing it yourself again
- Known Bug: TrumpScript might not support every method, due to how it works with words
- [Release](https://github.com/L3tum/BesiegeScriptingMod/releases/tag/0.1.3)

#v.0.1.2 Fixes and small updates
- Fixed Lua not working
- Fixed Python expecting an intended String
- Updated name checking so it has to contain at least 1 letter now
- Removed buttons "Execute" and "Add References" when no IDE is selected
- Deselecting also works now by deselecting all toggles
- Saved Scripts can now be deleted when you right-click on them in the "Load" screen
- `OnLevelWon` and `OnSimulationToggle` can now be used in your Script
- Updated Help Message
- [Milestone](https://github.com/L3tum/BesiegeScriptingMod/issues?q=milestone%3A%22Update+0.1.2%22)

#v.0.1.1 Initial Release
- Support for languages

  - CSharp
  
  - Lua
  
  - Python
  
  - UnityScript
  
  - Brainfuck
  
  - Ook
  
- Source Editor Window

- Reference Editor Window

- GameObject Options

  - Selecting multiple or only one
  
  - Deselecting GameObjects
  
  - Adding to the Default GameObject
  
- Loading of Scripts

- Scripts can be shared if:

  - The other user will edit the references OR
  
  - The Script uses only the standard libraries
  
- Scripts can be stopped

- Tooltips
