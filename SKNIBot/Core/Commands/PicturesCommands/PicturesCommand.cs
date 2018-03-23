﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using SKNIBot.Core.Containers.PicturesContainers;
using SKNIBot.Core.Database;

namespace SKNIBot.Core.Commands.PicturesCommands
{
    [CommandsGroup("Obrazki")]
    public class PicturesCommand
    {
        [Command("picture")]
        [Description("Wyświetl obrazek!")]
        [Aliases("pic")]
        public async Task Picture(CommandContext ctx, [Description("Wpisz !pic help aby uzyskać listę dostępnych opcji.")] string pictureName = null, [Description("Wzmianka")] DiscordMember member = null)
        {
            await ctx.TriggerTypingAsync();

            using (var databaseContext = new DatabaseContext())
            {
                if (pictureName == "list")
                {
                    await ctx.RespondAsync($"Dostępne obrazki:\r\n\r\n{GetAvailableParameters()}");
                    return;
                }

                var pictureData = databaseContext.Images.FirstOrDefault(vid =>
                    vid.Names.Any(p => p.Name.Equals(pictureName, StringComparison.InvariantCultureIgnoreCase)));
                if (pictureData == null)
                {
                    await ctx.RespondAsync("Nieznany parametr, wpisz !pic list aby uzyskać listę dostępnych.");
                    return;
                }

                var response = pictureData.Link;
                if (member != null)
                {
                    response += $" {member.Mention}";
                }

                await ctx.RespondAsync(response);
            }
        }

        private string GetAvailableParameters()
        {
            using (var databaseContext = new DatabaseContext())
            {
                var stringBuilder = new StringBuilder();
                var categories = databaseContext.Images
                    .Include(p => p.Names)
                    .GroupBy(p => p.ImageCategory.Name)
                    .OrderBy(p => p.Key)
                    .ToList();

                foreach (var category in categories)
                {
                    var sortedCategory = category.OrderBy(p => p.Names[0].Name);
                    var items = sortedCategory.Select(p => p.Names[0].Name);

                    stringBuilder.Append($"**{category.Key}**:\r\n");
                    stringBuilder.Append(string.Join(", ", items));
                    stringBuilder.Append("\r\n\r\n");
                }

                return stringBuilder.ToString();
            }
        }
    }
}
