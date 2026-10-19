using System;

namespace CartCollision
{
    public class CartModel
    {
        // Входные параметры
        public double M1 { get; set; } // масса первой тележки (кг)
        public double M2 { get; set; } // масса второй тележки (кг)
        public double V1 { get; set; } // начальная скорость первой тележки (м/с)
        public double V2 { get; set; } // начальная скорость второй тележки (м/с)

        // Текущие позиции тележек на экране (X-координаты)
        public double X1 { get; set; }
        public double X2 { get; set; }

        // Общая скорость после абсолютно неупругого столкновения
        public double VAfter { get; private set; }

        // Состояние: произошло ли столкновение
        public bool IsCollided { get; private set; }

        // Ширина тележки для точного определения касания
        public double CartWidth { get; set; } = 60.0;

        public CartModel(double m1, double m2, double v1, double v2, double startX1 = 50, double startX2 = 500)
        {
            Reset(m1, m2, v1, v2, startX1, startX2);
        }

        // Обновление положения тележек за промежуток времени dt
        public void Update(double dt)
        {
            if (!IsCollided)
            {
                // Движение до столкновения
                X1 += V1 * dt;
                X2 += V2 * dt;

                // Проверка столкновения: когда левый край второй тележки соприкасается с правым краем первой
                if (X1 + CartWidth >= X2)
                {
                    IsCollided = true;
                    // Выравниваем позиции в момент касания
                    X1 = X2 - CartWidth;

                    // Расчет общей скорости после абсолютно неупругого столкновения по закону сохранения импульса
                    VAfter = (M1 * V1 + M2 * V2) / (M1 + M2);
                }
            }
            else
            {
                // Движение после столкновения (обе тележки движутся слитно с общей скоростью)
                X1 += VAfter * dt;
                X2 = X1 + CartWidth;
            }
        }

        // Перезапуск с новыми начальными условиями
        public void Reset(double m1, double m2, double v1, double v2, double startX1 = 50, double startX2 = 500)
        {
            M1 = m1;
            M2 = m2;
            V1 = v1;
            V2 = v2;

            X1 = startX1;
            X2 = startX2;

            IsCollided = false;
            VAfter = (M1 * V1 + M2 * V2) / (M1 + M2);
        }
    }
}