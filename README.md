# K2XB
This software allow user to use single or multiple keyboards as independent virtual Xbox 360 controller(s).


Many other keyboard to virtual controller application are there on the web. But when you set multiple virtual controllers using those, key press in any keyboard will invoke all virtual controllers because by default windows treat all keyboard as one.


That's why this application is created. RawInput API allows this program to differentiate between multiple keyboard. So, you can set "W" key of one keyboard for Left Thumbstick Up of one controller and same key of another keyboard for Left Shoulder button of another controller and so on. 


Please note that, keyboard will still send input to Windows while virtual Xbox controller is active.


So in order to use two (or more) keyboards for PvP couch party in game like FIFA or whatever you like, you need to set two (or more) virtual controller each connected to different keyboard. 


More detailed instructions on how to use this is provided in the release.https://github.com/Shadow035/K2XB/releases/


Found a bug? Feel free to report it at https://github.com/Shadow035/K2XB/issues

**::Credits::**

ScpVbus (by Scarlet.Crush, maintained by Benjamin Höglinger)
https://github.com/nefarius/ScpVBus

vXboxInterface.dll (by Shaul Eizikovich)
https://github.com/shauleiz/vXboxInterface/

RawInput.dll (by Emma Burrows)
https://www.codeproject.com/articles/17123/using-raw-input-from-c-to-handle-multiple-keyboard

Used IDE : Visual Studio 17 
