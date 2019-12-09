using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BigBubble.Abstractions;
using BigBubble.Models;
using Microsoft.Extensions.Configuration;

namespace BigBubble.App
{
    public class BigBubbleClient : IDisposable
    {
        private readonly Publisher _publisher;
        private readonly Consumer _consumer;
        private readonly IUsernameProvider _usernameProvider;
        private readonly IRmqConnectionFactory _connectionFactory;


        StringBuilder currentLine = new StringBuilder();

        private bool _shouldExit = false;

        private Dictionary<string, ConsoleColor> Colors = new Dictionary<string, ConsoleColor>()
        {
            { KnownTypes.join.ToString(), ConsoleColor.Cyan },
            { KnownTypes.leave.ToString(), ConsoleColor.Red },
            { KnownTypes.publish.ToString(), ConsoleColor.Green },
        };


        public BigBubbleClient(Publisher publisher, Consumer consumer, IUsernameProvider usernameProvider, IConfiguration config, IRmqConnectionFactory connectionFactory)
        {
            _publisher = publisher;
            _consumer = consumer;

            _consumer.MessageReceived += Consumer_MessageReceived;

            _usernameProvider = usernameProvider;
            _connectionFactory = connectionFactory;
        }

        private void Consumer_MessageReceived(object sender, MessageReceivedModel message)
        {
            if (message == null)
            {
                return;
            }

            var time = DateTimeOffset.FromUnixTimeSeconds(message.Received).DateTime.ToLocalTime().TimeOfDay;

            Enum.TryParse<KnownTypes>(message.Type, out var messageType);

            var output = messageType switch
            {
                KnownTypes.join => $"User [{message.Nickname}] entered the party",
                KnownTypes.leave => $"User [{message.Nickname}] propably has something better to do.",
                KnownTypes.publish => $"{message.Nickname} [{time.ToString()}]: {message.Message}",
                _ => $"",
            };

            if (string.IsNullOrWhiteSpace(output))
            {
                return;
            }

            WriteToConsole(output, TryGetColor(messageType.ToString(), message.Nickname));
        }

        private ConsoleColor TryGetColor(string key, string username)
        {
            if (Colors.ContainsKey(username))
            {
                return Colors[username];
            }

            if (Colors.TryGetValue(key, out var color))
            {
                return color;
            }

            return ConsoleColor.White;
        }

        private void WriteToConsole(string output, ConsoleColor color)
        {
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new String(' ', Console.BufferWidth));
            Console.SetCursorPosition(0, Console.CursorTop);

            Console.ForegroundColor = color;
            Console.WriteLine(output);
            Console.ResetColor();
            Console.Write(currentLine);
        }

        public void InitChat(CancellationToken stoppingToken)
        {
            PrintImage();
            Console.WriteLine("Please enter your username:");
            var username = Console.ReadLine();
            _usernameProvider.SetUsername(username);
            Console.WriteLine("You are now connected");

            _consumer.InitConsume();
            JoinChat();

            while (!_shouldExit || stoppingToken.IsCancellationRequested)
            {
                currentLine = new StringBuilder();
                var currentKey = default(System.ConsoleKeyInfo);
                while (currentKey.Key != ConsoleKey.Enter)
                {
                    currentKey = Console.ReadKey();
                    currentLine.Append(currentKey.KeyChar);

                }
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write(new String(' ', Console.BufferWidth));
                Console.SetCursorPosition(0, Console.CursorTop);
                CheckMessage(currentLine.ToString().Replace("\r", ""));
            }

            Console.WriteLine("Service Stoped");
            Dispose();
        }

        private void CheckMessage(string message)
        {
            if (message.StartsWith("@exit"))
            {
                _shouldExit = true;
                LeaveChat($"{_usernameProvider.GetUsername()} waves goodbye");
            }
            else if (message.StartsWith("@user"))
            {
                var oldUser = _usernameProvider.GetUsername();
                if (Colors.ContainsKey(oldUser))
                {
                    Colors.Remove(oldUser);
                }
                var newUsername = message.Replace("@user", "").Trim();

                Colors.Add(newUsername, ConsoleColor.Cyan);
                _usernameProvider.SetUsername(newUsername);
                JoinChat($"Ahoy! {oldUser} changed name to {newUsername}.");
            }
            else
            {
                MessageChat(message);
            }
        }

        public void JoinChat(string message = "")
        {
            Colors.TryAdd(_usernameProvider.GetUsername(), ConsoleColor.Cyan);
            var joinMessage = new JoinModel() { Nickname = _usernameProvider.GetUsername() };
            _publisher.PublishMessage(joinMessage);
            if (string.IsNullOrWhiteSpace(message))
            {
                MessageChat(message);
            }

        }

        public void LeaveChat(string message = "")
        {
            var leaveMessage = new LeaveModel() { Nickname = _usernameProvider.GetUsername() };
            _publisher.PublishMessage(leaveMessage);
            if (string.IsNullOrWhiteSpace(message))
            {
                MessageChat(message);
            }
        }

        private void MessageChat(string messageStr)
        {
            if (string.IsNullOrWhiteSpace(messageStr))
            {
                return;
            }

            var message = new MessageModel() { Nickname = _usernameProvider.GetUsername(), Message = messageStr };
            _publisher.PublishMessage(message);
        }

        private void PrintImage()
        {
            Console.ForegroundColor = ConsoleColor.Magenta;



            Console.WriteLine(@"  ____ _____ _____                                        
 |  _ \_   _/ ____|                                       
 | |_) || || |  __                                        
 |  _ < | || | |_ |                                          
 | |_) || || |__| |                                       
 |____/_____\_____|  ____  _      ______                  ");

            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine(@" |  _ \| |  | |  _ \|  _ \| |    |  ____|                 
 | |_) | |  | | |_) | |_) | |    | |__                    
 |  _ <| |  | |  _ <|  _ <| |    |  __|                   
 | |_) | |__| | |_) | |_) | |____| |____                  
 |____/_\____/|____/|____/|______|______|__ ______ ______ "); Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(@" |______|______|______|______|______|______|______|______|
 |______|______|______|______|______|______|______|______|"); Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(@" |  ____|  __ \_   _/ ____|      |  __ \|  \/  |/ __ \    
 | |__  | |__) || || |           | |__) | \  / | |  | |   
 |  __| |  ___/ | || |           |  _  /| |\/| | |  | |   
 | |____| |    _| || |____       | | \ \| |  | | |__| |   
 |______|_|   |_____\_____|__    |_|  \_\_|  |_|\___\_\   
  / ____| |  | |   /\|__   __|                            
 | |    | |__| |  /  \  | |                               
 | |    |  __  | / /\ \ | |                               
 | |____| |  | |/ ____ \| |                               
  \_____|_|  |_/_/___ \_\_|_ _   _ _______                
  / ____| |    |_   _|  ____| \ | |__   __|               
 | |    | |      | | | |__  |  \| |  | |                  
 | |    | |      | | |  __| | . ` |  | |                  
 | |____| |____ _| |_| |____| |\  |  | |                  
  \_____|______|_____|______|_| \_|  |_|                  
                                                          
                                                          

");
            Console.ResetColor();
        }

        public void Dispose()
        {
            _connectionFactory.Dispose();
        }
    }
}
