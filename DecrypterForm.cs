using System.Diagnostics;
using System.Text;

namespace AGI_Decrypter {
    public partial class AGIDecrypter : Form {
        bool carryflag = false;
        bool isAGIfolder = false;
        bool isLoader = false;
        public AGIDecrypter() {
            InitializeComponent();
            // load the DriveListBox with the system drives
            foreach (var drive in DriveInfo.GetDrives()) {
                if (drive.IsReady) {
                    DriveList.Items.Add(drive.Name + " [" + drive.VolumeLabel + "]");
                }
            }
            if (DriveList.Items.Count > 0) {
                DriveList.SelectedIndex = 0; // select the first drive by default
                FolderList.Nodes[0].ImageIndex = 1; // set to open folder icon
                FolderList.Nodes[0].SelectedImageIndex = 1;
            }
            else {
                // No drives found, force close the form
                MessageBox.Show("No drives found. The application will now exit.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }

        #region Event Handlers
        private void FolderList_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e) {
            // open selected folder by deleting all sibling nodes after this 
            // node, then adding the subdirectories of this node
            if (e.Node == null) {
                return;
            }
            TreeNode parent = e.Node.Parent;
            if (parent != null) {
                TreeNode node = parent.LastNode;
                while (node != e.Node) {
                    parent.Nodes.Remove(node);
                    node = parent.LastNode;
                    if (node is null) {
                        Debug.Assert(false);
                        break;
                    }
                }
            }
            e.Node.Nodes.Clear();
            AddSubdirectories(e.Node, (string)e.Node.Tag);
            e.Node.Expand();
            FolderList.SelectedNode = e.Node;
            e.Node.ImageIndex = 1; // set to open folder icon
            e.Node.SelectedImageIndex = 1;
        }

        private void FolderList_AfterSelect(object sender, TreeViewEventArgs e) {
            // refresh file list to display all *.COM files in the selected folder
            if (e.Node == null) {
                return;
            }
            FileList.BeginUpdate();
            FileList.Items.Clear();
            foreach (var file in Directory.GetFiles((string)e.Node.Tag, "*.COM")) {
                FileList.Items.Add(Path.GetFileName(file));
            }
            AGIVersion.Text = GetAGIVersion(); // update version info for the new folder
            if (!isAGIfolder) {
                // also no loader
                isLoader = false;
            }
            if (FileList.Items.Count > 0) {
                FileList.SelectedIndex = 0; // select the first file by default
            }
            else {
                // no loader
                LoaderVersion.Text = "";
            }
            FileList.EndUpdate();
        }

        private void DriveList_SelectedIndexChanged(object sender, EventArgs e) {
            // update folder tree
            FolderList.Nodes.Clear();
            // add root node for the selected drive
            if (DriveList.SelectedItem == null) {
                return; // no drive selected
            }
            // remove the " [VolumeLabel]" part from the selected drive string
            string selectedDrive = ((string)DriveList.SelectedItem).Split(' ')[0]; // get drive letter
            TreeNode rootNode = new TreeNode(selectedDrive);

            // add root node to the tree
            rootNode.Tag = selectedDrive;
            FolderList.Nodes.Add(rootNode);
            // add subdirectories
            AddSubdirectories(rootNode, selectedDrive);
            FolderList.SelectedNode = rootNode;
            rootNode.Expand();
        }

        private void FileList_SelectedIndexChanged(object sender, EventArgs e) {
            // update version for the file clicked
            LoaderVersion.Text = GetLoaderVersion();
            if (isLoader) {
                DecryptButton.Text = "Remove Encryption and Modify Loader";
            }
            else {
                // if not a loader file, disable decryption
                DecryptButton.Text = "Remove Encryption";
            }
        }

        private void DecryptButton_Click(object sender, EventArgs e) {
            // this will break the key to the agi file,
            // modify the load file, build a decrypted version of agi,
            // and save the agi key to a text file

            UseWaitCursor = true;

            // first, break key to the agi file
            byte[] key = BreakKey();

            //now decrypt with this string
            DecryptAGI(key);

            UseWaitCursor = false;
        }

        private void HelpButton_Click(object sender, EventArgs e) {
            MessageBox.Show("This program is designed to decrypt the AGI game engine file " +
                            "used in Sierra's early adventure games. To use it, select the " +
                            "drive where your AGI game is installed, navigate to the game's " +
                            "folder, and select the loader file (if available). Then click the " +
                            "'Remove Encryption' button to decrypt the AGI file and modify " +
                            "the loader if applicable. The decryption key will also be saved to " +
                            "a text file for future reference.",
                            "AGI Decrypter Help",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
        }
        #endregion

        #region Methods
        static void AddSubdirectories(TreeNode parentNode, string path) {
            try {
                foreach (var dir in Directory.GetDirectories(path)) {
                    // don't add hidden or system directories
                    var dirInfo = new DirectoryInfo(dir);
                    if ((dirInfo.Attributes & (FileAttributes.Hidden | FileAttributes.System)) != 0) {
                        continue;
                    }
                    TreeNode childNode = new TreeNode(Path.GetFileName(dir))
                    {
                        Tag = dir, // store full path in Tag
                        ImageIndex = 0,
                        SelectedImageIndex = 0
                    };
                    parentNode.Nodes.Add(childNode);
                }
            }
            catch (UnauthorizedAccessException) {
                // skip directories we don't have access to
            }
        }

        byte[] BreakKey() {
            // Since the decrypted third 128 byte segment is always all zeros,
            // the encryption key can be gained from this 128 byte segment. 
            // The segment just needs to be shifted LEFT twice, and the result
            // will be the key!
            //
            // ***not QUITE true - Because of a bug in the decryption process,
            // the carry flag is not reset when decryption begins a new round,
            // and the last bit flag check doesn't do anything if the bit is
            // zero. In situations where the carry flag is 1 at start and last
            // bit is 0, the last bit gets overwritten as a 1 instead of a
            // zero. The upshot of this is that after two rotations, if the
            // last byte of the key ends in xxxxxx11, it is possible that it
            // should be xxxxxx01. It's easy to check by comparing the value
            // of the key byte to byte 128 of the encrypted file; that byte
            // is always zero when decrypted, so that byte will either match
            // the keybyte (and key is fine, or it will have bit 6 cleared,
            // and it should replace the key byte.
            // (I may need to work on this explanation so it is more clear.)

            string folderPath = (string)FolderList.SelectedNode.Tag;
            byte[] agidata = File.ReadAllBytes(folderPath + "\\agi");
            //get third 128 byte segment fill key array
            byte[] key = agidata[256..384];

            // cache the true last byte of the key (from key position 127, same
            // as data position 383)
            byte bytKeyEnd = agidata[127];

            // rotate  key LEFT twice
            for (int j = 0; j < 2; j++) {
                // the carry flag is high bit of first byte
                carryflag = (key[0] & 128) != 0;
                for (int i = 127; i >= 0; i--) {
                    key[i] = RCL(key[i]);
                }
            }

            // check last byte of key, and correct it if necessary
            if (key[127] != bytKeyEnd) {
                // should ONLY happen if this key byte has bits 6 and 7 set (xxxxxx11)
                Debug.Assert((key[127] & 3) == 3);
                // difference SHOULD be that bit 6 is cleared in real key value
                Debug.Assert((key[127] ^ bytKeyEnd) == 2);
                // replace last key byte with correct value
                key[127] = bytKeyEnd;
            }

            // if selected file is a valid loader file
            bool modifyLoader;
            if (isLoader) {
                // loader file is okay to modify; the key will be added
                // to the modified loader so it will automatically decrypt
                // the agi file
                modifyLoader = true;
                string loaderName = (string)FileList.SelectedItem;

                // copy the com file to the modified com file
                try {
                    File.Copy(folderPath + "\\" + loaderName,
                        folderPath + "\\" + Path.GetFileNameWithoutExtension(loaderName) + "M.COM");
                }
                catch {
                    // unable to copy file, so don't modify loader
                    modifyLoader = false;
                }
            }
            else {
                modifyLoader = false;
            }

            // save the key in a text file
            string keytext = "";
            for (int i = 0; i < 128; i++) {
                // convert key value to double-digit hex value
                keytext += key[i].ToString("X2");
                if (((i + 1) % 16) == 0) {
                    keytext += Environment.NewLine + Environment.NewLine;
                }
                else if (((i + 1) % 8) == 0) {
                    keytext += "-";
                }
                else {
                    keytext += " ";
                }
            }
            try {
                File.WriteAllText(folderPath + "\\KEY.TXT", keytext);
            }
            catch {
                // ignore errors in writing key file, since decryption
                // can still proceed without it; just won't have the key
                // for future reference
            }
            if (modifyLoader) {
                // get a stream for the modified loader file
                try {
                    BinaryWriter bw = new BinaryWriter(
                        File.Open(folderPath + "\\" +
                        Path.GetFileNameWithoutExtension((string)FileList.SelectedItem) +
                        "M.COM", FileMode.Open, FileAccess.ReadWrite));
                    // store key in sierra.com
                    for (int i = 0; i < 128; i++) {
                        bw.Seek(65 + i, SeekOrigin.Begin);
                        bw.Write(key[i]);
                    }
                    // put in modifications so it won't ask for original disks
                    bw.Seek(0x779, SeekOrigin.Begin);
                    byte[] datablock = [0x90, 0x90, 0x90, 0x90, 0x90, 0x90];
                    bw.Write(datablock);
                    bw.Seek(0x789, SeekOrigin.Begin);
                    datablock = [0x90, 0x90, 0x90, 0x90, 0x90, 0xB8, 1, 0];
                    bw.Write(datablock);
                    bw.Close();
                    bw.Dispose();
                }
                catch {
                    // ignore errors
                }
            }
            return key;
        }

        void DecryptAGI(byte[] key) {
            byte[] agidata = File.ReadAllBytes((string)FolderList.SelectedNode.Tag + "\\agi");

            // reset carry flag
            carryflag = false;
            int keypos = 0;
            for (int pos = 0; pos < agidata.Length; pos++) {
                // get this byte
                byte value = agidata[pos];
                // xor it with key
                value = (byte)(value ^ key[keypos]);
                // and restore it in the data array
                agidata[pos] = value;

                // rotate right, including carry flag:
                key[keypos] = RCR(key[keypos]);
                keypos++;
                if (keypos == 128) {
                    // if 128 bytes read, reset key
                    keypos = 0;
                    if (carryflag) {
                        // add it back to position 1
                        key[0] = (byte)(key[0] | 128);
                    }

                }
            }
            // write data to new file 'agi.exe' in the same folder
            try {
                File.WriteAllBytes((string)FolderList.SelectedNode.Tag + "\\agi.exe", agidata);
            }
            catch {
                // ignore errors in writing decrypted file, since decryption
                // was successful; just won't have the decrypted file saved
            }
        }

        string GetAGIVersion() {
            // attempt to extract version from agidata.ovl
            // which will establish whether this is an AGI folder or not,
            // and if so, what version of AGI; also checks for encryption
            // of the agi.exe/agi files

            if (FolderList.SelectedNode == null) {
                DecryptButton.Enabled = isAGIfolder = false;
                return "Not an AGI folder";
            }
            string file = (string)FolderList.SelectedNode.Tag + "\\agidata.ovl";
            // verify file exists,
            if (!File.Exists(file)) {
                DecryptButton.Enabled = isAGIfolder = false;
                return "Not an AGI folder";
            }
            try {
                byte[] filedata = File.ReadAllBytes(file);
                // find the word 'VERSION' in the file
                bool foundVersion = false;
                int pos = 0;
                do {
                    if (Encoding.ASCII.GetString(filedata, pos, 7).Equals(
                        "VERSION", StringComparison.OrdinalIgnoreCase)) {
                        foundVersion = true;
                        break;
                    }
                    pos++;
                } while (pos < filedata.Length - 7);

                // if not found,
                if (!foundVersion) {
                    DecryptButton.Enabled = isAGIfolder = false;
                    return "Not an AGI folder";
                }
                else {
                    // extract version info
                    pos += 8;
                    string strVersion = "";
                    do {
                        if (filedata[pos] == 0 || strVersion.Length >= 32) {
                            break;
                        }
                        strVersion += (char)filedata[pos];
                        pos++;
                    } while (true);

                    // if 32 characters read,
                    if (strVersion.Length == 32 || strVersion.Length == 0) {
                        // this is probably not a valid version
                        DecryptButton.Enabled = isAGIfolder = false;
                        return "Not an AGI folder";
                    }
                    // now check for encryption of the agi/agi.exe file
                    // check for agi.exe first
                    file = (string)FolderList.SelectedNode.Tag + "\\agi.exe";
                    if (File.Exists(file)) {
                        // if first two bytes are 'MZ' assume it's unencrypted
                        try {
                            BinaryReader br = new(File.OpenRead(file));
                            byte v1 = br.ReadByte();
                            byte v2 = br.ReadByte();
                            br.Close();
                            br.Dispose();
                            if (v1 == 'M' && v2 == 'Z') {
                                // not encrypted (although it is an AGI folder, don't enable
                                // decryption since it doesn't need it)
                                DecryptButton.Enabled = isAGIfolder = false;
                                return "AGI Version: " + strVersion + " (not encrypted)";
                            }
                        }
                        catch {
                            // assume not a valid folder
                            DecryptButton.Enabled = isAGIfolder = false;
                            return "Not an AGI folder";
                        }
                    }
                    // check for just 'agi'
                    file = (string)FolderList.SelectedNode.Tag + "\\agi";
                    if (File.Exists(file)) {
                        // check for 'MZ' as start of file
                        BinaryReader br = new(File.OpenRead(file));
                        byte v1 = br.ReadByte();
                        byte v2 = br.ReadByte();
                        br.Close();
                        br.Dispose();
                        if (v1 == 'M' && v2 == 'Z') {
                            // not encrypted (although it is an AGI folder, don't enable
                            // decryption since it doesn't need it)
                            DecryptButton.Enabled = isAGIfolder = false;
                            return "AGI Version: " + strVersion + " (not encrypted)";
                        }
                        else {
                            DecryptButton.Enabled = isAGIfolder = true;
                            return "AGI Version: " + strVersion + " (ENCRYPTED)";
                        }
                    }
                    else {
                        // no agi file found
                        DecryptButton.Enabled = isAGIfolder = false;
                        return "AGI Version: " + strVersion + " (agi file missing)";
                    }
                }
            }
            catch {
                DecryptButton.Enabled = isAGIfolder = false;
                return "File access error";
            }
        }

        string GetLoaderVersion() {
            //attempt to extract version from agidata.ovl

            string folderPath = (FolderList.SelectedNode != null) ?
                (string)FolderList.SelectedNode.Tag : "";

            // if no not an AGI folder (no 'agi' file in selected folder),
            // or no loader file selected, return empty string
            if (!isAGIfolder) {
                return "";
            }
            if (FileList.Items.Count == 0 ||
                FileList.SelectedIndex == -1) {
                return "No loader file found";
            }
            if (FileList.SelectedItem is null) {
                return "No loader file found";
            }
            // try to extract version info from this file
            try {
                byte[] filedata = File.ReadAllBytes(folderPath + "\\" + (string)FileList.SelectedItem);
                // find the word 'loader'
                bool foundLoader = false;
                int pos = 0;
                do {
                    // if the word 'loader' is found,
                    if (Encoding.ASCII.GetString(filedata, pos, 6).Equals(
                        "LOADER", StringComparison.CurrentCultureIgnoreCase)) {
                        foundLoader = true;
                        break;
                    }
                    pos++;
                } while (pos < filedata.Length - 6);

                // if 'loader' was NOT found
                if (!foundLoader) {
                    return "Not a Loader file";
                }

                // now extract version info
                pos = pos + 7;
                string strVersion = "";
                do {
                    // add character
                    strVersion += (char)filedata[pos];
                    pos = pos + 1;
                } while ((char)filedata[pos] != ' ' && strVersion.Length < 32);

                // if  32 characters read,
                if (strVersion.Length == 32 || strVersion.Length == 0) {
                    // not a loader file
                    return "Not a Loader file";
                }
                // return the version as found
                // (only v3.0 can be modified by this program)
                isLoader = strVersion == "v3.0";
                return "Loader Version: " + strVersion;
            }
            catch {
                // any errors
                return "File access error";
            }
        }

        // ROTATE-CARRY-LEFT
        byte RCL(byte value) {
            // set temporary carry flag, if bit 7 is set
            bool carry = (value & 128) == 128;

            // rotate once to left
            byte retval = (byte)(value << 1);

            // if carry flag is set,
            if (carryflag) {
                // roll it into bit 0
                retval = (byte)(retval | 1);
            }
            // put temporary cf value into permanent value
            carryflag = carry;
            return retval;
        }

        // ROTATE-CARRY-RIGHT
        byte RCR(byte bytIn) {
            // must include a module variable called CF for this to work
            // set temporary carry flag, if bit 0 is set
            bool carry = (bytIn & 1) == 1;

            // clear out bit 0 (AND it with '11111110')
            byte retval = (byte)(bytIn & 0xFE);

            // rotate once to right
            retval = (byte)(retval >> 1);

            // if carry flag is set,
            if (carryflag) {
                // roll it into bit 8
                retval = (byte)(retval | 128);
            }

            // save temporary cf value into permanent value
            carryflag = carry;
            return retval;
        }
        #endregion
    }
}
