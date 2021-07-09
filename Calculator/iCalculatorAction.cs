using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calculator
{
    internal interface ICalculatorAction
    {
        byte GetCharNum();

        EnteredNumber PerformAction(CalculatorEngine Engine, EnteredNumber FirstEN, EnteredNumber SecondEN);
    }
}