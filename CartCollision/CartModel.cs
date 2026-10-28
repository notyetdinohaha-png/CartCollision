
using System;

namespace CartCollision
{
    public class CartModel
    {
        public double Mass1 { get; private set; }
        public double Mass2 { get; private set; }

        public double InitialVelocity1 { get; private set; }
        public double InitialVelocity2 { get; private set; }

        public double CommonVelocity { get; private set; }

        public double MomentumBefore { get; private set; }
        public double MomentumAfter { get; private set; }

        // Положение тележек
        public double Position1 { get; private set; }
        public double Position2 { get; private set; }

        // Произошло ли столкновение
        public bool IsCollided { get; private set; }

        // Текущие скорости
        public double Velocity1 { get; private set; }
        public double Velocity2 { get; private set; }

        public CartModel(
    double mass1,
    double velocity1,
    double mass2,
    double velocity2,
    double position1 = 0,
    double position2 = 100)
        {
            if (double.IsNaN(mass1) || double.IsInfinity(mass1) ||
                double.IsNaN(mass2) || double.IsInfinity(mass2) ||
                double.IsNaN(velocity1) || double.IsInfinity(velocity1) ||
                double.IsNaN(velocity2) || double.IsInfinity(velocity2))
            {
                throw new ArgumentException(
                    "Параметры должны быть конечными числами.");
            }

            if (mass1 <= 0 || mass2 <= 0)
                throw new ArgumentException(
                    "Массы тележек должны быть положительными.");

            Mass1 = mass1;
            Mass2 = mass2;

            InitialVelocity1 = velocity1;
            InitialVelocity2 = velocity2;

            Velocity1 = velocity1;
            Velocity2 = velocity2;

            MomentumBefore =
                Mass1 * InitialVelocity1 +
                Mass2 * InitialVelocity2;

            CommonVelocity =
                MomentumBefore / (Mass1 + Mass2);

            MomentumAfter =
                (Mass1 + Mass2) * CommonVelocity;

            Position1 = position1;
            Position2 = position2;
            IsCollided = false;
        }

        // Один шаг моделирования.
        // cartWidth — ширина тележки в условных единицах модели.
        public void Step(double dt, double cartWidth)
        {
            if (dt < 0 || double.IsNaN(dt) ||
                double.IsInfinity(dt))
                throw new ArgumentException(
                    "Шаг времени должен быть конечным и неотрицательным.");

            if (cartWidth <= 0 || double.IsNaN(cartWidth) ||
                double.IsInfinity(cartWidth))
                throw new ArgumentException(
                    "Ширина тележки должна быть положительной.");

            if (IsCollided)
            {
                Position1 += CommonVelocity * dt;
                Position2 += CommonVelocity * dt;
                return;
            }

            Position1 += Velocity1 * dt;
            Position2 += Velocity2 * dt;

            // Тележки движутся слева направо по координатной оси.
            // Столкновение возможно, когда первая догоняет вторую.
            if (Velocity1 > Velocity2 &&
                Position1 + cartWidth >= Position2)
            {
                Position2 = Position1 + cartWidth;
                IsCollided = true;

                Velocity1 = CommonVelocity;
                Velocity2 = CommonVelocity;
            }
        }

        public bool CheckMomentumConservation(
            double tolerance = 0.000001)
        {
            return Math.Abs(MomentumBefore - MomentumAfter)
                   <= tolerance;
        }
    }
}
