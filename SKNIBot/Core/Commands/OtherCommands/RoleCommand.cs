﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using SKNIBot.Core.Database;

namespace SKNIBot.Core.Commands.OtherCommands
{
    [CommandsGroup("Role")]
    public class RoleCommand : BaseCommandModule
    {
        [Command("DodajRole")]
        public async Task AddRole(CommandContext ctx, string name)
        {
            using (var databaseContext = new StaticDBContext())
            {
                var role = ctx.Guild.Roles.FirstOrDefault(p => p.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));

                if (role == null)
                {
                    await ctx.RespondAsync("Nie ma takiej roli!");
                    return;
                }

                if (!databaseContext.Roles.Any(p => p.RoleId == role.Id.ToString()))
                {
                    await ctx.RespondAsync("Nie możesz przypisać tej roli!");
                    return;
                }

                if (ctx.Member.Roles.Contains(role))
                {
                    await ctx.RespondAsync("Jesteś już przypisany do tej roli!");
                    return;
                }

                await ctx.Member.GrantRoleAsync(role, "Przypisana przez Bocika");
                await ctx.RespondAsync("Rola przypisana!");
            }
        }

        [Command("UsuńRole")]
        public async Task RemoveRole(CommandContext ctx, string name)
        {
            using (var databaseContext = new StaticDBContext())
            {

            }
        }

        [Command("WylistujRole")]
        public async Task ListAllRoles(CommandContext ctx)
        {
            using (var databaseContext = new StaticDBContext())
            {
                //var roles = databaseContext.Roles.Select(p => p.Name).ToList();
                //await ctx.RespondAsync(string.Join(", ", roles));
            }
        }
    }
}
