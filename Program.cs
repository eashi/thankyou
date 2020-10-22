using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;
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
            parsedArguments.WithParsed(options =>
            {
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
            var gitAuthorName = "Emad Alashi"; //TODO: move as a parameter to the bot
            var gitAuthorEmail = "emad.ashi@gmail.com"; //TODO: move as a parameter to the bot

            var nameOfThankyouBranch = "thankyou";
            //TODO: please move to be taken from a commandline from the chat stream
            var repoUrl = "https://github.com/eashi/thankyou";
            var contributorsHeader = "Acknowledgement"; //TODO: this should be a configuration
            var fileHoldingContributorsInfo = "readme.md"; //TODO: this should be a configuration

            var cloneOptions = new CloneOptions();
            var gitCredentialsHandler = new CredentialsHandler(
                    (url, usernameFromUrl, types) =>
                        new UsernamePasswordCredentials()
                        {
                            Username = username,
                            Password = password
                        });
            cloneOptions.CredentialsProvider = gitCredentialsHandler;
            var tempPath = Path.GetTempPath();
            var tempPathForRepo = Path.Combine(tempPath, "Jaan");
            if (Directory.Exists(tempPathForRepo))
            {
                Directory.Delete(tempPathForRepo, true); //TODO: Don't remove and re-clone, clone only if doesn't exist
            }
            var tempPathGitFolder = Path.Combine(tempPath, "Jaan");
            if (!Directory.Exists(tempPathGitFolder))
            {
                var directoryInfo = Directory.CreateDirectory(tempPathGitFolder);
            }
            Repository.Clone(repoUrl, tempPathGitFolder, cloneOptions); //TODO: if the repo clone is already here, should we delete and reclone? or should we assume correct repo here.
            using (var repo = new Repository(tempPathGitFolder))
            {
                var remote = repo.Network.Remotes["origin"];
                var refSpecs = remote.FetchRefSpecs.Select(x => x.Specification);
                
                var remoteThankYouBranch = repo.Branches.FirstOrDefault(b => b.FriendlyName == repo.Network.Remotes.FirstOrDefault()?.Name + "/" + nameOfThankyouBranch);

                if (remoteThankYouBranch != null) {
                    Commands.Checkout(repo, remoteThankYouBranch);
                }

                if (repo.Head.FriendlyName != nameOfThankyouBranch)
                {
                    var newThankyouBranch = repo.CreateBranch(nameOfThankyouBranch);
                    repo.Branches.Update(newThankyouBranch,
                      b => b.UpstreamBranch = newThankyouBranch.CanonicalName,
                      b => b.Remote = repo.Network.Remotes.First().Name);
                    Commands.Checkout(repo, newThankyouBranch);
                }


                var pathToReadme = Path.Combine(tempPathGitFolder, fileHoldingContributorsInfo);
                // Change the file and save it
                using (StreamWriter sw = File.AppendText(pathToReadme))
                {
                    foreach (var contributor in _contributorsToday)
                    {
                        sw.WriteLine(contributor);
                    }
                }

                // Commit the file to the repo, on a non-master branch
                repo.Index.Add(fileHoldingContributorsInfo);
                repo.Index.Write();

                // Create the committer's signature and commit
                Signature author = new Signature(gitAuthorName, gitAuthorEmail, DateTime.Now);
                Signature committer = author;

                // Commit to the repository
                Commit commit = repo.Commit("A new list of awesome contributors", author, committer);

                //Push the commit to origin
                LibGit2Sharp.PushOptions options = new LibGit2Sharp.PushOptions();
                options.CredentialsProvider = gitCredentialsHandler;
                repo.Network.Push(repo.Head, options);

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
                if (e.ChatMessage.IsModerator)
                {
                    if (messageTokens.Length == 2)
                    {
                        Console.WriteLine($"Adding '{messageTokens[1]}' to the contributors list");
                        _contributorsToday.Add(messageTokens[1]);
                    }
                    else
                    {
                        _client.SendWhisper(author, "There should be one argument for !thanks");
                    }
                }
                else
                {
                    Console.WriteLine("Sorry, you're not a moderator");
                }

            }
            Console.WriteLine($"{author} wrote this: {input}");
        }
    }

    internal class Options
    {
        [Option]
        public string twitchUserName { get; set; }

        [Option(Required = true)]
        public string channelName { get; set; }

        [Option(Required = true)]
        public string accessToken { get; set; }

        [Option(Required = true)]
        public string repoUserName { get; internal set; }

        [Option(Required = true)]
        public string repoPassword { get; internal set; }
    }
}
