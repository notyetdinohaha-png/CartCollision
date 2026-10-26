using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CartCollision
{
    public partial class Form1 : Form
    {
        // Переменные для симуляции физики движения
        private float x1, x2;
        private float v1, v2;
        private float m1, m2;
        private Color color1, color2;
        private float currentTime = 0f;
        private bool isCollided = false;
        private float commonVelocity = 0f;

        private const int cartWidth = 60;
        private const int cartHeight = 40;

        // Таймер анимации
        private Timer simulationTimer;

        public Form1()
        {
            InitializeComponent();

            // Инициализация таймера
            simulationTimer = new Timer();
            simulationTimer.Interval = 50;
            simulationTimer.Tick += Timer1_Tick;

            // Включаем двойную буферизацию для panel1, чтобы анимация не мерцала
            typeof(Panel).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                null, panel1, new object[] { true });

            // Заполняем выпадающие списки цветов при запуске
            string[] colors = { "Красный", "Синий", "Зелёный", "Жёлтый", "Оранжевый", "Фиолетовый", "Розовый", "Бирюзовый" };
            comboBox1.Items.AddRange(colors);
            comboBox2.Items.AddRange(colors);

            if (comboBox1.Items.Count > 0) comboBox1.SelectedIndex = 0;
            if (comboBox2.Items.Count > 1) comboBox2.SelectedIndex = 1;

            comboBox1.SelectedIndexChanged += comboBox1_SelectedIndexChanged;
            comboBox2.SelectedIndexChanged += comboBox2_SelectedIndexChanged;

            color1 = MapColor(comboBox1.SelectedItem.ToString());
            color2 = MapColor(comboBox2.SelectedItem.ToString());

            x1 = 20;
            x2 = 350;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // 1) Проверка введенных данных первой тележки
            if (!double.TryParse(textBox1.Text, out double mass1) || mass1 <= 0)
            {
                errorProvider1.SetError(textBox1, "Введите корректную массу первой тележки!");
                return;
            }
            if (!double.TryParse(textBox2.Text, out double vel1))
            {
                errorProvider1.SetError(textBox2, "Введите корректную скорость первой тележки!");
                return;
            }

            // 2) Проверка введенных данных второй тележки
            if (!double.TryParse(textBox3.Text, out double mass2) || mass2 <= 0)
            {
                errorProvider1.SetError(textBox3, "Введите корректную массу второй тележки!");
                return;
            }
            if (!double.TryParse(textBox4.Text, out double vel2))
            {
                errorProvider1.SetError(textBox4, "Введите корректную скорость второй тележки!");
                return;
            }

            errorProvider1.Clear();

            // 3) Считываем выбранные цвета
            string colorName1 = comboBox1.SelectedItem?.ToString() ?? "Красный";
            string colorName2 = comboBox2.SelectedItem?.ToString() ?? "Синий";

            if (colorName1 == colorName2)
            {
                MessageBox.Show("Рекомендуется выбрать разные цвета для тележек, чтобы их было лучше различать на экране!",
                    "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            color1 = MapColor(colorName1);
            color2 = MapColor(colorName2);

            // 4) Инициализация параметров физики
            m1 = (float)mass1;
            v1 = (float)vel1;
            m2 = (float)mass2;
            v2 = (float)vel2;

            // Начальные позиции
            x1 = 20;
            x2 = Math.Max(panel1.Width - 100, 300);

            currentTime = 0f;
            isCollided = false;
            commonVelocity = 0f;

            if (lblPhysicsCheck != null)
                lblPhysicsCheck.Text = "Проверка: ожидание удара...";
            if (s12 != null)
                s12.Text = "Скорость после столкновения: -";

            simulationTimer.Start(); // Запуск анимации
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            float dt = 0.05f;
            float speedScale = 20f; // Коэффициент скорости анимации
            currentTime += dt;

            if (!isCollided)
            {
                // Движение до столкновения
                x1 += v1 * dt * speedScale;
                x2 += v2 * dt * speedScale;

                // Момент столкновения
                if (x1 + cartWidth >= x2)
                {
                    isCollided = true;

                    // --- ПРОВЕРКА ЗАКОНА СОХРАНЕНИЯ ИМПУЛЬСА ---
                    float pBefore = (m1 * v1) + (m2 * v2);
                    commonVelocity = (m1 * v1 + m2 * v2) / (m1 + m2);
                    float pAfter = (m1 + m2) * commonVelocity;

                    bool isPhysicsCorrect = Math.Abs(pBefore - pAfter) < 0.0001f;
                    string checkResult = isPhysicsCorrect ? "Успешно (импульс сохранен)" : "Ошибка расчета!";

                    if (lblPhysicsCheck != null)
                    {
                        lblPhysicsCheck.Text = $"Проверка: {checkResult} | До: {pBefore:F1} | После: {pAfter:F1}";
                    }

                    v1 = commonVelocity;
                    v2 = commonVelocity;
                }
            }
            else
            {
                // Движение после столкновения с общей скоростью
                x1 += commonVelocity * dt * speedScale;
                x2 += commonVelocity * dt * speedScale;
            }

            // Вывод текущих данных в метки на форме
            if (time != null) time.Text = $"Время: {currentTime:F2} с";
            if (S1 != null) S1.Text = $"S1: {v1:F2} м/с";
            if (S2 != null) S2.Text = $"S2: {v2:F2} м/с";
            if (s12 != null) s12.Text = $"Скорость после столкновения: {commonVelocity:F2} м/с";

            // Перерисовка холста
            panel1.Invalidate();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Color.White);

            float panelWidth = panel1.Width;
            float panelHeight = panel1.Height;

            // Находим центр между тележками и общее расстояние между ними
            float cartCenter = (x1 + x2 + cartWidth) / 2f;
            float cartSpan = Math.Abs((x2 + cartWidth) - x1);

            // Буферная зона (150 пикселей запаса), чтобы камера отдалялась заранее
            float buffer = 150f;
            float requiredWidth = cartSpan + buffer * 2f;

            float scale = 1.0f;
            if (requiredWidth > panelWidth)
            {
                scale = panelWidth / requiredWidth;
            }

            // Пропорциональное масштабирование (и по X, и по Y) с удержанием центра тележек посередине экрана
            g.TranslateTransform(panelWidth / 2f, panelHeight / 2f);
            g.ScaleTransform(scale, scale);
            g.TranslateTransform(-cartCenter, -panelHeight / 2f);

            int groundY = panel1.Height - 60;

            // Рисуем дорогу с большим запасом по длине
            g.DrawLine(Pens.Black, cartCenter - panelWidth * 3, groundY + cartHeight, cartCenter + panelWidth * 3, groundY + cartHeight);

            // Тележка 1
            using (Brush b1 = new SolidBrush(color1))
            {
                g.FillRectangle(b1, x1, groundY, cartWidth, cartHeight);
                g.FillEllipse(Brushes.DarkGray, x1 + 5, groundY + cartHeight - 5, 10, 10);
                g.FillEllipse(Brushes.DarkGray, x1 + cartWidth - 15, groundY + cartHeight - 5, 10, 10);
            }

            // Тележка 2
            using (Brush b2 = new SolidBrush(color2))
            {
                g.FillRectangle(b2, x2, groundY, cartWidth, cartHeight);
                g.FillEllipse(Brushes.DarkGray, x2 + 5, groundY + cartHeight - 5, 10, 10);
                g.FillEllipse(Brushes.DarkGray, x2 + cartWidth - 15, groundY + cartHeight - 5, 10, 10);
            }

            // Эффект удара
            if (isCollided)
            {
                float impactX = (x1 + x2 + cartWidth) / 2;
                g.DrawString("СТОЛКНОВЕНИЕ!", new Font("Arial", 12, FontStyle.Bold), Brushes.Red, impactX - 50, groundY - 30);
            }
        }

        private Color MapColor(string colorName)
        {
            switch (colorName)
            {
                case "Красный": return Color.Red;
                case "Синий": return Color.Blue;
                case "Зелёный": return Color.Green;
                case "Жёлтый": return Color.Yellow;
                case "Оранжевый": return Color.Orange;
                case "Фиолетовый": return Color.Purple;
                case "Розовый": return Color.Pink;
                case "Бирюзовый": return Color.Turquoise;
                default: return Color.Gray;
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem != null)
            {
                color1 = MapColor(comboBox1.SelectedItem.ToString());
                panel1.Invalidate();
            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox2.SelectedItem != null)
            {
                color2 = MapColor(comboBox2.SelectedItem.ToString());
                panel1.Invalidate();
            }
        }

        // Заглушки для событий
        private void timer1_Click(object sender, EventArgs e) { }
        private void label2_Click(object sender, EventArgs e) { }
        private void label14_Click(object sender, EventArgs e) { }
        private void label1_Click(object sender, EventArgs e) { }
        private void label3_Click(object sender, EventArgs e) { }
        private void textBox2_TextChanged(object sender, EventArgs e) { }
        private void textBox1_TextChanged(object sender, EventArgs e) { }
        private void label4_Click(object sender, EventArgs e) { }
        private void label5_Click(object sender, EventArgs e) { }
        private void label6_Click(object sender, EventArgs e) { }
        private void S1_Keypress(object sender, KeyPressEventArgs e) { }
        private void S1_Click(object sender, EventArgs e) { }
        private void S2_Click(object sender, EventArgs e) { }
        private void Form1_Load(object sender, EventArgs e) { }
    }
}