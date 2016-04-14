using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleFriend
{
	/// <summary>
	/// Copyright of Joshua Sideris 2014.
	/// https://github.com/JSideris/WindowsConsoleFriendAPI
	/// Code is free to use for anything, but please include this copyright notice.
	/// </summary>
	public static class Cli
	{
		private static Queue<string> commandHistory = new Queue<string>();
		private static int commandHistoryIndex = 0;

		#region command input
		//Get console input and perform a command.

		public static string GetCommand()
		{
			string sInput = "";
			List<string> guess = null;
			int guessindex = 0;

			Console.WriteLine();
			var starttop = Console.CursorTop;

			while (true)
			{
				int strindex = Console.CursorLeft + Console.WindowWidth * (Console.CursorTop - starttop);
				ConsoleKeyInfo myKey = Console.ReadKey(true);

				if (myKey.Key == ConsoleKey.Tab)
				{
					if (guess == null)
					{
						guess = commandOptions.Where(c => c.StartsWith(sInput)).ToList();
						if (!guess.Contains(sInput))
							guess.Add(sInput);
						guessindex = 0;
					}

					if (guess.Count() > 0)
					{
						//Set guess.
						clearCurrentConsoleLine(sInput.Length);
						sInput = guess.ToArray()[guessindex];
						Console.Write(sInput);
						guessindex = (guessindex + 1) % guess.Count();
					}
					else
					{
						guess = null;
					}
				}
				else if (myKey.Key == ConsoleKey.Enter)
				{
					commandHistory.Enqueue(sInput);
					if (commandHistory.Count() > 100)
					{
						commandHistory.Dequeue();
					}
					commandHistoryIndex = commandHistory.Count();
					Console.WriteLine();
					return sInput;
				}
				else if (myKey.Key == ConsoleKey.LeftArrow)
				{
					//Move cursor left
					if (strindex > 0)
					{
						if (Console.CursorLeft > 0)
						{
							Console.CursorLeft--;
						}
						else
						{
							Console.CursorTop--;
							Console.CursorLeft = Console.WindowWidth - 1;
						}
					}
				}
				else if (myKey.Key == ConsoleKey.RightArrow)
				{
					//Move cursor left
					if (strindex < sInput.Length)
					{
						if (Console.CursorLeft + 1 < Console.WindowWidth)
						{
							Console.CursorLeft++;
						}
						else
						{
							Console.CursorTop++;
							Console.CursorLeft = 0;
						}
					}
				}
				else
				{
					//Type something into console - reset guess (at end there).
					if (myKey.Key == ConsoleKey.Backspace)
					{
						int consolePos = Console.CursorLeft;
						int consoleTop = Console.CursorTop;
						if (strindex > 0)
						{
							if (consolePos > 0)
							{
								Console.SetCursorPosition(consolePos - 1, consoleTop);
								Console.Write(sInput.Substring(strindex, sInput.Length - strindex) + " ");
								Console.SetCursorPosition(consolePos - 1, consoleTop);

							}
							else
							{
								Console.SetCursorPosition(Console.WindowWidth - 1, consoleTop - 1);
								Console.Write(sInput.Substring(strindex, sInput.Length - strindex) + " ");
								Console.SetCursorPosition(Console.WindowWidth - 1, consoleTop - 1);
							}
							sInput = sInput.Substring(0, strindex - 1) + sInput.Substring(strindex, sInput.Length - strindex);
						}

					}
					else if (myKey.Key == ConsoleKey.Delete)
					{
						int consolePos = Console.CursorLeft;
						int consoleTop = Console.CursorTop;
						if (strindex < sInput.Length)
						{
							Console.Write(sInput.Substring(strindex + 1, sInput.Length - strindex - 1) + " ");
							Console.SetCursorPosition(consolePos, consoleTop);
							sInput = sInput.Substring(0, strindex) + sInput.Substring(strindex + 1, sInput.Length - strindex - 1);
						}
					}
					else if (myKey.Key == ConsoleKey.Escape)
					{
						clearCurrentConsoleLine(sInput.Length);
						sInput = "";
					}
					else if (myKey.Key == ConsoleKey.UpArrow)
					{
						//Previous command.
						if (commandHistoryIndex > 0)
						{
							clearCurrentConsoleLine(sInput.Length);
							commandHistoryIndex--;
							sInput = commandHistory.ToArray()[commandHistoryIndex];
							Console.Write(sInput);
						}
					}
					else if (myKey.Key == ConsoleKey.DownArrow)
					{
						//Next command.
						if (commandHistoryIndex < commandHistory.Count() - 1)
						{
							clearCurrentConsoleLine(sInput.Length);
							commandHistoryIndex++;
							sInput = commandHistory.ToArray()[commandHistoryIndex];
							Console.Write(sInput);
						}
						else
						{
							clearCurrentConsoleLine(sInput.Length);
							sInput = "";
							Console.Write("");
						}
					}
					else if (myKey.KeyChar != '\0')
					{
						//Console.Write(myKey.KeyChar);
						int consolePos = Console.CursorLeft;
						int consoleTop = Console.CursorTop;
						sInput = sInput.Substring(0, strindex) + myKey.KeyChar + sInput.Substring(strindex, sInput.Length - strindex);

						Console.Write(sInput.Substring(strindex, sInput.Length - strindex));
						if (consolePos + 1 < Console.WindowWidth)
						{
							Console.CursorLeft = consolePos + 1;
							Console.CursorTop = consoleTop;
						}
						else
						{
							Console.CursorTop = consoleTop + 1;
							Console.CursorLeft = 0;
						}
					}
					else
					{
						continue;
					}

					guess = null;
				}
			}
		}


		private static bool escapeMenu = false;
		private static bool returnSelectedOnEscape = false;
		private static int selectedMenuIndex = 0;

		public static int EscapeMenu(bool returnSelected) {
			returnSelectedOnEscape = returnSelected;
			escapeMenu = true;
			return selectedMenuIndex;
		}

		public delegate string KeyPressedCallback(int selectedIndex, ConsoleKeyInfo key);

		/*public static void ChangeMenuOptionText(int option, string text) {
			currentMenuOptions[option] = text;
		}*/

		private static string[] currentMenuOptions;

		public static int Menu(string title, string[] options, string doneText, int currentSelection = 0, KeyPressedCallback callback = null)
		{
			currentMenuOptions = options;
			selectedMenuIndex = currentSelection;
			escapeMenu = false;
			Console.WriteLine();
			Console.WriteLine(title);
			
			int starttop = Console.CursorTop;
			string sInput = "";
			DateTime sinceLastStroke = DateTime.Now;

			while (true)
			{
				Console.SetCursorPosition(0, starttop);
				int index = 1;
				foreach (var o in currentMenuOptions)
				{
					if (selectedMenuIndex + 1 == index)
						Console.BackgroundColor = ConsoleColor.Blue;
					Write("" + index + "\t" + o);
					Console.BackgroundColor = ConsoleColor.Black;
					index++;
				}
				//Escape option.
				if (selectedMenuIndex + 1 == index)
					Console.BackgroundColor = ConsoleColor.Blue;
				if (doneText != null)
				{
					Write("esc\t" + doneText);
				}
				Console.BackgroundColor = ConsoleColor.Black;
				Console.SetCursorPosition(0, starttop + selectedMenuIndex);

				while(!Console.KeyAvailable){
					if (escapeMenu)
					{
						if (returnSelectedOnEscape)
							return selectedMenuIndex;
						else
							return -1;
					}
					else {
						Thread.Sleep(10);
					}
				}
				ConsoleKeyInfo myKey = Console.ReadKey(true);

				if (callback != null)
				{
					var newtext = callback(selectedMenuIndex, myKey);

					if (!string.IsNullOrEmpty(newtext)) {
						//Console.SetCursorPosition(0, starttop + selectedMenuIndex);
						//clearCurrentConsoleLine(options[selectedMenuIndex].Length, newtext);
						options[selectedMenuIndex] = newtext;
						//Console.SetCursorPosition(0, starttop + selectedMenuIndex);
					}
				}

				if (myKey.Key == ConsoleKey.Enter)
				{
					Console.SetCursorPosition(0, starttop + currentMenuOptions.Length + 1);

					if (selectedMenuIndex < currentMenuOptions.Length)
						return selectedMenuIndex;
					else
						return -1;
				}
				else if (myKey.Key == ConsoleKey.Escape && doneText != null)
				{
					Console.SetCursorPosition(0, starttop + currentMenuOptions.Length + 1);
					return -1;
				}
				else if (myKey.Key == ConsoleKey.UpArrow)
				{
					if (selectedMenuIndex > 0)
						selectedMenuIndex--;
				}
				else if (myKey.Key == ConsoleKey.DownArrow)
				{
					if ((selectedMenuIndex < (doneText != null ? currentMenuOptions.Length : currentMenuOptions.Length - 1)))
						selectedMenuIndex++;
				}

				if (myKey.KeyChar >= '0' && myKey.KeyChar <= '9')
				{
					if ((DateTime.Now - sinceLastStroke).TotalSeconds > 5)
					{
						sInput = "";
					}

					sInput += myKey.KeyChar;
					int guessIndex = int.Parse(sInput) - 1;
					if (guessIndex > currentMenuOptions.Length || guessIndex < 0)
					{
						sInput = "" + myKey.KeyChar;
						guessIndex = int.Parse(sInput) - 1;
					}

					if (guessIndex <= currentMenuOptions.Length && guessIndex >= 0)
					{
						selectedMenuIndex = guessIndex;
					}
					else
					{
						sInput = "";
					}
				}
				else
				{
					sInput = "";
				}

				sinceLastStroke = DateTime.Now;
			}
		}

		private static void clearCurrentConsoleLine(int textLength, string replaceText = "")
		{
			//Console.CursorTop = starttop;
			Console.CursorLeft = 0;
			for (int i = 0; i < textLength; i++)
			{
				if (i < replaceText.Length)
				{
					Console.Write(replaceText[i]);
				}
				else
				{
					Console.Write(" ");
				}
			}
			//Console.SetCursorPosition(0, starttop);
		}

		#region general commands
		
		//Used for autocomplete.
		private static string[] commandOptions = new string[] 
		{ 
		};

		#endregion
		private static bool verifyParamCount(string command, int expectedMinCount, int expectedMaxCount, int actualCount)
		{
			if (actualCount > expectedMaxCount)
			{
				Write("*" + command + "* expects fewer parameters.", true);
				return false;
			}
			if (actualCount < expectedMinCount)
			{
				Write("*" + command + "* expects more parameters.", true);
				return false;
			}
			return true;
		}

		#endregion

		#region formatting and output
		//Write a formatted message.
		public static void Write(string message, bool error = false)
		{
			string[] parts = message.Split('*');
			bool cyan = false;
			write(" ");
			foreach (var p in parts)
			{
				string[] paramparts = p.Split('@');
				bool yellow = false;
				foreach (var pp in paramparts)
				{
					if (yellow)
						writeYellow(pp);
					else
						if (cyan)
							writeCyan(pp);
						else
							if (error)
								writeError(pp);
							else
								write(pp);
					yellow = !yellow;
				}
				cyan = !cyan;
			}
			Console.WriteLine();
			Thread.Sleep(10); //Better visual effects.
		}

		private static void writeCyan(string message)
		{
			var stack = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.Write(message);
			Console.ForegroundColor = stack;
		}
		private static void writeYellow(string message)
		{
			var stack = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.Write(message);
			Console.ForegroundColor = stack;
		}
		private static void write(string message)
		{
			var stack = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.White;
			Console.Write(message);
			Console.ForegroundColor = stack;
		}
		private static void writeError(string message)
		{
			var stack = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.Red;
			Console.Write(message);
			Console.ForegroundColor = stack;
		}

		#endregion
	}
}