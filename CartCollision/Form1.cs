
using System;
using System.Globalization;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using CartCollision.ViewModel;

namespace CartCollision
{
    public partial class Form1 : Form
    {
        // ViewModel — единственный источник данных симуляции
        private readonly MainViewModel viewModel = new MainViewModel();

        // Цвета тележек относятся к отображению, поэтому остаются в Form1
        private Color color1;
        private Color color2;

        private const int cartWidth = 60;
        private const int cartHeight = 40;

        // Параметры камеры
        private float cameraCenterX;
        private float cameraScale = 1.0f;
        private bool cameraLocked = false;

        // Таймеры интерфейса
        private readonly Stopwatch realTimer = new Stopwatch();
        private Timer simulationTimer;

        public Form1()
        {
            InitializeComponent();

            // Подписываем форму на изменения ViewModel
            viewModel.PropertyChanged += ViewModel_PropertyChanged;

            // Создаём таймер анимации
            simulationTimer = new Timer();
            simulationTimer.Interval = 50;
            simulationTimer.Tick += Timer1_Tick;

            // Убираем мерцание при отрисовке
            typeof(Panel).InvokeMember(
                "DoubleBuffered",
                System.Reflection.BindingFlags.SetProperty |
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic,
                null,
                panel1,
                new object[] { true }
            );

            // Заполняем списки цветов
            string[] colors =
            {
                "Красный",
                "Синий",
                "Зелёный",
                "Жёлтый",
                "Оранжевый",
                "Фиолетовый",
                "Розовый",
                "Бирюзовый"
            };

            comboBox1.Items.Clear();
            comboBox2.Items.Clear();

            comboBox1.Items.AddRange(colors);
            comboBox2.Items.AddRange(colors);

            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 1;

            color1 = MapColor(comboBox1.SelectedItem.ToString());
            color2 = MapColor(comboBox2.SelectedItem.ToString());

            // Обработчики выбора цвета
            comboBox1.SelectedIndexChanged += comboBox1_SelectedIndexChanged;
            comboBox2.SelectedIndexChanged += comboBox2_SelectedIndexChanged;

            // Начальные координаты камеры
            cameraCenterX = 0;
            cameraScale = 1.0f;

            ViewModel_PropertyChanged(
                this,
                new PropertyChangedEventArgs("")
            );
        }

        // Преобразование введённой строки в число
        private bool TryParseFlexibleDouble(string text, out double value)
        {
            text = text.Trim().Replace(',', '.');

            bool parsed = double.TryParse(
                text,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out value
            );

            return parsed &&
                   !double.IsNaN(value) &&
                   !double.IsInfinity(value);
        }

