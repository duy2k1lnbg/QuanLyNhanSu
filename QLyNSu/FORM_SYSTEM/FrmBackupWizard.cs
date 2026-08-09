using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Oracle.ManagedDataAccess.Client;
using DevExpress.XtraEditors;
using DA;
using System.Data.Common;
using System.Xml;

namespace QLyNSu.FORM_SYSTEM
{
    public partial class FrmBackupWizard : DevExpress.XtraEditors.XtraForm
    {
        private DevExpress.XtraEditors.CheckEdit chkHeaderData;
        private DevExpress.XtraEditors.LabelControl lblDelimiter;
        private DevExpress.XtraEditors.ComboBoxEdit cboDelimiter;
        private DevExpress.XtraEditors.LabelControl lblLeftEnclosure;
        private DevExpress.XtraEditors.ComboBoxEdit cboLeftEnclosure;
        private DevExpress.XtraEditors.LabelControl lblRightEnclosure;
        private DevExpress.XtraEditors.ComboBoxEdit cboRightEnclosure;
        private DevExpress.XtraEditors.LabelControl lblDataWorksheetName;
        private DevExpress.XtraEditors.TextEdit txtDataWorksheetName;
        private DevExpress.XtraEditors.CheckEdit chkQueryWorksheetName;
        private DevExpress.XtraEditors.TextEdit txtQueryWorksheetName;

        public FrmBackupWizard()
        {
            InitializeComponent();
            this.Load += FrmBackupWizard_Load;
            
            btnSelectAll.Click += BtnSelectAll_Click;
            btnUnselectAll.Click += BtnUnselectAll_Click;
            btnCancel.Click += (s, e) => this.Close();
            
            cboSaveAs.SelectedIndexChanged += CboSaveAs_SelectedIndexChanged;
            txtPath.ButtonClick += TxtPath_ButtonClick;
            btnStart.Click += BtnStart_Click;
            
            chkExportDDL.CheckedChanged += (s, e) => UpdateUI();
            chkExportData.CheckedChanged += (s, e) => UpdateUI();
        }

        private void UpdateUI()
        {
            bool exportDDL = chkExportDDL.Checked;
            grpExportDDL.Enabled = exportDDL;
            
            bool exportData = chkExportData.Checked;
            grpExportData.Enabled = exportData;
            
            UpdateFormatUI();
        }

        private void UpdateFormatUI()
        {
            if (chkHeaderData == null) return;
            string f = cboFormat.Text;
            
            bool isCsv = f == "csv" || f == "delimited";
            bool isText = f == "text";
            bool isExcel = f.Contains("excel 2003+") || f.Contains("excel 95-2003");
            bool isLoader = f == "loader";
            bool isInsert = f == "insert" || f == "sql";
            
            // Default visibility resets
            chkHeaderData.Visible = false;
            lblDelimiter.Visible = false;
            cboDelimiter.Visible = false;
            lblLeftEnclosure.Visible = false;
            cboLeftEnclosure.Visible = false;
            lblRightEnclosure.Visible = false;
            cboRightEnclosure.Visible = false;
            lblDataWorksheetName.Visible = false;
            txtDataWorksheetName.Visible = false;
            chkQueryWorksheetName.Visible = false;
            txtQueryWorksheetName.Visible = false;
            lblLineTerminator.Visible = false;
            cboLineTerminator.Visible = false;
            chkIncludeCommitEvery.Visible = false;
            spinCommitRows.Visible = false;
            lblRows.Visible = false;
            chkShowSchemaData.Visible = false;

            // Positioning Logic
            int y1 = 15; // Line 1
            int y2 = 50; // Line 2
            int y3 = 85; // Line 3
            
            int x1Label = 15;
            int x1Control = 120;
            int x2Label = 300;
            int x2Control = 405;
            
            if (isCsv)
            {
                chkHeaderData.Location = new System.Drawing.Point(x2Label, y1);
                chkHeaderData.Visible = true;
                
                lblDelimiter.Location = new System.Drawing.Point(x1Label, y2 + 3);
                cboDelimiter.Location = new System.Drawing.Point(x1Control, y2);
                lblDelimiter.Visible = true;
                cboDelimiter.Visible = true;
                cboDelimiter.Enabled = f == "delimited";
                if (f == "csv") cboDelimiter.Text = ",";
                
                lblLineTerminator.Location = new System.Drawing.Point(x2Label, y2 + 3);
                cboLineTerminator.Location = new System.Drawing.Point(x2Control, y2);
                lblLineTerminator.Visible = true;
                cboLineTerminator.Visible = true;
                
                lblLeftEnclosure.Location = new System.Drawing.Point(x1Label, y3 + 3);
                cboLeftEnclosure.Location = new System.Drawing.Point(x1Control, y3);
                lblLeftEnclosure.Visible = true;
                cboLeftEnclosure.Visible = true;
                
                lblRightEnclosure.Location = new System.Drawing.Point(x2Label, y3 + 3);
                cboRightEnclosure.Location = new System.Drawing.Point(x2Control, y3);
                lblRightEnclosure.Visible = true;
                cboRightEnclosure.Visible = true;
            }
            else if (isText)
            {
                chkHeaderData.Location = new System.Drawing.Point(x2Label, y1);
                chkHeaderData.Visible = true;
                
                lblDelimiter.Location = new System.Drawing.Point(x1Label, y2 + 3);
                cboDelimiter.Location = new System.Drawing.Point(x1Control, y2);
                lblDelimiter.Visible = true;
                cboDelimiter.Visible = true;
                cboDelimiter.Enabled = false; // Locked for text
                cboDelimiter.Text = "tab";
                
                lblLineTerminator.Location = new System.Drawing.Point(x2Label, y2 + 3);
                cboLineTerminator.Location = new System.Drawing.Point(x2Control, y2);
                lblLineTerminator.Visible = true;
                cboLineTerminator.Visible = true;
                
                lblLeftEnclosure.Location = new System.Drawing.Point(x1Label, y3 + 3);
                cboLeftEnclosure.Location = new System.Drawing.Point(x1Control, y3);
                lblLeftEnclosure.Visible = true;
                cboLeftEnclosure.Visible = true;
                
                lblRightEnclosure.Location = new System.Drawing.Point(x2Label, y3 + 3);
                cboRightEnclosure.Location = new System.Drawing.Point(x2Control, y3);
                lblRightEnclosure.Visible = true;
                cboRightEnclosure.Visible = true;
            }
            else if (isExcel)
            {
                chkHeaderData.Location = new System.Drawing.Point(x2Label, y1);
                chkHeaderData.Visible = true;
                
                lblDataWorksheetName.Location = new System.Drawing.Point(x1Label, y2 + 3);
                txtDataWorksheetName.Location = new System.Drawing.Point(x1Control, y2);
                txtDataWorksheetName.Width = 150;
                lblDataWorksheetName.Visible = true;
                txtDataWorksheetName.Visible = true;
                
                chkQueryWorksheetName.Location = new System.Drawing.Point(x1Label - 5, y3 + 1);
                txtQueryWorksheetName.Location = new System.Drawing.Point(x1Control, y3);
                txtQueryWorksheetName.Width = 150;
                chkQueryWorksheetName.Visible = true;
                txtQueryWorksheetName.Visible = true;
            }
            else if (isLoader)
            {
                lblDelimiter.Location = new System.Drawing.Point(x1Label, y2 + 3);
                cboDelimiter.Location = new System.Drawing.Point(x1Control, y2);
                lblDelimiter.Visible = true;
                cboDelimiter.Visible = true;
                cboDelimiter.Enabled = true;
                
                lblLineTerminator.Location = new System.Drawing.Point(x2Label, y2 + 3);
                cboLineTerminator.Location = new System.Drawing.Point(x2Control, y2);
                lblLineTerminator.Visible = true;
                cboLineTerminator.Visible = true;
                
                lblLeftEnclosure.Location = new System.Drawing.Point(x1Label, y3 + 3);
                cboLeftEnclosure.Location = new System.Drawing.Point(x1Control, y3);
                lblLeftEnclosure.Visible = true;
                cboLeftEnclosure.Visible = true;
                
                lblRightEnclosure.Location = new System.Drawing.Point(x2Label, y3 + 3);
                cboRightEnclosure.Location = new System.Drawing.Point(x2Control, y3);
                lblRightEnclosure.Visible = true;
                cboRightEnclosure.Visible = true;
            }
            else if (isInsert)
            {
                chkShowSchemaData.Location = new System.Drawing.Point(x2Label - 5, y1 + 1);
                chkShowSchemaData.Visible = true;
                
                lblLineTerminator.Location = new System.Drawing.Point(x1Label, y2 + 3);
                cboLineTerminator.Location = new System.Drawing.Point(x1Control, y2);
                lblLineTerminator.Visible = true;
                cboLineTerminator.Visible = true;
                
                chkIncludeCommitEvery.Location = new System.Drawing.Point(x1Label - 5, y3 + 1);
                spinCommitRows.Location = new System.Drawing.Point(x1Control + 25, y3);
                lblRows.Location = new System.Drawing.Point(x1Control + 100, y3 + 3);
                chkIncludeCommitEvery.Visible = true;
                spinCommitRows.Visible = true;
                lblRows.Visible = true;
            }
            else if (f == "excel.xml" || f == "xml")
            {
                lblLineTerminator.Location = new System.Drawing.Point(x1Label, y2 + 3);
                cboLineTerminator.Location = new System.Drawing.Point(x1Control, y2);
                lblLineTerminator.Visible = true;
                cboLineTerminator.Visible = true;
            }
        }

