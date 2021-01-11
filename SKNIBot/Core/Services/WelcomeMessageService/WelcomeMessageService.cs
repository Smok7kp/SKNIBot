﻿using Microsoft.EntityFrameworkCore;
using SKNIBot.Core.Database;
using SKNIBot.Core.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SKNIBot.Core.Services.WelcomeMessageService
{
    class WelcomeMessageService
    {
        public WelcomeMessageResponse GetWelcomeMessage(ulong serverId)
        {
            using (var databaseContext = new DynamicDBContext())
            {
                Server dbServer = databaseContext.Servers.Where(p => p.ServerID == serverId.ToString()).Include(p => p.WelcomeMessage).FirstOrDefault();

                if(dbServer != null && dbServer.WelcomeMessage != null)
                {
                    return new WelcomeMessageResponse(ulong.Parse(dbServer.ServerID), ulong.Parse(dbServer.WelcomeMessage.ChannelID), dbServer.WelcomeMessage.Content);
                }
                else
                {
                    return new WelcomeMessageResponse();
                }
            }
        }
        private bool IsWelcomeMessageOnServer(ulong serverId)
        {
            using (var databaseContext = new DynamicDBContext())
            {
                Server dbServer = databaseContext.Servers.Where(p => p.ServerID == serverId.ToString()).Include(p => p.WelcomeMessage).FirstOrDefault();

                if (dbServer != null)
                {
                    return dbServer.WelcomeMessage != null ? true : false;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}
