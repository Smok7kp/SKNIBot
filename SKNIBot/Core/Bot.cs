﻿using System;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using SKNIBot.Core.Commands.YouTubeCommands;
using SKNIBot.Core.Settings;
using SKNIBot.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using SKNIBot.Core.Services.RolesService;

namespace SKNIBot.Core
{
    public class Bot
    {
        public static DiscordClient DiscordClient { get; set; }
        private CommandsNextExtension _commands { get; set; }

        public void Run()
        {
            Connect();
            SetNetworkParameters();
            RegisterCommands();
        }

        private async void Connect()
        {
            var connectionConfig = new DiscordConfiguration
            {
                Token = SettingsLoader.SettingsContainer.Token,
                TokenType = TokenType.Bot,

                AutoReconnect = true,
                LogLevel = LogLevel.Debug,
                UseInternalLogHandler = true,
                
            };

            DiscordClient = new DiscordClient(connectionConfig);

            var commandsConfig = new CommandsNextConfiguration
            {
                StringPrefixes = new [] {SettingsLoader.SettingsContainer.Prefix},
                EnableDms = false,
                EnableMentionPrefix = true,
                CaseSensitive = false,
                IgnoreExtraArguments = true,
                Services = BuildDependencies()
            };

            _commands = DiscordClient.UseCommandsNext(commandsConfig);
            _commands.SetHelpFormatter<CustomHelpFormatter>();
            _commands.CommandExecuted += Commands_CommandExecuted;
            _commands.CommandErrored += Commands_CommandErrored;

            DiscordClient.MessageCreated += DiscordClient_MessageCreatedAsync;
            DiscordClient.MessageUpdated += DiscordClient_MessageUpdatedAsync;
            DiscordClient.MessageReactionAdded += DiscordClient_MessageReactionAddedAsync;

            await DiscordClient.ConnectAsync();
        }

        private ServiceProvider BuildDependencies()
        {
            return new ServiceCollection()

            // Singletons

            // Helpers

            // Services
            .AddScoped<AssignRolesService>()

            .BuildServiceProvider();
        }

        private async Task DiscordClient_MessageCreatedAsync(DSharpPlus.EventArgs.MessageCreateEventArgs e)
        {
            if (e.Channel.IsPrivate == false)
            {
                try
                {
                    EmojiCounterService emojiCounterService = new EmojiCounterService();
                    await emojiCounterService.CountEmojiInMessage(e.Message);
                }
                catch (Exception ie)
                {
                    Console.WriteLine("Error: Counting emoji in new message.");
                }
            }
        }

        private async Task DiscordClient_MessageUpdatedAsync(DSharpPlus.EventArgs.MessageUpdateEventArgs e)
        {
            if (e.Channel.IsPrivate == false)
            {
                try
                {
                    EmojiCounterService emojiCounterService = new EmojiCounterService();
                    await emojiCounterService.CountEmojiInMessage(e.Message);
                }
                catch (Exception ie)
                {
                    Console.WriteLine("Error: Counting emoji in edited message.");
                }
            }
        }

        private async Task DiscordClient_MessageReactionAddedAsync(DSharpPlus.EventArgs.MessageReactionAddEventArgs e)
        {
            if (e.Channel.IsPrivate == false)
            {
                try
                {
                    EmojiCounterService emojiCounterService = new EmojiCounterService();
                    await emojiCounterService.CountEmojiReaction(e.User, e.Emoji, e.Channel);
                }
                catch (Exception ie)
                {
                    Console.WriteLine("Error: Counting emoji in reactions.");
                }
            }
        }

        private void SetNetworkParameters()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }

        private void RegisterCommands()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var assemblyTypes = assembly.GetTypes();

            var registerCommandsMethod = _commands.GetType().GetMethods()
                .FirstOrDefault(p => p.Name == "RegisterCommands" && p.IsGenericMethod);

            foreach (var type in assemblyTypes)
            {
                var attributes = type.GetCustomAttributes();
                if (attributes.Any(p => p.GetType() == typeof(CommandsGroupAttribute)))
                {
                    var genericRegisterCommandMethod = registerCommandsMethod.MakeGenericMethod(type);
                    genericRegisterCommandMethod.Invoke(_commands, null);
                }

                foreach(var method in type.GetMethods())
                {
                    
                    var attribute = method.GetCustomAttribute<MessageRespondAttribute>();
                    if (attribute != null)
                    {
                        Console.WriteLine("Dappa");
                        if (!method.IsStatic)
                        {
                            throw new ArgumentException("Methods with MessageRespondAttribute must be static!");
                        }

                        DiscordClient.MessageCreated += async e =>
                        {
                            if (e.Author.IsBot)
                                return;

                            var del = (AsyncEventHandler<MessageCreateEventArgs>)Delegate.CreateDelegate(typeof(AsyncEventHandler<MessageCreateEventArgs>), method);
                            await del.Invoke(e);
                        };
                    }
                }
            }
        }

        private Task Commands_CommandExecuted(CommandExecutionEventArgs e)
        {
            e.Context.Client.DebugLogger.LogMessage(LogLevel.Info, "SKNI Bot", $"{e.Context.User.Username} successfully executed '{e.Command.QualifiedName}'", DateTime.Now);
            return Task.FromResult(0);
        }

        private Task Commands_CommandErrored(CommandErrorEventArgs e)
        {
            var responseBuilder = new StringBuilder();

            switch (e.Exception)
            {
                case CommandNotFoundException _:
                {
                    responseBuilder.Append("Nieznana komenda, wpisz !help aby uzyskać listę wszystkich dostępnych.");
                    break;
                }

                case ChecksFailedException _:
                {
                    var failedCheck = ((ChecksFailedException)e.Exception).FailedChecks.First();
                    var permission = (RequirePermissionsAttribute)failedCheck;

                    responseBuilder.Append("Nie masz uprawnień do wykonania tej akcji :<\n");
                    responseBuilder.Append($"Wymagane: *{permission.Permissions.ToPermissionString()}*");
                    break;
                }

                case ArgumentException _:
                {
                    responseBuilder.Append($"Nieprawidłowe parametry komendy, wpisz `!help {e.Command.Name}` aby uzyskać ich listę.\n");
                    break;
                }

                default:
                {
                    responseBuilder.Append($"**{e.Exception.Message}**\n");
                    responseBuilder.Append($"{e.Exception.StackTrace}\n");
                    break;
                }
            }

            if (responseBuilder.Length != 0)
            {
                var embed = new DiscordEmbedBuilder
                {
                    Color = new DiscordColor("#CD171E")
                };
                embed.AddField("Błąd", responseBuilder.ToString().Substring(0, Math.Min(responseBuilder.Length, 1000)));

                e.Context.RespondAsync("", false, embed);
            }

            e.Context.Client.DebugLogger.LogMessage(LogLevel.Error, "SKNI Bot",
                $"{e.Context.User.Username} tried executing '{e.Command?.QualifiedName ?? "<unknown command>"}' " +
                $"but it errored: {e.Exception.GetType()}: {e.Exception.Message ?? "<no message>"}", DateTime.Now);

            return Task.FromResult(0);
        }
    }
}
