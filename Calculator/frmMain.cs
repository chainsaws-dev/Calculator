using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Calculator
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }

        private CalculatorEngine CalcEngine;
        private Point BasePosition;
        private Label LabelForResult;
        private int WidthBind;

        /// <summary>
        /// Точка входа в программу
        /// </summary>
        /// <param name="sender">Объект источник</param>
        /// <param name="e">Аргументы события</param>
        private void frmMain_Load(object sender, EventArgs e)
        {
            this.CalcEngine = new CalculatorEngine(20, 10);
            this.CalcEngine.OnValidInput += OnValidCharInput;
            this.WidthBind = (this.Size.Width - 22) / 6;

            this.LabelForResult = CreateSetLabelFormatting("0");

            this.BasePosition.X = 5;
            this.BasePosition.Y = WidthBind * 3;

            for (int i = 0; i <= 2; i++)
            {
                for (int j = 0; j <= 2; j++)
                {
                    AddNumbersSquare(i, j);
                }
            }

            AddSwitchSign();

            AddZero();

            AddDecimalDelimiter();

            AddPlus();

            AddMinus();

            AddEquals();

            AddMultiply();

            AddDivide();

            AddClearLast();

            AddClearAll();
        }

        /// <summary>
        /// Создаем кнопку для нуля (особое положение)
        /// </summary>
        private void AddZero()
        {
            Button newButton = CreateSetButtonFormatting("0");

            newButton.Location = new Point(BasePosition.X + WidthBind, BasePosition.Y + WidthBind);
        }

        private void AddSwitchSign()
        {
            Button newButton = CreateSetButtonFormatting("+/-");

            newButton.Font = new Font(FontFamily.GenericSansSerif, 24);

            newButton.Location = new Point(BasePosition.X, BasePosition.Y + WidthBind);
        }

        /// <summary>
        /// Создаём кнопку для десятичного разделителя (особое положение)
        /// </summary>
        private void AddDecimalDelimiter()
        {
            Button newButton = CreateSetButtonFormatting(this.CalcEngine.DecSep);

            newButton.Location = new Point(BasePosition.X + WidthBind * 2, BasePosition.Y + WidthBind);
        }

        /// <summary>
        /// Создаём кнопку для вызова действия сложение
        /// </summary>
        private void AddPlus()
        {
            Button newButton = CreateSetButtonFormatting("+");

            newButton.Height *= 2;

            newButton.Location = new Point(BasePosition.X + WidthBind * 3, BasePosition.Y);
        }

        /// <summary>
        /// Создаём кнопку для вызова действия сложение
        /// </summary>
        private void AddMinus()
        {
            Button newButton = CreateSetButtonFormatting("-");

            newButton.Height *= 2;

            newButton.Location = new Point(BasePosition.X + WidthBind * 3, BasePosition.Y - WidthBind * 2);
        }

        /// <summary>
        /// Создаём кнопку для вызова действия умножение
        /// </summary>
        private void AddMultiply()
        {
            Button newButton = CreateSetButtonFormatting("*");

            newButton.Location = new Point(BasePosition.X + WidthBind * 4, BasePosition.Y - WidthBind * 2);
        }

        /// <summary>
        /// Создаём кнопку для вызова действия деление
        /// </summary>
        private void AddDivide()
        {
            Button newButton = CreateSetButtonFormatting("/");

            newButton.Location = new Point(BasePosition.X + WidthBind * 4, BasePosition.Y - WidthBind);
        }

        /// <summary>
        /// Создаём кнопку для вызова вычисления операции
        /// </summary>
        private void AddEquals()
        {
            Button newButton = CreateSetButtonFormatting("=");

            newButton.Height *= 2;

            newButton.Location = new Point(BasePosition.X + WidthBind * 4, BasePosition.Y);
        }

        /// <summary>
        /// Создаём кнопку для вызова очистки последнего числа в памяти калькулятора
        /// </summary>
        private void AddClearLast()
        {
            Button newButton = CreateSetButtonFormatting("C");

            newButton.Font = new Font(FontFamily.GenericSansSerif, 22);

            newButton.Location = new Point(BasePosition.X + WidthBind * 5, BasePosition.Y - WidthBind);
        }

        /// <summary>
        /// Создаём кнопку для вызова
        /// </summary>
        private void AddClearAll()
        {
            Button newButton = CreateSetButtonFormatting("CE");

            newButton.Font = new Font(FontFamily.GenericSansSerif, 22);

            newButton.Location = new Point(BasePosition.X + WidthBind * 5, BasePosition.Y - WidthBind * 2);
        }

        /// <summary>
        /// Создаём квадрат для чисел 1 - 9
        /// </summary>
        /// <param name="Col">Текущая колонка</param>
        /// <param name="Row">Текущая строка</param>
        private void AddNumbersSquare(int Col, int Row)
        {
            Button newButton = CreateSetButtonFormatting((Row * 3 + (Col + 1)).ToString());

            // Вычисляем координаты кнопок
            Point CurPos = new Point();
            CurPos.X = BasePosition.X + newButton.Size.Width * Col;
            CurPos.Y = BasePosition.Y - newButton.Size.Height * Row;
            newButton.Location = CurPos;
        }

        /// <summary>
        /// Создаём кнопку и настраиваем типовые её параметры
        /// </summary>
        /// <param name="ButtonText">Надпись на кнопке</param>
        /// <returns></returns>
        private Button CreateSetButtonFormatting(string ButtonText)
        {
            Button newButton = new Button();
            this.Controls.Add(newButton);

            newButton.Text = ButtonText;

            newButton.Size = new Size(WidthBind, WidthBind);
            newButton.Font = new Font(FontFamily.GenericSansSerif, 36);

            newButton.Click += OnAnyButtonClick;

            return newButton;
        }

        /// <summary>
        /// Создаём надпись для вывода результата
        /// </summary>
        /// <param name="LabelText">Значение для вывода на надписи</param>
        /// <returns>Объект надпись</returns>
        private Label CreateSetLabelFormatting(string LabelText)
        {
            Label newLabel = new Label();
            this.Controls.Add(newLabel);

            newLabel.TextAlign = ContentAlignment.MiddleRight;
            newLabel.BorderStyle = BorderStyle.Fixed3D;
            newLabel.Text = LabelText;
            newLabel.Size = new Size(this.Size.Width - 25, this.Size.Width / 7);
            newLabel.Font = new Font(FontFamily.GenericSansSerif, 36);
            newLabel.Location = new Point(5, 5);

            return newLabel;
        }

        /// <summary>
        /// Универсальная процедура для обработки нажатий на кнопки
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAnyButtonClick(object sender, EventArgs e)
        {
            Button ClickedBtn = (Button)sender;
            if (ClickedBtn.Text == "C" || ClickedBtn.Text == "CE")
            {
                this.CalcEngine.ClearEnteredNumbers(ClickedBtn.Text);
            }
            else if (ClickedBtn.Text == "+/-")
            {
                this.CalcEngine.SwitchSignCurrentNumber();
            }
            else
            {
                this.CalcEngine.ExpandEnteredNumber(ClickedBtn.Text.ToCharArray()[0]);
            }
        }

        /// <summary>
        /// Добавляем символы только в случае их валидности
        /// </summary>
        /// <param name="sender">экземпляр класса вызывающего обработчик</param>
        /// <param name="e">Параметры события</param>
        private void OnValidCharInput(object sender, ValidInputEventArgs e)
        {
            if (e.ResetInput)
            {
                LabelForResult.Text = "";
            }

            if (e.NumberNegative)
            {
                LabelForResult.Text = "-" + LabelForResult.Text.Replace("-", "") + e.ValidCharacter;
            }
            else
            {
                LabelForResult.Text = LabelForResult.Text.Replace("-", "") + e.ValidCharacter;
            }
        }

        /// <summary>
        /// Обработка нажатий на кнопки клавиатуры
        /// </summary>
        /// <param name="sender">Объект источник события</param>
        /// <param name="e">Аргументы события</param>
        private void frmMain_KeyPress(object sender, KeyPressEventArgs e)
        {
            string KeyString = e.KeyChar.ToString();

            foreach (Button btn in this.Controls.OfType<Button>())
            {
                if (btn.Text == "." || btn.Text == ",")
                {
                    if (KeyString == "." || KeyString == ",")
                    {
                        btn.Select();
                        btn.PerformClick();
                    }
                }

                if (KeyString == "\b" && btn.Text == "CE")
                {
                    btn.Select();
                    btn.PerformClick();
                }

                if (KeyString.ToLower() == "c" && btn.Text == "C")
                {
                    btn.Select();
                    btn.PerformClick();
                }

                if (btn.Text == KeyString)
                {
                    btn.Select();
                    btn.PerformClick();
                }
            }
        }
    }
}