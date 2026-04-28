namespace AGI_Decrypter {
    partial class AGIDecrypter {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AGIDecrypter));
            openFileDialog1 = new OpenFileDialog();
            DriveList = new ComboBox();
            FolderList = new TreeView();
            FolderIcons = new ImageList(components);
            FileList = new ListBox();
            AGIVersion = new Label();
            LoaderVersion = new Label();
            DecryptButton = new Button();
            label1 = new Label();
            label2 = new Label();
            HelpButton = new Button();
            SuspendLayout();
            // 
            // openFileDialog1
            // 
            openFileDialog1.FileName = "openFileDialog1";
            // 
            // DriveList
            // 
            DriveList.DropDownStyle = ComboBoxStyle.DropDownList;
            DriveList.FormattingEnabled = true;
            DriveList.Location = new Point(12, 25);
            DriveList.Name = "DriveList";
            DriveList.Size = new Size(776, 40);
            DriveList.TabIndex = 0;
            DriveList.SelectedIndexChanged += DriveList_SelectedIndexChanged;
            // 
            // FolderList
            // 
            FolderList.FullRowSelect = true;
            FolderList.HideSelection = false;
            FolderList.ImageIndex = 0;
            FolderList.ImageList = FolderIcons;
            FolderList.Location = new Point(12, 130);
            FolderList.Name = "FolderList";
            FolderList.SelectedImageIndex = 0;
            FolderList.ShowNodeToolTips = true;
            FolderList.Size = new Size(479, 293);
            FolderList.TabIndex = 1;
            FolderList.AfterSelect += FolderList_AfterSelect;
            FolderList.NodeMouseDoubleClick += FolderList_NodeMouseDoubleClick;
            // 
            // FolderIcons
            // 
            FolderIcons.ColorDepth = ColorDepth.Depth32Bit;
            FolderIcons.ImageStream = (ImageListStreamer)resources.GetObject("FolderIcons.ImageStream");
            FolderIcons.TransparentColor = Color.Transparent;
            FolderIcons.Images.SetKeyName(0, "closed32.ico");
            FolderIcons.Images.SetKeyName(1, "open32.ico");
            // 
            // FileList
            // 
            FileList.FormattingEnabled = true;
            FileList.Location = new Point(519, 131);
            FileList.Name = "FileList";
            FileList.Size = new Size(269, 292);
            FileList.TabIndex = 2;
            FileList.SelectedIndexChanged += FileList_SelectedIndexChanged;
            // 
            // AGIVersion
            // 
            AGIVersion.AutoSize = true;
            AGIVersion.Location = new Point(12, 433);
            AGIVersion.Name = "AGIVersion";
            AGIVersion.Size = new Size(42, 32);
            AGIVersion.TabIndex = 3;
            AGIVersion.Text = "    ";
            // 
            // LoaderVersion
            // 
            LoaderVersion.AutoSize = true;
            LoaderVersion.Location = new Point(519, 433);
            LoaderVersion.Name = "LoaderVersion";
            LoaderVersion.Size = new Size(42, 32);
            LoaderVersion.TabIndex = 4;
            LoaderVersion.Text = "    ";
            // 
            // DecryptButton
            // 
            DecryptButton.Enabled = false;
            DecryptButton.Location = new Point(12, 493);
            DecryptButton.Name = "DecryptButton";
            DecryptButton.Size = new Size(479, 75);
            DecryptButton.TabIndex = 5;
            DecryptButton.Text = "Remove Encryption";
            DecryptButton.UseVisualStyleBackColor = true;
            DecryptButton.Click += DecryptButton_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 85);
            label1.Name = "label1";
            label1.Size = new Size(91, 32);
            label1.TabIndex = 6;
            label1.Text = "Folders";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(519, 85);
            label2.Name = "label2";
            label2.Size = new Size(140, 32);
            label2.TabIndex = 7;
            label2.Text = "Loader Files";
            // 
            // HelpButton
            // 
            HelpButton.Location = new Point(519, 493);
            HelpButton.Name = "HelpButton";
            HelpButton.Size = new Size(269, 75);
            HelpButton.TabIndex = 8;
            HelpButton.Text = "Help";
            HelpButton.UseVisualStyleBackColor = true;
            HelpButton.Click += HelpButton_Click;
            // 
            // AGIDecrypter
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 589);
            Controls.Add(HelpButton);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(DecryptButton);
            Controls.Add(LoaderVersion);
            Controls.Add(AGIVersion);
            Controls.Add(FileList);
            Controls.Add(FolderList);
            Controls.Add(DriveList);
            FormBorderStyle = FormBorderStyle.Fixed3D;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "AGIDecrypter";
            Text = "AGI Decrypter v3";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private OpenFileDialog openFileDialog1;
        private ComboBox DriveList;
        private TreeView FolderList;
        private ListBox FileList;
        private Label AGIVersion;
        private Label LoaderVersion;
        private Button DecryptButton;
        private ImageList FolderIcons;
        private Label label1;
        private Label label2;
        private Button HelpButton;
    }
}
