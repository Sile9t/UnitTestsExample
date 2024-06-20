using EFDatabase;
using EFDatabase.Abstracts;
using EFDatabase.Services;
using System.Net;

namespace ServerTest
{
    public class MockMessageSource : IMessageSource
    {
        private Queue<NetMessage> _messages = new();
        private UDPServer _server;
        private Client _client;
        private IPEndPoint _endPoint = new IPEndPoint(IPAddress.Any, 0);
        public MockMessageSource()
        {
            _messages.Enqueue(new NetMessage() { Command = Command.Register, From = "Alex"});
            _messages.Enqueue(new NetMessage() { Command = Command.Register, From = "Anna"});
            _messages.Enqueue(new NetMessage() { Command = Command.Message, From = "Alex",
                To = "Anna", Text = "Hello, it's Alex"});
            _messages.Enqueue(new NetMessage() { Command = Command.Message, From = "Anna",
                To = "Alex", Text = "Glab to see you"});
        }
        public NetMessage Receive(ref IPEndPoint endPoint)
        {
            endPoint = _endPoint;
            if (_messages.Count == 0)
            {
                _server.Stop();
                _client.Stop();
                return null;
            }
            return _messages.Dequeue();
        }

        public async Task SendAsync(NetMessage msg, IPEndPoint endPoint)
        {
            await Task.Delay(1000);
        }
        public void AddServer(UDPServer server)
        { 
            _server = server;
        }
        public void AddClient(Client client) => _client = client;
    }
}
