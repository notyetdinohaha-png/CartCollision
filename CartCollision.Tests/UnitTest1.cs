
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CartCollision;
using CartCollision.Core;

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

    }

}
