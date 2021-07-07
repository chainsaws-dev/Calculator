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

        public bool NumberNegative { get; private set; }

        public ValidInputEventArgs(string ValidCharacter, bool ResetInput, bool NumberNegative)
        {
            this.ValidCharacter = ValidCharacter;
            this.ResetInput = ResetInput;
            this.NumberNegative = NumberNegative;
        }
    }
}