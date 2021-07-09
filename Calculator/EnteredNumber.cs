using System;
using System.Linq;
using System.Text;
using System.Threading;

namespace Calculator
{
    internal class EnteredNumber
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
        public const byte NumbersStart = 48;
        public const byte NumbersEnd = 57;

        #endregion Константы

        #region Свойства

        public byte[] Number { get; private set; }
        public bool Negative { get; private set; }

        #endregion Свойства

        #region Конструкторы

        /// <summary>
        /// Конструктор класса для установки
        /// </summary>
        /// <param name="Num">Число</param>
        /// <param name="IsNegative">Признак отрицательного</param>
        public EnteredNumber(byte[] Num, bool IsNegative)
        {
            this.Number = Num;
            this.Negative = IsNegative;
        }

        /// <summary>
        /// Конструктор класса для сброса
        /// </summary>
        public EnteredNumber()
        {
            this.Number = new byte[] { };
            this.Negative = false;
        }

        #endregion Конструкторы

        #region Основные функции

        /// <summary>
        /// Меняет знак числа на противоположный
        /// </summary>
        public ValidInputEventArgs SwitchNegative()
        {
            this.Negative = !this.Negative;
            return this.ToValidInputArgsFull(true);
        }

        /// <summary>
        /// Проверяет число на пустоту
        /// </summary>
        /// <returns>Заполнен ли хотя бы один разряд числа?</returns>
        public bool IsEmpty()
        {
            return this.Number.Length <= 0;
        }

        /// <summary>
        /// Проверяет что вводится первый символ и если это разделитель десятичных знаков то выводит ещё ноль
        /// </summary>
        /// <param name="CharNum">Номер вводимого символа</param>
        public ValidInputEventArgs AddDecSep(byte CharNum, bool DecDivUsed)
        {
            if (this.Number.Length == 0)
            {
                this.AddChar(NumbersStart, DecDivUsed);
                return this.AddChar(CharNum, DecDivUsed);
            }
            else
            {
                return this.AddChar(CharNum, DecDivUsed);
            }
        }

        /// <summary>
        /// Добавляет в выбранное число (первое или второе в операции)
        /// </summary>
        /// <param name="CharNum">Код ASCII цифры</param>
        /// <returns></returns>
        public ValidInputEventArgs AddChar(byte CharNum, bool DecDivUsed)
        {
            bool Reset;
            int NumLen = this.Number.Length;
            if (NumLen == 0)
            {
                this.Number = new byte[1];
                this.Number[0] = CharNum;
                Reset = true;
            }
            else
            {
                if (this.Number[0] == NumbersStart && !DecDivUsed)
                {
                    this.Number = new byte[1];
                    this.Number[0] = CharNum;
                    Reset = true;
                }
                else
                {
                    byte[] ExpandedArr = new byte[NumLen + 1];
                    this.Number.CopyTo(ExpandedArr, 0);
                    ExpandedArr[NumLen] = CharNum;
                    this.Number = ExpandedArr;
                    Reset = false;
                }
            }

            return ToValidInputArgsChar(Reset, CharNum);
        }

        /// <summary>
        /// Разбивает число по десятичному разделителю на два массива
        /// </summary>
        /// <returns>Кортеж из двух массивов: до и после десятичного разделителя</returns>
        public (byte[] BeforeDec, byte[] AfterDec) SplitNumberBySeparator()
        {
            byte[] Before = new byte[] { };
            byte[] After = new byte[] { };

            int NonDecLenFirst = GetSepPosition();

            if (NonDecLenFirst >= 0)
            {
                Before = this.Number.Take(NonDecLenFirst).ToArray();
                After = this.Number.Skip(NonDecLenFirst + 1).ToArray();
            }
            else
            {
                Before = this.Number;
            }

            return (Before, After);
        }

