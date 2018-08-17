using NUnit.Framework;
using System;
using System.IO;

namespace tickets.UITests.UnitTests
{

    [TestFixture()]
    public class DatabaseTest
    {

        Database db = new Database(
                      Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TicketsAppTest.db3"));

        [Test()]
        public void CurrentUserIsNull()
        {
            User user = db.GetCurrentUser().Result;

            Assert.IsNull(user);
        }

        [Test()]
        public void CanCreateFirstTimeUser()
        {
            User user = new User()
            {
                ID = 0,
                Name = "",
                Email = "",
                Campus = "",
                Profile = "",
                Account = "",
                Career = "",
                PhoneNumber = "",
                IsCurrent = true
            };
        }
    }
}
