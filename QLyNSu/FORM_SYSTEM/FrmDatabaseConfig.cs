using System;
using System.Collections.Generic;
using System.Configuration;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using System.IO;
using Newtonsoft.Json;
using System.Linq;
using Oracle.ManagedDataAccess.Client;

namespace QLyNSu.FORM_SYSTEM
{
    public partial class FrmDatabaseConfig : XtraForm
    {
        private string GetLocalIPAddress()
        {
            try
            {
                var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        return ip.ToString();
                    }
                }
            }
            catch { }
            return "localhost";
        }


        private readonly string _overrideFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "QLyNSu_DBConfig.json");
        private DatabaseConfigData _configData = new DatabaseConfigData();
        private bool _isBinding = false;
        private bool _isAdding = false;

        public FrmDatabaseConfig()
        {
            InitializeComponent();
            WireEvents();
        }

        private void WireEvents()
        {
            this.Load += FrmDatabaseConfig_Load;
            lstProfiles.SelectedIndexChanged += LstProfiles_SelectedIndexChanged;
            
            chkSID.CheckedChanged += ChkIdentifierType_CheckedChanged;
            chkServiceName.CheckedChanged += ChkIdentifierType_CheckedChanged;
            
            cboAuthType.SelectedIndexChanged += CboAuthType_SelectedIndexChanged;
            
            btnHelp.Click += (s, e) => { System.Diagnostics.Process.Start("https://docs.oracle.com/en/database/"); };
            
            btnAdd.Click += BtnAdd_Click;
            btnEdit.Click += BtnEdit_Click;
            btnDelete.Click += BtnDelete_Click;
            btnClose.Click += BtnClose_Click;
            
            btnClear.Click += BtnClear_Click;
            btnCancel.Click += BtnCancel_Click;
            btnSave.Click += BtnSave_Click;
            btnTestConnection.Click += BtnTestConnection_Click;
            btnConnect.Click += BtnConnect_Click;
        }

        private void FrmDatabaseConfig_Load(object sender, EventArgs e)
        {
            LoadData();
        }

        private void SetMode(bool isEditing)
        {
            xtraTabControl1.Enabled = isEditing;
            xtraTabControl2.Enabled = isEditing;
            txtProfileName.Enabled = isEditing;
            cboDbType.Enabled = isEditing;
            btnColor.Enabled = isEditing;
            cboConnType.Enabled = isEditing;
            
            btnSave.Visible = isEditing;
            btnCancel.Visible = isEditing;
            btnClear.Visible = isEditing;
            btnTestConnection.Visible = isEditing;

            btnAdd.Visible = !isEditing;
            btnEdit.Visible = !isEditing;
            btnDelete.Visible = !isEditing;
            btnClose.Visible = !isEditing;
            
            lstProfiles.Enabled = !isEditing;
            btnConnect.Visible = !isEditing;
            btnApply.Visible = !isEditing;
        }

        private void LoadData()
        {
            try
            {
                if (File.Exists(_overrideFile))
                {
                    string json = File.ReadAllText(_overrideFile);
                    _configData = JsonConvert.DeserializeObject<DatabaseConfigData>(json) ?? new DatabaseConfigData();
                }
                else
                {
                    CreateDefaultProfile();
                }

                RefreshList();

                if (!string.IsNullOrEmpty(_configData.ActiveProfileId))
                {
                    var active = _configData.Profiles.FirstOrDefault(p => p.Id == _configData.ActiveProfileId);
                    if (active != null)
                        lstProfiles.SelectedItem = active;
                }
                else if (lstProfiles.ItemCount > 0)
                {
                    lstProfiles.SelectedIndex = 0;
                }
                SetMode(false);
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("L?i d?c c?u h�nh: " + ex.Message, "L?i", MessageBoxButtons.OK, MessageBoxIcon.Error);
                CreateDefaultProfile();
                RefreshList();
                SetMode(false);
            }
        }

        private void CreateDefaultProfile()
        {
            _configData = new DatabaseConfigData();
            var profile = new DatabaseProfile
            {
                Id = Guid.NewGuid().ToString(),
                ProfileName = "Local XE",
                ServerIP = GetLocalIPAddress(),
                Port = "1521",
                Database = "xe",
                IdentifierType = "SID",
                Username = "SYS",
                Password = "",
                Role = "SYSDBA",
                AuthType = "Default"
            };
            _configData.Profiles.Add(profile);
            _configData.ActiveProfileId = profile.Id;
            SaveConfigToFile();
        }

        private void SaveConfigToFile()
        {
            try
            {
                File.WriteAllText(_overrideFile, JsonConvert.SerializeObject(_configData, Formatting.Indented));
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("L?i ghi file c?u h�nh: " + ex.Message, "L?i", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RefreshList()
        {
            _isBinding = true;
            lstProfiles.Items.Clear();
            foreach (var p in _configData.Profiles)
            {
                lstProfiles.Items.Add(p);
            }
            _isBinding = false;
        }

        private void LstProfiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isBinding) return;

            var profile = lstProfiles.SelectedItem as DatabaseProfile;
            if (profile != null)
            {
                txtProfileName.Text = profile.ProfileName;
                cboDbType.Text = "Oracle";
                
                cboAuthType.Text = profile.AuthType;
                txtUsername.Text = profile.Username;
                txtPassword.Text = profile.Password;
                cboRole.Text = profile.Role;
                chkSavePassword.Checked = profile.SavePassword;

                cboConnType.Text = "Basic";
                txtHostname.Text = profile.ServerIP;
                txtPort.Text = profile.Port;
                
                if (profile.IdentifierType == "ServiceName")
                {
                    chkServiceName.Checked = true;
                    txtServiceName.Text = profile.Database;
                    txtSID.Text = "";
                }
                else
                {
                    chkSID.Checked = true;
                    txtSID.Text = profile.Database;
                    txtServiceName.Text = "";
                }
                
                if (!string.IsNullOrEmpty(profile.ColorHex))
                {
                    btnColor.Color = System.Drawing.ColorTranslator.FromHtml(profile.ColorHex);
                }
                else
                {
                    btnColor.Color = System.Drawing.Color.Empty;
                }
                
                lblStatus.Text = "Status : Ready";
            }
        }

        private void ChkIdentifierType_CheckedChanged(object sender, EventArgs e)
        {
            if (chkSID.Checked) // SID
            {
                txtSID.Enabled = true;
                txtServiceName.Enabled = false;
                if (!_isBinding) txtServiceName.Text = "";
            }
            else // Service Name
            {
                txtSID.Enabled = false;
                txtServiceName.Enabled = true;
                if (!_isBinding) txtSID.Text = "";
            }
        }

        private void CboAuthType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboAuthType.Text == "OS")
            {
                txtUsername.Enabled = false;
                txtPassword.Enabled = false;
            }
            else
            {
                txtUsername.Enabled = true;
                txtPassword.Enabled = true;
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            _isAdding = true;
            lstProfiles.SelectedIndex = -1;
            BtnClear_Click(null, null);
            SetMode(true);
            txtProfileName.Focus();
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (lstProfiles.SelectedItem == null)
            {
                XtraMessageBox.Show("Vui l�ng ch?n m?t k?t n?i d? s?a.", "Th�ng b�o", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            _isAdding = false;
            SetMode(true);
            txtProfileName.Focus();
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            var profile = lstProfiles.SelectedItem as DatabaseProfile;
            if (profile != null)
            {
                if (XtraMessageBox.Show($"B?n c� ch?c mu?n xo� k?t n?i '{profile.ProfileName}'?", "X�c nh?n", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    _configData.Profiles.Remove(profile);
                    if (_configData.ActiveProfileId == profile.Id)
                        _configData.ActiveProfileId = null;
                    RefreshList();
                    if (lstProfiles.ItemCount > 0)
                        lstProfiles.SelectedIndex = 0;
                    else
                        BtnClear_Click(null, null);
                    
                    SaveConfigToFile();
                }
            }
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            txtProfileName.Text = "";
            txtUsername.Text = "";
            txtPassword.Text = "";
            txtHostname.Text = GetLocalIPAddress();
            txtPort.Text = "1521";
            txtSID.Text = "xe";
            txtServiceName.Text = "";
            chkSID.Checked = true;
            cboRole.Text = "default";
            cboAuthType.Text = "Default";
            chkSavePassword.Checked = false;
            btnColor.Color = System.Drawing.Color.Empty;
            lblStatus.Text = "Status : Ready";
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            SetMode(false);
            LstProfiles_SelectedIndexChanged(null, null);
        }
        
        private void BtnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void SaveCurrentFormToProfile(DatabaseProfile profile)
        {
            profile.ProfileName = string.IsNullOrEmpty(txtProfileName.Text) ? "Kh�ng T�n" : txtProfileName.Text.Trim();
            profile.ServerIP = string.IsNullOrEmpty(txtHostname.Text) ? "localhost" : txtHostname.Text.Trim();
            profile.Port = string.IsNullOrEmpty(txtPort.Text) ? "1521" : txtPort.Text.Trim();
            
            profile.IdentifierType = chkServiceName.Checked ? "ServiceName" : "SID";
            profile.Database = profile.IdentifierType == "ServiceName" ? txtServiceName.Text.Trim() : txtSID.Text.Trim();
            
            profile.AuthType = cboAuthType.Text;
            profile.Username = txtUsername.Text.Trim();
            profile.Password = txtPassword.Text.Trim(); // Ideally should be encrypted
            profile.Role = cboRole.Text;
            profile.SavePassword = chkSavePassword.Checked;
            
            profile.ColorHex = btnColor.Color != System.Drawing.Color.Empty ? System.Drawing.ColorTranslator.ToHtml(btnColor.Color) : "";
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtProfileName.Text.Trim()))
            {
                XtraMessageBox.Show("Name cannot be empty.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (cboAuthType.Text == "Default" && string.IsNullOrEmpty(txtUsername.Text.Trim()))
            {
                XtraMessageBox.Show("Username cannot be empty when AuthType is Default.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var profile = lstProfiles.SelectedItem as DatabaseProfile;
            if (_isAdding || profile == null)
            {
                // check if name already exists
                if (_configData.Profiles.Any(p => p.ProfileName.Equals(txtProfileName.Text.Trim(), StringComparison.OrdinalIgnoreCase)))
                {
                    XtraMessageBox.Show("A connection with this name already exists.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                profile = new DatabaseProfile { Id = Guid.NewGuid().ToString() };
                _configData.Profiles.Add(profile);
            }
            else
            {
                if (_configData.Profiles.Any(p => p.Id != profile.Id && p.ProfileName.Equals(txtProfileName.Text.Trim(), StringComparison.OrdinalIgnoreCase)))
                {
                    XtraMessageBox.Show("A connection with this name already exists.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            
            SaveCurrentFormToProfile(profile);
            SaveConfigToFile();
            RefreshList();
            lstProfiles.SelectedItem = profile;
            
            _isAdding = false;
            lblStatus.Text = "Status : Saved";
            SetMode(false);
        }

        private string BuildConnectionString()
        {
            string host = string.IsNullOrEmpty(txtHostname.Text) ? "localhost" : txtHostname.Text.Trim();
            string port = string.IsNullOrEmpty(txtPort.Text) ? "1521" : txtPort.Text.Trim();
            string identifier = chkServiceName.Checked ? txtServiceName.Text.Trim() : txtSID.Text.Trim();
            string user = txtUsername.Text.Trim();
            string pass = txtPassword.Text.Trim();
            string role = cboRole.Text;

            string connString = "";
            
            if (chkServiceName.Checked) // ServiceName
            {
                connString = $"DATA SOURCE={host}:{port}/{identifier};";
            }
            else // SID
            {
                connString = $"DATA SOURCE=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST={host})(PORT={port}))(CONNECT_DATA=(SID={identifier})));";
            }

            if (cboAuthType.Text == "OS")
            {
                connString += "USER ID=/;";
            }
            else
            {
                connString += $"USER ID={user};PASSWORD={pass};";
            }

            if (role == "SYSDBA")
            {
                connString += "DBA PRIVILEGE=SYSDBA;";
            }
            else if (role == "SYSOPER")
            {
                connString += "DBA PRIVILEGE=SYSOPER;";
            }

            return connString;
        }

        private bool PerformTest()
        {
            if (string.IsNullOrEmpty(txtProfileName.Text.Trim()) || (cboAuthType.Text == "Default" && string.IsNullOrEmpty(txtUsername.Text.Trim())))
            {
                lblStatus.Text = "Status : Failure - Missing required fields";
                return false;
            }

            try
            {
                string connString = BuildConnectionString();
                using (var conn = new OracleConnection(connString))
                {
                    conn.Open();
                    lblStatus.Text = "Status : Success";
                    return true;
                }
            }
            catch (OracleException ex)
            {
                lblStatus.Text = $"Status : Failure - ORA-{ex.Number}: {ex.Message}";
                return false;
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Status : Failure - " + ex.Message;
                return false;
            }
        }

        private void BtnTestConnection_Click(object sender, EventArgs e)
        {
            PerformTest();
        }

        private void BtnConnect_Click(object sender, EventArgs e)
        {
            if (PerformTest())
            {
                var profile = lstProfiles.SelectedItem as DatabaseProfile;
                if (profile != null)
                {
                    _configData.ActiveProfileId = profile.Id;
                    SaveConfigToFile();

                    XtraMessageBox.Show($"Connecting to '{profile.ProfileName}'. Application will restart to apply the connection.", "Connect", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Application.Restart();
                    Environment.Exit(0);
                }
            }
        }
    }

    public class DatabaseProfile
    {
        public string Id { get; set; }
        public string ProfileName { get; set; }
        public string ServerIP { get; set; }
        public string Port { get; set; }
        public string Database { get; set; } // We keep this as the value of SID/Service Name
        public string Username { get; set; }
        public string Password { get; set; }
        
        // New properties for Oracle SQL Developer style
        public string Role { get; set; } = "default";
        public bool SavePassword { get; set; } = false;
        public string AuthType { get; set; } = "Default";
        public string IdentifierType { get; set; } = "SID"; // "SID" or "ServiceName"
        public string ColorHex { get; set; } = "";

        public override string ToString()
        {
            return ProfileName;
        }
    }

    public class DatabaseConfigData
    {
        public string ActiveProfileId { get; set; }
        public List<DatabaseProfile> Profiles { get; set; } = new List<DatabaseProfile>();
    }
}
