using System;
using System.Collections.Generic;
using System.IO;
using CommandLine;
using LibGit2Sharp;
using Microsoft.Toolkit.Parsers.Markdown;
using Microsoft.Toolkit.Parsers.Markdown.Blocks;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;

namespace ThankYou
{
    class Program
    {
        private static TwitchClient _client;
        private static List<string> _contributorsToday = new List<string>();

        static void Main(string[] args)
        {
            var twitchUserName = ""; 
            var accessToken = ""; 
            var channelName = ""; 
            var repositoryUserName = "";
            var repositoryPassword = "";

            var parsedArguments = Parser.Default.ParseArguments<Options>(args);
            parsedArguments.WithParsed(options => {
                accessToken = options.accessToken;
                channelName = options.channelName;
                twitchUserName = options.twitchUserName;

                repositoryUserName = options.repoUserName;
                repositoryPassword = options.repoPassword;

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
            _client = new TwitchClient(customClient);
            _client.Initialize(credentials, channelName);
            _client.OnMessageReceived += Client_OnMessageReceived;
            _client.OnLog += Client_OnLog;
            _client.Connect();

            Console.ReadKey(true);

            WriteContributorsToRepo(repositoryUserName, repositoryPassword);
        }

        private static void WriteContributorsToRepo(string username, string password)
        {
            //TODO: please move to be taken from a commandline from the chat stream
            var repoUrl = "https://github.com/eashi/thankyou";
            var contributorsHeader = "Acknowledgement"; //TODO: this should be a configuration
            var fileHoldingContributorsInfo = "readme.md"; //TODO: this should be a configuration

            var cloneOptions = new CloneOptions();
            cloneOptions.CredentialsProvider = (_url, _user, _cred) => new UsernamePasswordCredentials { Username = username, Password = password };
            var tempPath = Path.GetTempPath();
            var tempPathGitFolder = Path.Combine(tempPath, "Jaan"); //TODO: remove "Jaan" and create random folder?
            if(!Directory.Exists(tempPathGitFolder))
            {
                var directoryInfo = Directory.CreateDirectory(tempPathGitFolder);
            }
            Repository.Clone(repoUrl, tempPathGitFolder, cloneOptions); //TODO: if the repo clone is already here, should we delete and reclone? or should we assume correct repo here.

            var pathToReadme = Path.Combine(tempPathGitFolder, fileHoldingContributorsInfo);
            var readMeContent = File.ReadAllText(pathToReadme);

            var readMeFileParsed = new MarkdownDocument();
            readMeFileParsed.Parse(readMeContent);

            //find the "CONTRIBUTORS" section. The title of the section should be a configuration
            for(int index = 0; index <= readMeFileParsed.Blocks.Count-1;  index++)
            {
                Console.WriteLine(readMeFileParsed.Blocks[index].ToString());
                if(readMeFileParsed.Blocks[index] is HeaderBlock &&  readMeFileParsed.Blocks[index].ToString().Trim() == contributorsHeader)
                {
                    for(int index2=index+1; index2 <= readMeFileParsed.Blocks.Count-1; index2++)
                    {
                        if (readMeFileParsed.Blocks[index2] is ListBlock)
                        {
                            Console.WriteLine($"Here is your friends: {readMeFileParsed.Blocks[index2].ToString()}");
                            break;
                        }
                    }
                }
            }

            //TOOD: change the file and save it

            //TODO: commit the file to the repo, on a non-master branch

            //TODO: push the commit to origin
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
            if (messageTokens.Length > 1 && messageTokens[0] == "!thanks" && e.ChatMessage.IsModerator)
            {
                if (messageTokens.Length == 2)
                {
                    _contributorsToday.Add(messageTokens[1]);
                }
                else 
                {
                    _client.SendWhisper(author, "There should be one argument for !thanks");
                }
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
        
        [Option(Required = true)]
        public string repoUserName { get; internal set; }
        
        [Option(Required = true)]
        public string repoPassword { get; internal set; }
    }
}
