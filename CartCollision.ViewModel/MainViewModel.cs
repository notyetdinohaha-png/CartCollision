
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using CartCollision.Core;

namespace CartCollision.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private CartModel cartModel;

        private readonly List<string> trajectoryData =
            new List<string>();

        private double simulationTime;

        private double position1;
        private double position2;
        private double velocity1;
        private double velocity2;
        private double commonVelocity;
        private double time;
        private bool isCollided;
        private bool isRunning;
        private double energyBefore;
        private double energyAfter;
        private double energyLoss;
        private bool momentumIsCorrect;
        private bool energyLossIsConfirmed;

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(
            [CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName));
        }

        public double Position1
        {
            get { return position1; }
            private set
            {
                if (position1 != value)
                {
                    position1 = value;
                    OnPropertyChanged();
                }
            }
        }

        public double Position2
        {
            get { return position2; }
            private set
            {
                if (position2 != value)
                {
                    position2 = value;
                    OnPropertyChanged();
                }
            }
        }

        public double Velocity1
        {
            get { return velocity1; }
            private set
            {
                if (velocity1 != value)
                {
                    velocity1 = value;
                    OnPropertyChanged();
                }
            }
        }

        public double Velocity2
        {
            get { return velocity2; }
            private set
            {
                if (velocity2 != value)
                {
                    velocity2 = value;
                    OnPropertyChanged();
                }
            }
        }

        public double CommonVelocity
        {
            get { return commonVelocity; }
            private set
            {
                if (commonVelocity != value)
                {
                    commonVelocity = value;
                    OnPropertyChanged();
                }
            }
        }

        public double Time
        {
            get { return time; }
            private set
            {
                if (time != value)
                {
                    time = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsCollided
        {
            get { return isCollided; }
            private set
            {
                if (isCollided != value)
                {
                    isCollided = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsRunning
        {
            get { return isRunning; }
            private set
            {
                if (isRunning != value)
                {
                    isRunning = value;
                    OnPropertyChanged();
                }
            }
        }

        public double EnergyBefore
        {
            get { return energyBefore; }
            private set
            {
                if (energyBefore != value)
                {
                    energyBefore = value;
                    OnPropertyChanged();
                }
            }
        }

        public double EnergyAfter
        {
            get { return energyAfter; }
            private set
            {
                if (energyAfter != value)
                {
                    energyAfter = value;
                    OnPropertyChanged();
                }
            }
        }

        public double EnergyLoss
        {
            get { return energyLoss; }
            private set
            {
                if (energyLoss != value)
                {
                    energyLoss = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool MomentumIsCorrect
        {
            get { return momentumIsCorrect; }
            private set
            {
                if (momentumIsCorrect != value)
                {
                    momentumIsCorrect = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool EnergyLossIsConfirmed
        {
            get { return energyLossIsConfirmed; }
            private set
            {
                if (energyLossIsConfirmed != value)
                {
                    energyLossIsConfirmed = value;
                    OnPropertyChanged();
                }
            }
        }

        public int TrajectoryCount
        {
            get { return trajectoryData.Count; }
        }

        public MainViewModel()
        {
            Reset();
        }

        public void Start(
            double mass1,
            double velocity1,
            double mass2,
            double velocity2,
            double position1,
            double position2)
        {
            if (mass1 <= 0 || mass2 <= 0)
                throw new ArgumentException(
                    "Массы должны быть положительными.");

            if (velocity1 <= velocity2)
                throw new ArgumentException(
                    "При заданных скоростях тележки не столкнутся.");

            cartModel = new CartModel(
                mass1,
                velocity1,
                mass2,
                velocity2,
                position1,
                position2);

            Position1 = cartModel.Position1;
            Position2 = cartModel.Position2;

            Velocity1 = cartModel.Velocity1;
            Velocity2 = cartModel.Velocity2;

            CommonVelocity = cartModel.CommonVelocity;

            IsCollided = false;
            IsRunning = true;
            Time = 0;
            simulationTime = 0;

            EnergyBefore = cartModel.EnergyBefore;
            EnergyAfter = cartModel.EnergyAfter;
            EnergyLoss = cartModel.EnergyLoss;

            MomentumIsCorrect = false;
            EnergyLossIsConfirmed = false;

            trajectoryData.Clear();
            OnPropertyChanged(nameof(TrajectoryCount));
        }

        public void Step(double dt, double cartWidth)
        {
            if (!IsRunning || cartModel == null)
                return;

            bool wasCollided = cartModel.IsCollided;

            cartModel.Step(dt, cartWidth);

            Position1 = cartModel.Position1;
            Position2 = cartModel.Position2;

            Velocity1 = cartModel.Velocity1;
            Velocity2 = cartModel.Velocity2;

            IsCollided = cartModel.IsCollided;
            CommonVelocity = cartModel.CommonVelocity;

            Time += dt;
            simulationTime += dt;

            trajectoryData.Add(string.Join(";",
                simulationTime.ToString(
                    "F4", CultureInfo.InvariantCulture),
                Position1.ToString(
                    "F4", CultureInfo.InvariantCulture),
                Position2.ToString(
                    "F4", CultureInfo.InvariantCulture),
                Velocity1.ToString(
                    "F4", CultureInfo.InvariantCulture),
                Velocity2.ToString(
                    "F4", CultureInfo.InvariantCulture),
                IsCollided ? "Да" : "Нет"
            ));

            OnPropertyChanged(nameof(TrajectoryCount));

            if (!wasCollided && IsCollided)
            {
                MomentumIsCorrect =
                    cartModel.CheckMomentumConservation();

                EnergyLossIsConfirmed =
                    cartModel.IsEnergyLossConfirmed;
            }
        }

        public void Stop()
        {
            IsRunning = false;
        }

        public void Reset()
        {
            cartModel = null;

            Position1 = 20;
            Position2 = 350;

            Velocity1 = 0;
            Velocity2 = 0;

            CommonVelocity = 0;
            Time = 0;

            simulationTime = 0;

            IsCollided = false;
            IsRunning = false;

            EnergyBefore = 0;
            EnergyAfter = 0;
            EnergyLoss = 0;

            MomentumIsCorrect = false;
            EnergyLossIsConfirmed = false;

            trajectoryData.Clear();
            OnPropertyChanged(nameof(TrajectoryCount));
        }

        public List<string> GetTrajectoryData()
        {
            return new List<string>(trajectoryData);
        }
    }
}
