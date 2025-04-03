using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestsUnitairesPourServices.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TestsUnitairesPourServices.Data;
using TestsUnitairesPourServices.Models;
using Microsoft.Extensions.FileSystemGlobbing.Internal.PathSegments;
using TestsUnitairesPourServices.Exceptions;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Options;

namespace TestsUnitairesPourServices.Services.Tests
{
    [TestClass()]
    public class CatsServiceTests
    {
        private DbContextOptions<ApplicationDBContext> _options;
        public CatsServiceTests()
        {
            _options = new DbContextOptionsBuilder<ApplicationDBContext>()
                                                                          .UseInMemoryDatabase(databaseName: "CatsService")
                                                                          .UseLazyLoadingProxies(true)
                                                                          .Options;
        }
        [TestInitialize]
        public void Init()
        {
            using ApplicationDBContext db = new ApplicationDBContext(_options);

            House maMaison = new House()
            {
                Id = 1,
                Address = "MaMaison",
                OwnerName = "Richard"
            };
            House pasMaMaison = new House()
            {
                Id = 2,
                Address = "pasMaMaison",
                OwnerName = "PasRichard"
            };

            db.House.Add(maMaison);
            db.House.Add(pasMaMaison);


            Cat GrosChat = new Cat()
            {
                Id = 1,
                Name = "GrosChat",
                Age = 69,
                House = maMaison

            };
            Cat chatObese = new Cat()
            {
                Id = 2,
                Name = "Bouboule",
                Age = 1
            };
            db.Cat.Add(GrosChat);
            db.Cat.Add(chatObese);
            db.SaveChanges();
        }
        [TestCleanup]
        public void Dispose()
        {
            using ApplicationDBContext db = new ApplicationDBContext(_options);
            db.Cat.RemoveRange(db.Cat);
            db.House.RemoveRange(db.House);
            db.SaveChanges();
        }

        [TestMethod()]
        public void MoveTest()
        {
            using ApplicationDBContext db = new ApplicationDBContext(_options);
            CatsService service = new CatsService(db);
            var maMaison = db.House.Find(1)!;
            var pasmaMaison = db.House.Find(2)!;
            var grosChat = service.Move(1, maMaison, pasmaMaison);
            Assert.IsNotNull(grosChat);
        }
        [TestMethod()]
        public void MoveTestCatNotFound()
        {
            using ApplicationDBContext db = new ApplicationDBContext(_options);
            CatsService service = new CatsService(db);
            var maMaison = db.House.Find(1)!;
            var pasmaMaison = db.House.Find(2)!;
            var grosChat = service.Move(242, maMaison, pasmaMaison);
            Assert.IsNull(grosChat);
        }
        [TestMethod()]
        public void MoveTestCatHomeless()
        {
            using ApplicationDBContext db = new ApplicationDBContext(_options);
            CatsService service = new CatsService(db);
            var maMaison = db.House.Find(1)!;
            var pasmaMaison = db.House.Find(2)!;

            Exception e = Assert.ThrowsException<WildCatException>(() => service.Move(1, maMaison, pasmaMaison));
            Assert.AreEqual("On n'apprivoise pas les chats sauvages", e.Message);
        }
        [TestMethod()]
        public void MoveTestStolenCat()
        {
            using ApplicationDBContext db = new ApplicationDBContext(_options);
            var catsService = new CatsService(db);
            var maMaison = db.House.Find(1)!;
            var pasMaMaison = db.House.Find(2)!;

            // Les maisons sont inversées
            Exception e = Assert.ThrowsException<DontStealMyCatException>(() => catsService.Move(2, maMaison, pasMaMaison));
            Assert.AreEqual("Touche pas à mon chat!", e.Message);
        }
    }
}