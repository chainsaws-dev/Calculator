using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Calculator
{
    internal class CalculatorEngine
    {
        #region Константы

        /*
            NUMBERS

            Binary	    Oct	Dec	Hex
            011 0000	060	48	30	0
            011 0001	061	49	31	1
            011 0010	062	50	32	2
            011 0011	063	51	33	3
            011 0100	064	52	34	4
            011 0101	065	53	35	5
            011 0110	066	54	36	6
            011 0111	067	55	37	7
            011 1000	070	56	38	8
            011 1001	071	57	39	9

        */
        private const int NumbersStart = 48;
        private const int NumbersEnd = 57;

        /*
            ACTIONS

            Binary	    Oct	Dec	Hex
            010 1010	052	42	2A	*
            010 1011	053	43	2B	+
            010 1100	054	44	2C	,
            010 1101	055	45	2D	-
            010 1110	056	46	2E	.
            010 1111	057	47	2F	/
            011 1101	075	61	3D	=
         */

        private const int ActionsStart = 42;
        private const int ActionsEnd = 47;

        private const int ActionEquals = 61;

        private enum ActionTypes
        {
            Add = 1,
            Subtract,
            Multiply,
            Divide,
            None
        }

        #endregion Константы

        #region Свойства

        public byte[] FirstEnteredNumber { get; private set; }
        public byte[] SecondEnteredNumber { get; private set; }
        public int MaxPlaces { get; private set; }
        public string DecSep { get; private set; }

        #endregion Свойства

        #region События

        public event EventHandler<ValidInputEventArgs> OnValidInput;

        #endregion События

        #region Поля

        private ActionTypes CurrentAction;
        private bool DecimalDividerUsed;

        #endregion Поля

        /// <summary>
        /// Конструктор класса
        /// </summary>
        /// <param name="MaxPlaces">Максимальная поддерживаемая разрядность калькулятора, определяется размером окна вывода</param>
        public CalculatorEngine(int MaxPlaces)
        {
            this.FirstEnteredNumber = new byte[] { };
            this.SecondEnteredNumber = new byte[] { };
            this.MaxPlaces = MaxPlaces;
            this.DecSep = this.GetCurrentDecimalSeparator().ToString();
            this.CurrentAction = ActionTypes.None;
            this.DecimalDividerUsed = false;
        }

        /// <summary>
        /// Метод для обновления числа при передаче всего числа строкой
        /// </summary>
        /// <param name="NumberString">Строка содержащая в себе число</param>
        public void SetEnteredNumber(string NumberString)
        {
            byte[] CharNums = GetStringBytes(NumberString);

            ValidateEvaluate(CharNums);
        }

        /// <summary>
        /// Преобразует строку в массив чисел соответствующих символам из таблицы ASCII
        /// </summary>
        /// <param name="NumberString">Строка содержащая в себе число</param>
        /// <returns>Массив чисел соответствующих символам</returns>
        private byte[] GetStringBytes(string NumberString)
        {
            return Encoding.ASCII.GetBytes(NumberString.ToCharArray());
        }

        /// <summary>
        /// Преобразует данные массива байтов в строку
        /// </summary>
        /// <param name="CharNums"></param>
        /// <returns>Строка из массива байтов</returns>
        private string GetBytesString(byte[] CharNums)
        {
            return Encoding.ASCII.GetString(CharNums);
        }

        /// <summary>
        /// Метод для добавления нового символа к числу
        /// </summary>
        /// <param name="NewChar">Символ введённый пользователем</param>
        public void ExpandEnteredNumber(char NewChar)
        {
            this.ValidateEvaluate(CharByte(NewChar));
        }

        /// <summary>
        /// Добавляет новую цифру в число
        /// </summary>
        /// <param name="CharNum">Код ASCII цифры или символа</param>
        private void AddCharNum(byte CharNum)
        {
            bool Reset;

            if (CurrentAction == ActionTypes.None)
            {
                var Res = AddCharToSelectedNumber(this.FirstEnteredNumber, CharNum);
                Reset = Res.Reset;
                this.FirstEnteredNumber = Res.ResultNumber;
            }
            else
            {
                var Res = AddCharToSelectedNumber(this.SecondEnteredNumber, CharNum);
                Reset = Res.Reset;
                this.SecondEnteredNumber = Res.ResultNumber;
            }

            this.OnValidInput?.Invoke(this, new ValidInputEventArgs(ByteChar(CharNum), Reset));
        }

        private (bool Reset, byte[] ResultNumber) AddCharToSelectedNumber(byte[] EnteredNumber, byte CharNum)
        {
            bool Reset;
            int SecondLen = EnteredNumber.Length;
            if (SecondLen == 0)
            {
                EnteredNumber = new byte[1];
                EnteredNumber[0] = CharNum;
                Reset = true;
            }
            else
            {
                if (EnteredNumber[0] == 48 && !this.DecimalDividerUsed)
                {
                    EnteredNumber = new byte[1];
                    EnteredNumber[0] = CharNum;
                    Reset = true;
                }
                else
                {
                    byte[] ExpandedArr = new byte[SecondLen + 1];
                    EnteredNumber.CopyTo(ExpandedArr, 0);
                    ExpandedArr[SecondLen] = CharNum;
                    EnteredNumber = ExpandedArr;
                    Reset = false;
                }
            }

            return (Reset, EnteredNumber);
        }

        /// <summary>
        /// Получает число соответствующее символу из таблицы ASCII
        /// </summary>
        /// <param name="NewChar"></param>
        /// <returns>Число соответствующее символу</returns>
        private byte CharByte(char NewChar)
        {
            return Encoding.ASCII.GetBytes(new char[1] { NewChar })[0];
        }

        /// <summary>
        /// Получает строку (символ) по числу из таблицы ASCII
        /// </summary>
        /// <param name="CharNum">Число из таблицы ASCII</param>
        /// <returns>Строка из одного символа</returns>
        private string ByteChar(byte CharNum)
        {
            return Encoding.ASCII.GetString(new byte[1] { CharNum });
        }

        /// <summary>
        /// Проверяет, что переданный символ относится к поддерживаемым
        /// </summary>
        /// <param name="CharNum">Число из таблицы ASCII</param>
        private void ValidateEvaluate(byte CharNum)
        {
            IntervalCheckAndEvaluation(CharNum);
        }

        /// <summary>
        /// Метод проверяет соответствие кода символа поддерживаемым интервалам
        /// </summary>
        /// <param name="CharNum"></param>
        /// <returns></returns>
        private void IntervalCheckAndEvaluation(byte CharNum)
        {
            if (CharNum >= NumbersStart && CharNum <= NumbersEnd)
            {
                if (CanExpandNumber())
                {
                    // Символ является числом
                    AddCharNum(CharNum);
                }
            }

            if (CharNum >= ActionsStart && CharNum <= ActionsEnd)
            {
                // Символ является действием над числом
                // или разделителем десятичных дробей
                if (CharNum == 44 || CharNum == 46)
                {
                    if (!this.DecimalDividerUsed && CanExpandNumber())
                    {
                        // Выводим разделители десятичных дробей
                        this.DecimalDividerUsed = true;

                        if (this.CurrentAction == ActionTypes.None)
                        {
                            CheckLengthAndAddDecSep(this.FirstEnteredNumber, CharNum);
                        }
                        else
                        {
                            CheckLengthAndAddDecSep(this.SecondEnteredNumber, CharNum);
                        }
                    }
                }
                else
                {
                    // Остальные действия устанавливают режим ввода второго числа
                    switch (CharNum)
                    {
                        case 42:
                            this.CurrentAction = ActionTypes.Multiply;
                            break;

                        case 43:
                            this.CurrentAction = ActionTypes.Add;
                            break;

                        case 45:
                            this.CurrentAction = ActionTypes.Subtract;
                            break;

                        case 47:
                            this.CurrentAction = ActionTypes.Divide;
                            break;

                        default:
                            new ArgumentOutOfRangeException("CharNum", CharNum, "Core failure: Character number out of range");
                            break;
                    }

                    this.DecimalDividerUsed = false;
                    this.OnValidInput?.Invoke(this, new ValidInputEventArgs("0", true));
                }
            }

            if (CharNum == ActionEquals)
            {
                // Символ равно - выполняем действие над двумя числами

                // TODO

                this.DecimalDividerUsed = false;
                this.CurrentAction = ActionTypes.None;
            }
        }

        /// <summary>
        /// Проверяет что вводится первый символ и если это разделитель десятичных знаков то выводит ещё ноль
        /// </summary>
        /// <param name="EnteredNumber">Массив byte с кодами символов</param>
        /// <param name="CharNum">Номер вводимого символа</param>
        private void CheckLengthAndAddDecSep(byte[] EnteredNumber, byte CharNum)
        {
            if (EnteredNumber.Length == 0)
            {
                AddCharNum(48);
                AddCharNum(CharNum);
            }
            else
            {
                AddCharNum(CharNum);
            }
        }

        /// <summary>
        /// Проверяет, что все коды символов в переданной строке относятся к поддерживаемым
        /// </summary>
        /// <param name="CharNums">Массив кодов символов строки</param>
        /// <returns>Истина - если относятся, Ложь - если не относятся</returns>
        private void ValidateEvaluate(byte[] CharNums)
        {
            foreach (byte CharNum in CharNums)
            {
                IntervalCheckAndEvaluation(CharNum);
            }
        }

        /// <summary>
        /// Проверяет, что можно наращивать разряд числа
        /// </summary>
        /// <returns>Истина - можно, Ложь - нельзя</returns>
        private bool CanExpandNumber()
        {
            return this.FirstEnteredNumber.Length < this.MaxPlaces && this.SecondEnteredNumber.Length < this.MaxPlaces;
        }

        /// <summary>
        /// Получае из региональных настроек значение десятичного разделителя
        /// </summary>
        /// <returns>Символ десятичного разделителя</returns>
        private char GetCurrentDecimalSeparator()
        {
            return Convert.ToChar(Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator);
        }
    }
}