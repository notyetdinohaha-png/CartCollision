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
        // viewModel - источник данных симуляции
        private readonly MainViewModel viewModel = new MainViewModel();

        // цвета тележек
        private Color color1;
        private Color color2;

        private const int cartWidth = 60;
        private const int cartHeight = 40;

        // параметры камеры
        private float cameraCenterX;
        private float cameraScale = 1.0f;
        private bool cameraLocked = false;

        // режим дня/ночи
        private bool isNightMode = true;
        private Button btnToggleTheme;

        // таймеры интерфейса
        private readonly Stopwatch realTimer = new Stopwatch();
        private Timer simulationTimer;

        public Form1()
        {


            InitializeComponent();
            // Привязываем обе кнопки к левому нижнему углу
            btnExportCsv.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            Den.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;

            viewModel.PropertyChanged += ViewModel_PropertyChanged;

            // создаём таймер анимации
            simulationTimer = new Timer();
            simulationTimer.Interval = 50;
            simulationTimer.Tick += Timer1_Tick;

            // убираем мерцание при отрисовке
            typeof(Panel).InvokeMember(
                "DoubleBuffered",
                System.Reflection.BindingFlags.SetProperty |
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic,
                null,
                panel1,
                new object[] { true }
            );

            // исправлены артефакты когда растягиваем и развертываем на весь экран
            panel1.Resize += (s, e) => panel1.Invalidate();

            // Список цветов 
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

            // обработчики выбора цвета
            comboBox1.SelectedIndexChanged += comboBox1_SelectedIndexChanged;
            comboBox2.SelectedIndexChanged += comboBox2_SelectedIndexChanged;

            // начальные координаты камеры
            cameraCenterX = 0;
            cameraScale = 1.0f;

            ViewModel_PropertyChanged(
                this,
                new PropertyChangedEventArgs("")
            );
        }

        // обработчик кнопки День/Ночь 
        private void Den_Click(object sender, EventArgs e)
        {
            isNightMode = !isNightMode;
            Den.Text = isNightMode ? "🌙 Ночной режим" : "☀️ Дневной режим";
            panel1.Invalidate();
        }

        // преобразование введённой строки в число
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

        // кнопка запуска
        private void button1_Click(object sender, EventArgs e)
        {
            double mass1;
            double mass2;
            double velocity1;
            double velocity2;

            if (!TryParseFlexibleDouble(textBox1.Text, out mass1) || mass1 <= 0)
            {
                errorProvider1.SetError(textBox1, "Введите корректную массу первой тележки!");
                return;
            }

            if (!TryParseFlexibleDouble(textBox2.Text, out velocity1))
            {
                errorProvider1.SetError(textBox2, "Введите корректную скорость первой тележки!");
                return;
            }

            if (!TryParseFlexibleDouble(textBox3.Text, out mass2) || mass2 <= 0)
            {
                errorProvider1.SetError(textBox3, "Введите корректную массу второй тележки!");
                return;
            }

            if (!TryParseFlexibleDouble(textBox4.Text, out velocity2))
            {
                errorProvider1.SetError(textBox4, "Введите корректную скорость второй тележки!");
                return;
            }

            errorProvider1.Clear();

            if (velocity1 <= velocity2)
            {
                MessageBox.Show(
                    "При заданных скоростях тележки не столкнутся.\nВведите другие значения.",
                    "Столкновение невозможно",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
                return;
            }

            //цвета по умолчанию 
            string colorName1 = comboBox1.SelectedItem?.ToString() ?? "Красный";
            string colorName2 = comboBox2.SelectedItem?.ToString() ?? "Синий";  

            color1 = MapColor(colorName1);
            color2 = MapColor(colorName2);

            if (color1 == color2)
            {
                MessageBox.Show(
                    "Лучше выбрать разные цвета тележек.",
                    "Вниманииииииеееее!! аллллооо",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }

            double position1 = 20;
            double position2 = Math.Max(panel1.Width - 100, 300);

            simulationTimer.Stop();
            realTimer.Stop();
            viewModel.Stop();

            try
            {
                viewModel.Start(mass1, velocity1, mass2, velocity2, position1, position2);
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message, "Ошибка запуска", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            cameraLocked = false;
            cameraScale = 1.0f;
            cameraCenterX = (float)((position1 + position2 + cartWidth) / 2.0);

            if (lblPhysicsCheck != null)
                lblPhysicsCheck.Text = "Проверка: ожидание столкновения...";

            if (time != null)
                time.Text = "Время эксперимента: 0.00 с";

            realTimer.Restart();
            simulationTimer.Start();

            panel1.Invalidate();
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (S1 != null)
                S1.Text = $"V1: {viewModel.Velocity1:F2} м/с";

            if (S2 != null)
                S2.Text = $"V2: {viewModel.Velocity2:F2} м/с";

            if (s12 != null)
            {
                if (viewModel.IsCollided)
                {
                    s12.Text = $"Общая скорость после столкновения V': {viewModel.CommonVelocity:F2} м/с";
                }
                else
                {
                    s12.Text = "Общая скорость после столкновения V': -";
                }
            }

            if (e.PropertyName == nameof(MainViewModel.Position1) ||
                e.PropertyName == nameof(MainViewModel.Position2) ||
                e.PropertyName == nameof(MainViewModel.IsCollided) ||
                string.IsNullOrEmpty(e.PropertyName))
            {
                if (panel1 != null)
                    panel1.Invalidate();
            }
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            if (!viewModel.IsRunning)
                return;

            bool wasCollided = viewModel.IsCollided;
            double dt = 0.05 * 20.0;

            try
            {
                viewModel.Step(dt, cartWidth);
            }
            catch (ArgumentException ex)
            {
                simulationTimer.Stop();
                realTimer.Stop();
                MessageBox.Show(ex.Message, "Ошибка симуляции", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!wasCollided && viewModel.IsCollided)
            {
                realTimer.Stop();

                string momentumResult = viewModel.MomentumIsCorrect ? "импульс сохранён" : "ошибка сохранения импульса";
                string energyResult = viewModel.EnergyLossIsConfirmed ? "потеря энергии подтверждена" : "потеря энергии не подтверждена";

                if (lblPhysicsCheck != null)
                {
                    lblPhysicsCheck.Text =
                        $"Импульс: {momentumResult}\n" +
                        $"Энергия до: {viewModel.EnergyBefore:F2} Дж\n" +
                        $"Энергия после: {viewModel.EnergyAfter:F2} Дж\n" +
                        $"Потеря энергии: {viewModel.EnergyLoss:F2} Дж\n" +
                        $"Проверка энергии: {energyResult}";
                }

                cameraCenterX = (float)((viewModel.Position1 + viewModel.Position2 + cartWidth) / 2.0);
                cameraLocked = true;
            }

            if (time != null)
            {
                time.Text = $"Время эксперимента: {realTimer.Elapsed.TotalSeconds:F2} с";
            }

            if (viewModel.IsCollided)
            {
                bool shouldStop = false;

                if (Math.Abs(viewModel.CommonVelocity) < 0.001)
                {
                    shouldStop = true;
                }
                else
                {
                    float visibleLeft = cameraCenterX - panel1.Width / (2f * cameraScale);
                    float visibleRight = cameraCenterX + panel1.Width / (2f * cameraScale);

                    // теперь останавливаемся, когда тележки упираются в границы экрана своими краями, а то блин не останавливались 
                    bool movedOffScreenRight = viewModel.CommonVelocity > 0 &&
                        (viewModel.Position1 + cartWidth >= visibleRight || viewModel.Position2 + cartWidth >= visibleRight);

                    bool movedOffScreenLeft = viewModel.CommonVelocity < 0 &&
                        (viewModel.Position1 <= visibleLeft || viewModel.Position2 <= visibleLeft);

                    shouldStop = movedOffScreenRight || movedOffScreenLeft;
                }

                if (shouldStop)
                {
                    simulationTimer.Stop();
                    viewModel.Stop();
                }
            }

            panel1.Invalidate();
        }

        // отрисовка сцены
        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            if (isNightMode)
                g.Clear(Color.FromArgb(15, 20, 35));
            else
                g.Clear(Color.FromArgb(225, 240, 255));

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            float panelWidth = panel1.Width;
            float panelHeight = panel1.Height;

            float x1 = (float)viewModel.Position1;
            float x2 = (float)viewModel.Position2;

            float v1 = (float)viewModel.Velocity1;
            float v2 = (float)viewModel.Velocity2;

            float cartCenter = (x1 + x2 + cartWidth) / 2f;
            float scale = cameraScale;

            if (!cameraLocked)
            {
                float cartSpan = Math.Abs((x2 + cartWidth) - x1);
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

            g.TranslateTransform(panelWidth / 2f, panelHeight / 2f);
            g.ScaleTransform(scale, scale);
            g.TranslateTransform(-cartCenter, -panelHeight / 2f);

            int groundY = panel1.Height - 60;

            // дорога отрисовывается
            using (Pen railPen = new Pen(isNightMode ? Color.FromArgb(100, 110, 120) : Color.DarkGray, 3))
            {
                g.DrawLine(railPen, cartCenter - panelWidth * 3, groundY + cartHeight, cartCenter + panelWidth * 3, groundY + cartHeight);
            }
            using (Pen sleeperPen = new Pen(isNightMode ? Color.FromArgb(60, 45, 30) : Color.SaddleBrown, 4))
            {
                float sleeperSpacing = 20f;
                float startSleeper = (float)(Math.Floor((cartCenter - panelWidth * 3) / sleeperSpacing) * sleeperSpacing);
                for (float sx = startSleeper; sx <= cartCenter + panelWidth * 3; sx += sleeperSpacing)
                {
                    g.DrawLine(sleeperPen, sx, groundY + cartHeight, sx, groundY + cartHeight + 6);
                }
            }

            float worldLeft = cartCenter - panelWidth * 2 / scale;
            float worldRight = cartCenter + panelWidth * 2 / scale;

            // фонари
            float lampSpacing = 280f;
            float firstLampX = (float)(Math.Floor(worldLeft / lampSpacing) * lampSpacing);
            for (float lx = firstLampX; lx <= worldRight; lx += lampSpacing)
            {
                DrawLampPost(g, lx, groundY + cartHeight, isNightMode);
            }
            // деревья
            float treeSpacing = 100f;
            float firstTreeX = (float)(Math.Floor(worldLeft / treeSpacing) * treeSpacing);

            for (float tx = firstTreeX; tx <= worldRight; tx += treeSpacing)
            {
                int treeType = Math.Abs((int)(tx / treeSpacing)) % 3;

                DrawVariedTree(g, tx, groundY + cartHeight, isNightMode, treeType);
            }

            DrawDetailedCart(g, x1, groundY, color1, "№1");
            DrawDetailedCart(g, x2, groundY, color2, "№2");

            float offset1 = viewModel.IsCollided ? -35f : 0f;
            float offset2 = viewModel.IsCollided ? 35f : 0f;

            DrawVelocityIndicator(g, x1 + cartWidth / 2f + offset1, groundY - 45, v1, "v1", isNightMode);
            DrawVelocityIndicator(g, x2 + cartWidth / 2f + offset2, groundY - 45, v2, "v2", isNightMode);

            if (viewModel.IsCollided)
            {
                float impactX = (x1 + x2 + cartWidth) / 2f;
                using (Font font = new Font("Arial", 13, FontStyle.Bold))
                {
                    g.DrawString("СТОЛКНОВЕНИЕ!", font, Brushes.OrangeRed, impactX - 65, groundY - 40);
                }
            }

            g.ResetTransform();
        }

        private void DrawLampPost(Graphics g, float x, float groundY, bool night)
        {
            Color poleColor = night ? Color.FromArgb(70, 75, 85) : Color.DarkSlateGray;
            using (Pen polePen = new Pen(poleColor, 4))
            {
                g.DrawLine(polePen, x, groundY, x, groundY - 120);
            }
            using (Pen armPen = new Pen(poleColor, 3))
            {
                g.DrawLine(armPen, x, groundY - 120, x + 20, groundY - 125);
            }

            if (night)
            {
                using (Brush bulbBrush = new SolidBrush(Color.FromArgb(255, 240, 150)))
                {
                    g.FillEllipse(bulbBrush, x + 16, groundY - 128, 8, 8);
                }
                using (Brush lightConeBrush = new SolidBrush(Color.FromArgb(18, 255, 255, 120)))
                {
                    PointF[] lightPoints = {
                        new PointF(x + 20, groundY - 122),
                        new PointF(x - 90, groundY),
                        new PointF(x + 120, groundY)
                    };
                    g.FillPolygon(lightConeBrush, lightPoints);
                }
            }
            else
            {
                using (Brush bulbBrush = new SolidBrush(Color.LightGray))
                {
                    g.FillEllipse(bulbBrush, x + 16, groundY - 128, 8, 8);
                }
            }
        }

        private void DrawVariedTree(Graphics g, float x, float groundY, bool night, int type)
        {
            Color trunkColor = night ? Color.FromArgb(45, 35, 25) : Color.SaddleBrown;
            Color crownColor = night ? Color.FromArgb(20, 65, 45) : Color.ForestGreen;
            Color highlightColor = night ? Color.FromArgb(40, 95, 65) : Color.LimeGreen;
            Color pineColor = night ? Color.FromArgb(15, 50, 35) : Color.DarkGreen;

            using (Brush trunkBrush = new SolidBrush(trunkColor))
            using (Brush crownBrush = new SolidBrush(crownColor))
            using (Brush highlightBrush = new SolidBrush(highlightColor))
            using (Brush pineBrush = new SolidBrush(pineColor))
            {
                if (type == 0)
                {
                    g.FillRectangle(trunkBrush, x - 5, groundY - 35, 10, 35);
                    g.FillEllipse(crownBrush, x - 22, groundY - 70, 44, 45);
                    g.FillEllipse(highlightBrush, x - 15, groundY - 65, 20, 20);
                }
                else if (type == 1)
                {
                    g.FillRectangle(trunkBrush, x - 6, groundY - 30, 12, 30);
                    g.FillEllipse(crownBrush, x - 30, groundY - 55, 60, 35);
                    g.FillEllipse(highlightBrush, x - 20, groundY - 50, 25, 15);
                }
                else
                {
                    g.FillRectangle(trunkBrush, x - 4, groundY - 25, 8, 25);
                    PointF[] pinePoints1 = {
                        new PointF(x, groundY - 75),
                        new PointF(x - 22, groundY - 40),
                        new PointF(x + 22, groundY - 40)
                    };
                    PointF[] pinePoints2 = {
                        new PointF(x, groundY - 50),
                        new PointF(x - 28, groundY - 20),
                        new PointF(x + 28, groundY - 20)
                    };
                    g.FillPolygon(pineBrush, pinePoints2);
                    g.FillPolygon(crownBrush, pinePoints1);
                }
            }
        }

        private void DrawDetailedCart(Graphics g, float x, float groundY, Color baseColor, string label)
        {
            using (Brush baseBrush = new SolidBrush(Color.FromArgb(60, 60, 60)))
            {
                g.FillRectangle(baseBrush, x - 2, groundY + cartHeight - 8, cartWidth + 4, 8);
            }

            using (Brush bodyBrush = new SolidBrush(baseColor))
            using (Pen borderPen = new Pen(Color.FromArgb(30, 30, 30), 1.5f))
            {
                g.FillRectangle(bodyBrush, x, groundY + 8, cartWidth, cartHeight - 14);
                g.DrawRectangle(borderPen, x, groundY + 8, cartWidth, cartHeight - 14);
                g.DrawRectangle(Pens.White, x + 3, groundY + 11, cartWidth - 6, cartHeight - 20);
            }

            using (Font font = new Font("Arial", 8, FontStyle.Bold))
            {
                g.DrawString(label, font, Brushes.White, x + cartWidth / 2 - 8, groundY + 14);
            }

            using (Brush wheelBrush = new SolidBrush(Color.FromArgb(40, 40, 40)))
            using (Brush rimBrush = new SolidBrush(Color.LightGray))
            {
                g.FillEllipse(wheelBrush, x + 4, groundY + cartHeight - 4, 14, 14);
                g.FillEllipse(rimBrush, x + 7, groundY + cartHeight - 1, 8, 8);

                g.FillEllipse(wheelBrush, x + cartWidth - 18, groundY + cartHeight - 4, 14, 14);
                g.FillEllipse(rimBrush, x + cartWidth - 15, groundY + cartHeight - 1, 8, 8);
            }

            using (Pen hookPen = new Pen(Color.DimGray, 2))
            {
                g.DrawLine(hookPen, x + cartWidth, groundY + cartHeight - 15, x + cartWidth + 6, groundY + cartHeight - 15);
                g.FillEllipse(Brushes.Black, x + cartWidth + 5, groundY + cartHeight - 17, 4, 4);
            }
        }

        private Color MapColor(string colorName)
        {
            switch (colorName)
            {
                case "Красный": return Color.Crimson;
                case "Синий": return Color.RoyalBlue;
                case "Зелёный": return Color.SeaGreen;
                case "Жёлтый": return Color.Gold;
                case "Оранжевый": return Color.DarkOrange;
                case "Фиолетовый": return Color.MediumPurple;
                case "Розовый": return Color.HotPink;
                case "Бирюзовый": return Color.DarkTurquoise;
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

        private void btnShowCharts_Click(object sender, EventArgs e)
        {
            List<string> trajectoryData = viewModel.GetTrajectoryData();

            if (trajectoryData == null || trajectoryData.Count == 0)
            {
                MessageBox.Show(
                    "Нет данных для построения графиков. Сначала запустите симуляцию.",
                    "Графики",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
                return;
            }

            using (Form chartForm = new Form())
            {
                chartForm.Text = "Графики эксперимента: Координаты и Скорости";
                chartForm.Size = new Size(950, 800);
                chartForm.StartPosition = FormStartPosition.CenterParent;
                chartForm.MinimumSize = new Size(600, 500);

                PictureBox pb = new PictureBox
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.White
                };

                pb.Paint += (s, pe) =>
                {
                    Graphics g = pe.Graphics;
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                    int w = pb.Width;
                    int h = pb.Height;
                    int leftMargin = 75;
                    int rightMargin = 40;
                    int topMargin = 40;
                    int bottomMargin = 50;

                    int availableHeight = h - topMargin - bottomMargin;
                    int graphHeight = (availableHeight - 40) / 2;

                    List<double> times = new List<double>();
                    List<double> p1List = new List<double>();
                    List<double> p2List = new List<double>();
                    List<double> v1List = new List<double>();
                    List<double> v2List = new List<double>();

                    foreach (var row in trajectoryData)
                    {
                        var parts = row.Split(';');
                        if (parts.Length >= 5)
                        {
                            if (double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out double t) &&
                                double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out double p1) &&
                                double.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out double p2) &&
                                double.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out double v1) &&
                                double.TryParse(parts[4], NumberStyles.Float, CultureInfo.InvariantCulture, out double v2))
                            {
                                times.Add(t);
                                p1List.Add(p1);
                                p2List.Add(p2);
                                v1List.Add(v1);
                                v2List.Add(v2);
                            }
                        }
                    }

                    if (times.Count == 0) return;

                    double tMin = times[0];
                    double tMax = times[times.Count - 1];
                    if (tMax <= tMin) tMax = tMin + 1.0;

                    int graphWidth = w - leftMargin - rightMargin;
                    if (graphWidth < 50) graphWidth = 50;

                    using (Pen axisPen = new Pen(Color.Black, 1.5f))
                    using (Pen gridPen = new Pen(Color.LightGray, 1) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dot })
                    using (Font font = new Font("Arial", 8.5f))
                    using (Font titleFont = new Font("Arial", 9.5f, FontStyle.Bold))
                    using (Brush brush = new SolidBrush(Color.Black))
                    {
                        int topY1 = topMargin + 20;
                        double pMin = double.MaxValue, pMax = double.MinValue;
                        foreach (var val in p1List) { if (val < pMin) pMin = val; if (val > pMax) pMax = val; }
                        foreach (var val in p2List) { if (val < pMin) pMin = val; if (val > pMax) pMax = val; }
                        if (Math.Abs(pMax - pMin) < 0.001) { pMax += 1.0; pMin -= 1.0; }

                        g.DrawString("Зависимость координат от времени (Position)", titleFont, Brushes.DarkBlue, leftMargin, topMargin);

                        g.DrawLine(axisPen, leftMargin, topY1 + graphHeight, leftMargin + graphWidth, topY1 + graphHeight);
                        g.DrawLine(axisPen, leftMargin, topY1, leftMargin, topY1 + graphHeight);

                        int ySteps = 5;
                        for (int i = 0; i <= ySteps; i++)
                        {
                            double val = pMin + (pMax - pMin) * i / ySteps;
                            float y = topY1 + graphHeight - (float)((val - pMin) / (pMax - pMin) * graphHeight);
                            g.DrawLine(gridPen, leftMargin, y, leftMargin + graphWidth, y);
                            g.DrawString($"{val:F1}", font, brush, leftMargin - 50, y - 6);
                        }

                        int xSteps = 6;
                        for (int i = 0; i <= xSteps; i++)
                        {
                            double tVal = tMin + (tMax - tMin) * i / xSteps;
                            float x = leftMargin + (float)((tVal - tMin) / (tMax - tMin) * graphWidth);
                            g.DrawLine(gridPen, x, topY1, x, topY1 + graphHeight);
                            g.DrawString($"{tVal:F1}с", font, brush, x - 12, topY1 + graphHeight + 4);
                        }

                        using (Pen pen1 = new Pen(Color.Crimson, 2))
                        using (Pen pen2 = new Pen(Color.RoyalBlue, 2))
                        {
                            for (int i = 0; i < times.Count - 1; i++)
                            {
                                float x1 = leftMargin + (float)((times[i] - tMin) / (tMax - tMin) * graphWidth);
                                float x2 = leftMargin + (float)((times[i + 1] - tMin) / (tMax - tMin) * graphWidth);

                                float y1_1 = topY1 + graphHeight - (float)((p1List[i] - pMin) / (pMax - pMin) * graphHeight);
                                float y1_2 = topY1 + graphHeight - (float)((p1List[i + 1] - pMin) / (pMax - pMin) * graphHeight);
                                g.DrawLine(pen1, x1, y1_1, x2, y1_2);

                                float y2_1 = topY1 + graphHeight - (float)((p2List[i] - pMin) / (pMax - pMin) * graphHeight);
                                float y2_2 = topY1 + graphHeight - (float)((p2List[i + 1] - pMin) / (pMax - pMin) * graphHeight);
                                g.DrawLine(pen2, x1, y2_1, x2, y2_2);
                            }
                        }

                        int topY2 = topY1 + graphHeight + 50;
                        double vMin = double.MaxValue, vMax = double.MinValue;
                        foreach (var val in v1List) { if (val < vMin) vMin = val; if (val > vMax) vMax = val; }
                        foreach (var val in v2List) { if (val < vMin) vMin = val; if (val > vMax) vMax = val; }
                        if (Math.Abs(vMax - vMin) < 0.001) { vMax += 1.0; vMin -= 1.0; }

                        g.DrawString("Зависимость скоростей от времени (Velocity, м/с)", titleFont, Brushes.DarkBlue, leftMargin, topY2 - 20);

                        g.DrawLine(axisPen, leftMargin, topY2 + graphHeight, leftMargin + graphWidth, topY2 + graphHeight);
                        g.DrawLine(axisPen, leftMargin, topY2, leftMargin, topY2 + graphHeight);

                        for (int i = 0; i <= ySteps; i++)
                        {
                            double val = vMin + (vMax - vMin) * i / ySteps;
                            float y = topY2 + graphHeight - (float)((val - vMin) / (vMax - vMin) * graphHeight);
                            g.DrawLine(gridPen, leftMargin, y, leftMargin + graphWidth, y);
                            g.DrawString($"{val:F1}", font, brush, leftMargin - 50, y - 6);
                        }

                        for (int i = 0; i <= xSteps; i++)
                        {
                            double tVal = tMin + (tMax - tMin) * i / xSteps;
                            float x = leftMargin + (float)((tVal - tMin) / (tMax - tMin) * graphWidth);
                            g.DrawLine(gridPen, x, topY2, x, topY2 + graphHeight);
                            g.DrawString($"{tVal:F1}с", font, brush, x - 12, topY2 + graphHeight + 4);
                        }

                        using (Pen pen1 = new Pen(Color.Crimson, 2))
                        using (Pen pen2 = new Pen(Color.RoyalBlue, 2))
                        {
                            for (int i = 0; i < times.Count - 1; i++)
                            {
                                float x1 = leftMargin + (float)((times[i] - tMin) / (tMax - tMin) * graphWidth);
                                float x2 = leftMargin + (float)((times[i + 1] - tMin) / (tMax - tMin) * graphWidth);

                                float y1_1 = topY2 + graphHeight - (float)((v1List[i] - vMin) / (vMax - vMin) * graphHeight);
                                float y1_2 = topY2 + graphHeight - (float)((v1List[i + 1] - vMin) / (vMax - vMin) * graphHeight);
                                g.DrawLine(pen1, x1, y1_1, x2, y1_2);

                                float y2_1 = topY2 + graphHeight - (float)((v2List[i] - vMin) / (vMax - vMin) * graphHeight);
                                float y2_2 = topY2 + graphHeight - (float)((v2List[i + 1] - vMin) / (vMax - vMin) * graphHeight);
                                g.DrawLine(pen2, x1, y2_1, x2, y2_2);
                            }
                        }

                        int legX = w - 160;
                        int legY = 20;
                        g.DrawRectangle(Pens.Gray, legX - 10, legY - 5, 140, 45);
                        g.FillRectangle(Brushes.Crimson, legX, legY, 15, 10);
                        g.DrawString("Тележка 1", font, brush, legX + 22, legY - 3);

                        g.FillRectangle(Brushes.RoyalBlue, legX, legY + 22, 15, 10);
                        g.DrawString("Тележка 2", font, brush, legX + 22, legY + 19);
                    }
                };

                pb.Resize += (s, pe) => pb.Invalidate();

                chartForm.Controls.Add(pb);
                chartForm.ShowDialog(this);
            }
        }

        private void btnExportCsv_Click(object sender, EventArgs e)
        {
            List<string> trajectoryData = viewModel.GetTrajectoryData();

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
                        $"Экспорт завершён!\nЗаписей: {trajectoryData.Count}\nФайл: {dialog.FileName}",
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

        private void DrawVelocityIndicator(
            Graphics g,
            float centerX,
            float y,
            float velocity,
            string name,
            bool nightMode) // принимаем ночной режим
        {
            //  цвет темы и текста от времени дня 
            Color textColor = nightMode ? Color.White : Color.Black;

            using (Font font = new Font("Arial", 9, FontStyle.Bold))
            using (Pen pen = new Pen(textColor, 2))
            using (Brush brush = new SolidBrush(textColor))
            {
                string text = $"{name} = {velocity:+0.00;-0.00;0.00} м/с";
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