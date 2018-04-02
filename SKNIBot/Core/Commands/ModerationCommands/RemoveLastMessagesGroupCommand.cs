﻿using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKNIBot.Core.Commands.ModerationCommands
{
    [CommandsGroup("Moderacja")]
    class RemoveLastMessagesGroupCommand
    {
        [Command("usuńGrupowo")]
        [Aliases("usunGrupowo")]
        [Description("Usuwa ostatnie x wiadomości w grupach.")]
        [RequireRolesAttribute("Projekt - Bot")]
        public async Task RemoveLastMessagesGroup(CommandContext ctx, [Description("Liczba usunięć.")] int deleteCount, [Description("Liczba wiadomości do usunięcia w grupie.")] int messagesCount)
        {
            var messages = await ctx.Channel.GetMessagesAsync(1);
            await ctx.Channel.DeleteMessagesAsync(messages);
            for (int i = 0; i < deleteCount; i++)
            {
                messages = await ctx.Channel.GetMessagesAsync(Math.Min(messagesCount, 100));
                if (messages.Count == 0) break;
                await ctx.Channel.DeleteMessagesAsync(messages);
            }
        }
    }
}