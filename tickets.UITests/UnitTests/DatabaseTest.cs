using NUnit.Framework;
using System;
using System.IO;
using System.ComponentModel;
using System.Threading.Tasks;

namespace tickets.UITests.UnitTests
{

    [TestFixture()]
    public class DatabaseTest
    {

        Database db = new Database("Test");

        [Test()]
        public async Task FirstTimeuserIsNull()
        {
            await db.GetConnection().DropTableAsync<User>();
            await db.GetConnection().CreateTableAsync<User>();
            User user = await db.GetCurrentUserAsync();

            Assert.IsNull(user);
        }

        [Test()]
        public async Task CanCreateNewUser()
        {
            await db.GetConnection().DropTableAsync<User>();
            await db.GetConnection().CreateTableAsync<User>();
            User user = new User()
            {
                ID = 0,
                Name = "PRUEBA",
                Email = "prueba@unitec.edu",
                Campus = "CAMPUS1",
                Profile = "PERFIL1",
                Account = "20202020",
                Career = "ING. EN PRUEBAS",
                PhoneNumber = "99990000",
                IsCurrent = true
            };
            await db.CreateNewCurrentUser(user);

            User user2 = new User()
            {
                ID = 0,
                Name = "NUEVO",
                Email = "prueba@unitec.edu",
                Campus = "CAMPUS2",
                Profile = "PERFIL2",
                Account = "20202020",
                Career = "ING. EN PRUEBAS",
                PhoneNumber = "99990000",
                IsCurrent = true
            };

            await db.CreateNewCurrentUser(user2);

            User current = await db.GetCurrentUserAsync();
            Assert.IsTrue(current.Name.Equals("NUEVO"));
        }

        [Test()]
        public async Task CanUpdateCurrentUser()
        {
            await db.GetConnection().DropTableAsync<User>();
            await db.GetConnection().CreateTableAsync<User>();

            User user = new User()
            {
                ID = 0,
                Name = "PRUEBA",
                Email = "prueba@unitec.edu",
                Campus = "CAMPUS1",
                Profile = "PERFIL1",
                Account = "20202020",
                Career = "ING. EN PRUEBAS",
                PhoneNumber = "99990000",
                IsCurrent = true
            };
            await db.CreateNewCurrentUser(user);


            User current = await db.GetCurrentUserAsync();

            current.Name = "ACTUALIZADO";

            await db.SaveUserAsync(current);

            User nuevoCurrent = await db.GetCurrentUserAsync();

            Assert.IsTrue(nuevoCurrent.Name.Equals("ACTUALIZADO"));

        }
    }
}
