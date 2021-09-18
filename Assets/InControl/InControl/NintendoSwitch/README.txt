InControl For Nintendo Switch, version 1.0.1

USAGE:

To add this module to InControl, unzip and add to your InControl folder and then edit InputManager.cs to see if the following code is already there (it should be in more recent versions):

In SetupInternal(), approximately around line 150:

#if UNITY_SWITCH
if (NintendoSwitchInputDeviceManager.Enable())
{
	enableUnityInput = false;
}
#endif


NOTES:

Due to the complex nature of input on the Switch (grips, modes, styles, etc.) you’ll need to familiarize yourself with it through Nintendo's documentation on input as the defaults set up by InControl are not necessarily going to be right for your game. Also, you should open up NintendoSwitchInputDeviceManager.cs and read through the top few methods. I’ve tried to comment it thoroughly because there are a lot of configuration options and they are not exposed through the Unity inspector.

It should also go without saying that due to Nintendo’s NDA, you should not share this code with anyone or post it anywhere publicly.


CONTROLLER APPLET:

While InControl will recognize controllers attached and detached on its own, it's probably a good idea to show the official controller applet when changes are detected. You can use InputManager.OnDeviceDetached and InputManager.OnDeviceAttached for this purpose if you like, but they tend to fire in quick succession as controllers rearrange themselves. You can experiment with adding a slight delay before and after showing the applet and eliminating redundant calls:

// During setup:
// InputManager.OnDeviceAttached += OnDeviceAttached;
// InputManager.OnDeviceDetached += OnDeviceDetached;

bool willShowControllerApplet = false;

void OnDeviceAttached( InputDevice device )
{
	ShowControllerApplet();
}

void OnDeviceDetached( InputDevice device )
{
	ShowControllerApplet();
}

void ShowControllerApplet()
{
	if (willShowControllerApplet) return;
	willShowControllerApplet = true;
	StartCoroutine( ShowControllerAppletSoon() );
}

IEnumerator ShowControllerAppletSoon()
{
	yield return new WaitForSecondsRealtime( 0.1f );
	NintendoSwitchInputDeviceManager.ShowControllerSupportForMultiPlayer( 1, 4, true );
	yield return new WaitForSecondsRealtime( 0.1f );
	willShowControllerApplet = false;
	yield return null;
}
