using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calculator
{
    internal class ValidInputEventArgs : EventArgs
    {
        public string ValidCharacter { get; private set; }

        public bool ResetInput { get; private set; }

        public ValidInputEventArgs(string ValidCharacter, bool ResetInput)
        {
            this.ValidCharacter = ValidCharacter;
            this.ResetInput = ResetInput;
        }
    }
}