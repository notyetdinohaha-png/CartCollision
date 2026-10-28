
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CartCollision;

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
    }
}
