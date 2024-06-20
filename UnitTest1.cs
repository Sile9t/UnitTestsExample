using EFDatabase;
using EFDatabase.Services;

namespace ServerTest
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class Tests : PageTest
    {
        [SetUp]
        public void Setup()
        {
            using (ChatContext context = new ChatContext()) 
            {
                context.Messages.RemoveRange(context.Messages);
                context.Users.RemoveRange(context.Users);
                context.SaveChanges();
            }
        }
        [TearDown]
        public void TearDown()
        {
            using (ChatContext context = new ChatContext())
            {
                context.Messages.RemoveRange(context.Messages);
                context.Users.RemoveRange(context.Users);
                context.SaveChanges();
            }
        }
        [Test]
        public async Task ServerTest()
        {
            var mock = new MockMessageSource();
            var server = new UDPServer(mock);
            mock.AddServer(server);
            await server.Listen();
            using (var context = new ChatContext())
            {
                Assert.That(context.Users.Count() == 2, "Users not created");

                var user1 = context.Users.FirstOrDefault(x => x.FullName == "Alex");
                var user2 = context.Users.FirstOrDefault(x => x.FullName == "Anna");

                Assert.That(user1 != null, "User1 not found");
                Assert.That(user2 != null, "User2 not found");

                Assert.That(user1.MessagesFrom?.Count == 1);
                Assert.That(user2.MessagesFrom?.Count == 1);

                Assert.That(user1.MessagesTo?.Count == 1);
                Assert.That(user2.MessagesTo?.Count == 1);

                var msg1 = context.Messages.FirstOrDefault(x => x.From == user1 && x.To == user2);
                var msg2 = context.Messages.FirstOrDefault(x => x.From == user2 && x.To == user1);

                Assert.That("Hello, it's Alex" == msg1.Text);
                Assert.That("Glab to see you" == msg2.Text);
            }
        }
        [Test]
        public async Task ClientTest()
        {
            var mock = new MockMessageSource();
            var client = new Client("Alex", mock, "127.0.0.1", 12345);
            mock.AddClient(client);
            //var server = new UDPServer(mock);
            //mock.AddServer(server);
            ////await server.Listen();
            await client.Start();
            using (var context = new ChatContext())
            {
                Assert.That(context.Users.Count() == 0, "Users not created");

                var user = context.Users.FirstOrDefault(x => x.FullName == "Alex");

                Assert.That(user != null, "User not found");

                Assert.That(user.MessagesFrom?.Count > 0, "Messages from user not found");

                Assert.That(user.MessagesTo?.Count > 0, "Messages to user not found");
                
                var msg = context.Messages.FirstOrDefault(x => x.From == user && x.To.FullName == "Anna");

                Assert.That(msg.Text == "Hello, it's Alex", "Hello message from Alex not found");
            }
        }
    }
}
