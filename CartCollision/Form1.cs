using System;
using System.Globalization;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CartCollision.Core;

namespace CartCollision
{
    public partial class Form1 : Form
    {
        // Переменные для симуляции физики движения
        private float x1, x2;
        private float v1, v2;
        private float m1, m2;
        private float initialV1, initialV2;
        private Color color1, color2;
        private float currentTime = 0f;
        private System.Diagnostics.Stopwatch realTimer =
    new System.Diagnostics.Stopwatch();
        private bool isCollided = false;
        private float commonVelocity = 0f;

        // Физическая модель столкновения
        private CartModel cartModel;

        private const int cartWidth = 60;
        private const int cartHeight = 40;

        // Параметры камеры
        private float cameraCenterX;
        private float cameraScale = 1.0f;
        private bool cameraLocked = false;

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


        private bool TryParseFlexibleDouble(string text, out double value)
        {
            text = text.Trim().Replace(',', '.');

            bool parsed = double.TryParse(
                text,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out value);

            return parsed &&
                   !double.IsNaN(value) &&
                   !double.IsInfinity(value);
        }


        private void button1_Click(object sender, EventArgs e)
        {
            // 1) Проверка введенных данных первой тележки
            if (!TryParseFlexibleDouble(textBox1.Text, out double mass1) || mass1 <= 0)
            {
                errorProvider1.SetError(textBox1, "Введите корректную массу первой тележки!");
                return;
            }
            if (!TryParseFlexibleDouble(textBox2.Text, out double vel1))
            {
                errorProvider1.SetError(textBox2, "Введите корректную скорость первой тележки!");
                return;
            }

            // 2) Проверка введенных данных второй тележки
            if (!TryParseFlexibleDouble(textBox3.Text, out double mass2) || mass2 <= 0)
            {
                errorProvider1.SetError(textBox3, "Введите корректную массу второй тележки!");
                return;
            }
            if (!TryParseFlexibleDouble(textBox4.Text, out double vel2))
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
            initialV1 = v1;
            initialV2 = v2;

            // Начальные позиции
            x1 = 20;
            x2 = Math.Max(panel1.Width - 100, 300);

            currentTime = 0f;
            isCollided = false;
            commonVelocity = 0f;

            // Возвращаем камеру в режим слежения
            cameraLocked = false;
            cameraScale = 1.0f;
            cameraCenterX = (x1 + x2 + cartWidth) / 2f;

            // Проверяем, сближаются ли тележки
            if (v1 <= v2)
            {
                MessageBox.Show(
                    "При заданных Вами скоростях тележки не столкнутся.\n" +
                    "Пожалуйста, введите другие значения.",
                    "Столкновение невозможно",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                panel1.Invalidate();
                return;
            }

            // Создаём физическую модель с введёнными параметрами и реальными начальными координатами тележек
            cartModel = new CartModel(
                m1, v1,
                m2, v2,
                x1, x2);

            if (lblPhysicsCheck != null)
                lblPhysicsCheck.Text = "Проверка: ожидание удара...";

            if (s12 != null)
                s12.Text = "Общая скорость после столкновения V': -";

            realTimer.Restart();
            simulationTimer.Start();
        }



        private void Timer1_Tick(object sender, EventArgs e)
        {
            float dt = 0.05f;
            float speedScale = 20f;

            currentTime += dt;

            if (cartModel == null)
                return;

            bool wasCollided = cartModel.IsCollided;

            cartModel.Step(dt * speedScale, cartWidth);

            // Получаем координаты и скорости из модели
            x1 = (float)cartModel.Position1;
            x2 = (float)cartModel.Position2;

            v1 = (float)cartModel.Velocity1;
            v2 = (float)cartModel.Velocity2;

            isCollided = cartModel.IsCollided;

            // Обрабатываем момент столкновения один раз
            // Обрабатываем момент столкновения один раз
            if (!wasCollided && isCollided)
            {
                realTimer.Stop();
                commonVelocity = (float)cartModel.CommonVelocity;

                // 1. Проверяем сохранение импульса
                bool isMomentumCorrect =
                    cartModel.CheckMomentumConservation();

                // 2. Рассчитываем кинетическую энергию до столкновения
                double energyBefore =
    0.5 * m1 * initialV1 * initialV1 +
    0.5 * m2 * initialV2 * initialV2;

                // 3. Рассчитываем кинетическую энергию после столкновения
                double energyAfter =
                    0.5 * (m1 + m2) *
                    commonVelocity * commonVelocity;

                // 4. Находим потерю кинетической энергии
                double energyLoss = energyBefore - energyAfter;

                // 5. Проверяем, что энергия действительно потеряна
                bool isEnergyCorrect =
                    energyLoss > 0.0001;

                string momentumResult = isMomentumCorrect
                    ? "импульс сохранён"
                    : "ошибка импульса";

                string energyResult = isEnergyCorrect
                    ? "потеря энергии подтверждена"
                    : "потеря энергии не подтверждена";

                if (lblPhysicsCheck != null)
                {
                    lblPhysicsCheck.Text =
                        $"Импульс: {momentumResult}\n" +
                        $"Энергия до: {energyBefore:F2} Дж\n" +
                        $"Энергия после: {energyAfter:F2} Дж\n" +
                        $"Потеря энергии: {energyLoss:F2} Дж\n" +
                        $"Проверка энергии: {energyResult}";
                }

                // Фиксируем камеру в месте столкновения
                cameraCenterX = (x1 + x2 + cartWidth) / 2f;
                cameraLocked = true;
            }

            // Обновляем показания на экране
            if (time != null)
                time.Text = isCollided
                    ? $"Время эксперимента: {realTimer.Elapsed.TotalSeconds:F2} с"
                    : $"Время эксперимента: {realTimer.Elapsed.TotalSeconds:F2} с";

            if (S1 != null)
                S1.Text = $"V1: {v1:F2} м/с";

            if (S2 != null)
                S2.Text = $"V2: {v2:F2} м/с";

            if (s12 != null && isCollided)
            {
                s12.Text =
                    $"Общая скорость после столкновения V': {commonVelocity:F2} м/с";
            }

            // Останавливаем симуляцию после ухода тележек за экран
            if (isCollided)
            {
                if (Math.Abs(commonVelocity) < 0.001f)
                {
                    simulationTimer.Stop();
                }
                else
                {
                    float visibleLeft =
                        cameraCenterX -
                        panel1.Width / (2f * cameraScale);

                    float visibleRight =
                        cameraCenterX +
                        panel1.Width / (2f * cameraScale);

                    bool movedOffScreenRight =
                        commonVelocity > 0 &&
                        x1 > visibleRight &&
                        x2 > visibleRight;

                    bool movedOffScreenLeft =
                        commonVelocity < 0 &&
                        x1 + cartWidth < visibleLeft &&
                        x2 + cartWidth < visibleLeft;

                    if (movedOffScreenRight || movedOffScreenLeft)
                        simulationTimer.Stop();
                }
            }

            panel1.Invalidate();
        }



        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Color.White);

