using SkiaSharp;
using System.Diagnostics;
using System.Security.Policy;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace YTDownloader_GUI
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
        private async Task InstallYTDlp()
        {
            var httpClient = new HttpClient();


            string fileToDownload = "https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp.exe";

            byte[] fileBytes = await httpClient.GetByteArrayAsync(fileToDownload);

            await File.WriteAllBytesAsync("C:\\tools\\yt-dlp.exe", fileBytes);
        }
        private async Task InstallFFMPeg()
        {
            string command = "winget";
            string args = "install ffmpeg";
            var psi = new ProcessStartInfo
            {
                FileName = command,
                Arguments = args,
                RedirectStandardOutput = false,
                RedirectStandardError = false,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var process = new Process { StartInfo = psi };
            process.Start();
            await process.WaitForExitAsync();
        }
        private bool CheckIfFFMpegInstalled()
        {
            string path = Environment.GetEnvironmentVariable("PATH");
            string executable = "ffmpeg.exe";
            string[] directories = path.Split(";");
            return directories.Any(dir =>
            {
                string fullPath = Path.Combine(dir, executable);
                return File.Exists(fullPath);
            });
        }
        private bool CheckIfYTDlpInstalled()
        {
            string path = Environment.GetEnvironmentVariable("PATH");
            string executable = "yt-dlp.exe";
            if (!string.IsNullOrEmpty(path))
            {
                string[] directories = path.Split(";");
                return directories.Any(dir =>
                {
                    string fullPath = Path.Combine(dir, executable);
                    return File.Exists(fullPath);
                });
            }
            else { return false; }
        }
        private async void Program_Load(object sender, EventArgs e)
        {
            if (!CheckIfYTDlpInstalled())
            {
                MessageBox.Show("YT-Dlp is not installed, installing...", "YT-Dlp Installation", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await InstallYTDlp();
            }
            else if (!CheckIfFFMpegInstalled())
            {
                MessageBox.Show("FFmpeg is not installed, installing...", "FFmpeg Installation", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await InstallFFMPeg();
            }
        }
        private void Exit(object sender, EventArgs e)
        {
            Application.Exit();
        }
        private async Task DownloadVideo(string url, string title)
        {
            SharedPercentage.Title = title;
            //  yt-dlp --newline --no-warnings -f bestvideo+bestaudio --merge-output-format mp4 -o "%(title)s.mp4" {url}
            if (string.IsNullOrEmpty(url))
            {
                return;
            }
            string path = "";
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select a folder for your video to be saved in";
                DialogResult result = dialog.ShowDialog();
                dialog.RootFolder = Environment.SpecialFolder.Desktop;
                if (result == DialogResult.OK)
                {
                    path = dialog.SelectedPath;
                }
            }
            string path_save = $@"{path}\video.mp4";
            string arguments = $"--newline --no-warnings -f bestvideo+bestaudio --merge-output-format mp4 -o {path_save} {url}";
            var psi = new ProcessStartInfo
            {
                FileName = "yt-dlp",
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            using var process = new Process { StartInfo = psi };
            process.Start();
            Regex percentRegex = new Regex(@"(\d{1,3}\.\d+)%");
            Form2 form2 = new Form2();
            form2.Show();
            var outputTask = Task.Run(async () =>
            {
                while (!process.StandardOutput.EndOfStream)
                {
                    string line = await process.StandardOutput.ReadLineAsync();
                    var match = percentRegex.Match(line);
                    if (match.Success)
                    {
                        string percentage = match.Groups[1].Value;
                        if (double.TryParse(percentage, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double percentDouble))
                        {
                            int percentInt = (int)Math.Round(percentDouble);
                            Console.WriteLine($"Percentage: {percentInt}");
                            SharedPercentage.Percentage = percentInt;
                        }

                    }
                    Console.WriteLine($"stdout: {line}");
                }
            });
            var errorTask = Task.Run(async () =>
            {
                while (!process.StandardError.EndOfStream)
                {
                    string line = await process.StandardError.ReadLineAsync();
                    Console.WriteLine($"stderr: {line}");
                }
            });
            await process.WaitForExitAsync();
            await Task.WhenAll(outputTask, errorTask);
            SharedPercentage.Complete = true;
            Console.WriteLine("Complete :3");
        }
        private async Task<string> GetThumbnailURL(string videoUrl)
        {
            if (string.IsNullOrEmpty(videoUrl))
            {
                return "";
            }

            string arguments = $"-q --no-warnings --get-thumbnail {videoUrl}";

            var psi = new ProcessStartInfo
            {
                FileName = "yt-dlp",
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            try
            {
                using var process = new Process { StartInfo = psi };

                process.Start();
                Console.WriteLine("Getting thumbnail URL...");
                string output = await process.StandardOutput.ReadToEndAsync();
                string errors = await process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync();

                if (!string.IsNullOrWhiteSpace(errors))
                {
                    MessageBox.Show("Error getting thumbnail:\n" + errors, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return "Error";
                }
                Console.WriteLine(output.Trim());
                return output.Trim();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception while getting thumbnail:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return "Error";
            }
        }
        private async Task<string> GetTitle(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return "";
            }
            string arguments = $"-q --no-warnings --get-title {url}";
            var psi = new ProcessStartInfo
            {
                FileName = "yt-dlp",
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            try
            {
                using var process = new Process { StartInfo = psi };

                process.Start();
                Console.WriteLine("Title process created!");
                string output = await process.StandardOutput.ReadToEndAsync();
                string errors = await process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync();

                if (!string.IsNullOrWhiteSpace(errors))
                {
                    MessageBox.Show("Error getting title:\n" + errors, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return "Error";
                }
                Console.WriteLine(output.Trim());
                return output.Trim();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception while getting title:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return "Error";
            }
        }
        private async Task LoadImageFromURL(string url)
        {
            try
            {
                using HttpClient client = new HttpClient();
                var imageBytes = await client.GetByteArrayAsync(url);

                using var skData = SKData.CreateCopy(imageBytes);
                using var skCodec = SKCodec.Create(skData); // had to use external library because youtube likes to randomly use webp
                using var skBitmap = SKBitmap.Decode(skCodec);

                using var ms = new MemoryStream();
                skBitmap.Encode(ms, SKEncodedImageFormat.Png, 100);
                ms.Seek(0, SeekOrigin.Begin);

                pictureBox1.Image = Image.FromStream(ms);
                pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load image: " + ex.Message);
            }
        }
        private async void GetButton(object sender, EventArgs e)
        {
            string url = YTUrlInput.Text;
            if (url.Contains("&start_radio") || url.Contains("&list"))
            {
                MessageBox.Show("This tool cannot download playlists, you have to manually enter each link and remove &start_radio along with &list from the link.", "Invalid URL", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }
            Console.WriteLine("Got URL!");
            string title = await GetTitle(url);
            Console.WriteLine("Got title!");
            string thumbnailurl = await GetThumbnailURL(url);
            Console.WriteLine("Got thumbnail!");
            this.VideoDisplay.Visible = true;
            await LoadImageFromURL(thumbnailurl);
            this.videoTitle.Text = title;
        }
        private async void Download(object sender, EventArgs e)
        {
            Console.WriteLine("Downloading...");
            string url = YTUrlInput.Text;
            string title = videoTitle.Text;
            await DownloadVideo(url, title);
        }
        private void About(object sender, EventArgs e)
        {
            MessageBox.Show("This tool was made by superhenryman, UI was copied (i'm not afraid to admit it) from https://github.com/harborsiem/YouTubeDownloader, if you find any bugs, please tell me :D", "About", MessageBoxButtons.OK, MessageBoxIcon.Information);    ;
        }
        private void InitializeComponent()
        {
            label1 = new Label();
            label2 = new Label();
            YTUrlInput = new TextBox();
            GetVideoButton = new Button();
            panel1 = new Panel();
            ExitButton = new Button();
            AboutButton = new Button();
            panel2 = new Panel();
            VideoDisplay = new Panel();
            button1 = new Button();
            videoTitle = new Label();
            label3 = new Label();
            pictureBox1 = new PictureBox();
            panel1.SuspendLayout();
            panel2.SuspendLayout();
            VideoDisplay.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.BackColor = SystemColors.Control;
            label1.ForeColor = SystemColors.ControlText;
            label1.Location = new Point(12, 29);
            label1.Name = "label1";
            label1.Size = new Size(245, 28);
            label1.TabIndex = 0;
            label1.Text = "Youtube Downloader";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(30, 10);
            label2.Name = "label2";
            label2.Size = new Size(160, 28);
            label2.TabIndex = 1;
            label2.Text = "Youtube URL";
            // 
            // YTUrlInput
            // 
            YTUrlInput.BorderStyle = BorderStyle.FixedSingle;
            YTUrlInput.Font = new Font("Arial", 11F);
            YTUrlInput.Location = new Point(30, 54);
            YTUrlInput.Name = "YTUrlInput";
            YTUrlInput.Size = new Size(420, 29);
            YTUrlInput.TabIndex = 2;
            // 
            // GetVideoButton
            // 
            GetVideoButton.Font = new Font("Arial", 12F);
            GetVideoButton.Location = new Point(291, 99);
            GetVideoButton.Name = "GetVideoButton";
            GetVideoButton.Size = new Size(159, 33);
            GetVideoButton.TabIndex = 3;
            GetVideoButton.Text = "Get Video";
            GetVideoButton.UseVisualStyleBackColor = true;
            GetVideoButton.Click += GetButton;
            // 
            // panel1
            // 
            panel1.BackColor = Color.FromArgb(0, 12, 241, 255);
            panel1.Controls.Add(ExitButton);
            panel1.Controls.Add(AboutButton);
            panel1.Location = new Point(-3, 497);
            panel1.Name = "panel1";
            panel1.Size = new Size(799, 116);
            panel1.TabIndex = 4;
            // 
            // ExitButton
            // 
            ExitButton.Location = new Point(669, 45);
            ExitButton.Name = "ExitButton";
            ExitButton.Size = new Size(111, 37);
            ExitButton.TabIndex = 1;
            ExitButton.Text = "Exit";
            ExitButton.UseVisualStyleBackColor = true;
            ExitButton.Click += Exit;
            // 
            // AboutButton
            // 
            AboutButton.Location = new Point(519, 45);
            AboutButton.Name = "AboutButton";
            AboutButton.Size = new Size(111, 37);
            AboutButton.TabIndex = 0;
            AboutButton.Text = "About";
            AboutButton.UseVisualStyleBackColor = true;
            AboutButton.Click += About;
            // 
            // panel2
            // 
            panel2.BackColor = Color.Gainsboro;
            panel2.Controls.Add(label2);
            panel2.Controls.Add(GetVideoButton);
            panel2.Controls.Add(YTUrlInput);
            panel2.Location = new Point(303, 29);
            panel2.Name = "panel2";
            panel2.Size = new Size(474, 147);
            panel2.TabIndex = 5;
            // 
            // VideoDisplay
            // 
            VideoDisplay.BackColor = Color.Gainsboro;
            VideoDisplay.Controls.Add(button1);
            VideoDisplay.Controls.Add(videoTitle);
            VideoDisplay.Controls.Add(label3);
            VideoDisplay.Controls.Add(pictureBox1);
            VideoDisplay.Location = new Point(-3, 225);
            VideoDisplay.Name = "VideoDisplay";
            VideoDisplay.Size = new Size(799, 189);
            VideoDisplay.TabIndex = 6;
            VideoDisplay.Visible = false;
            // 
            // button1
            // 
            button1.Location = new Point(306, 133);
            button1.Name = "button1";
            button1.Size = new Size(450, 38);
            button1.TabIndex = 3;
            button1.Text = "Download";
            button1.UseVisualStyleBackColor = true;
            button1.Click += Download;
            // 
            // videoTitle
            // 
            videoTitle.AutoSize = true;
            videoTitle.Font = new Font("Arial", 11F);
            videoTitle.Location = new Point(399, 42);
            videoTitle.Name = "videoTitle";
            videoTitle.Size = new Size(0, 22);
            videoTitle.TabIndex = 2;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.ForeColor = Color.FromArgb(132, 131, 131);
            label3.Location = new Point(306, 36);
            label3.Name = "label3";
            label3.Size = new Size(65, 28);
            label3.TabIndex = 1;
            label3.Text = "Title:";
            // 
            // pictureBox1
            // 
            pictureBox1.Location = new Point(15, 15);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(260, 156);
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(15F, 28F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.Control;
            ClientSize = new Size(789, 612);
            Controls.Add(VideoDisplay);
            Controls.Add(panel2);
            Controls.Add(panel1);
            Controls.Add(label1);
            Font = new Font("Arial", 15F);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Location = new Point(2, 2);
            Margin = new Padding(6, 4, 6, 4);
            MaximizeBox = false;
            Name = "Form1";
            Text = "Youtube Downloader";
            Load += Program_Load;
            panel1.ResumeLayout(false);
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            VideoDisplay.ResumeLayout(false);
            VideoDisplay.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
            PerformLayout();

        }
        private Label label1;
        private Label label2;
        private TextBox YTUrlInput;
        private Button GetVideoButton;
        private Panel panel1;
        private Button ExitButton;
        private Button AboutButton;
        private Panel panel2;
        private Panel VideoDisplay;
        private PictureBox pictureBox1;
        private Label videoTitle;
        private Label label3;
        private Button button1;
    }
}
