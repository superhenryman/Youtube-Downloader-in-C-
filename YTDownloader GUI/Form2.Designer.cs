using System.Threading.Tasks;

namespace YTDownloader_GUI
{
    partial class Form2
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        private async Task RunConstantly()
        {
            while (true)
            {
                if (downloadStatus.InvokeRequired)
                {
                    downloadStatus.Invoke(() =>
                    {
                        downloadStatus.Value = Math.Min(SharedPercentage.Percentage, 100);
                    });
                }
                else
                {
                    downloadStatus.Value = Math.Min(SharedPercentage.Percentage, 100);
                }
                if (downloadStatus.Value == 100 || SharedPercentage.Complete)
                {
                    this.Close();
                    break;
                }
                await Task.Delay(1000);
            }
        }
        private async void Load_In(object sender, EventArgs e)
        {
            title.Text = SharedPercentage.Title;
            await RunConstantly();

        }
        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            label1 = new Label();
            title = new Label();
            downloadStatus = new ProgressBar();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Arial", 13F);
            label1.ForeColor = Color.FromArgb(132, 131, 131);
            label1.Location = new Point(12, 23);
            label1.Name = "label1";
            label1.Size = new Size(76, 25);
            label1.TabIndex = 0;
            label1.Text = "Title:";
            // 
            // title
            // 
            title.AutoSize = true;
            title.Font = new Font("Arial", 12F);
            title.Location = new Point(111, 23);
            title.Name = "title";
            title.Size = new Size(0, 23);
            title.TabIndex = 1;
            // 
            // downloadStatus
            // 
            downloadStatus.Location = new Point(12, 133);
            downloadStatus.Name = "downloadStatus";
            downloadStatus.Size = new Size(646, 29);
            downloadStatus.Step = 1;
            downloadStatus.TabIndex = 2;
            // 
            // Form2
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(670, 195);
            Controls.Add(downloadStatus);
            Controls.Add(title);
            Controls.Add(label1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "Form2";
            ShowIcon = false;
            Text = "Download Progress";
            Load += Load_In;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private Label title;
        private ProgressBar downloadStatus;
    }
}