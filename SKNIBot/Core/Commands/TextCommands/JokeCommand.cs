﻿using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using SKNIBot.Core.Database;

namespace SKNIBot.Core.Commands.TextCommands
{
    [CommandsGroup("Tekst")]
    public class JokeCommand
    {
        private Random _random;

        public JokeCommand()
        {
            _random = new Random();
        }

        [Command("żart")]
        [Description("Żarty i suchary w postaci tekstu i obrazków.")]
        [Aliases("suchar", "joke", "itsJoke")]
        public async Task Joke(CommandContext ctx, [Description("Użytkownik do wzmienienia.")] DiscordMember member = null)
        {
            await ctx.TriggerTypingAsync();

            using (var databaseContext = new StaticDBContext())
            {
                var jokes = databaseContext.SimpleResponses.Where(p => p.Command.Name == "Joke");
                var randomIndex = _random.Next(jokes.Count());

                var jokeContent = jokes
                    .OrderBy(p => p.ID)
                    .Select(p => p.Content)
                    .Skip(randomIndex)
                    .First();

                //Jeżeli długość jest jeden nie podano kodu
                if (member != null)
                {
                    jokeContent += " " + member.Mention;
                }

                await ctx.RespondAsync(jokeContent);
            }
        }
    }
}
