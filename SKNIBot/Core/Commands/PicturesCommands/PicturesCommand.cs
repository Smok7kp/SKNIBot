﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using SKNIBot.Core.Containers.PicturesContainers;

namespace SKNIBot.Core.Commands.PicturesCommands
{
    [CommandsGroup("Obrazki")]
    public class PicturesCommand
    {
        private List<PictureData> _images;
        private const string _imagesFile = "images.json";

        public PicturesCommand()
        {
            using (var file = new StreamReader(_imagesFile))
            {
                _images = JsonConvert.DeserializeObject<List<PictureData>>(file.ReadToEnd());
            }
        }

        [Command("picture")]
        [Description("Wyświetl obrazek!")]
        [Aliases("pic")]
        public async Task Picture(CommandContext ctx, [Description("Wpisz !pic help aby uzyskać listę dostępnych opcji.")] string pictureName = null, [Description("Wzmianka")] DiscordMember member = null)
        {
            await ctx.TriggerTypingAsync();
            if (pictureName == "list")
            {
                await ctx.RespondAsync($"Dostępne obrazki:\r\n\r\n{GetAvailableParameters()}");
                return;
            }

            var videoData = _images.FirstOrDefault(vid => vid.Names.Exists(p => p.Equals(pictureName, StringComparison.InvariantCultureIgnoreCase)));
            if (videoData == null)
            {
                await ctx.RespondAsync("Nieznany parametr, wpisz !pic list aby uzyskać listę dostępnych.");
                return;
            }

            var response = videoData.Link;
            if (member != null)
            {
                response += $" {member.Mention}";
            }

            await ctx.RespondAsync(response);
        }

        private string GetAvailableParameters()
        {
            var stringBuilder = new StringBuilder();
            var categories = _images.GroupBy(p => p.Category).OrderBy(p => p.Key).ToList();

            foreach (var category in categories)
            {
                var sortedCategory = category.OrderBy(p => p.Names[0]);
                var items = sortedCategory.Select(p => p.Names[0]);

                stringBuilder.Append($"**{category.Key}**:\r\n");
                stringBuilder.Append(string.Join(", ", items));
                stringBuilder.Append("\r\n\r\n");
            }

            return stringBuilder.ToString();
        }
    }
}
