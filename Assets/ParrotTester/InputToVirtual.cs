using UnityEngine;
using System.Collections;
#if UNITY_EDITOR_WIN
using WindowsInput;
using WindowsInput.Native;
#endif
using System.Collections.Generic;

public static class InputToVirtual
{
#if UNITY_EDITOR_WIN
    public static Dictionary<KeyCode, VirtualKeyCode> map = new Dictionary<KeyCode, VirtualKeyCode>()
        {
            //none
            {KeyCode.Backspace, VirtualKeyCode.BACK},
            {KeyCode.Tab, VirtualKeyCode.TAB},
            {KeyCode.Clear, VirtualKeyCode.CLEAR}, //doesn't work on macbook, also is an OEM_CLEAR that doesn't work either
            {KeyCode.Return, VirtualKeyCode.RETURN},
            {KeyCode.Pause, VirtualKeyCode.PAUSE},
            {KeyCode.Escape, VirtualKeyCode.ESCAPE},
            {KeyCode.Space, VirtualKeyCode.SPACE},
            //exclaim
            //doublequote
            //hash
            //dollar
            //ampersand
            {KeyCode.Quote, VirtualKeyCode.OEM_7},
            //leftparen
            //rightparen
            {KeyCode.Plus, VirtualKeyCode.OEM_PLUS}, //doesn't work on macbook
            {KeyCode.Comma, VirtualKeyCode.OEM_COMMA},
            {KeyCode.Minus, VirtualKeyCode.OEM_MINUS},
            {KeyCode.Period, VirtualKeyCode.OEM_PERIOD},
            //slash
            {KeyCode.Alpha0, VirtualKeyCode.VK_0},
            {KeyCode.Alpha1, VirtualKeyCode.VK_1},
            {KeyCode.Alpha2, VirtualKeyCode.VK_2},
            {KeyCode.Alpha3, VirtualKeyCode.VK_3},
            {KeyCode.Alpha4, VirtualKeyCode.VK_4},
            {KeyCode.Alpha5, VirtualKeyCode.VK_5},
            {KeyCode.Alpha6, VirtualKeyCode.VK_6},
            {KeyCode.Alpha7, VirtualKeyCode.VK_7},
            {KeyCode.Alpha8, VirtualKeyCode.VK_8},
            {KeyCode.Alpha9, VirtualKeyCode.VK_9},

            //colon, semicolon, less, equals, greater, question, at, leftbracket
            {KeyCode.Backslash, VirtualKeyCode.OEM_102},
            
            //rightbracket, caret, underscore, backquote

            {KeyCode.A, VirtualKeyCode.VK_A},
            {KeyCode.B, VirtualKeyCode.VK_B},
            {KeyCode.C, VirtualKeyCode.VK_C},
            {KeyCode.D, VirtualKeyCode.VK_D},
            {KeyCode.E, VirtualKeyCode.VK_E},
            {KeyCode.F, VirtualKeyCode.VK_F},
            {KeyCode.G, VirtualKeyCode.VK_G},
            {KeyCode.H, VirtualKeyCode.VK_H},
            {KeyCode.I, VirtualKeyCode.VK_I},
            {KeyCode.J, VirtualKeyCode.VK_J},
            {KeyCode.K, VirtualKeyCode.VK_K},
            {KeyCode.L, VirtualKeyCode.VK_L},
            {KeyCode.M, VirtualKeyCode.VK_M},
            {KeyCode.N, VirtualKeyCode.VK_N},
            {KeyCode.O, VirtualKeyCode.VK_O},
            {KeyCode.P, VirtualKeyCode.VK_P},
            {KeyCode.Q, VirtualKeyCode.VK_Q},
            {KeyCode.R, VirtualKeyCode.VK_R},
            {KeyCode.S, VirtualKeyCode.VK_S},
            {KeyCode.T, VirtualKeyCode.VK_T},
            {KeyCode.U, VirtualKeyCode.VK_U},
            {KeyCode.V, VirtualKeyCode.VK_V},
            {KeyCode.W, VirtualKeyCode.VK_W},
            {KeyCode.X, VirtualKeyCode.VK_X},
            {KeyCode.Y, VirtualKeyCode.VK_Y},
            {KeyCode.Z, VirtualKeyCode.VK_Z},

            {KeyCode.Delete, VirtualKeyCode.DELETE},

            {KeyCode.Keypad0, VirtualKeyCode.NUMPAD0},
            {KeyCode.Keypad1, VirtualKeyCode.NUMPAD1},
            {KeyCode.Keypad2, VirtualKeyCode.NUMPAD2},
            {KeyCode.Keypad3, VirtualKeyCode.NUMPAD3},
            {KeyCode.Keypad4, VirtualKeyCode.NUMPAD4},
            {KeyCode.Keypad5, VirtualKeyCode.NUMPAD5},
            {KeyCode.Keypad6, VirtualKeyCode.NUMPAD6},
            {KeyCode.Keypad7, VirtualKeyCode.NUMPAD7},
            {KeyCode.Keypad8, VirtualKeyCode.NUMPAD8},
            {KeyCode.Keypad9, VirtualKeyCode.NUMPAD9},
            {KeyCode.KeypadPeriod, VirtualKeyCode.DECIMAL},
            {KeyCode.KeypadDivide, VirtualKeyCode.DIVIDE},
            {KeyCode.KeypadMultiply, VirtualKeyCode.MULTIPLY},

            //if they did work they would be treated the same as Minus, Plus, Return
            {KeyCode.KeypadMinus, VirtualKeyCode.OEM_MINUS}, //doesn't work on macbook
            {KeyCode.KeypadPlus, VirtualKeyCode.OEM_PLUS}, //doesn't work on macbook
            {KeyCode.KeypadEnter, VirtualKeyCode.RETURN}, //doesn't work on macbook
           ///keypadequals
            
         

            {KeyCode.UpArrow, VirtualKeyCode.UP},
            {KeyCode.DownArrow, VirtualKeyCode.DOWN},
            {KeyCode.RightArrow, VirtualKeyCode.RIGHT},
            {KeyCode.LeftArrow, VirtualKeyCode.LEFT},
            {KeyCode.Insert, VirtualKeyCode.INSERT},
            {KeyCode.Home, VirtualKeyCode.HOME},
            {KeyCode.End, VirtualKeyCode.END},

            {KeyCode.PageUp, VirtualKeyCode.PRIOR},
            {KeyCode.PageDown, VirtualKeyCode.NEXT},
            
            {KeyCode.F1, VirtualKeyCode.F1},
            {KeyCode.F2, VirtualKeyCode.F2},
            {KeyCode.F3, VirtualKeyCode.F3},
            {KeyCode.F4, VirtualKeyCode.F4},
            {KeyCode.F5, VirtualKeyCode.F5},
            {KeyCode.F6, VirtualKeyCode.F6},
            {KeyCode.F7, VirtualKeyCode.F7},
            {KeyCode.F8, VirtualKeyCode.F8},
            {KeyCode.F9, VirtualKeyCode.F9},
            {KeyCode.F10, VirtualKeyCode.F10},
            {KeyCode.F11, VirtualKeyCode.F11},
            {KeyCode.F12, VirtualKeyCode.F12},
            {KeyCode.F13, VirtualKeyCode.F13},
            {KeyCode.F14, VirtualKeyCode.F14},
            {KeyCode.F15, VirtualKeyCode.F15},
            {KeyCode.Numlock, VirtualKeyCode.NUMLOCK},
            {KeyCode.CapsLock, VirtualKeyCode.CAPITAL},
            {KeyCode.ScrollLock, VirtualKeyCode.SCROLL},
            {KeyCode.RightShift, VirtualKeyCode.RSHIFT},
            {KeyCode.LeftShift, VirtualKeyCode.LSHIFT},
            {KeyCode.RightControl, VirtualKeyCode.RCONTROL},
            {KeyCode.LeftControl, VirtualKeyCode.LCONTROL},
            {KeyCode.RightAlt, VirtualKeyCode.RMENU},
            {KeyCode.LeftAlt, VirtualKeyCode.LMENU}, //doesn't work on macbook
           
            //rightapple - leftcommand

            //windows key takes focus away from app so doesn't get recorded
            {KeyCode.LeftWindows, VirtualKeyCode.LWIN}, 
            {KeyCode.RightWindows, VirtualKeyCode.RWIN},

            //altgr
            {KeyCode.Help, VirtualKeyCode.HELP},
            {KeyCode.Print, VirtualKeyCode.PRINT}, //doesn't work on macbook

            //sysreq
            //break
            {KeyCode.Menu, VirtualKeyCode.MENU}, //doesn't work on macbook

            {KeyCode.Mouse0, VirtualKeyCode.NONAME},
            {KeyCode.Mouse1, VirtualKeyCode.NONAME}

            //mouse3 - joystick4button19
        };
#elif UNITY_EDITOR_OSX
	public static Dictionary<KeyCode, int> map = new Dictionary<KeyCode, int>()
	{
		//none
		{KeyCode.Backspace, 0x33},
		{KeyCode.Tab, 0x30},
		{KeyCode.Clear, 0x47}, //doesn't work on macbook
		{KeyCode.Return, 0x24},
		//pause
		{KeyCode.Escape, 0x35},
		{KeyCode.Space, 0x31},
		//exclaim
		//doublequote
		//hash
		//dollar
		//ampersand
		//quote
		//leftparen
		//rightparen
//		{KeyCode.Plus, 0x45}, //keypad: 0x45, won't work for the plus key on the left of delete
		{KeyCode.Comma, 0x2B},
		{KeyCode.Minus, 0x1B}, //ansi: 0x1B, keypad: 0x4E
		{KeyCode.Period, 0x2F},
		{KeyCode.Slash, 0x2C},
		{KeyCode.Alpha0, 0x1D},
		{KeyCode.Alpha1, 0x12},
		{KeyCode.Alpha2, 0x13},
		{KeyCode.Alpha3, 0x14},
		{KeyCode.Alpha4, 0x15},
		{KeyCode.Alpha5, 0x17},
		{KeyCode.Alpha6, 0x16},
		{KeyCode.Alpha7, 0x1A},
		{KeyCode.Alpha8, 0x1C},
		{KeyCode.Alpha9, 0x19},
		
		//colon
		{KeyCode.Semicolon, 0x29},

		//less,
		{KeyCode.Equals, 0x18},
		//greater, question, at

		{KeyCode.LeftBracket, 0x21},
		{KeyCode.Backslash, 0x2A},
		{KeyCode.RightBracket, 0x1E},

		//caret, underscore, backquote
		
		{KeyCode.A, 0x00},
		{KeyCode.B, 0x0B},
		{KeyCode.C, 0x08},
		{KeyCode.D, 0x02},
		{KeyCode.E, 0x0E},
		{KeyCode.F, 0x03},
		{KeyCode.G, 0x05},
		{KeyCode.H, 0x04},
		{KeyCode.I, 0x22},
		{KeyCode.J, 0x26},
		{KeyCode.K, 0x28},
		{KeyCode.L, 0x25},
		{KeyCode.M, 0x2E},
		{KeyCode.N, 0x2D},
		{KeyCode.O, 0x1F},
		{KeyCode.P, 0x23},
		{KeyCode.Q, 0x0C},
		{KeyCode.R, 0x0F},
		{KeyCode.S, 0x01},
		{KeyCode.T, 0x11},
		{KeyCode.U, 0x20},
		{KeyCode.V, 0x09},
		{KeyCode.W, 0x0D},
		{KeyCode.X, 0x07},
		{KeyCode.Y, 0x10},
		{KeyCode.Z, 0x06},
		
		{KeyCode.Delete, 0x75}, //forward delete,

		{KeyCode.Keypad0, 0x52},
		{KeyCode.Keypad1, 0x53},
		{KeyCode.Keypad2, 0x54},
		{KeyCode.Keypad3, 0x55},
		{KeyCode.Keypad4, 0x56},
		{KeyCode.Keypad5, 0x57},
		{KeyCode.Keypad6, 0x58},
		{KeyCode.Keypad7, 0x59},
		{KeyCode.Keypad8, 0x5B},
		{KeyCode.Keypad9, 0x5C},

		{KeyCode.KeypadPeriod, 0x41},
		{KeyCode.KeypadDivide, 0x4B},
		{KeyCode.KeypadMultiply, 0x43},
		{KeyCode.KeypadMinus, 0x4E},
		{KeyCode.KeypadPlus, 0x45},
		{KeyCode.KeypadEnter, 0x4C},
		{KeyCode.KeypadEquals, 0x51},
	
		{KeyCode.UpArrow, 0x7E},
		{KeyCode.DownArrow, 0x7D},
		{KeyCode.RightArrow, 0x7C},
		{KeyCode.LeftArrow, 0x7B},
//		{KeyCode.Insert, VirtualKeyCode.INSERT},
		{KeyCode.Home, 0x73},
		{KeyCode.End, 0x77},
		
		{KeyCode.PageUp, 0x74},
		{KeyCode.PageDown, 0x79},
		
		{KeyCode.F1, 0x7A},
		{KeyCode.F2, 0x78},
		{KeyCode.F3, 0x63},
		{KeyCode.F4, 0x76},
		{KeyCode.F5, 0x60},
		{KeyCode.F6, 0x61},
		{KeyCode.F7, 0x62},
		{KeyCode.F8, 0x64},
		{KeyCode.F9, 0x65},
		{KeyCode.F10, 0x6D},
		{KeyCode.F11, 0x67},
		{KeyCode.F12, 0x6F},
		{KeyCode.F13, 0x69}, //works on macbook even though the button doesn't exist
		{KeyCode.F14, 0x6B}, //doesn't work on macbook
		{KeyCode.F15, 0x71}, //doesn't work on macbook
//		{KeyCode.Numlock, VirtualKeyCode.NUMLOCK},
		{KeyCode.CapsLock, 0x39}, //doesn't work on macbook
//		{KeyCode.ScrollLock, VirtualKeyCode.SCROLL},
		{KeyCode.RightShift, 0x3C},
		{KeyCode.LeftShift, 0x38},
		{KeyCode.RightControl, 0x3E},
		{KeyCode.LeftControl, 0x3B},
		{KeyCode.RightAlt, 0x3D},
		{KeyCode.LeftAlt, 0x3A},

//		//all the same
		{KeyCode.LeftCommand, 0x37},
//		{KeyCode.LeftApple, 0x37}, //apple and command keycodes are the same
		{KeyCode.RightCommand, 0x37}, //can't differentiate left and right command
//		{KeyCode.RightApple, 0x37},
		
//		{KeyCode.LeftWindows, VirtualKeyCode.LWIN},
//		{KeyCode.RightWindows, VirtualKeyCode.RWIN},
		
		//altgr
		{KeyCode.Help, 0x72}, //doesn't work on macbook
//		{KeyCode.Print, VirtualKeyCode.PRINT},
		
		//sysreq
		//break
//		{KeyCode.Menu, VirtualKeyCode.MENU},
//		
		//only for recording, the value doesn't matter
		{KeyCode.Mouse0, -1},
		{KeyCode.Mouse1, -1},
		{KeyCode.Mouse2, -1}
//		
//		//mouse3 - joystick4button19
	};
#endif
}
