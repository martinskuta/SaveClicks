#region using

using System.Windows.Input;
using JetBrains.Annotations;
using SaveClicks.Services.InteractionService;

#endregion

namespace SaveClicks.Services.CommandService.Impl
{
    /// <summary>
    ///   Converter that translates <see cref="Key" /> into string, that visual studio uses in command bindings - which is
    ///   equal to what you can see in Options -> Keyboard.
    /// </summary>
    [UsedImplicitly]
    internal class VsKeyConverter : IKeyConverter
    {
        public string ConvertToString(Key key)
        {
            switch (key)
            {
                case Key.Pause:
                    return "Break";
                case Key.Escape:
                    return "Esc";
                case Key.Back:
                    return "Bkspce";
                case Key.Delete:
                    return "Del";
                case Key.Insert:
                    return "Ins";
                case Key.PageDown:
                    return "PgDn";
                case Key.PageUp:
                    return "PgUp";
                case Key.Return:
                case Key.Separator:
                    return "Enter";
                case Key.Multiply:
                    return "Num *";
                case Key.Add:
                    return "Num +";
                case Key.Subtract:
                    return "Num -";
                case Key.Decimal:
                    return "Num .";
                case Key.Divide:
                    return "Num /";
            }

            var keyCode = (int)key;

            if (keyCode >= (int)Key.Left && keyCode <= (int)Key.Down)
            {
                return $"{key} Arrow";
            }

            //D0 -> "0", ..., D9 -> "9"
            if (keyCode >= (int)Key.D0 && keyCode <= (int)Key.D9)
            {
                return (keyCode - (int)Key.D0).ToString();
            }

            //NumPad0 -> "Num 0", ..., NumPad9 -> "Num 9"
            if (keyCode >= (int)Key.NumPad0 && keyCode <= (int)Key.NumPad9)
            {
                return $"Num {keyCode - (int)Key.NumPad0}";
            }

            //OEM keys
            if (keyCode >= (int)Key.Oem1 && keyCode <= (int)Key.OemBackslash)
            {
                return key.GetText();
            }

            if (IsNotSupported(key))
            {
                return null;
            }

            return key.ToString();
        }

        private bool IsNotSupported(Key key)
        {
            switch (key)
            {
                case Key.None:
                case Key.Cancel:
                case Key.LineFeed:
                case Key.Capital:
                case Key.KanaMode:
                case Key.JunjaMode:
                case Key.FinalMode:
                case Key.HanjaMode:
                case Key.ImeConvert:
                case Key.ImeNonConvert:
                case Key.ImeAccept:
                case Key.ImeModeChange:
                case Key.Select:
                case Key.Print:
                case Key.Execute:
                case Key.Snapshot:
                case Key.Help:
                case Key.LWin:
                case Key.RWin:
                case Key.Apps:
                case Key.Sleep:
                case Key.NumLock:
                case Key.Scroll:
                case Key.LeftShift:
                case Key.RightShift:
                case Key.LeftCtrl:
                case Key.RightCtrl:
                case Key.LeftAlt:
                case Key.RightAlt:
                case Key.BrowserBack:
                case Key.BrowserForward:
                case Key.BrowserRefresh:
                case Key.BrowserStop:
                case Key.BrowserSearch:
                case Key.BrowserHome:
                case Key.VolumeMute:
                case Key.VolumeDown:
                case Key.VolumeUp:
                case Key.MediaNextTrack:
                case Key.MediaPreviousTrack:
                case Key.MediaStop:
                case Key.MediaPlayPause:
                case Key.LaunchMail:
                case Key.SelectMedia:
                case Key.LaunchApplication1:
                case Key.LaunchApplication2:
                case Key.AbntC1:
                case Key.AbntC2:
                case Key.ImeProcessed:
                case Key.System:
                case Key.OemAttn:
                case Key.OemFinish:
                case Key.OemCopy:
                case Key.OemAuto:
                case Key.OemEnlw:
                case Key.OemBackTab:
                case Key.Attn:
                case Key.CrSel:
                case Key.ExSel:
                case Key.EraseEof:
                case Key.Play:
                case Key.Zoom:
                case Key.NoName:
                case Key.Pa1:
                case Key.OemClear:
                case Key.DeadCharProcessed:
                    return true;
            }

            return false;
        }
    }
}