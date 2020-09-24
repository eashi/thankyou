using System;
using System.Collections.Generic;
using CommandLine;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;

namespace ThankYou
{
    class Program
    {
        private static List<string> _contributorsToday = new List<string>();

        static void Main(string[] args)
        {
            //TODO: Connect to the Twitch channel
            var twitchUserName = ""; 
            var accessToken = ""; 
            var channelName = ""; 

            var parsedArguments = Parser.Default.ParseArguments<Options>(args);
            parsedArguments.WithParsed(options => {
                accessToken = options.accessToken;
                channelName = options.channelName;
                twitchUserName = options.twitchUserName;

                if (string.IsNullOrEmpty(twitchUserName))
                {
                    twitchUserName = channelName;        
                }
            });

            ConnectionCredentials credentials = new ConnectionCredentials(twitchUserName, accessToken);
            var clientOptions = new ClientOptions
            {
                MessagesAllowedInPeriod = 750,
                ThrottlingPeriod = TimeSpan.FromSeconds(30),
            };

            WebSocketClient customClient = new WebSocketClient(clientOptions);
            var client = new TwitchClient(customClient);
            client.Initialize(credentials, channelName);
            client.OnMessageReceived += Client_OnMessageReceived;
            client.OnLog += Client_OnLog;
            client.Connect();

            Console.ReadKey(true);

            foreach (var contributor in _contributorsToday)
            {
                Console.WriteLine(contributor);
            }
        }

        private static void Client_OnLog(object sender, OnLogArgs e)
        {
            Console.WriteLine($"Log {e.DateTime}: botname: {e.BotUsername}, Data: {e.Data}");
        }

        private static void Client_OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            var input = e.ChatMessage.Message;
            var author = e.ChatMessage.Username;

            var messageTokens = e.ChatMessage.Message.Split(' ');
            if (messageTokens.Length > 1 && messageTokens[0] == "!thanks")
            {
                if (messageTokens.Length > 2)
                {
                    //TODO: respond to the channel and say there should be only one argument to !thanks
                }
                _contributorsToday.Add(messageTokens[1]);
            }
            Console.WriteLine($"{author} wrote this: {input}");
        }
    }

    internal class Options
    {
        [Option]
        public string twitchUserName {get; set;}

        [Option(Required=true)]
        public string channelName {get; set;}

        [Option(Required=true)]
        public string accessToken {get; set;}
    }
}
