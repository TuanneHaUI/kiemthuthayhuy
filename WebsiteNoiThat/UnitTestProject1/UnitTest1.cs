using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using WebsiteNoiThat.Controllers;
using System.Web;
using System.Web.Routing;
using System.Web.Mvc;
using System.Collections.Generic;
using WebsiteNoiThat.Models;
using Newtonsoft.Json.Linq;
using Models.DAO;
using Models.EF;
using System.Linq;
using System.Data.Entity;
using System.Net.Sockets;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {

        private CartController _controller;
        private Mock<HttpSessionStateBase> _sessionMock;
        private Mock<HttpContextBase> _httpContextMock;
        private Mock<DbSet<Product>> _mockProduct;

        [TestInitialize]
        public void TestInitialize()
        {
            // Gia lap HttpSessionStateBase va HttpContextBase
            _mockProduct = new Mock<DbSet<Product>>();
            _sessionMock = new Mock<HttpSessionStateBase>();
            _httpContextMock = new Mock<HttpContextBase>();
            _httpContextMock.Setup(ctx => ctx.Session).Returns(_sessionMock.Object);
            _controller = new CartController();
            _controller = new CartController
            {
                ControllerContext = new ControllerContext(_httpContextMock.Object, new RouteData(), _controller)
            };
        }

        // cap nhat gio hang chi model gio hang hop le thi se cap nhat so luong san pham trong gio hang do
        [TestMethod]
        public void Update_WhencartModelIsValid_UpdatesQuantities()
        {
            // Arrange: Chuan bi du lieu gia
            var cartModel = "[{\"Product\":{\"ProductId\":1},\"Quantity\":3}]";
            var sessionCart = new List<CartItem>
            {
                new CartItem { Product = new Models.EF.Product { ProductId = 1 }, Quantity = 1 }
            };

            // Gia lap session
            _sessionMock.Setup(s => s["CartSession"]).Returns(sessionCart);

            // Act: Goi phuong thuc update
            var result = _controller.Update(cartModel) as JsonResult;
            var data = JObject.FromObject(result.Data);

            // Assert: Kiem tra ket qua
            Assert.IsNotNull(result); // ket qua tra ve cua ham khong phai null

            Assert.AreEqual(true, (bool)data["status"]); // ket qua tra ve cua ham voi status la true
           
            Assert.AreEqual(3, sessionCart[0].Quantity);  // so luong tra ve tu session la 3


        }

        [TestMethod]
        public void Update_WhenCartModelIsInvalid_DoesNotUpdateQuantities()
        {
            var cartModel = "[{\"Product\":{\"ProductId\":2},\"Quantity\":5}]";
            var sessionCart = new List<CartItem>
            {
                new CartItem { Product = new Models.EF.Product { ProductId =  1 }, Quantity = 1 }
            };

            // Gia lap session
            _sessionMock.Setup(s => s["CartSession"]).Returns(() => sessionCart);

            // Act: Goi phuong thuc Update
            var result = _controller.Update(cartModel) as JsonResult;
            var data = JObject.FromObject(result.Data);

            // Assert: Kiem tra ket qua
            Assert.IsNotNull(result);
            Assert.AreEqual(true , (bool)data["status"]); // dam bao trang thai sau ham update la true
            Assert.AreEqual(1, sessionCart[0].Quantity);    // dam bao so luong san pham khong bi thay doi khi mocart model khong hop le
        }

        [TestMethod]
        public void AddCart_ProductExistsInCart_IncreasesQuantity()
        {
            // Arrange: Chuẩn bị một session giỏ hàng chứa sản phẩm hiện tại
            var productId = 1;
            var quantityToAdd = 2;
            var existingCart = new List<CartItem>
        {
            new CartItem { Product = new Product { ProductId = productId, Name = "Product 1" }, Quantity = 1 }
        };
            _sessionMock.Setup(s => s["CartSession"]).Returns(existingCart);

            // Mock ProductDao trả về Product
            var productDaoMock = new Mock<ProductDao>();
            productDaoMock.Setup(dao => dao.ViewDetail(productId)).Returns(new Product { ProductId = productId });

            // Act: Thực hiện thêm vào giỏ hàng
            var result = _controller.AddCart(productId, quantityToAdd) as RedirectToRouteResult;

            // Assert: Đảm bảo số lượng trong giỏ hàng tăng lên
            Assert.IsNotNull(result);
            var updatedCart = _sessionMock.Object["CartSession"] as List<CartItem>;
            Assert.AreEqual(1, updatedCart.Count); // Không thêm sản phẩm mới, chỉ cập nhật sản phẩm cũ
            Assert.AreEqual(3, updatedCart[0].Quantity); // Số lượng tăng đúng
        }

    }
}
