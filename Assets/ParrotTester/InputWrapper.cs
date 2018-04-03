using UnityEngine;
using System.Collections;

#if UNITY_EDITOR_WIN
using WindowsInput;
using WindowsInput.Native;
#elif UNITY_EDITOR_OSX
using System.Runtime.InteropServices;
#endif


public class InputWrapper
{

#if UNITY_EDITOR_WIN
    InputSimulator inputSim = new InputSimulator();
#elif UNITY_EDITOR_OSX
	[DllImport ("ParrotTesterOSX")]
	static extern int GetMousePosX();
	
	[DllImport ("ParrotTesterOSX")]
	static extern int GetMousePosY();
	
	[DllImport ("ParrotTesterOSX")]
	static extern void SetMousePos(int x, int y);
	
	[DllImport ("ParrotTesterOSX")]
	static extern void MouseButton(int mouseButton, bool down);
	
	[DllImport ("ParrotTesterOSX")]
	static extern void KeyPress(int macKeyCode, bool down);

	[DllImport ("ParrotTesterOSX")]
	static extern void Scroll(float scrollX, float scrollY);
#endif

	public void MouseLeftDown ()
	{
#if UNITY_EDITOR_WIN
        inputSim.Mouse.LeftButtonDown();
#elif UNITY_EDITOR_OSX
		MouseButton(0, true);
#endif
	}

	public void MouseLeftUp ()
	{
#if UNITY_EDITOR_WIN
        inputSim.Mouse.LeftButtonUp();
#elif UNITY_EDITOR_OSX
		MouseButton(0, false);
#endif
	}

	public void MouseRightDown ()
	{
#if UNITY_EDITOR_WIN
        inputSim.Mouse.RightButtonDown();
#elif UNITY_EDITOR_OSX
		MouseButton(1, true);
#endif
	}

	public void MouseRightUp ()
	{
#if UNITY_EDITOR_WIN
        inputSim.Mouse.RightButtonUp();
#elif UNITY_EDITOR_OSX
		MouseButton (1, false);
#endif
	}

	public void MouseMiddleDown ()
	{
#if UNITY_EDITOR_WIN
        //doesn't work
#elif UNITY_EDITOR_OSX
		MouseButton (2, true);
#endif
	}

	public void MouseMiddleUp ()
	{
		#if UNITY_EDITOR_WIN
        //doesn't work
		#elif UNITY_EDITOR_OSX
		MouseButton (2, false);
		#endif
	}

	//todo: scroll
	
	
	public void MoveMouseTo (int x, int y)
	{
#if UNITY_EDITOR_WIN
        System.Windows.Forms.Cursor.Position = new System.Drawing.Point(x, y);
        //inputSim.Mouse.MoveMouseTo(x, y);
        
#elif UNITY_EDITOR_OSX
		SetMousePos (x, y);
#endif
	}

	public int[] GetMousePos ()
	{
		int[] mousePos = new int[2];
#if UNITY_EDITOR_WIN
        System.Drawing.Point mousePosWindows = System.Windows.Forms.Cursor.Position;
        mousePos[0] = mousePosWindows.X;
        mousePos[1] = mousePosWindows.Y;
#elif UNITY_EDITOR_OSX
		mousePos[0] = GetMousePosX();
		mousePos[1] = GetMousePosY();
#endif

		return mousePos;
	}

#if UNITY_EDITOR_WIN
    public void KeyDown(VirtualKeyCode windowsKeyCode)
    {
        inputSim.Keyboard.KeyDown(windowsKeyCode);
    }

	public void KeyUp(VirtualKeyCode windowsKeyCode)
	{
		inputSim.Keyboard.KeyUp(windowsKeyCode);
	}
#endif

#if UNITY_EDITOR_OSX
    public void KeyDown(int macKeyCode)
    {
		KeyPress(macKeyCode, true);
    }

    public void KeyUp(int macKeyCode)
    {
		KeyPress(macKeyCode, false);
    }
#endif

}
