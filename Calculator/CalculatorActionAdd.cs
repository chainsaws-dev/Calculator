using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calculator
{
    internal class CalculatorActionAdd : ICalculatorAction
    {
        /// <summary>
        /// Возвращает ASCII код символа сложения
        /// </summary>
        /// <returns>ASCII код символа сложения</returns>
        public byte GetCharNum()
        {
            byte CharNum = 43;
            return CharNum;
        }

        /// <summary>
        /// Операция сложения
        /// </summary>
        /// <returns>тип EnteredNumber с массовом байтов результата и знаком</returns>
        public EnteredNumber PerformAction(CalculatorEngine Engine, EnteredNumber FirstEN, EnteredNumber SecondEN)
        {
            if (FirstEN.Negative ^ SecondEN.Negative)
            {
                throw new InvalidOperationException("Cannot add numbers with different signs. Use subtraction instead.");
            }

            var LeveledNumbers = ActionsShared.LevelUpNumbers(FirstEN, SecondEN);

            EnteredNumber Result = new EnteredNumber(new byte[LeveledNumbers.First.Length], FirstEN.Negative);

            int LeftOver = 0;

            for (int i = LeveledNumbers.First.Length - 1; i >= 0; i--)
            {
                byte FirstCharNum = LeveledNumbers.First[i];
                byte SecondCharNum = LeveledNumbers.Second[i];

                if (EnteredNumber.CheckInNumberInterval(FirstCharNum) && EnteredNumber.CheckInNumberInterval(SecondCharNum))
                {
                    // Игнорируем всё, кроме цифр
                    int First = FirstCharNum - EnteredNumber.NumbersStart;
                    int Second = SecondCharNum - EnteredNumber.NumbersStart;

                    int Sum = First + Second + LeftOver;

                    if (Sum > Engine.NumberBase - 1)
                    {
                        LeftOver = 1;
                        Sum -= Engine.NumberBase;
                    }
                    else
                    {
                        LeftOver = 0;
                    }

                    Sum += EnteredNumber.NumbersStart;

                    Result.Number[i] = (byte)Sum;
                }
                else
                {
                    Result.Number[i] = FirstCharNum;
                }
            }

            if (LeftOver > 0)
            {
                byte[] ResExt = new byte[Result.Number.Length + 1];
                ResExt[0] = (byte)(LeftOver + EnteredNumber.NumbersStart);
                Result.Number.CopyTo(ResExt, 1);
                Result = new EnteredNumber(ResExt, false);
            }

            Engine.SetDefaults(Engine.MaxPlaces, Engine.NumberBase, Engine.SupportedActions, true, true);

            return ActionsShared.RemoveExtraZeroes(Result);
        }
    }
}