        private bool IsAllCheckedState(string prefix, bool lookForChecked)
        {
            int total = 0;
            int matchState = 0;
            for (int i = 0; i < chkTables.ItemCount; i++)
            {
                var item = chkTables.Items[i];
                if (string.IsNullOrEmpty(prefix) || item.Value.ToString().StartsWith(prefix))
                {
                    total++;
                    if (lookForChecked && item.CheckState == CheckState.Checked) matchState++;
                    else if (!lookForChecked && item.CheckState == CheckState.Unchecked) matchState++;
                }
            }
            return total > 0 && total == matchState;
        }

        private void BtnSelectAll_Click(object sender, EventArgs e)
        {
            var menu = new ContextMenuStrip();
            
            bool isAllFullyChecked = IsAllCheckedState("", true);
            var itemAll = new ToolStripMenuItem("Chọn tất cả", null, (s, ev) => 
            {
                if (isAllFullyChecked) chkTables.UnCheckAll();
                else chkTables.CheckAll();
            });
            itemAll.Checked = isAllFullyChecked;
            menu.Items.Add(itemAll);
            
            menu.Items.Add(new ToolStripSeparator());
            
            string[] types = { "TABLE", "VIEW", "PROCEDURE", "FUNCTION", "TRIGGER", "SEQUENCE" };
            foreach (var t in types)
            {
                bool isFullyChecked = IsAllCheckedState($"[{t}]", true);
                var item = new ToolStripMenuItem($"Tất cả {t}", null, (s, ev) => 
                {
                    if (isFullyChecked) UncheckByType($"[{t}]");
                    else CheckByType($"[{t}]");
                });
                item.Checked = isFullyChecked;
                menu.Items.Add(item);
            }
            
            menu.Show(btnSelectAll, new System.Drawing.Point(0, btnSelectAll.Height));
        }

        private void CheckByType(string prefix)
        {
            for (int i = 0; i < chkTables.ItemCount; i++)
            {
                var item = chkTables.Items[i];
                if (item.Value.ToString().StartsWith(prefix))
                {
                    item.CheckState = CheckState.Checked;
                }
            }
        }

        private void BtnUnselectAll_Click(object sender, EventArgs e)
        {
            var menu = new ContextMenuStrip();
            
            bool isAllFullyUnchecked = IsAllCheckedState("", false);
            var itemAll = new ToolStripMenuItem("Bỏ chọn tất cả", null, (s, ev) => 
            {
                if (isAllFullyUnchecked) chkTables.CheckAll();
                else chkTables.UnCheckAll();
            });
            itemAll.Checked = isAllFullyUnchecked;
            menu.Items.Add(itemAll);
            
            menu.Items.Add(new ToolStripSeparator());
            
            string[] types = { "TABLE", "VIEW", "PROCEDURE", "FUNCTION", "TRIGGER", "SEQUENCE" };
            foreach (var t in types)
            {
                bool isFullyUnchecked = IsAllCheckedState($"[{t}]", false);
                var item = new ToolStripMenuItem($"Bỏ chọn {t}", null, (s, ev) => 
                {
                    if (isFullyUnchecked) CheckByType($"[{t}]");
                    else UncheckByType($"[{t}]");
                });
                item.Checked = isFullyUnchecked;
                menu.Items.Add(item);
            }
            
            menu.Show(btnUnselectAll, new System.Drawing.Point(0, btnUnselectAll.Height));
        }

