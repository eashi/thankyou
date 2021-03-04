# thankyou

"thankyou" is Twitch chat bot that listens to a channel, and captures commands from the streamer to credit guests on the channel who contribute with their suggestions. Eventually it will create a commit/PR to the project's code repository.

This project itself is built live on [Twitch](https://twitch.tv/emadashi), we stream every Thursday at 20:00 Melbourne Australia time. Make sure to join us! :)

## How it Works

Once the bot is started, it will connecto to the Twitch channel and starts listening to the chat. 

If the streamer wants to give credit to someone in the chat for their help, the streamer or the moderator would send the following message on the chat channel:

```
!thanks [name of contributor] [github/twitch]
```
Example:
```
!thanks @tkoster github
```

Once the bot receives this message, it will register `tkoster` and will generate a link to `https://github.com/tkoster` stored in memory. 

Once the streamer is ready about to finish the stream, they will send the command `!bye`, which will signal to the bot to wrap up.
Then the bot will find the designated part of the markdown file, and will insert the link in that area.

### The Designated Part in Markdown

How does the bot know which part of the Markdown file it needs to put the aknolwedgement in? Whereever the streamer wants really! 

All what the streamer needs to do is to insert the following comment in the Markdown file, and the bot will find it and will insert the records there.

```
[//]: # (ThankYouBlockStart)
[//]: # "ThankYouTemplate: - [@name](@serviceUrl/@name)"

[//]: # (ThankYouBlockEnd)
```

## Running The Bot

The bot is just a simple console application. It needs the following parameters to run successfully

- --twitchusername
- --channelname
- --accesstoken
- --repousername
- --repopassword
- --gitauthorname
- --gitauthoremail
- --githubrepoowner
- --githubreponame

### From Command Line

### From Container
If you build the docker image from the Docker file in the solution with the name `thankyou:manual`, then you can use the following command to run the container. Don't forget to set the right values to the environment variables!
```
docker run thankyou:manual --twitchusername $twitchUserName --channelname $twitchChannelName --accesstoken $twitchAccessToken --repousername $gitUserName --repopassword $githubAccessToken --gitauthorname $gitAuthorName --gitauthoremail $gitAuthorEmail --githubrepoowner $githubRepoOwner --githubreponame $githubRepoName --fileinrepoforacknowledgement README.md
```

## Contributing


### Prerequisites

You will need the .NET Core 3.1 SDK or later. https://dotnet.microsoft.com/download/dotnet-core

### Build and test

After cloning the source code, run `dotnet test` in the repository directory to compile the program and run the test suite.

## Acknowledgement

This work wouldn't have been the same without the awesome contributors on the Twitch channel. Here is a list of these awesome people:

[//]: # (ThankYouBlockStart)
[//]: # "ThankYouTemplate: - [@name](@serviceUrl/@name)"
- [melbshaker](https://twitch.tv/melbshaker)
- [JaanEnn](https://twitch.tv/jaanenn)
- [Vote_Anarchy](https://twitch.tv/vote_anarchy)
- [CodeAndCoffee](https://github.com/tkoster)
- [yawnston](https://github.com/yawnston)
- [Mitch Denny](https://github.com/mitchdenny)
- [David Wengier](https://github.com/davidwengier)
- [jonp333](https://twitch.tv/jonp333)
- [ColloquialOwl](https://twitch.tv/ColloquialOwl)
 - [jsobell](https://github.com/jsobell)
 - [yashints](https://twitch.com/yashints)
 - [tkoster](https://github.com/tkoster)
[//]: # (ThankYouBlockEnd)
