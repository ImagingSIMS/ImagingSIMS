using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ImagingSIMS.Controls.BaseControls
{
    public class MaskedTextBox : TextBox
    {
        MaskedTextProvider _mProvider = null;
        bool _ignoreSpace = true;
        bool _insertIsOn = false;
        bool _newTextIsOk = false;
        bool _stayInFocusUntilValid;

        public string Mask
        {
            get
            {
                if (_mProvider != null) return _mProvider.Mask;
                else return string.Empty;
            }
            set
            {
                _mProvider = new MaskedTextProvider(value);
                this.Text = _mProvider.ToDisplayString();
            }
        }
        public bool IgnoreSpace
        {
            get { return _ignoreSpace; }
            set { _ignoreSpace = value; }
        }
        public bool NewTextIsOk
        {
            get { return _newTextIsOk; }
            set { _newTextIsOk = value; }
        }
        public bool StayInFocusUntilValid
        {
            get { return _stayInFocusUntilValid; }
            set { _stayInFocusUntilValid = value; }
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if(this.SelectionLength > 1)
            {
                this.SelectionLength = 0;
                e.Handled = true;
            }
            if (e.Key == Key.Insert ||
                e.Key == Key.Delete ||
                e.Key == Key.Back ||
                (e.Key == Key.Space && _ignoreSpace))
            {
                e.Handled = true;
            }
            base.OnPreviewKeyDown(e);
        }
        private void PressKey(Key key)
        {
            KeyEventArgs eInsertBack = new KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0, key);
            eInsertBack.RoutedEvent = KeyDownEvent;
            InputManager.Current.ProcessInput(eInsertBack);
        }
        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
            if (!_insertIsOn)
            {
                PressKey(Key.Insert);
                _insertIsOn = true;
            }
        }
        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            MaskedTextResultHint hint;
            int testPosition;

            if (e.Text.Length == 1)
                this._newTextIsOk = _mProvider.VerifyChar(e.Text[0], this.CaretIndex, out hint);
            else
                this._newTextIsOk = _mProvider.VerifyString(e.Text, out testPosition, out hint);

            base.OnPreviewTextInput(e);
        }
        protected override void OnTextInput(TextCompositionEventArgs e)
        {
            string previousText = this.Text;
            if (NewTextIsOk)
            {
                base.OnTextInput(e);
                if (_mProvider.VerifyString(this.Text) == false) this.Text = previousText;
                while (!_mProvider.IsEditPosition(this.CaretIndex) && _mProvider.Length > this.CaretIndex) this.CaretIndex++;
            }
            else
                e.Handled = true;
        }
        protected override void OnPreviewLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            if (StayInFocusUntilValid)
            {
                _mProvider.Clear();
                _mProvider.Add(this.Text);
                if (_mProvider.MaskFull) e.Handled = true;
            }

            base.OnPreviewLostKeyboardFocus(e);
        }
    }
}