            float panelWidth = panel1.Width;
            float panelHeight = panel1.Height;


            // Рассчитываем положение и масштаб камеры
            float cartCenter = (x1 + x2 + cartWidth) / 2f;
            float scale = cameraScale;

            if (!cameraLocked)
            {
                // Пока столкновения не было, камера следует за тележками
                float cartSpan = Math.Abs((x2 + cartWidth) - x1);

                float buffer = 150f;
                float requiredWidth = cartSpan + buffer * 2f;

                scale = 1.0f;

                if (requiredWidth > panelWidth)
                {
                    scale = panelWidth / requiredWidth;
                }

                cameraCenterX = cartCenter;
                cameraScale = scale;

                // Фиксируем камеру при первом кадре после столкновения
                if (isCollided)
                {
                    cameraLocked = true;
                }
            }
            else
            {
                cartCenter = cameraCenterX;
                scale = cameraScale;
            }


            // Пропорциональное масштабирование (и по X, и по Y) с удержанием центра тележек посередине экрана
            g.TranslateTransform(panelWidth / 2f, panelHeight / 2f);
            g.ScaleTransform(scale, scale);
            g.TranslateTransform(-cartCenter, -panelHeight / 2f);

            int groundY = panel1.Height - 60;

            // Рисуем дорогу
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


            // Показываем направления и скорости тележек
            float offset1 = isCollided ? -35f : 0f;
            float offset2 = isCollided ? 35f : 0f;

            DrawVelocityIndicator(
                g, x1 + cartWidth / 2f + offset1, groundY - 35,
                v1, "v1");

            DrawVelocityIndicator(
                g, x2 + cartWidth / 2f + offset2, groundY - 35,
                v2, "v2");


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

        private void Form1_Load_1(object sender, EventArgs e)
        {

        }

        private void S1_Keypress(object sender, KeyPressEventArgs e) { }
        private void S1_Click(object sender, EventArgs e) { }
        private void S2_Click(object sender, EventArgs e) { }
        private void Form1_Load(object sender, EventArgs e) { }

        private void DrawVelocityIndicator(
    Graphics g, float centerX, float y,
    float velocity, string name)
        {
            using (Font font = new Font("Arial", 9, FontStyle.Bold))
            using (Pen pen = new Pen(Color.Black, 2))
            using (Brush brush = new SolidBrush(Color.Black))
            {
                string text = $"{name} = {velocity:+0.00;-0.00;0.00} м/с";
                SizeF textSize = g.MeasureString(text, font);

                g.DrawString(
                    text, font, brush,
                    centerX - textSize.Width / 2f, y - 25);

                if (Math.Abs(velocity) < 0.001f)
                    return;

                float direction = velocity > 0 ? 1f : -1f;
                float length = 35f;

                float startX = centerX - direction * length / 2f;
                float endX = centerX + direction * length / 2f;

                g.DrawLine(pen, startX, y, endX, y);

                PointF[] arrowHead =
                {
                    new PointF(endX, y),
                    new PointF(endX - direction * 8f, y - 5f),
                    new PointF(endX - direction * 8f, y + 5f)
                };

                g.FillPolygon(brush, arrowHead);
            }
        }
    }
}