        /// <summary>
        /// Получает позицию десятичного разделителя в массиве байтов числа
        /// </summary>
        /// <returns>Число индекс в массиве разделителя или -1 если разделитель не найден</returns>
        private int GetSepPosition()
        {
            byte DecSep = CharByte(GetCurrentDecimalSeparator());

            return Array.IndexOf(this.Number, DecSep);
        }

        #endregion Основные функции

        #region Обратная связь

        /// <summary>
        /// Формирует аргументы события при валидном вводе (возвращает только новый символ)
        /// </summary>
        /// <param name="ResetInput">Сбросить значение в поле вывода?</param>
        /// <param name="CharNum">Код символа в таблице ASCII</param>
        /// <returns>Экземпляр типа ValidInputEventArgs для использовании в событии ValidInput</returns>
        private ValidInputEventArgs ToValidInputArgsChar(bool ResetInput, byte CharNum)
        {
            return new ValidInputEventArgs(ByteChar(CharNum), ResetInput, this.Negative);
        }

        /// <summary>
        /// Формирует аргументы события при валидном вводе (возвращает всё число)
        /// </summary>
        /// <param name="ResetInput">Сбросить значение в поле вывода?</param>
        /// <returns>Экземпляр типа ValidInputEventArgs для использовании в событии ValidInput</returns>
        public ValidInputEventArgs ToValidInputArgsFull(bool ResetInput)
        {
            return new ValidInputEventArgs(BytesString(this.Number), ResetInput, this.Negative);
        }

        #endregion Обратная связь

        #region Вспомогательные функции

        /// <summary>
        /// Проверяет что код символа это десятичный разделитель
        /// </summary>
        /// <param name="CharNum">Код символа ASCII</param>
        /// <returns>Является ли символ разделителем?</returns>
        public static bool CheckDecSepAction(byte CharNum)
        {
            return CharNum == 44 || CharNum == 46;
        }

        /// <summary>
        /// Проверяет что код символа в пределах интервала цифр
        /// </summary>
        /// <param name="CharNum">Код символа ASCII</param>
        /// <returns>Входит ли символ в интервал цифр</returns>
        public static bool CheckInNumberInterval(byte CharNum)
        {
            return CharNum >= NumbersStart && CharNum <= NumbersEnd;
        }

        /// <summary>
        /// Преобразует строку в массив чисел соответствующих символам из таблицы ASCII
        /// </summary>
        /// <param name="NumberString">Строка содержащая в себе число</param>
        /// <returns>Массив чисел соответствующих символам</returns>
        public static byte[] StringBytes(string NumberString)
        {
            return Encoding.ASCII.GetBytes(NumberString.ToCharArray());
        }

        /// <summary>
        /// Преобразует данные массива байтов в строку
        /// </summary>
        /// <param name="CharNums"></param>
        /// <returns>Строка из массива байтов</returns>
        public static string BytesString(byte[] CharNums)
        {
            return Encoding.ASCII.GetString(CharNums);
        }

        /// <summary>
        /// Получает число соответствующее символу из таблицы ASCII
        /// </summary>
        /// <param name="NewChar"></param>
        /// <returns>Число соответствующее символу</returns>
        public static byte CharByte(char NewChar)
        {
            return Encoding.ASCII.GetBytes(new char[1] { NewChar })[0];
        }

        /// <summary>
        /// Получает строку (символ) по числу из таблицы ASCII
        /// </summary>
        /// <param name="CharNum">Число из таблицы ASCII</param>
        /// <returns>Строка из одного символа</returns>
        public static string ByteChar(byte CharNum)
        {
            return Encoding.ASCII.GetString(new byte[1] { CharNum });
        }

        /// <summary>
        /// Получае из региональных настроек значение десятичного разделителя
        /// </summary>
        /// <returns>Символ десятичного разделителя</returns>
        public static char GetCurrentDecimalSeparator()
        {
            return Convert.ToChar(Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator);
        }

        #endregion Вспомогательные функции
    }
}