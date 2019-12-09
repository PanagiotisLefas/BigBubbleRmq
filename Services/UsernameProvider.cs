using BigBubble.Abstractions;

namespace BigBubble.Services
{
    public class UsernameProvider : IUsernameProvider
    {
        private string _username = "";
        public string GetUsername()
        {
            return _username;
        }

        public void SetUsername(string username)
        {
            _username = username;
        }
    }

}
