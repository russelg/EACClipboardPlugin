using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Edamame.ClipboardPlugin
{
    public partial class Form1 : Form
    {

        private List<CTDBResponseMeta> metadata = new List<CTDBResponseMeta>();
        private InternetImage image = new InternetImage();

        public Form1()
        {
            InitializeComponent();
        }

        public IEnumerable<CTDBResponseMeta> Metadata
        {
            get
            {
                return metadata;
            }
        }

        public InternetImage Image
        {
            get
            {
                return image.Data != null ? image : null;
            }
        }

        public CTDBResponseMeta ParsedInput(string format, string data)
        {
            var lines = data.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            // var artist = Regex.Match(lines[0], @"Artist: (.*)$", RegexOptions.IgnoreCase).Groups[1].Value;
            // var album = Regex.Match(lines[1], @"Album: (.*)$", RegexOptions.IgnoreCase).Groups[1].Value;
            var reg = Regex.Replace(format, @"%(.*?)%", @"(?<$1>.*?)") + "$";
            var traks = lines.ToList();
            var tracks = new CTDBResponseMetaTrack[traks.Count()];
            for (int i = 0; i < traks.Count(); i++)
            {
                var matches = Regex.Match(traks[i], reg).Groups;

                tracks[i] = new CTDBResponseMetaTrack(){};
                if (!String.IsNullOrEmpty(matches["title"].Value))
                {
                    tracks[i].name = matches["title"].Value;
                }
                if (!String.IsNullOrEmpty(matches["artist"].Value))
                {
                    tracks[i].artist = matches["artist"].Value;
                }
            }
            return new CTDBResponseMeta()
            {
                track = tracks
            };
        }

        public CTDBResponseMeta Meta
        {
            get
            {
                // var parsed = ParsedInput("%track%. %title%", albums);
                var parsed = ParsedInput(formatBox.Text, inputBox.Text);
                metadata.Add(parsed);
                return parsed;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            image.Data = null;
            image.Image = null;
            var imageExtensions = string.Join(";", ImageCodecInfo.GetImageDecoders().Select(ici => ici.FilenameExtension).ToArray());
            openFileDialog1.Filter = string.Format("Images|{0}|All Files|*.*", imageExtensions);
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK) // Test result.
            {
                string file = openFileDialog1.FileName;
                var ms = new MemoryStream(File.ReadAllBytes(file));
                image.Data = ms.ToArray();
                image.Image = new Bitmap(ms);
                fileLabel.Text = openFileDialog1.FileName;
            }
            Console.WriteLine(result); // <-- For debugging use.
        }
    }
}