        // Кнопка запуска эксперимента
        private void button1_Click(object sender, EventArgs e)
        {
            double mass1;
            double mass2;
            double velocity1;
            double velocity2;

            // Проверка массы первой тележки
            if (!TryParseFlexibleDouble(textBox1.Text, out mass1) ||
                mass1 <= 0)
            {
                errorProvider1.SetError(
                    textBox1,
                    "Введите корректную массу первой тележки!"
                );
                return;
            }

            // Проверка скорости первой тележки
            if (!TryParseFlexibleDouble(textBox2.Text, out velocity1))
            {
                errorProvider1.SetError(
                    textBox2,
                    "Введите корректную скорость первой тележки!"
                );
                return;
            }

            // Проверка массы второй тележки
            if (!TryParseFlexibleDouble(textBox3.Text, out mass2) ||
                mass2 <= 0)
            {
                errorProvider1.SetError(
                    textBox3,
                    "Введите корректную массу второй тележки!"
                );
                return;
            }

            // Проверка скорости второй тележки
            if (!TryParseFlexibleDouble(textBox4.Text, out velocity2))
            {
                errorProvider1.SetError(
                    textBox4,
                    "Введите корректную скорость второй тележки!"
                );
                return;
            }

            errorProvider1.Clear();

            // Проверяем, будут ли тележки сближаться
            if (velocity1 <= velocity2)
            {
                MessageBox.Show(
                    "При заданных скоростях тележки не столкнутся.\n" +
                    "Введите другие значения.",
                    "Столкновение невозможно",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
                return;
            }

            // Получаем выбранные цвета
            string colorName1 =
                comboBox1.SelectedItem?.ToString() ?? "Красный";

            string colorName2 =
                comboBox2.SelectedItem?.ToString() ?? "Синий";

            color1 = MapColor(colorName1);
            color2 = MapColor(colorName2);

            if (color1 == color2)
            {
                MessageBox.Show(
                    "Рекомендуется выбрать разные цвета тележек.",
                    "Внимание",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }

            // Начальные координаты тележек
            double position1 = 20;
            double position2 = Math.Max(panel1.Width - 100, 300);

            // Останавливаем предыдущий эксперимент, если он выполнялся
            simulationTimer.Stop();
            realTimer.Stop();
            viewModel.Stop();

            // Запускаем физическую модель через ViewModel
            try
            {
                viewModel.Start(
                    mass1,
                    velocity1,
                    mass2,
                    velocity2,
                    position1,
                    position2
                );
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Ошибка запуска",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            // Сбрасываем камеру
            cameraLocked = false;
            cameraScale = 1.0f;
            cameraCenterX = (float)(
                (position1 + position2 + cartWidth) / 2.0
            );

            // Обновляем надписи
            if (lblPhysicsCheck != null)
                lblPhysicsCheck.Text = "Проверка: ожидание столкновения...";

            if (time != null)
                time.Text = "Время эксперимента: 0.00 с";

            // Запускаем таймеры
            realTimer.Restart();
            simulationTimer.Start();

            panel1.Invalidate();
        }

        // Реакция формы на изменения ViewModel
        private void ViewModel_PropertyChanged(
            object sender,
            PropertyChangedEventArgs e)
        {
            if (S1 != null)
                S1.Text = $"V1: {viewModel.Velocity1:F2} м/с";

            if (S2 != null)
                S2.Text = $"V2: {viewModel.Velocity2:F2} м/с";

            if (s12 != null)
            {
                if (viewModel.IsCollided)
                {
                    s12.Text =
                        $"Общая скорость после столкновения V': " +
                        $"{viewModel.CommonVelocity:F2} м/с";
                }
                else
                {
                    s12.Text = "Общая скорость после столкновения V': -";
                }
            }

            // При изменении координат или состояния столкновения
            // перерисовываем область анимации
            if (e.PropertyName == nameof(MainViewModel.Position1) ||
                e.PropertyName == nameof(MainViewModel.Position2) ||
                e.PropertyName == nameof(MainViewModel.IsCollided) ||
                string.IsNullOrEmpty(e.PropertyName))
            {
                if (panel1 != null)
                    panel1.Invalidate();
            }
        }

        // Один шаг анимации
        private void Timer1_Tick(object sender, EventArgs e)
        {
            if (!viewModel.IsRunning)
                return;

            bool wasCollided = viewModel.IsCollided;

            // Шаг времени модели
            double dt = 0.05 * 20.0;

            try
            {
                // Физический расчёт выполняется в ViewModel
                viewModel.Step(dt, cartWidth);
            }
            catch (ArgumentException ex)
            {
                simulationTimer.Stop();
                realTimer.Stop();

                MessageBox.Show(
                    ex.Message,
                    "Ошибка симуляции",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return;
            }

            // Обрабатываем момент столкновения один раз
            if (!wasCollided && viewModel.IsCollided)
            {
                realTimer.Stop();

                string momentumResult = viewModel.MomentumIsCorrect
                    ? "импульс сохранён"
                    : "ошибка сохранения импульса";

                string energyResult = viewModel.EnergyLossIsConfirmed
                    ? "потеря энергии подтверждена"
                    : "потеря энергии не подтверждена";

                if (lblPhysicsCheck != null)
                {
                    lblPhysicsCheck.Text =
                        $"Импульс: {momentumResult}\n" +
                        $"Энергия до: {viewModel.EnergyBefore:F2} Дж\n" +
                        $"Энергия после: {viewModel.EnergyAfter:F2} Дж\n" +
                        $"Потеря энергии: {viewModel.EnergyLoss:F2} Дж\n" +
                        $"Проверка энергии: {energyResult}";
                }

                // Фиксируем камеру в месте столкновения
                cameraCenterX = (float)(
                    (viewModel.Position1 +
                     viewModel.Position2 +
                     cartWidth) / 2.0
                );

                cameraLocked = true;
            }

            // Обновляем время эксперимента
            if (time != null)
            {
                time.Text =
                    $"Время эксперимента: " +
                    $"{realTimer.Elapsed.TotalSeconds:F2} с";
            }

            // Проверяем, не ушли ли тележки за пределы экрана
            if (viewModel.IsCollided)
            {
                bool shouldStop = false;

                if (Math.Abs(viewModel.CommonVelocity) < 0.001)
                {
                    shouldStop = true;
                }
                else
                {
                    float visibleLeft =
                        cameraCenterX - panel1.Width / (2f * cameraScale);

                    float visibleRight =
                        cameraCenterX + panel1.Width / (2f * cameraScale);

                    bool movedOffScreenRight =
                        viewModel.CommonVelocity > 0 &&
                        viewModel.Position1 > visibleRight &&
                        viewModel.Position2 > visibleRight;

                    bool movedOffScreenLeft =
                        viewModel.CommonVelocity < 0 &&
                        viewModel.Position1 + cartWidth < visibleLeft &&
                        viewModel.Position2 + cartWidth < visibleLeft;

                    shouldStop =
                        movedOffScreenRight || movedOffScreenLeft;
                }

                if (shouldStop)
                {
                    simulationTimer.Stop();
                    viewModel.Stop();
                }
            }

            panel1.Invalidate();
        }

        // Отрисовка анимации
        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Color.White);

            float panelWidth = panel1.Width;
            float panelHeight = panel1.Height;

            // Координаты берём непосредственно из ViewModel
            float x1 = (float)viewModel.Position1;
            float x2 = (float)viewModel.Position2;

            float v1 = (float)viewModel.Velocity1;
            float v2 = (float)viewModel.Velocity2;

            // Центр между тележками
            float cartCenter = (x1 + x2 + cartWidth) / 2f;

            float scale = cameraScale;

            if (!cameraLocked)
            {
                // Камера следует за тележками
                float cartSpan =
                    Math.Abs((x2 + cartWidth) - x1);

                float buffer = 150f;
                float requiredWidth = cartSpan + buffer * 2f;

                scale = 1.0f;

                if (requiredWidth > panelWidth)
                    scale = panelWidth / requiredWidth;

                cameraCenterX = cartCenter;
                cameraScale = scale;
            }
            else
            {
                cartCenter = cameraCenterX;
                scale = cameraScale;
            }

            // Настройка системы координат
            g.TranslateTransform(panelWidth / 2f, panelHeight / 2f);
            g.ScaleTransform(scale, scale);
            g.TranslateTransform(-cartCenter, -panelHeight / 2f);

            int groundY = panel1.Height - 60;

            // Дорога
            g.DrawLine(
                Pens.Black,
                cartCenter - panelWidth * 3,
                groundY + cartHeight,
                cartCenter + panelWidth * 3,
                groundY + cartHeight
            );

            // Первая тележка
            using (Brush b1 = new SolidBrush(color1))
            {
                g.FillRectangle(
                    b1,
                    x1,
                    groundY,
                    cartWidth,
                    cartHeight
                );

                g.FillEllipse(
                    Brushes.DarkGray,
                    x1 + 5,
                    groundY + cartHeight - 5,
                    10,
                    10
                );

                g.FillEllipse(
                    Brushes.DarkGray,
                    x1 + cartWidth - 15,
                    groundY + cartHeight - 5,
                    10,
                    10
                );
            }

            // Вторая тележка
            using (Brush b2 = new SolidBrush(color2))
            {
                g.FillRectangle(
                    b2,
                    x2,
                    groundY,
                    cartWidth,
                    cartHeight
                );

                g.FillEllipse(
                    Brushes.DarkGray,
                    x2 + 5,
                    groundY + cartHeight - 5,
                    10,
                    10
                );

                g.FillEllipse(
                    Brushes.DarkGray,
                    x2 + cartWidth - 15,
                    groundY + cartHeight - 5,
                    10,
                    10
                );
            }

            // Индикаторы скоростей
            float offset1 = viewModel.IsCollided ? -35f : 0f;
            float offset2 = viewModel.IsCollided ? 35f : 0f;

            DrawVelocityIndicator(
                g,
                x1 + cartWidth / 2f + offset1,
                groundY - 35,
                v1,
                "v1"
            );

            DrawVelocityIndicator(
                g,
                x2 + cartWidth / 2f + offset2,
                groundY - 35,
                v2,
                "v2"
            );

            // Надпись в момент столкновения
            if (viewModel.IsCollided)
            {
                float impactX = (x1 + x2 + cartWidth) / 2f;

                using (Font font =
                    new Font("Arial", 12, FontStyle.Bold))
                {
                    g.DrawString(
                        "СТОЛКНОВЕНИЕ!",
                        font,
                        Brushes.Red,
                        impactX - 50,
                        groundY - 30
                    );
                }
            }

            // Возвращаем исходную систему координат
            g.ResetTransform();
        }

        // Преобразование названия цвета в Color
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

        // Изменение цвета первой тележки
        private void comboBox1_SelectedIndexChanged(
            object sender,
            EventArgs e)
        {
            if (comboBox1.SelectedItem != null)
            {
                color1 = MapColor(comboBox1.SelectedItem.ToString());
                panel1.Invalidate();
            }
        }

        // Изменение цвета второй тележки
        private void comboBox2_SelectedIndexChanged(
            object sender,
            EventArgs e)
        {
            if (comboBox2.SelectedItem != null)
            {
                color2 = MapColor(comboBox2.SelectedItem.ToString());
                panel1.Invalidate();
            }
        }

        // Экспорт траектории в CSV
        private void btnExportCsv_Click(object sender, EventArgs e)
        {
            List<string> trajectoryData =
                viewModel.GetTrajectoryData();

            if (trajectoryData == null || trajectoryData.Count == 0)
            {
                MessageBox.Show(
                    "Нет данных для экспорта. Сначала запустите симуляцию.",
                    "Экспорт CSV",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
                return;
            }

            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "CSV-файлы (*.csv)|*.csv";
                dialog.FileName = "trajectory.csv";
                dialog.Title = "Сохранить траекторию тележек";

                if (dialog.ShowDialog() != DialogResult.OK)
                    return;

                try
                {
                    using (System.IO.StreamWriter writer =
                        new System.IO.StreamWriter(
                            dialog.FileName,
                            false,
                            new UTF8Encoding(true)))
                    {
                        writer.WriteLine(
                            "Time;Position1;Position2;Velocity1;Velocity2;Collided"
                        );

                        foreach (string row in trajectoryData)
                        {
                            writer.WriteLine(row);
                        }
                    }

                    MessageBox.Show(
                        $"Экспорт завершён!\n" +
                        $"Записей: {trajectoryData.Count}\n" +
                        $"Файл: {dialog.FileName}",
                        "Экспорт CSV",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Не удалось сохранить файл:\n" + ex.Message,
                        "Ошибка экспорта",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            }
        }

        // Заглушки событий, подключённых в Designer
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

        private void Form1_Load_1(object sender, EventArgs e) { }

        private void S1_Keypress(object sender, KeyPressEventArgs e) { }
        private void S1_Click(object sender, EventArgs e) { }
        private void S2_Click(object sender, EventArgs e) { }
        private void Form1_Load(object sender, EventArgs e) { }

        // Отображение скорости и направления движения
        private void DrawVelocityIndicator(
            Graphics g,
            float centerX,
            float y,
            float velocity,
            string name)
        {
            using (Font font =
                new Font("Arial", 9, FontStyle.Bold))
            using (Pen pen = new Pen(Color.Black, 2))
            using (Brush brush = new SolidBrush(Color.Black))
            {
                string text =
                    $"{name} = {velocity:+0.00;-0.00;0.00} м/с";

                SizeF textSize = g.MeasureString(text, font);

                g.DrawString(
                    text,
                    font,
                    brush,
                    centerX - textSize.Width / 2f,
                    y - 25
                );

                if (Math.Abs(velocity) < 0.001f)
                    return;

                float direction = velocity > 0 ? 1f : -1f;
                float length = 35f;

                float startX =
                    centerX - direction * length / 2f;

                float endX =
                    centerX + direction * length / 2f;

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
