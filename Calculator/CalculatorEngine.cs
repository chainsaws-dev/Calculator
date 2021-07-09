using System;

namespace Calculator
{
    internal class CalculatorEngine
    {
        #region Константы

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

        public const byte ActionsStart = 42;
        public const byte ActionsEnd = 47;

        public const byte ActionEquals = 61;

        #endregion Константы

        #region Свойства

        public EnteredNumber First { get; private set; }
        public EnteredNumber Second { get; private set; }
        public ICalculatorAction[] SupportedActions { get; private set; }

        public int NumberBase { get; private set; }

        public int MaxPlaces { get; private set; }
        public string DecSep { get; private set; }

        #endregion Свойства

        #region События

        public event EventHandler<ValidInputEventArgs> OnValidInput;

        #endregion События

        #region Поля

        private byte CurrentAction;
        private bool DecimalDividerUsed;
        private bool ResetInputDigits;

        #endregion Поля

        /// <summary>
        /// Конструктор класса
        /// </summary>
        /// <param name="MaxPlaces">Максимальная поддерживаемая разрядность калькулятора, определяется размером окна вывода</param>
        /// <param name="NumberBase">Основание системы счисления</param>
        public CalculatorEngine(int MaxPlaces, int NumberBase, ICalculatorAction[] SupportedActions)
        {
            SetDefaults(MaxPlaces, NumberBase, SupportedActions);
        }

        /// <summary>
        /// Сбрасывает состояние калькулятора
        /// </summary>
        /// <param name="MaxPlaces">Максимальная поддерживаемая разрядность калькулятора, определяется размером окна вывода</param>
        /// <param name="ResetLastOnly">Сбрасывать только последнее число?</param>
        public void SetDefaults(int MaxPlaces, int NumberBase, ICalculatorAction[] SupportedActions, bool ResetLastOnly = false, bool ResetInputDigits = false)
        {
            if (ResetLastOnly)
            {
                if (this.CurrentAction == 0)
                {
                    this.First = new EnteredNumber();
                }
                else
                {
                    this.Second = new EnteredNumber();
                }
            }
            else
            {
                this.First = new EnteredNumber();
                this.Second = new EnteredNumber();
            }

            this.SupportedActions = SupportedActions;
            this.NumberBase = NumberBase;
            this.MaxPlaces = MaxPlaces;
            this.DecSep = EnteredNumber.GetCurrentDecimalSeparator().ToString();
            this.CurrentAction = 0;
            this.DecimalDividerUsed = false;
            this.ResetInputDigits = ResetInputDigits;
        }

        /// <summary>
        /// Метод для обновления числа при передаче всего числа строкой
        /// </summary>
        /// <param name="NumberString">Строка содержащая в себе число</param>
        public void SetEnteredNumber(string NumberString)
        {
            byte[] CharNums = EnteredNumber.StringBytes(NumberString);

            ValidateEvaluate(CharNums);
        }

        /// <summary>
        /// Метод для добавления нового символа к числу
        /// </summary>
        /// <param name="NewChar">Символ введённый пользователем</param>
        public void ExpandEnteredNumber(char NewChar)
        {
            this.ValidateEvaluate(EnteredNumber.CharByte(NewChar));
        }

        /// <summary>
        /// Метод для вызова очистки введённого числа
        /// </summary>
        /// <param name="Mode"></param>
        public void ClearEnteredNumbers(string Mode)
        {
            switch (Mode)
            {
                case "CE":
                    this.SetDefaults(this.MaxPlaces, this.NumberBase, this.SupportedActions, true);
                    this.OnValidInput?.Invoke(this, new ValidInputEventArgs("0", true, false));
                    break;

                case "C":
                    this.SetDefaults(this.MaxPlaces, this.NumberBase, this.SupportedActions);
                    this.OnValidInput?.Invoke(this, new ValidInputEventArgs("0", true, false));
                    break;

                default:
                    new ArgumentOutOfRangeException("Mode", Mode, "Core failure: Unsupported clear mode");
                    break;
            }
        }

        /// <summary>
        /// Добавляет новую цифру в число
        /// </summary>
        /// <param name="CharNum">Код ASCII цифры или символа</param>
        private void AddCharNum(byte CharNum)
        {
            if (CurrentAction == 0)
            {
                this.OnValidInput?.Invoke(this, First.AddChar(CharNum, this.DecimalDividerUsed));
            }
            else
            {
                this.OnValidInput?.Invoke(this, Second.AddChar(CharNum, this.DecimalDividerUsed));
            }
        }

