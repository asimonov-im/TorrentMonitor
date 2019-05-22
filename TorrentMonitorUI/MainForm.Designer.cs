namespace TorrentMonitorUI
{
    partial class MainForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.patternListView = new TorrentMonitorUI.NoFocusOutlineListView();
            this.patternTextbox = new System.Windows.Forms.TextBox();
            this.addPatternButton = new System.Windows.Forms.Button();
            this.updatePatternButton = new System.Windows.Forms.Button();
            this.removePatternButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // notifyIcon
            // 
            this.notifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.notifyIcon.Text = "TorrentMonitor";
            this.notifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon_MouseDoubleClick);
            // 
            // patternListView
            // 
            this.patternListView.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.patternListView.FullRowSelect = true;
            this.patternListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.patternListView.HideSelection = false;
            this.patternListView.Location = new System.Drawing.Point(13, 13);
            this.patternListView.MultiSelect = false;
            this.patternListView.Name = "patternListView";
            this.patternListView.ShowGroups = false;
            this.patternListView.Size = new System.Drawing.Size(658, 334);
            this.patternListView.TabIndex = 0;
            this.patternListView.UseCompatibleStateImageBehavior = false;
            this.patternListView.View = System.Windows.Forms.View.Details;
            this.patternListView.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.PatternListView_ItemSelectionChanged);
            // 
            // patternTextbox
            // 
            this.patternTextbox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.patternTextbox.Location = new System.Drawing.Point(13, 365);
            this.patternTextbox.Name = "patternTextbox";
            this.patternTextbox.Size = new System.Drawing.Size(453, 22);
            this.patternTextbox.TabIndex = 1;
            // 
            // addPatternButton
            // 
            this.addPatternButton.Location = new System.Drawing.Point(486, 363);
            this.addPatternButton.Name = "addPatternButton";
            this.addPatternButton.Size = new System.Drawing.Size(54, 23);
            this.addPatternButton.TabIndex = 2;
            this.addPatternButton.Text = "Add";
            this.addPatternButton.UseVisualStyleBackColor = true;
            this.addPatternButton.Click += new System.EventHandler(this.AddPatternButton_Click);
            // 
            // updatePatternButton
            // 
            this.updatePatternButton.Enabled = false;
            this.updatePatternButton.Location = new System.Drawing.Point(546, 363);
            this.updatePatternButton.Name = "updatePatternButton";
            this.updatePatternButton.Size = new System.Drawing.Size(56, 23);
            this.updatePatternButton.TabIndex = 3;
            this.updatePatternButton.Text = "Update";
            this.updatePatternButton.UseVisualStyleBackColor = true;
            this.updatePatternButton.Click += new System.EventHandler(this.UpdatePatternButton_Click);
            // 
            // removePatternButton
            // 
            this.removePatternButton.Enabled = false;
            this.removePatternButton.Location = new System.Drawing.Point(608, 363);
            this.removePatternButton.Name = "removePatternButton";
            this.removePatternButton.Size = new System.Drawing.Size(63, 23);
            this.removePatternButton.TabIndex = 4;
            this.removePatternButton.Text = "Remove";
            this.removePatternButton.UseVisualStyleBackColor = true;
            this.removePatternButton.Click += new System.EventHandler(this.RemovePatternButton_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(683, 435);
            this.Controls.Add(this.removePatternButton);
            this.Controls.Add(this.updatePatternButton);
            this.Controls.Add(this.addPatternButton);
            this.Controls.Add(this.patternTextbox);
            this.Controls.Add(this.patternListView);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Text = "TorrentMonitor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_Closing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Resize += new System.EventHandler(this.Form1_Resize);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.TextBox patternTextbox;
        private System.Windows.Forms.Button addPatternButton;
        private System.Windows.Forms.Button updatePatternButton;
        private System.Windows.Forms.Button removePatternButton;
        private NoFocusOutlineListView patternListView;
    }
}

