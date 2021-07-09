using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calculator
{
    internal class ActionsShared
    {
        /// <summary>
        /// Выравнивает числа
        /// </summary>
        public static (byte[] First, byte[] Second) LevelUpNumbers(EnteredNumber FirstEnteredNumber, EnteredNumber SecondEnteredNumber)
        {
            var First = FirstEnteredNumber.SplitNumberBySeparator();
            var Second = SecondEnteredNumber.SplitNumberBySeparator();

            if (First.BeforeDec.Length != Second.BeforeDec.Length)
            {
                var BeforeZeroed = LevelNumberParts(First.BeforeDec, Second.BeforeDec, false);
                First.BeforeDec = BeforeZeroed.FRes;
                Second.BeforeDec = BeforeZeroed.SRes;
            }

            if (First.AfterDec.Length != Second.AfterDec.Length)
            {
                var AfterZeroed = LevelNumberParts(First.AfterDec, Second.AfterDec, true);
                First.AfterDec = AfterZeroed.FRes;
                Second.AfterDec = AfterZeroed.SRes;
            }

            return (JoinNumberParts(First.BeforeDec, First.AfterDec), JoinNumberParts(Second.BeforeDec, Second.AfterDec));
        }

        public static (byte Action, EnteredNumber Fir, EnteredNumber Sec) DefineFinalAction(byte CurrentAction, EnteredNumber First, EnteredNumber Second)
        {
            byte ActionPlus = 43;
            byte ActionMinus = 45;

            // Плюс
            if (CurrentAction == ActionPlus)
            {
                if (First.Negative ^ Second.Negative == false)
                {
                    return (CurrentAction, First, Second);
                }
                else if (First.Negative && !Second.Negative)
                {
                    First.SwitchNegative();
                    return (ActionMinus, Second, First);
                }
                else if (!First.Negative && Second.Negative)
                {
                    Second.SwitchNegative();
                    return (ActionMinus, First, Second);
                }
            }

            // Минус
            if (CurrentAction == ActionMinus)
            {
                if (First.Negative ^ Second.Negative == false)
                {
                    return (CurrentAction, First, Second);
                }
                else if (First.Negative && !Second.Negative)
                {
                    Second.SwitchNegative();
                    return (ActionPlus, First, Second);
                }
                else if (!First.Negative && Second.Negative)
                {
                    Second.SwitchNegative();
                    return (ActionPlus, First, Second);
                }
            }

            // TODO Добавить перестановки для других действий

            return (CurrentAction, First, Second);
        }

        /// <summary>
        /// Выравнивает десятичные или целые части двух чисел
        /// </summary>
        /// <param name="First">Часть первого числа (десятичная или целая)</param>
        /// <param name="Second">Часть второго числа (десятичная или целая) </param>
        /// <param name="ZeroesAfter">Необходимо ли добавлять нули после имеющихся цифр меньшей по длине части? (для целой - нет, для десятичной - да)</param>
        /// <returns>Кортеж из двух выравненных частей двух чисел</returns>
        private static (byte[] FRes, byte[] SRes) LevelNumberParts(byte[] First, byte[] Second, bool ZeroesAfter)
        {
            byte[] FirstRes = new byte[] { };
            byte[] SecondRes = new byte[] { };

            if (First.Length != Second.Length)
            {
                int Diff = First.Length - Second.Length;

                if (Diff > 0)
                {
                    FirstRes = First;
                    SecondRes = AddZeros(Diff, Second, ZeroesAfter);
                }
                else
                {
                    FirstRes = AddZeros(Diff, First, ZeroesAfter);
                    SecondRes = Second;
                }
            }
            else
            {
                FirstRes = First;
                SecondRes = Second;
            }

            return (FirstRes, SecondRes);
        }

        /// <summary>
        /// Собираем число обратно из десятичной и целой части, добавляя разделитель
        /// </summary>
        /// <param name="BeforeSep">Часть до разделителя (целая)</param>
        /// <param name="AfterSep">Часть после разделителя (десятичная)</param>
        /// <returns>Массив байтов представляющих коды ASCII цифр числа</returns>
        private static byte[] JoinNumberParts(byte[] BeforeSep, byte[] AfterSep)
        {
            if (AfterSep.Length > 0 && BeforeSep.Length > 0)
            {
                byte[] Result = new byte[BeforeSep.Length + AfterSep.Length + 1];

                BeforeSep.CopyTo(Result, 0);

                byte DecSep = EnteredNumber.CharByte(EnteredNumber.GetCurrentDecimalSeparator());

                Result[BeforeSep.Length] = DecSep;

                AfterSep.CopyTo(Result, BeforeSep.Length + 1);

                return Result;
            }
            else if (BeforeSep.Length > 0 && AfterSep.Length <= 0)
            {
                return BeforeSep;
            }
            else if (AfterSep.Length > 0 && BeforeSep.Length <= 0)
            {
                return AfterSep;
            }
            else
            {
                return new byte[] { };
            }
        }

        /// <summary>
        /// Добавляет необходимое число нулей с нужной стороны части числа (десятичной или целой)
        /// </summary>
        /// <param name="Diff">Разница между количеством цифр в части числа</param>
        /// <param name="NumArr">Массив цифр дополняемого числа</param>
        /// <param name="After">Нужно ли добавлять нули после массива цифр?</param>
        /// <returns>Дополненный нулями массив цифр части числа</returns>
        public static byte[] AddZeros(int Diff, byte[] NumArr, bool After)
        {
            byte[] Result = new byte[] { };
            byte[] Extended = CreateZerosArray(Math.Abs(Diff));

            int AbsDiff = Math.Abs(Diff);
            Result = new byte[NumArr.Length + AbsDiff];

            if (After)
            {
                NumArr.CopyTo(Result, 0);
                Extended.CopyTo(Result, NumArr.Length);
            }
            else
            {
                Extended.CopyTo(Result, 0);
                NumArr.CopyTo(Result, AbsDiff);
            }

            return Result;
        }

        /// <summary>
        /// Создаёт массив с заданным числом нулей
        /// </summary>
        /// <param name="ZerosCount">Количество нулей которые нужно добавить в массив</param>
        /// <returns>Массив байтов с заданным числом нулей для выравнивания чисел</returns>
        private static byte[] CreateZerosArray(int ZerosCount)
        {
            byte[] Result = new byte[ZerosCount];

            for (int i = 0; i <= ZerosCount - 1; i++)
            {
                Result[i] = EnteredNumber.NumbersStart;
            }

            return Result;
        }

        /// <summary>
        /// Удаляет лишние нули после запятой (для результата)
        /// </summary>
        /// <param name="Number">Число которое нужно почистить</param>
        /// <returns>Число очищенное от лишних нулей</returns>
        public static EnteredNumber RemoveExtraZeroes(EnteredNumber Result)
        {
            var First = Result.SplitNumberBySeparator();

            if (First.AfterDec.Length > 0)
            {
                foreach (byte CharNum in First.AfterDec)
                {
                    if (CharNum != EnteredNumber.NumbersStart)
                    {
                        return Result;
                    }
                }

                return new EnteredNumber(First.BeforeDec, Result.Negative);
            }

            return Result;
        }
    }
}