        private void UncheckByType(string prefix)
        {
            for (int i = 0; i < chkTables.ItemCount; i++)
            {
                var item = chkTables.Items[i];
                if (item.Value.ToString().StartsWith(prefix))
                {
                    item.CheckState = CheckState.Unchecked;
                }
            }
        }

        private void FrmBackupWizard_Load(object sender, EventArgs e)
        {
            cboConnection.Properties.Items.Clear();
            cboConnection.Properties.Items.Add("my_db");
            cboConnection.SelectedIndex = 0;

            cboVersion.Properties.Items.Clear();
            cboVersion.Properties.Items.AddRange(new object[] {
                "COMPATIBLE", "LATEST", "10.1", "10.2", "11.1", "11.2", "12.1", "12.2"
            });
            cboVersion.SelectedIndex = 0;

            cboFormat.Properties.Items.Clear();
            cboFormat.Properties.Items.AddRange(new object[] {
                "csv", "delimited", "excel 2003+ ( xlsx )", "excel 95-2003 ( xls )", "excel.xml", 
                "fixed", "html", "insert", "json", "json-formatted", "loader", "text", "xml"
            });
            cboFormat.SelectedItem = "insert";

            cboEncoding.Properties.Items.Clear();
            cboEncoding.Properties.Items.AddRange(new object[] {
                "UTF-8", "Cp1252", "ASCII", "Shift_JIS", "MS932", "UTF-16"
            });
            cboEncoding.SelectedItem = "Cp1252";

            cboSaveAs.Properties.Items.Clear();
            cboSaveAs.Properties.Items.AddRange(new object[] {
                "Single File", "Separate Files", "Type Files", "Separate Directories", "Worksheet", "Clipboard"
            });
            cboSaveAs.SelectedIndex = 0;

            cboLineTerminator.Properties.Items.Clear();
            cboLineTerminator.Properties.Items.AddRange(new object[] {
                "environment default", "platform default", "Unix/Mac LF", "Windows CR LF", "CR", "{EOL}"
            });
            cboLineTerminator.SelectedIndex = 0;

            chkShowSchemaData.Checked = true;
            chkIncludeCommitEvery.Checked = false;
            spinCommitRows.Value = 100;

            chkCompressed.Checked = false;

            chkExportData.Checked = true;
            chkExportDDL.Checked = true;

            // DDL Default settings
            chkAddByteKeyword.Checked = true;
            chkAddForceToViews.Checked = true;
            chkCascadeDrops.Checked = false;
            chkDependents.Checked = false;
            chkDrops.Checked = false;
            chkGrants.Checked = true;
            chkPartitioning.Checked = true;
            chkPrettyPrint.Checked = true;
            chkShowSchema.Checked = true;
            chkStorage.Checked = true;
            chkTablespace.Checked = true;
            chkTerminator.Checked = true;

            // Initialize Dynamic UI Controls
            chkHeaderData = new DevExpress.XtraEditors.CheckEdit() { Text = "Header" };
            chkHeaderData.Properties.Caption = "Header";
            chkHeaderData.Checked = true;
            
            lblDelimiter = new DevExpress.XtraEditors.LabelControl() { Text = "Delimiter:" };
            cboDelimiter = new DevExpress.XtraEditors.ComboBoxEdit();
            cboDelimiter.Properties.Items.AddRange(new object[] { ",", "|", ";", "\\t" });
            cboDelimiter.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.Standard;
            cboDelimiter.Text = ",";
            cboDelimiter.Size = new System.Drawing.Size(80, 22);
            
            lblLeftEnclosure = new DevExpress.XtraEditors.LabelControl() { Text = "Left Enclosure:" };
            cboLeftEnclosure = new DevExpress.XtraEditors.ComboBoxEdit();
            cboLeftEnclosure.Properties.Items.AddRange(new object[] { "\"", "'", "none" });
            cboLeftEnclosure.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            cboLeftEnclosure.Text = "\"";
            cboLeftEnclosure.Size = new System.Drawing.Size(80, 22);
            
            lblRightEnclosure = new DevExpress.XtraEditors.LabelControl() { Text = "Right Enclosure:" };
            cboRightEnclosure = new DevExpress.XtraEditors.ComboBoxEdit();
            cboRightEnclosure.Properties.Items.AddRange(new object[] { "\"", "'", "none" });
            cboRightEnclosure.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            cboRightEnclosure.Text = "\"";
            cboRightEnclosure.Size = new System.Drawing.Size(80, 22);
            
            lblDataWorksheetName = new DevExpress.XtraEditors.LabelControl() { Text = "Data Worksheet Name:" };
            txtDataWorksheetName = new DevExpress.XtraEditors.TextEdit();
            
            chkQueryWorksheetName = new DevExpress.XtraEditors.CheckEdit() { Text = "Query Worksheet Name:" };
            chkQueryWorksheetName.Properties.Caption = "Query Worksheet Name:";
            txtQueryWorksheetName = new DevExpress.XtraEditors.TextEdit();
            
            grpExportData.Controls.AddRange(new Control[] {
                chkHeaderData, lblDelimiter, cboDelimiter, lblLeftEnclosure, cboLeftEnclosure,
                lblRightEnclosure, cboRightEnclosure, lblDataWorksheetName, txtDataWorksheetName,
                chkQueryWorksheetName, txtQueryWorksheetName
            });
            
            cboFormat.SelectedIndexChanged += (s, ev) => UpdateFormatUI();

            UpdateUI();
            LoadTables();
        }

