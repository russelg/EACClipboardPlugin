using System;
using System.Collections.Generic;
using System.Text;
using HelperFunctionsLib;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Net;
using System.IO;
using CUETools.CDImage;
using Edamame.ClipboardPlugin;
using Edamame.ClipboardPlugin.Properties;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace MetadataPlugIn
{
    [Guid("8271734A-126F-44e9-AC9C-836449B39E52"),
    ClassInterface(ClassInterfaceType.None),
    ComSourceInterfaces(typeof(IMetadataRetriever)),
    ]

    public class MetadataRetriever : IMetadataRetriever
    {
        public void SetCDInfo(CCDMetadata data, int track, CTDBResponseMetaTrack[] tr)
        {
            var t = tr[track];
            data.SetTrackTitle(track, t.name ?? "");
            data.SetTrackArtist(track, t.artist ?? "");
            data.SetExtendedTrackInformation(track, t.extra ?? "");
        }
        public bool GetCDInformation(CCDMetadata data, bool cdinfo, bool cover, bool lyrics)
        {
            var TOC = new CDImageLayout();
            for (int i = 0; i < data.NumberOfTracks; i++)
            {
                uint start = data.GetTrackStartPosition(i);
                uint next = data.GetTrackEndPosition(i);
                TOC.AddTrack(new CDTrack(
                    (uint)i + 1,
                    start,
                    next - start,
                    !data.GetTrackDataTrack(i),
                    data.GetTrackPreemphasis(i)));
            }
            TOC[1][0].Start = 0U;

            var form = new Form1();
            form.ShowDialog();
            var meta = form.Meta;

            int year, disccount, discnumber;
            string extra = meta.extra ?? "";
            if (!string.IsNullOrEmpty(meta.discname))
                extra += "Disc name: " + meta.discname + "\r\n";
            if (!string.IsNullOrEmpty(meta.infourl))
                extra += "Info URL: " + meta.infourl + "\r\n";
            if (!string.IsNullOrEmpty(meta.barcode))
                extra += "Barcode: " + meta.barcode + "\r\n";
            if (meta.release != null)
                foreach (var release in meta.release)
                {
                    if (!string.IsNullOrEmpty(release.date))
                        extra += "Release date: " + release.date + "\r\n";
                    if (!string.IsNullOrEmpty(release.country))
                        extra += "Release country: " + release.country + "\r\n";
                }
            if (meta.label != null)
            {
                foreach (var label in meta.label)
                {
                    if (!string.IsNullOrEmpty(label.name))
                        extra += "Release label: " + label.name + "\r\n";
                    if (!string.IsNullOrEmpty(label.catno))
                        extra += "Release catalog#: " + label.catno + "\r\n";
                }
            }
            if (meta.track != null)
            {
                int firstAudio = meta.track.Length == TOC.AudioTracks ? TOC.FirstAudio - 1 : 0;
                for (int track = 0; track < data.NumberOfTracks; track++)
                {
                    if (track - firstAudio >= 0 && track - firstAudio < meta.track.Length)
                    {
                        SetCDInfo(data, track - firstAudio, meta.track);
                    }
                    else if (!TOC[track + 1].IsAudio)
                    {
                        data.SetTrackTitle(track, "[data track]");
                        if (!string.IsNullOrEmpty(meta.artist))
                            data.SetTrackArtist(track, meta.artist);
                        data.SetExtendedTrackInformation(track, "");
                    }
                    else
                    {
                        SetCDInfo(data, track, meta.track);
                    }
                    /*if (!string.IsNullOrEmpty(meta.artist))
                        data.SetTrackComposer(track, meta.artist);*/
                }
            }
            data.Year = meta.year != null && int.TryParse(meta.year, out year) ? year : -1;
            data.TotalNumberOfCDs = meta.disccount != null && int.TryParse(meta.disccount, out disccount) ? disccount : 1;
            data.CDNumber = meta.discnumber != null && int.TryParse(meta.discnumber, out discnumber) ? discnumber : 1;
            data.FirstTrackNumber = 1;
            data.AlbumTitle = meta.album ?? "";
            data.AlbumArtist = meta.artist ?? "";
            if (!string.IsNullOrEmpty(extra))
                data.ExtendedDiscInformation = extra;
            data.Revision = -1;

            if (cover)
            {
                data.CoverImage = null;
                data.CoverImageURL = "";
                if (form.Image != null)
                {
                    data.CoverImage = form.Image.Data;
                }
            }

            return true;
        }

        public int GetMP3MusicType(int freedbtype)
        {
            int[] list = { 17, 29, 34, 95, 53, 77, 90, 113, 117, 129, 95 };
            return (freedbtype <= 0 || freedbtype >= list.Length) ? -1 : list[freedbtype];
        }

        public int GetFreeDBMusicType(CTDBResponseMeta meta)
        {
            int pos = meta.id.IndexOf('/');
            if (meta.source != "freedb" || pos < 0)
                return -1;
            string freedbtype = meta.id.Substring(0, pos);
            switch (freedbtype.ToUpper())
            {
                case "BLUES":
                    return 0;
                case "CLASSICAL":
                    return 1;
                case "COUNTRY":
                    return 2;
                case "DATA":
                    return 3;
                case "FOLK":
                    return 4;
                case "JAZZ":
                    return 5;
                case "NEWAGE":
                    return 6;
                case "REGGAE":
                    return 7;
                case "ROCK":
                    return 8;
                case "SOUNDTRACK":
                    return 9;
                case "MISC":
                    return 10;
                default:
                    return -1;
            }
        }

        public string GetPluginGuid()
        {
            return ((GuidAttribute)Attribute.GetCustomAttribute(GetType(), typeof(GuidAttribute))).Value;
        }

        public Array GetPluginLogo()
        {
            MemoryStream ms = new MemoryStream();
            Resources.logo.Save(ms, ImageFormat.Png);
            return ms.ToArray();
        }

        public string GetPluginName()
        {
            return "edamame clipboard plugin";
        }

        public void ShowOptions()
        {
            AudioDataPlugIn.Options opt = new AudioDataPlugIn.Options();
            opt.ShowDialog();
        }

        public bool SubmitCDInformation(IMetadataLookup data)
        {
            return false;
        }

        public bool SupportsCoverRetrieval()
        {
            return true;
        }

        public bool SupportsLyricsRetrieval()
        {
            return false;
        }

        public bool SupportsMetadataRetrieval()
        {
            return true;
        }

        public bool SupportsMetadataSubmission()
        {
            return false;
        }
    }
}
