
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CartCollision;
using CartCollision.Core;
using CartCollision.ViewModel;

namespace CartCollision.Tests
{
    [TestClass]
    public class CartModelTests
    {
        [TestMethod]
        public void Collision_ShouldConserveMomentum()
        {
            // Arrange: исходные параметры тележек
            double m1 = 1;
            double v1 = 3;
            double m2 = 2;
            double v2 = -4;

            // Act: рассчитываем результат столкновения
            CartModel model = new CartModel(m1, v1, m2, v2);

            // Assert: проверяем ожидаемые результаты
            Assert.AreEqual(-5.0, model.MomentumBefore, 0.000001);
            Assert.AreEqual(-5.0 / 3.0, model.CommonVelocity, 0.000001);
            Assert.AreEqual(-5.0, model.MomentumAfter, 0.000001);
            Assert.IsTrue(model.CheckMomentumConservation());
        }



        [TestMethod]
        public void Step_ShouldDetectCollisionAndSetCommonVelocity()
        {
            // Arrange: первая тележка догоняет вторую
            CartModel model = new CartModel(
                2, 3,
                1, 1,
                0, 10
            );

            // Act: моделируем движение
            model.Step(3, 5);

            // Assert: столкновение обнаружено
            Assert.IsTrue(model.IsCollided);

            // Обе тележки должны двигаться с общей скоростью
            Assert.AreEqual(
                model.CommonVelocity,
                model.Velocity1,
                0.000001
            );

            Assert.AreEqual(
                model.CommonVelocity,
                model.Velocity2,
                0.000001
            );
        }




        [TestMethod]
        public void Step_ShouldNotCollideBeforeContact()
        {
            // Arrange: тележки ещё далеко друг от друга
            CartModel model = new CartModel(
                2, 3,
                1, 1,
                0, 100
            );

            // Act: небольшой шаг времени
            model.Step(1, 5);

            // Assert: столкновения ещё нет
            Assert.IsFalse(model.IsCollided);

            // Проверяем, что тележки продолжили движение
            Assert.AreEqual(3.0, model.Position1, 0.000001);
            Assert.AreEqual(101.0, model.Position2, 0.000001);
        }




        [TestMethod]
        public void InelasticCollision_ShouldLoseKineticEnergy()
        {
            // Arrange — создаём две тележки с разными скоростями
            CartModel cartModel = new CartModel(
                2.0,  // масса первой тележки
                4.0,  // скорость первой тележки
                2.0,  // масса второй тележки
                0.0   // скорость второй тележки
            );

            // Act — получаем энергию до и после столкновения
            double energyBefore = cartModel.EnergyBefore;
            double energyAfter = cartModel.EnergyAfter;

            // Assert — проверяем, что энергия уменьшилась
            Assert.IsTrue(
                energyAfter < energyBefore,
                "После неупругого столкновения энергия должна уменьшиться."
            );

            Assert.IsTrue(
                cartModel.EnergyLoss > 0,
                "Потеря кинетической энергии должна быть положительной."
            );
        }


        [TestMethod]
        public void Start_ShouldInitializeSimulation()
        {
            // Arrange
            var vm = new MainViewModel();

            // Act
            vm.Start(2, 3, 1, -1, 20, 100);

            // Assert
            Assert.IsTrue(vm.IsRunning);
            Assert.AreEqual(20, vm.Position1);
            Assert.AreEqual(100, vm.Position2);
            Assert.AreEqual(3, vm.Velocity1);
            Assert.AreEqual(-1, vm.Velocity2);
        }


        [TestMethod]
        public void Step_ShouldMoveCartsForward()
        {
            // Arrange
            var vm = new MainViewModel();
            vm.Start(2, 3, 1, 1, 20, 100);

            // Act
            vm.Step(1, 5);

            // Assert
            Assert.AreEqual(23, vm.Position1);
            Assert.AreEqual(101, vm.Position2);
        }


        [TestMethod]
        public void Stop_ShouldStopSimulation()
        {
            // Arrange
            var vm = new MainViewModel();
            vm.Start(2, 3, 1, 1, 20, 100);

            // Act
            vm.Stop();

            // Assert
            Assert.IsFalse(vm.IsRunning);
        }


        [TestMethod]
        public void Step_ShouldDetectCollision()
        {
            // Arrange
            var vm = new MainViewModel();
            vm.Start(2, 3, 1, 1, 0, 10);

            // Act
            vm.Step(2, 5);
            vm.Step(2, 5);

            // Assert
            Assert.IsTrue(vm.IsCollided);
            Assert.AreEqual(vm.CommonVelocity, vm.Velocity1, 0.000001);
            Assert.AreEqual(vm.CommonVelocity, vm.Velocity2, 0.000001);
        }


        [TestMethod]
        public void Start_ShouldCalculatePhysicsChecks()
        {
            var vm = new MainViewModel();

            // Запускаем эксперимент
            vm.Start(2, 4, 2, 0, 20, 100);

            // Проверяем, что до столкновения проверки ещё не выполнены
            Assert.IsFalse(vm.MomentumIsCorrect);
            Assert.IsFalse(vm.EnergyLossIsConfirmed);

            // Выполняем шаг, достаточный для столкновения
            vm.Step(5, 60);

            // Проверяем результаты после столкновения
            Assert.IsTrue(vm.IsCollided);
            Assert.IsTrue(vm.MomentumIsCorrect);
            Assert.IsTrue(vm.EnergyLossIsConfirmed);
        }


    }
}
