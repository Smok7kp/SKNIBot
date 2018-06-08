﻿using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using SKNIBot.Core.Database;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SKNIBot.Core.Commands.PicturesCommands
{
    [CommandsGroup("Obrazki")]
    class CreateMemeCommand
    {

        FontFamily _fontFamily;
        Pen _outlinePen;
        int _currentFontSize = 40;

        public CreateMemeCommand()
        {
            _fontFamily = new FontFamily("Liberation Mono");
            _outlinePen = new Pen(Color.Black)
            {
                Width = 4
            };
        }

        [Command("meme")]
        public async Task CreateMeme(CommandContext ctx, string picName, string upText, string bottomText = null)
        {
            using (var databaseContext = new StaticDBContext())
            {
                var pictureLink = databaseContext.Media
                   .Where(vid => vid.Command.Name == "Picture" && vid.Names.Any(p => DbFunctions.Like(p.Name, picName)))
                   .Select(p => p.Link)
                   .FirstOrDefault();

                if (pictureLink == null)
                {
                    await ctx.RespondAsync("Nieznany parametr, wpisz !pic list aby uzyskać listę dostępnych.");
                    return;
                }


                WebClient web = new WebClient();
                var picture = web.DownloadData(pictureLink);
                var stream = new MemoryStream(picture);

                Image img = Bitmap.FromStream(stream);

                var upPosition = new RectangleF(0, 0, img.Width - 10, img.Height / 2);

                Graphics g = GetGraphicsFromImage(img);

                StringFormat format = new StringFormat(StringFormat.GenericDefault);
                format.Alignment = StringAlignment.Center;

                //g.DrawString(upText, _font, Brushes.Black, upPosition, format);

                GraphicsPath path = new GraphicsPath();
                path.AddString(upText, _fontFamily, (int)FontStyle.Bold, g.DpiY * _currentFontSize / 72, upPosition, format);

                g.DrawPath(_outlinePen, path);
                g.FillPath(Brushes.White, path);
                g.Flush();

                MemoryStream mem = new MemoryStream();
                img.Save(mem, ImageFormat.Jpeg);
                mem.Position = 0;

                await ctx.RespondWithFileAsync(mem, "test.jpg");
            }
        }

        Graphics GetGraphicsFromImage(Image img)
        {
            Graphics g = Graphics.FromImage(img);

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

            return g;
        }
    }
}