        /// <summary>
        /// Добавляет десятичный разделитель в число
        /// </summary>
        /// <param name="CharNum">Код ASCII разделителя</param>
        private void AddDecSep(byte CharNum)
        {
            if (this.CurrentAction == 0)
            {
                this.OnValidInput?.Invoke(this, this.First.AddDecSep(CharNum, this.DecimalDividerUsed));
            }
            else
            {
                this.OnValidInput?.Invoke(this, this.Second.AddDecSep(CharNum, this.DecimalDividerUsed));
            }
        }

        /// <summary>
        /// Меняет знак текущего вводимого числа
        /// </summary>
        public void SwitchSignCurrentNumber()
        {
            if (CurrentAction == 0)
            {
                if (!this.First.IsEmpty())
                {
                    this.OnValidInput?.Invoke(this, this.First.SwitchNegative());
                }
            }
            else
            {
                if (!this.Second.IsEmpty())
                {
                    this.OnValidInput?.Invoke(this, this.Second.SwitchNegative());
                }
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
            if (EnteredNumber.CheckInNumberInterval(CharNum))
            {
                if (CanExpandNumber())
                {
                    if (this.ResetInputDigits)
                    {
                        this.SetDefaults(this.MaxPlaces, this.NumberBase, this.SupportedActions);
                    }
                    // Символ является числом
                    AddCharNum(CharNum);
                }
            }

            if (CheckIntervalAction(CharNum))
            {
                // Символ является действием над числом
                // или разделителем десятичных дробей
                if (EnteredNumber.CheckDecSepAction(CharNum))
                {
                    if (!this.DecimalDividerUsed && CanExpandNumber())
                    {
                        if (this.ResetInputDigits)
                        {
                            this.SetDefaults(this.MaxPlaces, this.NumberBase, this.SupportedActions);
                        }

                        // Выводим разделители десятичных дробей
                        this.DecimalDividerUsed = true;

                        AddDecSep(CharNum);
                    }
                }
                else
                {
                    // Остальные действия устанавливают режим ввода второго числа
                    this.CurrentAction = CharNum;

                    this.DecimalDividerUsed = false;
                    this.ResetInputDigits = false;

                    this.OnValidInput?.Invoke(this, new ValidInputEventArgs("0", true, false));
                }
            }

            if (CharNum == ActionEquals)
            {
                // Символ равно - выполняем действие над двумя числами
                this.PerformAction();
            }
        }

        /// <summary>
        /// Для поддержки прочих операций которые возможно придётся добавить в будущем
        /// </summary>
        /// <param name="SpecialActionID">Число идентификатор команды больше 100 именьше 256</param>
        public void SetSpecialAction(byte SpecialActionID)
        {
            this.CurrentAction = SpecialActionID;

            this.DecimalDividerUsed = false;
            this.ResetInputDigits = false;

            this.OnValidInput?.Invoke(this, new ValidInputEventArgs("0", true, false));
        }

        /// <summary>
        /// Проверяет что код символа в пределах интервала действий
        /// </summary>
        /// <param name="CharNum">Код символа ASCII</param>
        /// <returns>Входит ли символ в интервал действий?</returns>
        private bool CheckIntervalAction(byte CharNum)
        {
            return CharNum >= ActionsStart && CharNum <= ActionsEnd;
        }

        /// <summary>
        /// Универсальный метод для вызова действия над двумя числами
        /// </summary>
        /// <returns></returns>
        private void PerformAction()
        {
            // если равно нажато до ввода вида действия
            // также если не введено первое или второе число
            //  - считаем ошибочным вводом
            if (CheckNumbersEntered())
            {
                EnteredNumber Result;

                foreach (ICalculatorAction ca in this.SupportedActions)
                {
                    if (ca.GetCharNum() == this.CurrentAction)
                    {
                        Result = ca.PerformAction(this, this.First, this.Second);

                        if (Result.Number.Length > this.MaxPlaces)
                        {
                            this.OnValidInput?.Invoke(this, new ValidInputEventArgs("E", true, false));
                        }
                        else
                        {
                            // TODO Добавить получение знака результата
                            this.First = Result;
                            this.OnValidInput?.Invoke(this, this.First.ToValidInputArgsFull(true));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Проверяет что оба числа введены
        /// </summary>
        /// <returns>Заполнены ли данные двух чисел?</returns>
        private bool CheckNumbersEntered()
        {
            return this.First.Number.Length > 0 && this.Second.Number.Length > 0;
        }

        /// <summary>
        /// Проверяет, что можно наращивать разряд числа
        /// </summary>
        /// <returns>Истина - можно, Ложь - нельзя</returns>
        private bool CanExpandNumber()
        {
            if (this.CurrentAction == 0)
            {
                return this.First.Number.Length < this.MaxPlaces;
            }
            else
            {
                return this.Second.Number.Length < this.MaxPlaces;
            }
        }
    }
}