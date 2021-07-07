using System;
using System.Linq;
using System.Text;
using System.Threading;

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
        private const byte NumbersStart = 48;
        private const byte NumbersEnd = 57;

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

        private const byte ActionsStart = 42;
        private const byte ActionsEnd = 47;

        private const byte ActionEquals = 61;

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

        public int NumberBase { get; private set; }

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
        public CalculatorEngine(int MaxPlaces, int NumberBase)
        {
            SetDefaults(MaxPlaces, NumberBase);
        }

        /// <summary>
        /// Сбрасывает состояние калькулятора
        /// </summary>
        /// <param name="MaxPlaces">Максимальная поддерживаемая разрядность калькулятора, определяется размером окна вывода</param>
        /// <param name="ResetLastOnly">Сбрасывать только последнее число?</param>
        private void SetDefaults(int MaxPlaces, int NumberBase, bool ResetLastOnly = false)
        {
            if (ResetLastOnly)
            {
                if (this.CurrentAction == ActionTypes.None)
                {
                    this.FirstEnteredNumber = new byte[] { };
                }
                else
                {
                    this.SecondEnteredNumber = new byte[] { };
                }
            }
            else
            {
                this.FirstEnteredNumber = new byte[] { };
                this.SecondEnteredNumber = new byte[] { };
            }

            this.NumberBase = NumberBase;
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
            byte[] CharNums = StringBytes(NumberString);

            ValidateEvaluate(CharNums);
        }

        /// <summary>
        /// Преобразует строку в массив чисел соответствующих символам из таблицы ASCII
        /// </summary>
        /// <param name="NumberString">Строка содержащая в себе число</param>
        /// <returns>Массив чисел соответствующих символам</returns>
        private byte[] StringBytes(string NumberString)
        {
            return Encoding.ASCII.GetBytes(NumberString.ToCharArray());
        }

        /// <summary>
        /// Преобразует данные массива байтов в строку
        /// </summary>
        /// <param name="CharNums"></param>
        /// <returns>Строка из массива байтов</returns>
        private string BytesString(byte[] CharNums)
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
        /// Метод для вызова очистки введённого числа
        /// </summary>
        /// <param name="Mode"></param>
        public void ClearEnteredNumbers(string Mode)
        {
            switch (Mode)
            {
                case "CE":
                    this.SetDefaults(this.MaxPlaces, this.NumberBase, true);
                    this.OnValidInput?.Invoke(this, new ValidInputEventArgs("0", true));
                    break;

                case "C":
                    this.SetDefaults(this.MaxPlaces, this.NumberBase);
                    this.OnValidInput?.Invoke(this, new ValidInputEventArgs("0", true));
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

        /// <summary>
        /// Добавляет в выбранное число (первое или второе в операции)
        /// </summary>
        /// <param name="EnteredNumber">Изменяемый массив байтов</param>
        /// <param name="CharNum">Код ASCII цифры</param>
        /// <returns></returns>
        private (bool Reset, byte[] ResultNumber) AddCharToSelectedNumber(byte[] EnteredNumber, byte CharNum)
        {
            bool Reset;
            int NumLen = EnteredNumber.Length;
            if (NumLen == 0)
            {
                EnteredNumber = new byte[1];
                EnteredNumber[0] = CharNum;
                Reset = true;
            }
            else
            {
                if (EnteredNumber[0] == NumbersStart && !this.DecimalDividerUsed)
                {
                    EnteredNumber = new byte[1];
                    EnteredNumber[0] = CharNum;
                    Reset = true;
                }
                else
                {
                    byte[] ExpandedArr = new byte[NumLen + 1];
                    EnteredNumber.CopyTo(ExpandedArr, 0);
                    ExpandedArr[NumLen] = CharNum;
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
            if (CheckInNumberInterval(CharNum))
            {
                if (CanExpandNumber())
                {
                    // Символ является числом
                    AddCharNum(CharNum);
                }
            }

            if (CheckIntervalAction(CharNum))
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
                this.PerformAction();
            }
        }

        /// <summary>
        /// Проверяет что код символа в пределах интервала цифр
        /// </summary>
        /// <param name="CharNum">Код символа ASCII</param>
        /// <returns>Входит ли символ в интервал цифр</returns>
        private bool CheckInNumberInterval(byte CharNum)
        {
            return CharNum >= NumbersStart && CharNum <= NumbersEnd;
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
                switch (this.CurrentAction)
                {
                    case ActionTypes.Add:
                        this.FirstEnteredNumber = this.Add();
                        this.OnValidInput?.Invoke(this, new ValidInputEventArgs(BytesString(this.FirstEnteredNumber), true));
                        break;

                    case ActionTypes.Subtract:
                        // TODO
                        break;

                    case ActionTypes.Multiply:
                        // TODO
                        break;

                    case ActionTypes.Divide:
                        // TODO
                        break;

                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Проверяет что оба числа введены
        /// </summary>
        /// <returns>Заполнены ли данные двух чисел?</returns>
        private bool CheckNumbersEntered()
        {
            return this.FirstEnteredNumber.Length > 0 && this.SecondEnteredNumber.Length > 0;
        }

        /// <summary>
        /// Операция сложения (Числа должны быть выравнены)
        /// </summary>
        /// <returns>Массив байтов результата</returns>
        private byte[] Add()
        {
            byte[] Result = new byte[this.FirstEnteredNumber.Length];

            this.LevelUpNumbers();

            int LeftOver = 0;

            for (int i = this.FirstEnteredNumber.Length - 1; i >= 0; i--)
            {
                byte FirstCharNum = this.FirstEnteredNumber[i];
                byte SecondCharNum = this.SecondEnteredNumber[i];

                if (CheckInNumberInterval(FirstCharNum) && CheckInNumberInterval(SecondCharNum))
                {
                    // Игнорируем всё, кроме цифр
                    int First = FirstCharNum - NumbersStart;
                    int Second = SecondCharNum - NumbersStart;

                    int Sum = First + Second + LeftOver;

                    if (Sum > this.NumberBase - 1)
                    {
                        LeftOver = 1;
                        Sum -= this.NumberBase;
                    }
                    else
                    {
                        LeftOver = 0;
                    }

                    Sum += NumbersStart;

                    Result[i] = (byte)Sum;
                }
            }

            if (LeftOver > 0)
            {
                byte[] ResExt = new byte[Result.Length + 1];
                ResExt[0] = (byte)(LeftOver + NumbersStart);
                Result.CopyTo(ResExt, 1);
                Result = ResExt;
            }

            this.SetDefaults(this.MaxPlaces, this.NumberBase, true);

            return Result;
        }

        /// <summary>
        /// Выравнивает числа
        /// </summary>
        private void LevelUpNumbers()
        {
            var First = this.SplitNumberBySeparator(this.FirstEnteredNumber);
            var Second = this.SplitNumberBySeparator(this.SecondEnteredNumber);

            if (First.BeforeDec.Length != Second.BeforeDec.Length)
            {
                var BeforeZeroed = this.LevelNumberParts(First.BeforeDec, Second.BeforeDec, false);
                First.BeforeDec = BeforeZeroed.FRes;
                Second.BeforeDec = BeforeZeroed.SRes;
            }

            if (First.AfterDec.Length != Second.AfterDec.Length)
            {
                var AfterZeroed = this.LevelNumberParts(First.AfterDec, Second.AfterDec, true);
                First.AfterDec = AfterZeroed.FRes;
                Second.AfterDec = AfterZeroed.SRes;
            }

            this.FirstEnteredNumber = this.JoinNumberParts(First.BeforeDec, First.AfterDec);
            this.SecondEnteredNumber = this.JoinNumberParts(Second.BeforeDec, Second.AfterDec);
        }

        /// <summary>
        /// Собираем число обратно из десятичной и целой части, добавляя разделитель
        /// </summary>
        /// <param name="BeforeSep">Часть до разделителя (целая)</param>
        /// <param name="AfterSep">Часть после разделителя (десятичная)</param>
        /// <returns>Массив байтов представляющих коды ASCII цифр числа</returns>
        private byte[] JoinNumberParts(byte[] BeforeSep, byte[] AfterSep)
        {
            byte[] Result = new byte[BeforeSep.Length + AfterSep.Length + 1];

            BeforeSep.CopyTo(Result, 0);

            byte DecSep = this.CharByte(this.GetCurrentDecimalSeparator());

            Result[BeforeSep.Length] = DecSep;

            AfterSep.CopyTo(Result, BeforeSep.Length + 1);

            return Result;
        }

        /// <summary>
        /// Разбивает число по десятичному разделителю на два массива
        /// </summary>
        /// <param name="EnteredNumber">Число которое нужно разбить</param>
        /// <returns>Кортеж из двух массивов: до и после десятичного разделителя</returns>
        private (byte[] BeforeDec, byte[] AfterDec) SplitNumberBySeparator(byte[] EnteredNumber)
        {
            byte[] Before = new byte[] { };
            byte[] After = new byte[] { };

            int NonDecLenFirst = this.GetSepPosition(EnteredNumber);

            if (NonDecLenFirst >= 0)
            {
                Before = EnteredNumber.Take(NonDecLenFirst).ToArray();
                After = EnteredNumber.Skip(NonDecLenFirst + 1).ToArray();
            }
            else
            {
                Before = EnteredNumber;
            }

            return (Before, After);
        }

        /// <summary>
        /// Получает позицию десятичного разделителя в массиве байтов числа
        /// </summary>
        /// <param name="EnteredNumber">Число в котором происходит поиск разделителя</param>
        /// <returns>Число индекс в массиве разделителя или -1 если разделитель не найден</returns>
        private int GetSepPosition(byte[] EnteredNumber)
        {
            byte DecSep = this.CharByte(this.GetCurrentDecimalSeparator());

            return Array.IndexOf(EnteredNumber, DecSep);
        }

        /// <summary>
        /// Выравнивает десятичные или целые части двух чисел
        /// </summary>
        /// <param name="First">Часть первого числа (десятичная или целая)</param>
        /// <param name="Second">Часть второго числа (десятичная или целая) </param>
        /// <param name="ZeroesAfter">Необходимо ли добавлять нули после имеющихся цифр меньшей по длине части? (для целой - нет, для десятичной - да)</param>
        /// <returns>Кортеж из двух выравненных частей двух чисел</returns>
        private (byte[] FRes, byte[] SRes) LevelNumberParts(byte[] First, byte[] Second, bool ZeroesAfter)
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
        /// Добавляет необходимое число нулей с нужной стороны части числа (десятичной или целой)
        /// </summary>
        /// <param name="Diff">Разница между количеством цифр в части числа</param>
        /// <param name="NumArr">Массив цифр дополняемого числа</param>
        /// <param name="After">Нужно ли добавлять нули после массива цифр?</param>
        /// <returns>Дополненный нулями массив цифр части числа</returns>
        private byte[] AddZeros(int Diff, byte[] NumArr, bool After)
        {
            byte[] Result = new byte[] { };
            byte[] Extended = this.CreateZerosArray(Math.Abs(Diff));

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
        private byte[] CreateZerosArray(int ZerosCount)
        {
            byte[] Result = new byte[ZerosCount];

            for (int i = 0; i <= ZerosCount - 1; i++)
            {
                Result[i] = NumbersStart;
            }

            return Result;
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
                AddCharNum(NumbersStart);
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
            if (this.CurrentAction == ActionTypes.None)
            {
                return this.FirstEnteredNumber.Length < this.MaxPlaces;
            }
            else
            {
                return this.SecondEnteredNumber.Length < this.MaxPlaces;
            }
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