        private void LoadTables()
        {
            try
            {
                using (var db = new DA.MyEntities())
                {
                    chkTables.Items.Clear();
                    var objects = db.Database.SqlQuery<string>("SELECT '[' || OBJECT_TYPE || '] ' || OBJECT_NAME FROM USER_OBJECTS WHERE OBJECT_TYPE IN ('TABLE', 'VIEW', 'PROCEDURE', 'FUNCTION', 'TRIGGER', 'SEQUENCE') AND OBJECT_NAME NOT LIKE 'BIN$%' ORDER BY OBJECT_TYPE, OBJECT_NAME").ToList();
                    foreach (var obj in objects)
                    {
                        chkTables.Items.Add(obj, true);
                    }
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Lỗi khi tải danh sách bảng: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CboSaveAs_SelectedIndexChanged(object sender, EventArgs e)
        {
            string saveAs = cboSaveAs.Text;
            if (saveAs == "Worksheet" || saveAs == "Clipboard")
            {
                txtPath.Text = "";
                txtPath.Enabled = false;
                chkCompressed.Enabled = false;
                chkCompressed.Checked = false;
            }
            else
            {
                txtPath.Enabled = true;
                chkCompressed.Enabled = true;
            }
        }

        private void TxtPath_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            if (!txtPath.Enabled) return;
            
            string saveAs = cboSaveAs.Text;
            if (saveAs == "Single File")
            {
                using (var sfd = new SaveFileDialog())
                {
                    string ext = GetExtensionForFormat(cboFormat.Text);
                    sfd.Filter = $"{ext.ToUpper()} Files (*.{ext})|*.{ext}";
                    sfd.FileName = $"export_{DateTime.Now:yyyyMMdd_HHmmss}.{ext}";
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        txtPath.Text = sfd.FileName;
                    }
                }
            }
            else // Separate Files, Type Files, Separate Directories
            {
                using (var fbd = new FolderBrowserDialog())
                {
                    fbd.Description = "Chọn thư mục lưu dữ liệu";
                    if (fbd.ShowDialog() == DialogResult.OK)
                    {
                        txtPath.Text = fbd.SelectedPath;
                    }
                }
            }
        }

        public class ExportOptions
        {
            public List<string> Tables { get; set; }
            public string Path { get; set; }
            public string SaveAs { get; set; }
            public string Format { get; set; }
            public string Encoding { get; set; }
            public string LineTerminator { get; set; }
            public bool Compressed { get; set; }
            
            public bool ExportDDL { get; set; }
            public bool DdlAddByteKeyword { get; set; }
            public bool DdlForceToViews { get; set; }
            public bool DdlCascadeDrops { get; set; }
            public bool DdlDependents { get; set; }
            public bool DdlDrops { get; set; }
            public bool DdlGrants { get; set; }
            public bool DdlPartitioning { get; set; }
            public bool DdlPrettyPrint { get; set; }
            public bool DdlShowSchema { get; set; }
            public bool DdlStorage { get; set; }
            public bool DdlTablespace { get; set; }
            public bool DdlTerminator { get; set; }
            public string DdlVersion { get; set; }
            
            public bool ExportData { get; set; }
            public bool DataShowSchema { get; set; }
            public bool DataIncludeCommit { get; set; }
            public int DataCommitRows { get; set; }
            
            public bool DataHeader { get; set; }
            public string DataDelimiter { get; set; }
            public string DataLeftEnclosure { get; set; }
            public string DataRightEnclosure { get; set; }
            public string DataWorksheetName { get; set; }
            public bool DataQueryWorksheetName { get; set; }
        }

        private async void BtnStart_Click(object sender, EventArgs e)
        {
            string saveAs = cboSaveAs.Text;
            if (saveAs != "Worksheet" && saveAs != "Clipboard" && string.IsNullOrWhiteSpace(txtPath.Text))
            {
                XtraMessageBox.Show("Vui lòng chọn đường dẫn lưu file!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cboFormat.Text == "excel 2003+ ( xlsx )" || cboFormat.Text == "excel 95-2003 ( xls )")
            {
                if (saveAs == "Single File" || saveAs == "Type Files" || saveAs == "Worksheet" || saveAs == "Clipboard")
                {
                    XtraMessageBox.Show("Định dạng Excel Native (.xlsx, .xls) chỉ hỗ trợ xuất dưới dạng 'Separate Files' hoặc 'Separate Directories'. Vui lòng chọn lại cấu trúc lưu trữ.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            var selectedTables = chkTables.CheckedItems.Cast<DevExpress.XtraEditors.Controls.CheckedListBoxItem>()
                                          .Select(x => x.Value.ToString()).ToList();
                                          
            if (selectedTables.Count == 0)
            {
                XtraMessageBox.Show("Vui lòng chọn ít nhất một bảng để sao lưu!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnStart.Enabled = false;
            btnCancel.Enabled = false;
            progressBar.Properties.Maximum = selectedTables.Count;
            progressBar.Position = 0;

            var opt = new ExportOptions
            {
                Tables = selectedTables,
                Path = txtPath.Text,
                SaveAs = saveAs,
                Format = cboFormat.Text,
                Encoding = cboEncoding.Text,
                LineTerminator = cboLineTerminator.Text,
                Compressed = chkCompressed.Checked,
                
                ExportDDL = chkExportDDL.Checked,
                DdlAddByteKeyword = chkAddByteKeyword.Checked,
                DdlForceToViews = chkAddForceToViews.Checked,
                DdlCascadeDrops = chkCascadeDrops.Checked,
                DdlDependents = chkDependents.Checked,
                DdlDrops = chkDrops.Checked,
                DdlGrants = chkGrants.Checked,
                DdlPartitioning = chkPartitioning.Checked,
                DdlPrettyPrint = chkPrettyPrint.Checked,
                DdlShowSchema = chkShowSchema.Checked,
                DdlStorage = chkStorage.Checked,
                DdlTablespace = chkTablespace.Checked,
                DdlTerminator = chkTerminator.Checked,
                DdlVersion = cboVersion.Text,
                
                ExportData = chkExportData.Checked,
                DataShowSchema = chkShowSchemaData.Checked,
                DataIncludeCommit = chkIncludeCommitEvery.Checked,
                DataCommitRows = (int)spinCommitRows.Value,
                
                DataHeader = chkHeaderData?.Checked ?? true,
                DataDelimiter = cboDelimiter?.Text ?? ",",
                DataLeftEnclosure = cboLeftEnclosure?.Text == "none" ? "" : (cboLeftEnclosure?.Text ?? "\""),
                DataRightEnclosure = cboRightEnclosure?.Text == "none" ? "" : (cboRightEnclosure?.Text ?? "\""),
                DataWorksheetName = txtDataWorksheetName?.Text ?? "",
                DataQueryWorksheetName = chkQueryWorksheetName?.Checked ?? false
            };

            try
            {
                string inMemoryResult = await Task.Run(() => PerformExport(opt));
                
                if (opt.SaveAs == "Worksheet")
                {
                    using (var frm = new FrmWorksheet("Export Data", inMemoryResult))
                    {
                        frm.ShowDialog();
                    }
                }
                else if (opt.SaveAs == "Clipboard")
                {
                    if (inMemoryResult.Length > 50 * 1024 * 1024) // 50MB warning
                    {
                        if (XtraMessageBox.Show("Dữ liệu xuất ra quá lớn (hơn 50MB). Việc copy vào Clipboard có thể gây treo máy. Bạn có muốn tiếp tục?", "Cảnh báo", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                        {
                            Clipboard.SetText(inMemoryResult);
                            XtraMessageBox.Show("Đã copy dữ liệu vào Clipboard!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    else
                    {
                        Clipboard.SetText(inMemoryResult);
                        XtraMessageBox.Show("Đã copy dữ liệu vào Clipboard!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    XtraMessageBox.Show($"Sao lưu hoàn tất!\nĐã lưu tại: {opt.Path}", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Có lỗi xảy ra: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnStart.Enabled = true;
                btnCancel.Enabled = true;
                lblStatus.Text = "Sẵn sàng...";
                progressBar.Position = 0;
            }
        }

        private string BoolToSql(bool value) => value ? "TRUE" : "FALSE";
        private string GetLineTerminator(string opt)
        {
            if (opt == "Unix/Mac LF" || opt == "LF") return "\n";
            if (opt == "Windows CR LF" || opt == "CRLF") return "\r\n";
            if (opt == "CR") return "\r";
            return Environment.NewLine;
        }
        
        private Encoding GetEncoding(string opt)
        {
            if (opt == "Cp1252") return Encoding.GetEncoding(1252);
            if (opt == "ASCII") return Encoding.ASCII;
            if (opt == "UTF-16") return Encoding.Unicode;
            if (opt == "Shift_JIS") return Encoding.GetEncoding(932);
            if (opt == "MS932") return Encoding.GetEncoding(932);
            return new UTF8Encoding(false); // UTF-8 without BOM
        }

        private string PerformExport(ExportOptions opt)
        {
            Encoding encoding = GetEncoding(opt.Encoding);
            string newline = GetLineTerminator(opt.LineTerminator);
            bool isMemoryOutput = (opt.SaveAs == "Worksheet" || opt.SaveAs == "Clipboard");
            bool isSingleFile = (opt.SaveAs == "Single File");
            bool isTypeFiles = (opt.SaveAs == "Type Files");
            bool isSeparateDirs = (opt.SaveAs == "Separate Directories");
            
            string outputDir = opt.Path;
            string tempDir = null;
            
            if (opt.Compressed && !isMemoryOutput)
            {
                tempDir = Path.Combine(Path.GetTempPath(), "Export_" + Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(tempDir);
                if (!isSingleFile) outputDir = tempDir;
            }

            var activeWriters = new Dictionary<string, TextWriter>();
            StringWriter memoryWriter = null;
            if (isMemoryOutput)
            {
                memoryWriter = new StringWriter();
                memoryWriter.NewLine = newline;
            }

            TextWriter GetWriter(string objectType, string tableName)
            {
                if (isMemoryOutput) return memoryWriter;

                string filePath = "";
                bool isData = objectType == "Data";
                string fileExt = isData ? GetExtensionForFormat(opt.Format) : "sql";
                
                string typeSuffix = "";

                if (isSingleFile)
                {
                    filePath = opt.Compressed ? Path.Combine(tempDir, Path.GetFileName(opt.Path)) : opt.Path;
                }
                else if (isTypeFiles)
                {
                    string typeName = objectType.ToLower() + "s";
                    if (objectType == "Data" || objectType == "Table") typeName = "tables";
                    filePath = Path.Combine(outputDir, $"{typeName}.{fileExt}");
                }
                else // Separate Files & Separate Directories
                {
                    string dir = outputDir;
                    if (isSeparateDirs)
                    {
                        dir = Path.Combine(outputDir, objectType + "s");
                        if (objectType == "Data") dir = Path.Combine(outputDir, "Tables");
                        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                    }
                    filePath = Path.Combine(dir, $"{tableName}{typeSuffix}.{fileExt}");
                }

                if (!activeWriters.TryGetValue(filePath, out var writer))
                {
                    writer = new StreamWriter(filePath, false, encoding);
                    writer.NewLine = newline;
                    activeWriters[filePath] = writer;
                    
                    if (opt.ExportData && (objectType == "Data" || objectType == "Table" || objectType == "View")) 
                    {
                        WriteDataHeader(writer, opt.Format, opt.Tables.Count > 1 && (isSingleFile || isTypeFiles));
                    }
                }
                return writer;
            }

            void CloseTableWriters()
            {
                if (isMemoryOutput || isSingleFile || isTypeFiles) return;
                
                foreach (var kvp in activeWriters)
                {
                    if (opt.ExportData && kvp.Key.EndsWith($".{GetExtensionForFormat(opt.Format)}"))
                    {
                        WriteDataFooter(kvp.Value, opt.Format, false);
                    }
                    kvp.Value.Dispose();
                }
                activeWriters.Clear();
            }

            using (var db = new DA.MyEntities())
            {
                var con = db.Database.Connection;
                if (con.State != ConnectionState.Open) con.Open();
                
                string schemaName = "";
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText = "SELECT SYS_CONTEXT('USERENV', 'CURRENT_SCHEMA') FROM DUAL";
                    schemaName = cmd.ExecuteScalar()?.ToString();
                }

                if (opt.ExportDDL) SetupDdlTransforms(con, opt);

                int tableIndex = 0;
                foreach (var objStr in opt.Tables)
                {
                    tableIndex++;
                    string objType = "TABLE";
                    string objName = objStr;
                    if (objStr.StartsWith("["))
                    {
                        int idx = objStr.IndexOf(']');
                        if (idx > 0)
                        {
                            objType = objStr.Substring(1, idx - 1);
                            objName = objStr.Substring(idx + 1).Trim();
                        }
                    }

                    UpdateProgress($"Đang xuất {objType.ToLower()}: {objName} ({tableIndex}/{opt.Tables.Count})", tableIndex);

                    if (opt.Format == "excel 2003+ ( xlsx )" || opt.Format == "excel 95-2003 ( xls )")
                    {
                        if (objType != "TABLE" && objType != "VIEW") continue;
                        
                        string ext = GetExtensionForFormat(opt.Format);
                        string dir = isSeparateDirs ? Path.Combine(outputDir, "Tables") : outputDir;
                        if (isSeparateDirs && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
                        string filePath = Path.Combine(dir, $"{objName}.{ext}");
                            
                        if (opt.ExportData) ExportExcelNative(con, objName, opt, filePath);
                        continue;
                    }

                    if (opt.ExportDDL)
                    {
                        ExportDDLForObject(con, objType, objName, schemaName, opt, GetWriter);
                    }

                    if (opt.ExportData && (objType == "TABLE" || objType == "VIEW"))
                    {
                        var dataWriter = GetWriter("Data", objName);
                        ExportDataForTable(con, dataWriter, objName, schemaName, opt, tableIndex, opt.Tables.Count);
                    }

                    CloseTableWriters();
                }

                foreach (var kvp in activeWriters)
                {
                    if (opt.ExportData && kvp.Key.EndsWith($".{GetExtensionForFormat(opt.Format)}")) 
                    {
                        WriteDataFooter(kvp.Value, opt.Format, opt.Tables.Count > 1 && (isSingleFile || isTypeFiles));
                    }
                    kvp.Value.Dispose();
                }
                activeWriters.Clear();
            }

            // Perform Zip Compression
            if (opt.Compressed && !isMemoryOutput)
            {
                UpdateProgress("Đang nén dữ liệu...", opt.Tables.Count);
                string zipPath = opt.Path;
                if (!zipPath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase)) zipPath += ".zip";
                
                if (File.Exists(zipPath)) File.Delete(zipPath);
                ZipFile.CreateFromDirectory(tempDir, zipPath, CompressionLevel.Optimal, false);
                Directory.Delete(tempDir, true);
            }
            
            return isMemoryOutput ? memoryWriter.ToString() : string.Empty;
        }

        private string GetExtensionForFormat(string format)
        {
            if (format.Contains("excel 2003+")) return "xlsx";
            if (format.Contains("excel 95-2003")) return "xls";
            if (format == "excel.xml") return "xml";
            if (format.StartsWith("json")) return "json";
            if (format == "csv" || format == "delimited") return "csv";
            if (format == "html") return "html";
            if (format == "xml") return "xml";
            if (format == "loader") return "ctl";
            if (format == "text" || format == "fixed") return "txt";
            return "sql";
        }

        private void SetupDdlTransforms(DbConnection con, ExportOptions opt)
        {
            try
            {
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText = "BEGIN " + 
                        $"DBMS_METADATA.SET_TRANSFORM_PARAM(DBMS_METADATA.SESSION_TRANSFORM, 'SQLTERMINATOR', {BoolToSql(opt.DdlTerminator)}); " +
                        $"DBMS_METADATA.SET_TRANSFORM_PARAM(DBMS_METADATA.SESSION_TRANSFORM, 'PRETTY', {BoolToSql(opt.DdlPrettyPrint)}); " +
                        $"DBMS_METADATA.SET_TRANSFORM_PARAM(DBMS_METADATA.SESSION_TRANSFORM, 'EMIT_SCHEMA', {BoolToSql(opt.DdlShowSchema)}); " +
                        $"DBMS_METADATA.SET_TRANSFORM_PARAM(DBMS_METADATA.SESSION_TRANSFORM, 'STORAGE', {BoolToSql(opt.DdlStorage)}); " +
                        $"DBMS_METADATA.SET_TRANSFORM_PARAM(DBMS_METADATA.SESSION_TRANSFORM, 'TABLESPACE', {BoolToSql(opt.DdlTablespace)}); " +
                        $"DBMS_METADATA.SET_TRANSFORM_PARAM(DBMS_METADATA.SESSION_TRANSFORM, 'PARTITIONING', {BoolToSql(opt.DdlPartitioning)}); " +
                        $"DBMS_METADATA.SET_TRANSFORM_PARAM(DBMS_METADATA.SESSION_TRANSFORM, 'SIZE_BYTE_KEYWORD', {BoolToSql(opt.DdlAddByteKeyword)}); " +
                        $"DBMS_METADATA.SET_TRANSFORM_PARAM(DBMS_METADATA.SESSION_TRANSFORM, 'FORCE', {BoolToSql(opt.DdlForceToViews)}); " +
                        $"DBMS_METADATA.SET_TRANSFORM_PARAM(DBMS_METADATA.SESSION_TRANSFORM, 'CONSTRAINTS', {BoolToSql(opt.DdlDependents)}); " +
                        $"DBMS_METADATA.SET_TRANSFORM_PARAM(DBMS_METADATA.SESSION_TRANSFORM, 'REF_CONSTRAINTS', {BoolToSql(opt.DdlDependents)}); " +
                        "END;";
                    cmd.ExecuteNonQuery();
                }
            }
            catch { /* Ignore if no privileges */ }
        }

        private void ExportDDLForObject(DbConnection con, string objType, string objName, string schemaName, ExportOptions opt, Func<string, string, TextWriter> getWriter)
        {
            string niceType = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(objType.ToLower());
            var writer = getWriter(niceType, objName);
            
            if (opt.DdlDrops && writer != null)
            {
                string dropTarget = opt.DdlShowSchema ? $"\"{schemaName}\".\"{objName}\"" : $"\"{objName}\"";
                string cascade = (objType == "TABLE" && opt.DdlCascadeDrops) ? " CASCADE CONSTRAINTS" : "";
                writer.WriteLine($"DROP {objType} {dropTarget}{cascade};");
            }

            if (writer != null)
            {
                using (var cmdDDL = con.CreateCommand())
                {
                    cmdDDL.CommandText = $"SELECT DBMS_METADATA.GET_DDL('{objType}', '{objName}') FROM DUAL";
                    try
                    {
                        var ddl = cmdDDL.ExecuteScalar()?.ToString();
                        if (!string.IsNullOrEmpty(ddl)) writer.WriteLine(ddl.TrimEnd());
                    }
                    catch { }
                }
            }
            
            if (opt.DdlGrants)
            {
                var grantWriter = getWriter("Grant", objName);
                if (grantWriter != null)
                {
                    using (var cmdGrants = con.CreateCommand())
                    {
                        cmdGrants.CommandText = $"SELECT DBMS_METADATA.GET_DEPENDENT_DDL('OBJECT_GRANT', '{objName}') FROM DUAL";
                        try { grantWriter.WriteLine(cmdGrants.ExecuteScalar()?.ToString().TrimEnd()); } catch { }
                    }
                }
            }
            
            if (opt.DdlDependents && objType == "TABLE")
            {
                var indexWriter = getWriter("Index", objName);
                if (indexWriter != null)
                {
                    using (var cmdIdx = con.CreateCommand())
                    {
                        cmdIdx.CommandText = $"SELECT INDEX_NAME FROM USER_INDEXES WHERE TABLE_NAME = '{objName}' AND INDEX_TYPE NOT LIKE '%LOB%' AND GENERATED = 'N'";
                        using (var r = cmdIdx.ExecuteReader())
                        {
                            while (r.Read())
                            {
                                string idx = r.GetString(0);
                                using (var c2 = con.CreateCommand())
                                {
                                    c2.CommandText = $"SELECT DBMS_METADATA.GET_DDL('INDEX', '{idx}') FROM DUAL";
                                    try { indexWriter.WriteLine(c2.ExecuteScalar()?.ToString().TrimEnd()); } catch { }
                                }
                            }
                        }
                    }
                }
                
                var triggerWriter = getWriter("Trigger", objName);
                if (triggerWriter != null)
                {
                    using (var cmdTrg = con.CreateCommand())
                    {
                        cmdTrg.CommandText = $"SELECT TRIGGER_NAME FROM USER_TRIGGERS WHERE TABLE_NAME = '{objName}'";
                        using (var r = cmdTrg.ExecuteReader())
                        {
                            while (r.Read())
                            {
                                string trg = r.GetString(0);
                                using (var c2 = con.CreateCommand())
                                {
                                    c2.CommandText = $"SELECT DBMS_METADATA.GET_DDL('TRIGGER', '{trg}') FROM DUAL";
                                    try { triggerWriter.WriteLine(c2.ExecuteScalar()?.ToString().TrimEnd()); } catch { }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void WriteDataHeader(TextWriter writer, string format, bool isMultipleTablesSingleFile)
        {
            if (format.StartsWith("json"))
            {
                writer.WriteLine(isMultipleTablesSingleFile ? "{" : "[");
            }
            else if (format == "xml")
            {
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                writer.WriteLine("<DATA>");
            }
            else if (format == "html")
            {
                writer.WriteLine("<html><body>");
            }
            else if (format.Contains("excel"))
            {
                writer.WriteLine("<?xml version=\"1.0\"?>");
                writer.WriteLine("<?mso-application progid=\"Excel.Sheet\"?>");
                writer.WriteLine("<Workbook xmlns=\"urn:schemas-microsoft-com:office:spreadsheet\"");
                writer.WriteLine(" xmlns:o=\"urn:schemas-microsoft-com:office:office\"");
                writer.WriteLine(" xmlns:x=\"urn:schemas-microsoft-com:office:excel\"");
                writer.WriteLine(" xmlns:ss=\"urn:schemas-microsoft-com:office:spreadsheet\"");
                writer.WriteLine(" xmlns:html=\"http://www.w3.org/TR/REC-html40\">");
            }
        }

        private void WriteDataFooter(TextWriter writer, string format, bool isMultipleTablesSingleFile)
        {
            if (format.StartsWith("json"))
            {
                writer.WriteLine(isMultipleTablesSingleFile ? "}" : "]");
            }
            else if (format == "xml")
            {
                writer.WriteLine("</DATA>");
            }
            else if (format == "html")
            {
                writer.WriteLine("</body></html>");
            }
            else if (format.Contains("excel"))
            {
                writer.WriteLine("</Workbook>");
            }
        }

        private void ExportDataForTable(DbConnection con, TextWriter writer, string table, string schemaName, ExportOptions opt, int tableIndex, int totalTables)
        {
            string f = opt.Format;
            bool isFirstRow = true;
            int rowCount = 0;

            if (f.StartsWith("json") && (opt.SaveAs == "Single File" || opt.SaveAs == "Worksheet" || opt.SaveAs == "Clipboard") && totalTables > 1)
            {
                writer.WriteLine($"\"{table}\": [");
            }
            else if (f == "html")
            {
                writer.WriteLine($"<h2>{table}</h2><table border='1'>");
            }
            else if (f.Contains("excel"))
            {
                string sheetName = string.IsNullOrEmpty(opt.DataWorksheetName) ? table : opt.DataWorksheetName;
                if (!string.IsNullOrEmpty(opt.DataWorksheetName) && totalTables > 1) sheetName += $"_{tableIndex}";
                
                writer.WriteLine($" <Worksheet ss:Name=\"{sheetName}\">");
                writer.WriteLine("  <Table>");
            }
            else if (f == "xml" && totalTables > 1)
            {
                writer.WriteLine($"<{table}>");
            }

            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = $"SELECT * FROM \"{table}\"";
                using (var reader = cmd.ExecuteReader())
                {
                    int fieldCount = reader.FieldCount;
                    var cols = new List<string>();
                    for (int i = 0; i < fieldCount; i++) cols.Add(reader.GetName(i));
                    
                    int fixedWidth = 30; // Default for fixed width

                    // Write Headers for certain formats
                    if ((f == "csv" || f == "delimited" || f == "text") && opt.DataHeader)
                    {
                        string headerSep = opt.DataDelimiter;
                        if (f == "csv") headerSep = ",";
                        else if (f == "text") headerSep = "\t";
                        else if (headerSep == "\\t") headerSep = "\t";
                        
                        writer.WriteLine(string.Join(headerSep, cols.Select(c => $"{opt.DataLeftEnclosure}{c}{opt.DataRightEnclosure}")));
                    }
                    else if (f == "html")
                    {
                        writer.WriteLine("<tr>" + string.Join("", cols.Select(c => $"<th>{c}</th>")) + "</tr>");
                    }
                    else if (f.Contains("excel"))
                    {
                        if (opt.DataHeader)
                        {
                            writer.WriteLine("   <Row>");
                            foreach (var c in cols)
                                writer.WriteLine($"    <Cell><Data ss:Type=\"String\">{System.Security.SecurityElement.Escape(c)}</Data></Cell>");
                            writer.WriteLine("   </Row>");
                        }
                    }
                    else if (f == "loader")
                    {
                        string sep = opt.DataDelimiter;
                        if (sep == "\\t") sep = "\t";
                        string lEnc = opt.DataLeftEnclosure;
                        string encStr = string.IsNullOrEmpty(lEnc) ? "" : $" OPTIONALLY ENCLOSED BY '{lEnc}'";
                        writer.WriteLine($"LOAD DATA\nINFILE *\nINTO TABLE \"{table}\"\nFIELDS TERMINATED BY '{sep}'{encStr}\n(" + string.Join(", ", cols) + "\n)\nBEGINDATA");
                    }
                    else if (f == "fixed")
                    {
                        writer.WriteLine(string.Join("", cols.Select(c => c.PadRight(fixedWidth))));
                    }
                    // text logic is handled above in csv block.

                    while (reader.Read())
                    {
                        rowCount++;
                        var vals = new List<string>();
                        for (int i = 0; i < fieldCount; i++)
                        {
                            vals.Add(reader.IsDBNull(i) ? null : reader.GetValue(i).ToString());
                        }

                        if (f == "insert")
                        {
                            string target = opt.DataShowSchema ? $"\"{schemaName}\".\"{table}\"" : $"\"{table}\"";
                            var sqlVals = new List<string>();
                            for (int i = 0; i < fieldCount; i++)
                            {
                                if (reader.IsDBNull(i)) sqlVals.Add("NULL");
                                else
                                {
                                    var type = reader.GetFieldType(i);
                                    if (type == typeof(string)) sqlVals.Add($"'{reader.GetString(i).Replace("'", "''")}'");
                                    else if (type == typeof(DateTime)) sqlVals.Add($"TO_DATE('{reader.GetDateTime(i):yyyy-MM-dd HH:mm:ss}', 'YYYY-MM-DD HH24:MI:SS')");
                                    else sqlVals.Add(reader.GetValue(i).ToString());
                                }
                            }
                            writer.WriteLine($"INSERT INTO {target} ({string.Join(",", cols)}) VALUES ({string.Join(",", sqlVals)});");
                            
                            if (opt.DataIncludeCommit && rowCount % opt.DataCommitRows == 0)
                            {
                                writer.WriteLine("COMMIT;");
                            }
                        }
                        else if (f == "csv" || f == "delimited" || f == "loader" || f == "text")
                        {
                            string sep = opt.DataDelimiter;
                            if (f == "csv") sep = ",";
                            else if (f == "text") sep = "\t";
                            else if (sep == "\\t") sep = "\t";
                            
                            var csvVals = vals.Select(v => v == null ? "" : $"{opt.DataLeftEnclosure}{v.Replace("\"", "\"\"")}{opt.DataRightEnclosure}");
                            writer.WriteLine(string.Join(sep, csvVals));
                        }
                        else if (f.StartsWith("json"))
                        {
                            if (!isFirstRow) writer.WriteLine(",");
                            var props = new List<string>();
                            for (int i = 0; i < fieldCount; i++)
                            {
                                string v = reader.IsDBNull(i) ? "null" : JsonConvertValue(reader.GetValue(i), reader.GetFieldType(i));
                                props.Add($"\"{cols[i]}\": {v}");
                            }
                            if (f == "json-formatted")
                                writer.Write("  {\n    " + string.Join(",\n    ", props) + "\n  }");
                            else
                                writer.Write("{" + string.Join(", ", props) + "}");
                        }
                        else if (f == "xml")
                        {
                            writer.WriteLine("  <ROW>");
                            for (int i = 0; i < fieldCount; i++)
                            {
                                string v = reader.IsDBNull(i) ? "" : System.Security.SecurityElement.Escape(reader.GetValue(i).ToString());
                                writer.WriteLine($"    <{cols[i]}>{v}</{cols[i]}>");
                            }
                            writer.WriteLine("  </ROW>");
                        }
                        else if (f == "html")
                        {
                            writer.WriteLine("<tr>" + string.Join("", vals.Select(v => $"<td>{System.Security.SecurityElement.Escape(v ?? "")}</td>")) + "</tr>");
                        }
                        else if (f.Contains("excel"))
                        {
                            writer.WriteLine("   <Row>");
                            foreach (var v in vals)
                            {
                                string safeVal = System.Security.SecurityElement.Escape(v ?? "");
                                writer.WriteLine($"    <Cell><Data ss:Type=\"String\">{safeVal}</Data></Cell>");
                            }
                            writer.WriteLine("   </Row>");
                        }
                        else if (f == "fixed")
                        {
                            writer.WriteLine(string.Join("", vals.Select(v => (v ?? "").PadRight(fixedWidth))));
                        }

                        isFirstRow = false;
                    }
                }
            }

            if (f == "insert" && opt.DataIncludeCommit && rowCount % opt.DataCommitRows != 0)
            {
                writer.WriteLine("COMMIT;");
            }

            if (f.StartsWith("json") && (opt.SaveAs == "Single File" || opt.SaveAs == "Worksheet" || opt.SaveAs == "Clipboard") && totalTables > 1)
            {
                writer.WriteLine();
                writer.Write("]");
                if (tableIndex < totalTables) writer.Write(",");
                writer.WriteLine();
            }
            else if (f == "html")
            {
                writer.WriteLine("</table>");
            }
            else if (f.Contains("excel"))
            {
                writer.WriteLine("  </Table>");
                writer.WriteLine(" </Worksheet>");
            }
            else if (f == "xml" && totalTables > 1)
            {
                writer.WriteLine($"</{table}>");
            }
        }

        private string JsonConvertValue(object value, Type type)
        {
            if (type == typeof(string))
            {
                return "\"" + value.ToString().Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r") + "\"";
            }
            if (type == typeof(DateTime))
            {
                return "\"" + ((DateTime)value).ToString("yyyy-MM-ddTHH:mm:ss") + "\"";
            }
            if (type == typeof(bool)) return value.ToString().ToLower();
            if (type == typeof(decimal) || type == typeof(int) || type == typeof(double) || type == typeof(float)) return value.ToString();
            
            return "\"" + value.ToString().Replace("\"", "\\\"") + "\"";
        }

        private void ExportExcelNative(DbConnection con, string table, ExportOptions opt, string path)
        {
            var dt = new DataTable();
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = $"SELECT * FROM \"{table}\"";
                using (var reader = cmd.ExecuteReader())
                {
                    dt.Load(reader);
                }
            }

            if (opt.Format != "excel 2003+ ( xlsx )" && dt.Rows.Count > 65535)
            {
                throw new Exception($"Bảng '{table}' có {dt.Rows.Count} dòng, vượt quá giới hạn 65,535 dòng của định dạng Excel 95-2003 (.xls) cũ. Vui lòng chọn định dạng 'excel 2003+ ( xlsx )' để xuất dữ liệu này!");
            }

            using (var grid = new DevExpress.XtraGrid.GridControl())
            {
                grid.BindingContext = new System.Windows.Forms.BindingContext();
                var view = new DevExpress.XtraGrid.Views.Grid.GridView(grid);
                grid.MainView = view;
                grid.DataSource = dt;
                grid.ForceInitialize();
                view.PopulateColumns();

                if (opt.Format == "excel 2003+ ( xlsx )")
                {
                    grid.ExportToXlsx(path);
                }
                else
                {
                    grid.ExportToXls(path);
                }
            }
        }

        private void UpdateProgress(string text, int position)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => UpdateProgress(text, position)));
                return;
            }
            lblStatus.Text = text;
            progressBar.Position = position;
        }
    }
}
