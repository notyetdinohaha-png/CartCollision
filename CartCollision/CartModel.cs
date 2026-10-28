
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

        public CartModel(
            double mass1,
            double velocity1,
            double mass2,
            double velocity2)
        {
            if (mass1 <= 0 || mass2 <= 0)
                throw new ArgumentException(
                    "Массы тележек должны быть положительными.");

            if (double.IsNaN(mass1) || double.IsInfinity(mass1) ||
                double.IsNaN(mass2) || double.IsInfinity(mass2) ||
                double.IsNaN(velocity1) || double.IsInfinity(velocity1) ||
                double.IsNaN(velocity2) || double.IsInfinity(velocity2))
            {
                throw new ArgumentException(
                    "Параметры должны быть конечными числами.");
            }

            Mass1 = mass1;
            Mass2 = mass2;

            InitialVelocity1 = velocity1;
            InitialVelocity2 = velocity2;

            // Импульс до столкновения
            MomentumBefore =
                Mass1 * InitialVelocity1 +
                Mass2 * InitialVelocity2;

            // Общая скорость после абсолютно неупругого столкновения
            CommonVelocity =
                MomentumBefore / (Mass1 + Mass2);

            // Импульс после столкновения
            MomentumAfter =
                (Mass1 + Mass2) * CommonVelocity;
        }

        public bool CheckMomentumConservation(
            double tolerance = 0.000001)
        {
            return Math.Abs(MomentumBefore - MomentumAfter)
                   <= tolerance;
        }
    }
}
