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
            //tukaj ostaja samo se X. Ali to potrebujemo?
            if(newInputCharacter == "Enter")
            {
                return wholeString;
            }
            if(newInputCharacter == "Backspace")
            {
                if(string.IsNullOrEmpty(wholeString))
                {
                    return wholeString;
                }
                else
                {
                    if (cursorIndex.HasValue)
                    {
                        if (cursorIndex.Value > 0) return wholeString.Remove(cursorIndex.Value - 1, 1);
                        else return wholeString;
                    }
                    else
                    {
                        return wholeString.Substring(0, wholeString.Length - 1);
                    }
                }
            }
            if (newInputCharacter == "Clear")
            {
                wholeString = string.Empty;
                return wholeString;
            }
            //else if(newInputCharacter == "," || newInputCharacter == "-")
            //{
            //    if (string.IsNullOrEmpty(wholeString))
            //    {
            //        return wholeString;
            //    }
            //    else
            //    {
            //        wholeString += newInputCharacter;
            //        return wholeString;
            //    }
            //}

            else
            {
                if (wholeString is null)
                    wholeString = string.Empty;
                if (wholeString == string.Empty && newInputCharacter == "-") // minus first char
                    return wholeString + newInputCharacter;
                try
                {
                    if (cursorIndex.HasValue && cursorIndex.Value >= 0 && cursorIndex.Value <= wholeString.Length)
                    {
                        wholeString = wholeString.Insert(cursorIndex.Value, newInputCharacter);
                    }
                    else
                    {
                        double test = Convert.ToDouble(wholeString += newInputCharacter);
                    }
                }
                catch
                {
                    // remove wrong char
                    wholeString = wholeString.Substring(0, wholeString.Length - newInputCharacter.Length);
                }
                return wholeString;
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
            //Enter - SendValueToParentComponent
            //arrow-left - izbrise eno vrednost nazaj; ce ni nic, ne naredi nic
            //ctrl, shift pustimo zaenkrat. Caps bi bilo fajn da bi se tipke spremenile potem...
            if (newInputCharacter == "Backspace")
            {
                if (string.IsNullOrEmpty(wholeString))
                {
                    return wholeString;
                }
                else
                {
                    if (cursorIndex.HasValue)
                    {
                        if (cursorIndex.Value > 0) return wholeString.Remove(cursorIndex.Value - 1, 1);
                        else return wholeString;
                    }
                    else
                    {
                        return wholeString.Substring(0, wholeString.Length - 1);
                    }
                }
            }
            if (newInputCharacter == "Clear")
            {
                wholeString = string.Empty;
                return wholeString;
            }
            if (newInputCharacter == "space") 
            {
                if (string.IsNullOrEmpty(wholeString))
                {
                    return wholeString;
                }
                else
                {
                    wholeString += " ";
                    return wholeString;
                }
            }
            if(newInputCharacter == "Escape" || newInputCharacter == "F1" || newInputCharacter == "F2" || newInputCharacter == "F3" || newInputCharacter == "F4"
                || newInputCharacter == "F5" || newInputCharacter == "F6" || newInputCharacter == "F7" || newInputCharacter == "F8" || newInputCharacter == "F9"
                || newInputCharacter == "F10" || newInputCharacter == "F11" || newInputCharacter == "F12" || newInputCharacter == "ScrollLock" || newInputCharacter == "Pause"
                || newInputCharacter == "Dead" || newInputCharacter == "Insert" || newInputCharacter == "Home" || newInputCharacter == "PageUp" || newInputCharacter == "Delete" || newInputCharacter == "NumLock"
                || newInputCharacter == "End" || newInputCharacter == "PageDown" || newInputCharacter == "ArrowUp" || newInputCharacter == "ArrowLeft" || newInputCharacter == "ArrowDown" || newInputCharacter == "ArrowRight"
                || newInputCharacter == "Tab" || newInputCharacter == "Shift" || newInputCharacter == "Fn" || newInputCharacter == "Meta"
                || newInputCharacter == "Control" || newInputCharacter == "Alt" || newInputCharacter == "AltGraph" || newInputCharacter == "Enter" || newInputCharacter == "CapsLock")
            {
                //fsta TO DO to bo treba popravit
                return wholeString;
            }
            else
            {
                if (cursorIndex.HasValue && wholeString is not null)
                {
                    
                    wholeString = wholeString.Insert(cursorIndex.Value, newInputCharacter);
                }
                else wholeString += newInputCharacter;
                return wholeString;
            }
        }
    }
}
