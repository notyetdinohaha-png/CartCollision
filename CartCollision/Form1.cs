using System;
using System.Drawing;
using System.Windows.Forms;

namespace CartCollision
{
    public partial class Form1 : Form
    {
        private CartModel _model;

        public Form1()
        {
            InitializeComponent();

            typeof(Panel).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.SetProperty |
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic,
                null, pnlCanvas, new object[] { true });
            // включаем двойную буферизацию для панели, чтоб анимация не мерцала
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //метод загрузки формы(ничего писать не нужно) 
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            try
            {
                // чтение параметров из текстовых полейDDS
                double m1 = double.Parse(txtM1.Text);
                double m2 = double.Parse(txtM2.Text);
                double v1 = double.Parse(txtV1.Text);
                double v2 = double.Parse(txtV2.Text);

                
                if (v2 > 0)
                {
                    v2 = -v2;
                }

                // создаем или сбрасываем физическую модель
                if (_model == null)
                {
                    _model = new CartModel(m1, m2, v1, v2, 50, pnlCanvas.Width - 110);
                }
                else
                {
                    _model.Reset(m1, m2, v1, v2, 50, pnlCanvas.Width - 110);
                }

                lblResult.Text = "Движение...";
                timerAnimation.Start();
            }
            catch (FormatException)
            {
                MessageBox.Show("Пожалуйста, введите корректные числовые значения!", "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            timerAnimation.Stop();
            _model = null;
            lblResult.Text = "Скорость после столкновения: ";
            pnlCanvas.Invalidate(); // очистить/перерисовать панель
        }

        private void timerAnimation_Tick(object sender, EventArgs e)
        {
            if (_model != null)
            {
                // шаг обновления (0.05 сек на каждый кадр) 
                _model.Update(0.05);

                // после абсолютно неупругое столкновения выводим результат
                if (_model.IsCollided)
                {
                    lblResult.Text = $"Скорость после столкновения: {_model.VAfter:F2} м/с";
                }

                // перерисовываем панель с анимацией
                pnlCanvas.Invalidate();
            }
        }

        private void pnlCanvas_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // линия поверхности
            int groundY = pnlCanvas.Height - 30;
            g.DrawLine(Pens.Black, 0, groundY, pnlCanvas.Width, groundY);

            // отрисовка моделей тележек, если модель создана
            // отрисовка моделей тележек, если модель создана
            if (_model != null)
            {
                int cartWidth = (int)_model.CartWidth;
                int cartHeight = 40;
                int cartY = groundY - cartHeight;

                // первая тележка (синяя)
                g.FillRectangle(Brushes.DodgerBlue, (float)_model.X1, cartY, cartWidth, cartHeight);
                g.DrawRectangle(Pens.Black, (float)_model.X1, cartY, cartWidth, cartHeight);
                g.DrawString("m1", Font, Brushes.White, (float)_model.X1 + 15, cartY + 12);

                // вторая тележка (красная)
                g.FillRectangle(Brushes.Crimson, (float)_model.X2, cartY, cartWidth, cartHeight);
                g.DrawRectangle(Pens.Black, (float)_model.X2, cartY, cartWidth, cartHeight);
                g.DrawString("m2", Font, Brushes.White, (float)_model.X2 + 15, cartY + 12);
            }
        }
    }
}