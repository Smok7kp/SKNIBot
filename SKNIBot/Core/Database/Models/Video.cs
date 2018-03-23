﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKNIBot.Core.Database.Models
{
    public class Video
    {
        public int ID { get; set; }
        public string Link { get; set; }

        public int VideoCategoryID { get; set; }
        public virtual VideoCategory VideoCategory { get; set; }

        public virtual List<VideoName> Names { get; set; }
    }
}
