using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNTCommon.Business.Utilites
{
    public static class KeyboardUtilities
    {
        /// <summary>
        /// Adds a character (newInputCharacter) or creates an operation on the whole inserted string (wholeString) for numeric keyboard 
        /// </summary>
        /// <param name="newInputCharacter"></param>
        /// <param name="wholeString"></param>
        /// <returns></returns>
        public static string? ParseKeyboardInputForNumericKeyboard(string newInputCharacter, string? wholeString, int? cursorIndex)
        {
            try
            {
                if (newInputCharacter == "Enter")
                    return wholeString;

                if (newInputCharacter == "Backspace")
                {
                    if (string.IsNullOrEmpty(wholeString))
                        return wholeString;

                    if (cursorIndex.HasValue && cursorIndex.Value > 0 && cursorIndex.Value <= wholeString.Length)
                        return wholeString.Remove(cursorIndex.Value - 1, 1);

                    if (!cursorIndex.HasValue && wholeString.Length > 0)
                        return wholeString.Substring(0, wholeString.Length - 1);

                    return wholeString;
                }

                if (newInputCharacter == "Clear")
                    return string.Empty;

                if (wholeString is null)
                    wholeString = string.Empty;

                if (wholeString == string.Empty && newInputCharacter == "-")
                    return "-" + wholeString;

                try
                {
                    if (cursorIndex.HasValue && cursorIndex.Value >= 0 && cursorIndex.Value <= wholeString.Length)
                    {
                        wholeString = wholeString.Insert(cursorIndex.Value, newInputCharacter);
                    }
                    else
                    {
                        string newValue = wholeString + newInputCharacter;
                        _ = Convert.ToDouble(newValue); // ali uporabi InvariantCulture, če želiš konsistenco
                        wholeString = newValue;
                    }
                }
                catch
                {
                    int newLength = wholeString.Length - newInputCharacter.Length;
                    wholeString = newLength > 0 ? wholeString.Substring(0, newLength) : string.Empty;
                }

                return wholeString;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Adds a character (newInputCharacter) or creates an operation on the whole inserted string (wholeString) for alphanumeric keyboard 
        /// </summary>
        /// <param name="newInputCharacter"></param>
        /// <param name="wholeString"></param>
        /// <returns></returns>
        public static string? ParseKeyboardInputForAlphanumericKeyboard(string newInputCharacter, string? wholeString, int? cursorIndex)
        {
            try
            {
                if (newInputCharacter == "Backspace")
                {
                    if (string.IsNullOrEmpty(wholeString))
                    {
                        return wholeString;
                    }

                    if (cursorIndex.HasValue && cursorIndex.Value > 0 && cursorIndex.Value <= wholeString.Length)
                    {
                        return wholeString.Remove(cursorIndex.Value - 1, 1);
                    }
                    else if (!cursorIndex.HasValue)
                    {
                        return wholeString.Substring(0, wholeString.Length - 1);
                    }

                    return wholeString;
                }

                if (newInputCharacter == "Clear")
                {
                    return string.Empty;
                }

                if (newInputCharacter == "space")
                {
                    return (wholeString ?? "") + " ";
                }

                // Ignore special keys
                var ignoredKeys = new HashSet<string> {
                    "Escape", "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9",
                    "F10", "F11", "F12", "ScrollLock", "Pause", "Dead", "Insert", "Home",
                    "PageUp", "Delete", "NumLock", "End", "PageDown", "ArrowUp",
                    "ArrowLeft", "ArrowDown", "ArrowRight", "Tab", "Shift", "Fn", "Meta",
                    "Control", "Alt", "AltGraph", "Enter", "CapsLock"
                };

                if (ignoredKeys.Contains(newInputCharacter))
                {
                    return wholeString;
                }

                // Običajen znak
                if (cursorIndex.HasValue && wholeString is not null)
                {
                    int index = cursorIndex.Value;

                    // Preveri mejo
                    if (index >= 0 && index <= wholeString.Length)
                    {
                        wholeString = wholeString.Insert(index, newInputCharacter);
                    }
                    else
                    {
                        // Out of bounds - dodaj na konec
                        wholeString += newInputCharacter;
                    }
                }
                else
                {
                    wholeString = (wholeString ?? "") + newInputCharacter;
                }

                return wholeString;
            }
            catch
            {
                return string.Empty;
            }
        }

    }
}
