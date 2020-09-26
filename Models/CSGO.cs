using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace DangerZoneHackerTracker.Models
{
	public static class CSGO
	{
		// https://gist.github.com/moritzuehling/abdb5ecf81496ba768d9
		public static string GetDirectory()
		{
			string steamPath = (string)Registry.GetValue("HKEY_CURRENT_USER\\Software\\Valve\\Steam", "SteamPath", "");

			string pathsFile = Path.Combine(steamPath, "steamapps", "libraryfolders.vdf");

			if (!File.Exists(pathsFile))
			{
				return null;
			}

			List<string> libraries = new List<string>();
			libraries.Add(Path.Combine(steamPath));

			var pathVDF = File.ReadAllLines(pathsFile);


			// Okay, this is not a full vdf-parser, but it seems to work pretty much, since the 
			// vdf-grammar is pretty easy. Hopefully it never breaks. I'm too lazy to write a full vdf-parser though. 
			Regex pathRegex = new Regex(@"\""(([^\""]*):\\([^\""]*))\""");
			foreach (var line in pathVDF)
			{
				if (pathRegex.IsMatch(line))
				{
					string match = pathRegex.Matches(line)[0].Groups[1].Value;

					// De-Escape vdf. 
					libraries.Add(match.Replace("\\\\", "\\"));
				}
			}

			foreach (var library in libraries)
			{
				string csgoPath = Path.Combine(library, "steamapps\\common\\Counter-Strike Global Offensive\\csgo");
				if (Directory.Exists(csgoPath))
				{
					return csgoPath;
				}
			}

			return null;
		}

		public enum KeyBinds
		{
			NONE,
			ZERO,
			ONE,
			TWO,
			THREE,
			FOUR,
			FIVE,
			SIX,
			SEVEN,
			EIGHT,
			NINE,
			A,
			B,
			C,
			D,
			E,
			F,
			G,
			H,
			I,
			J,
			K,
			L,
			M,
			N,
			O,
			P,
			Q,
			R,
			S,
			T,
			U,
			V,
			W,
			X,
			Y,
			Z,
			PAD_0,
			PAD_1,
			PAD_2,
			PAD_3,
			PAD_4,
			PAD_5,
			PAD_6,
			PAD_7,
			PAD_8,
			PAD_9,
			PAD_DIVIDE,
			PAD_MULTIPLY,
			PAD_MINUS,
			PAD_PLUS,
			PAD_ENTER,
			PAD_DECIMAL,
			LBRACKET,
			RBRACKET,
			SEMICOLON,
			APOSTROPHE,
			BACKQUOTE,
			COMMA,
			PERIOD,
			SLASH,
			BACKSLASH,
			MINUS,
			EQUAL,
			ENTER,
			SPACE,
			BACKSPACE,
			TAB,
			CAPSLOCK,
			NUMLOCK,
			ESCAPE,
			SCROLLLOCK,
			INSERT,
			DELETE,
			HOME,
			END,
			PAGEUP,
			PAGEDOWN,
			BREAK,
			LSHIFT,
			RSHIFT,
			LALT,
			RALT,
			LCONTROL,
			RCONTROL,
			LWIN,
			RWIN,
			APP,
			UP,
			LEFT,
			DOWN,
			RIGHT,
			F1,
			F2,
			F3,
			F4,
			F5,
			F6,
			F7,
			F8,
			F9,
			F10,
			F11,
			F12,
			CAPSLOCKTOGGLE,
			NUMLOCKTOGGLE,
			SCROLLLOCKTOGGLE,
			MOUSE_LEFT,
			MOUSE_RIGHT,
			MOUSE_MIDDLE,
			MOUSE_4,
			MOUSE_5,
			MOUSE_WHEEL_UP,
			MOUSE_WHEEL_DOWN
		}

		public static readonly Dictionary<KeyBinds, string> KeyBindStrings = new Dictionary<KeyBinds, string>()
		{
			{KeyBinds.NONE,             "" },
			{KeyBinds.ZERO,             "0" },
			{KeyBinds.ONE,              "1" },
			{KeyBinds.TWO,              "2" },
			{KeyBinds.THREE,            "3" },
			{KeyBinds.FOUR,             "4" },
			{KeyBinds.FIVE,             "5" },
			{KeyBinds.SIX,              "6" },
			{KeyBinds.SEVEN,            "7" },
			{KeyBinds.EIGHT,            "8" },
			{KeyBinds.NINE,             "9" },
			{KeyBinds.A,                "a" },
			{KeyBinds.B,                "b" },
			{KeyBinds.C,                "c" },
			{KeyBinds.D,                "d" },
			{KeyBinds.E,                "e" },
			{KeyBinds.F,                "f" },
			{KeyBinds.G,                "g" },
			{KeyBinds.H,                "h" },
			{KeyBinds.I,                "i" },
			{KeyBinds.J,                "j" },
			{KeyBinds.K,                "k" },
			{KeyBinds.L,                "l" },
			{KeyBinds.M,                "m" },
			{KeyBinds.N,                "n" },
			{KeyBinds.O,                "o" },
			{KeyBinds.P,                "p" },
			{KeyBinds.Q,                "q" },
			{KeyBinds.R,                "r" },
			{KeyBinds.S,                "s" },
			{KeyBinds.T,                "t" },
			{KeyBinds.U,                "u" },
			{KeyBinds.V,                "v" },
			{KeyBinds.W,                "w" },
			{KeyBinds.X,                "x" },
			{KeyBinds.Y,                "y" },
			{KeyBinds.Z,                "z" },
			{KeyBinds.PAD_0,            "KP_INS" },
			{KeyBinds.PAD_1,            "KP_END" },
			{KeyBinds.PAD_2,            "KP_DOWNARROW" },
			{KeyBinds.PAD_3,            "KP_PGDN" },
			{KeyBinds.PAD_4,            "KP_LEFTARROW" },
			{KeyBinds.PAD_5,            "KP_5" },
			{KeyBinds.PAD_6,            "KP_RIGHTARROW" },
			{KeyBinds.PAD_7,            "KP_HOME" },
			{KeyBinds.PAD_8,            "KP_UPARROW" },
			{KeyBinds.PAD_9,            "KP_PGUP" },
			{KeyBinds.PAD_DIVIDE,       "KP_SLASH" },
			{KeyBinds.PAD_MULTIPLY,     "KP_MULTIPLY" },
			{KeyBinds.PAD_MINUS,        "KP_MINUS" },
			{KeyBinds.PAD_PLUS,         "KP_PLUS" },
			{KeyBinds.PAD_ENTER,        "KP_ENTER" },
			{KeyBinds.PAD_DECIMAL,      "KP_DEL" },
			{KeyBinds.LBRACKET,         "["  },
			{KeyBinds.RBRACKET,         "]"  },
			{KeyBinds.SEMICOLON,        "SEMICOLON" },
			{KeyBinds.APOSTROPHE,       "'"  },
			{KeyBinds.BACKQUOTE,        "`"  },
			{KeyBinds.COMMA,            ","  },
			{KeyBinds.PERIOD,           "."  },
			{KeyBinds.SLASH,            "/"  },
			{KeyBinds.BACKSLASH,        "\\"     },
			{KeyBinds.MINUS,            "-"  },
			{KeyBinds.EQUAL,            "="  },
			{KeyBinds.ENTER,            "ENTER" },
			{KeyBinds.SPACE,            "SPACE" },
			{KeyBinds.BACKSPACE,        "BACKSPACE" },
			{KeyBinds.TAB,              "TAB" },
			{KeyBinds.CAPSLOCK,         "CAPSLOCK" },
			{KeyBinds.NUMLOCK,          "NUMLOCK" },
			{KeyBinds.ESCAPE,           "ESCAPE" },
			{KeyBinds.SCROLLLOCK,       "SCROLLLOCK" },
			{KeyBinds.INSERT,           "INS" },
			{KeyBinds.DELETE,           "DEL" },
			{KeyBinds.HOME,             "HOME" },
			{KeyBinds.END,              "END" },
			{KeyBinds.PAGEUP,           "PGUP" },
			{KeyBinds.PAGEDOWN,         "PGDN" },
			{KeyBinds.BREAK,            "PAUSE" },
			{KeyBinds.LSHIFT,           "SHIFT" },
			{KeyBinds.RSHIFT,           "RSHIFT" },
			{KeyBinds.LALT,             "ALT" },
			{KeyBinds.RALT,             "RALT" },
			{KeyBinds.LCONTROL,         "CTRL" },
			{KeyBinds.RCONTROL,         "RCTRL" },
			{KeyBinds.LWIN,             "LWIN" },
			{KeyBinds.RWIN,             "RWIN" },
			{KeyBinds.APP,              "APP" },
			{KeyBinds.UP,               "UPARROW" },
			{KeyBinds.LEFT,             "LEFTARROW" },
			{KeyBinds.DOWN,             "DOWNARROW" },
			{KeyBinds.RIGHT,            "RIGHTARROW" },
			{KeyBinds.F1,               "F1" },
			{KeyBinds.F2,               "F2" },
			{KeyBinds.F3,               "F3" },
			{KeyBinds.F4,               "F4" },
			{KeyBinds.F5,               "F5" },
			{KeyBinds.F6,               "F6" },
			{KeyBinds.F7,               "F7" },
			{KeyBinds.F8,               "F8" },
			{KeyBinds.F9,               "F9" },
			{KeyBinds.F10,              "F10" },
			{KeyBinds.F11,              "F11" },
			{KeyBinds.F12,              "F12" },
			{KeyBinds.CAPSLOCKTOGGLE,   "CAPSLOCKTOGGLE" },
			{KeyBinds.NUMLOCKTOGGLE,    "NUMLOCKTOGGLE" },
			{KeyBinds.SCROLLLOCKTOGGLE, "SCROLLLOCKTOGGLE" },
			{KeyBinds.MOUSE_LEFT,       "MOUSE1" },
			{KeyBinds.MOUSE_RIGHT,      "MOUSE2" },
			{KeyBinds.MOUSE_MIDDLE,     "MOUSE3" },
			{KeyBinds.MOUSE_4,          "MOUSE4" },
			{KeyBinds.MOUSE_5,          "MOUSE5" },
			{KeyBinds.MOUSE_WHEEL_UP,   "MWHEELUP" },
			{KeyBinds.MOUSE_WHEEL_DOWN, "MWHEELDOWN" }
		};
	}
}
