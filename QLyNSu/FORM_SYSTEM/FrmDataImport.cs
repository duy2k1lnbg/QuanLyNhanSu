using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace QLyNSu.FORM_SYSTEM
{
    public partial class FrmDataImport : DevExpress.XtraEditors.XtraForm
    {
        public FrmDataImport()
        {
            InitializeComponent();
            InitData();
            InitEvents();
            UpdateUI();
        }

        private void InitData()
        {
            cboConnection.Properties.Items.Add("my_db (Default Connection)");
            cboConnection.SelectedIndex = 0;

            cboEncoding.Properties.Items.Add("UTF-8");
            cboEncoding.Properties.Items.Add("Cp1252 (Match Export)");
            cboEncoding.SelectedIndex = 0;

            cboFormat.Properties.Items.Add("insert (SQL Script)");
            cboFormat.Properties.Items.Add("json (JSON Data)");
            cboFormat.SelectedIndex = 0; // Default to SQL for this advanced UI

            cboSourceSchema.Properties.Items.Add("DEV");
            cboSourceSchema.SelectedIndex = 0;
            cboTargetSchema.Properties.Items.Add("DEV");
            cboTargetSchema.SelectedIndex = 0;

            cboErrorHandling.Properties.Items.Add("Continue and Log Error");
            cboErrorHandling.Properties.Items.Add("Abort on Error");
            cboErrorHandling.SelectedIndex = 0;

            spinCommitRows.Value = 100;

            chkImportDDL.Checked = true;
            chkExecuteDDL.Checked = true;
            chkDropExisting.Checked = true;
            chkIgnoreCreateErrors.Checked = true;
            
            chkExecuteDataInsertion.Checked = true;
            rdoTruncate.Checked = true;
            chkDisableConstraints.Checked = true;
            chkDisableTriggers.Checked = true;
            chkCommitEvery.Checked = true;

            txtLogFileOutput.Text = Path.Combine(Application.StartupPath, "import_results.log");
        }

        private DevExpress.XtraEditors.CheckEdit chkIncludeCommitEvery { get { return chkCommitEvery; } }

        private void InitEvents()
        {
            txtImportFile.ButtonClick += TxtImportFile_ButtonClick;
            txtLogFileOutput.ButtonClick += TxtLogFileOutput_ButtonClick;
            chkImportDDL.CheckedChanged += (s, e) => UpdateUI();
            chkExecuteDataInsertion.CheckedChanged += (s, e) => UpdateUI();
            chkRemapSchema.CheckedChanged += (s, e) => UpdateUI();
            chkCommitEvery.CheckedChanged += (s, e) => UpdateUI();
            chkObjectSelection.CheckedChanged += (s, e) => UpdateUI();

            btnCancel.Click += (s, e) => this.Close();
            btnFinish.Click += BtnFinish_Click;
            btnNext.Click += (s, e) => XtraMessageBox.Show("Chức năng Summary đang được cập nhật.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            btnBack.Click += (s, e) => { };
            btnEditFilter.Click += (s, e) => XtraMessageBox.Show("Chức năng lọc đối tượng đang được cập nhật.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void UpdateUI()
        {
            bool ddlEnabled = chkImportDDL.Checked;
            chkExecuteDDL.Enabled = ddlEnabled;
            chkDropExisting.Enabled = ddlEnabled;
            chkIgnoreCreateErrors.Enabled = ddlEnabled;
            chkIncludeGrants.Enabled = ddlEnabled;
            chkIncludeStorage.Enabled = ddlEnabled;
            chkIncludeTablespace.Enabled = ddlEnabled;
            chkApplyPartitioning.Enabled = ddlEnabled;

            bool dataEnabled = chkExecuteDataInsertion.Checked;
            rdoTruncate.Enabled = dataEnabled;
            rdoAppend.Enabled = dataEnabled;
            rdoReplace.Enabled = dataEnabled;
            chkDisableTriggers.Enabled = dataEnabled;
            chkDisableConstraints.Enabled = dataEnabled;
            grpAdvanced.Enabled = dataEnabled;
            chkCommitEvery.Enabled = dataEnabled;
            chkObjectSelection.Enabled = dataEnabled;
            
            cboSourceSchema.Enabled = chkRemapSchema.Checked;
            cboTargetSchema.Enabled = chkRemapSchema.Checked;
            spinCommitRows.Enabled = chkCommitEvery.Checked && chkCommitEvery.Enabled;
            btnEditFilter.Enabled = chkObjectSelection.Checked && chkObjectSelection.Enabled;
        }

        private void TxtImportFile_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "SQL Files (*.sql)|*.sql|JSON Files (*.json)|*.json|All Files (*.*)|*.*";
                ofd.Title = "Chọn tệp tin nguồn (Import File)";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    txtImportFile.Text = ofd.FileName;
                    if (ofd.FileName.EndsWith(".sql", StringComparison.OrdinalIgnoreCase))
                        cboFormat.SelectedIndex = 0;
                    else if (ofd.FileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                        cboFormat.SelectedIndex = 1;
                }
            }
        }

        private void TxtLogFileOutput_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "Log Files (*.log)|*.log|All Files (*.*)|*.*";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    txtLogFileOutput.Text = sfd.FileName;
                }
            }
        }

        private async void BtnFinish_Click(object sender, EventArgs e)
        {
            // Bước 1: Validation & Connect
            if (string.IsNullOrWhiteSpace(txtImportFile.Text) || !File.Exists(txtImportFile.Text))
            {
                XtraMessageBox.Show("Vui lòng chọn một tệp tin hợp lệ để phục hồi!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var confirm = XtraMessageBox.Show(
                "Bạn có chắc chắn muốn tiến hành Import/Restore dữ liệu không?\nHành động này có thể thay đổi cấu trúc và ghi đè dữ liệu hiện tại.", 
                "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes) return;

            // Gather options
            var options = new RestoreOptions
            {
                FilePath = txtImportFile.Text,
                LogPath = txtLogFileOutput.Text,
                EncodingName = cboEncoding.Text,
                Format = cboFormat.Text,
                
                ImportDDL = chkImportDDL.Checked,
                ExecuteDDL = chkExecuteDDL.Checked,
                DropExisting = chkDropExisting.Checked,
                IgnoreCreateErrors = chkIgnoreCreateErrors.Checked,
                
                ExecuteData = chkExecuteDataInsertion.Checked,
                TruncateFirst = rdoTruncate.Checked,
                ReplaceData = rdoReplace.Checked,
                DisableTriggers = chkDisableTriggers.Checked,
                DisableConstraints = chkDisableConstraints.Checked,
                
                RemapSchema = chkRemapSchema.Checked,
                SourceSchema = cboSourceSchema.Text,
                TargetSchema = cboTargetSchema.Text,
                
                CommitEveryRows = chkCommitEvery.Checked ? Convert.ToInt32(spinCommitRows.Value) : 0,
                AbortOnError = cboErrorHandling.SelectedIndex == 1
            };

            btnFinish.Enabled = false;
            btnCancel.Enabled = false;

            try
            {
                bool success = await Task.Run(() => ExecuteRestore(options));
                if (success)
                    XtraMessageBox.Show("Import successfully completed.\nVui lòng xem file Log để biết thêm chi tiết.", "Hoàn tất", MessageBoxButtons.OK, MessageBoxIcon.Information);
                else
                    XtraMessageBox.Show("Import có lỗi xảy ra. Xem file Log để biết nguyên nhân.", "Hoàn tất kèm Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.Close();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"Đã xảy ra lỗi nghiêm trọng:\n{ex.Message}", "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnFinish.Enabled = true;
                btnCancel.Enabled = true;
            }
        }

        private bool ExecuteRestore(RestoreOptions opt)
        {
            int errorCount = 0;
            // Bước 2: Chuẩn bị Môi trường & Logging
            using (var sw = new StreamWriter(opt.LogPath, false, Encoding.UTF8))
            {
                Action<string> log = (msg) => 
                {
                    string logLine = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {msg}";
                    sw.WriteLine(logLine);
                    sw.Flush();
                };

                log("=========================================");
                log($"Bắt đầu Import/Restore từ file: {opt.FilePath}");
                log($"Encoding: {opt.EncodingName} | Format: {opt.Format}");

                Encoding enc = opt.EncodingName.Contains("UTF-8") ? Encoding.UTF8 : Encoding.GetEncoding(1252);
                string fileContent = File.ReadAllText(opt.FilePath, enc);

                if (opt.RemapSchema && !string.IsNullOrEmpty(opt.SourceSchema) && !string.IsNullOrEmpty(opt.TargetSchema))
                {
                    log($"Remapping Schema từ {opt.SourceSchema} sang {opt.TargetSchema}");
                    fileContent = fileContent.Replace(opt.SourceSchema + ".", opt.TargetSchema + ".");
                    fileContent = fileContent.Replace($"\"{opt.SourceSchema}\".", $"\"{opt.TargetSchema}\".");
                }

                List<string> statements = SplitSqlStatements(fileContent);
                log($"Đã phân tích được {statements.Count} lệnh SQL.");

                using (var db = new DA.MyEntities())
                {
                    var con = db.Database.Connection;
                    if (con.State != ConnectionState.Open) con.Open();

                    // Bước 3: Tạm tắt Ràng buộc (Nếu được chọn)
                    if (opt.ExecuteData)
                    {
                        if (opt.DisableConstraints)
                        {
                            log("[PRE-EXECUTION] Vô hiệu hóa Khóa Ngoại (Constraints)...");
                            ExecuteSafe(con, "BEGIN FOR c IN (SELECT table_name, constraint_name FROM user_constraints WHERE constraint_type = 'R') LOOP EXECUTE IMMEDIATE 'ALTER TABLE \"' || c.table_name || '\" DISABLE CONSTRAINT \"' || c.constraint_name || '\"'; END LOOP; END;", log, ref errorCount, opt.AbortOnError);
                        }
                        if (opt.DisableTriggers)
                        {
                            log("[PRE-EXECUTION] Vô hiệu hóa Triggers...");
                            ExecuteSafe(con, "BEGIN FOR t IN (SELECT trigger_name FROM user_triggers) LOOP EXECUTE IMMEDIATE 'ALTER TRIGGER \"' || t.trigger_name || '\" DISABLE'; END LOOP; END;", log, ref errorCount, opt.AbortOnError);
                        }
                    }

                    HashSet<string> truncatedTables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    int currentInserts = 0;

                    // Bước 4 & 5: Thực thi DDL & Data Insertion
                    DbTransaction trans = null;
                    if (opt.ExecuteData && opt.CommitEveryRows > 0)
                        trans = con.BeginTransaction();

                    try
                    {
                        for (int i = 0; i < statements.Count; i++)
                        {
                            string stmt = statements[i];
                            string upperStmt = stmt.ToUpper();

                            bool isDDL = upperStmt.StartsWith("CREATE") || upperStmt.StartsWith("ALTER") || upperStmt.StartsWith("DROP") || upperStmt.StartsWith("GRANT");
                            bool isDML = upperStmt.StartsWith("INSERT") || upperStmt.StartsWith("UPDATE") || upperStmt.StartsWith("DELETE");

                            if (isDDL && opt.ImportDDL && opt.ExecuteDDL)
                            {
                                // Handle Drop Existing First (CASCADE)
                                if (opt.DropExisting && upperStmt.StartsWith("CREATE TABLE"))
                                {
                                    string tableName = ExtractTableName(stmt, "CREATE TABLE");
                                    if (!string.IsNullOrEmpty(tableName))
                                    {
                                        log($"[DDL] Drop existing table {tableName} CASCADE CONSTRAINTS");
                                        ExecuteSafe(con, $"DROP TABLE {tableName} CASCADE CONSTRAINTS", null, ref errorCount, false); // always ignore drop errors
                                    }
                                }

                                log($"[DDL] Executing: {stmt.Substring(0, Math.Min(50, stmt.Length))}...");
                                ExecuteSafe(con, stmt, log, ref errorCount, !opt.IgnoreCreateErrors && opt.AbortOnError);
                            }
                            else if (isDML && opt.ExecuteData)
                            {
                                if (upperStmt.StartsWith("INSERT INTO"))
                                {
                                    string tableName = ExtractTableName(stmt, "INSERT INTO");
                                    if (!string.IsNullOrEmpty(tableName))
                                    {
                                        if (opt.TruncateFirst && !truncatedTables.Contains(tableName))
                                        {
                                            log($"[DML] Truncate Table First: {tableName}");
                                            ExecuteSafe(con, $"TRUNCATE TABLE {tableName}", log, ref errorCount, opt.AbortOnError);
                                            truncatedTables.Add(tableName);
                                        }
                                    }
                                }

                                using (var cmd = con.CreateCommand())
                                {
                                    if (trans != null) cmd.Transaction = trans;
                                    cmd.CommandText = stmt;
                                    try
                                    {
                                        cmd.ExecuteNonQuery();
                                        currentInserts++;

                                        if (opt.CommitEveryRows > 0 && currentInserts % opt.CommitEveryRows == 0)
                                        {
                                            trans.Commit();
                                            log($"[DML] Đã COMMIT sau {currentInserts} dòng Insert.");
                                            trans = con.BeginTransaction(); // Start next batch
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        errorCount++;
                                        log($"[DML ERROR] {ex.Message} \n Query: {stmt.Substring(0, Math.Min(100, stmt.Length))}...");
                                        
                                        // Handle Replace by attempting an UPDATE if we had PK mapping, but since we don't, we just log.
                                        if (opt.AbortOnError) throw new Exception("Abort on error configured. Stopping execution.");
                                    }
                                }
                            }
                        }

                        if (trans != null)
                        {
                            trans.Commit();
                            log($"[DML] Đã COMMIT mẻ dữ liệu cuối cùng. (Tổng cộng {currentInserts} dòng)");
                        }
                    }
                    catch (Exception loopEx)
                    {
                        if (trans != null)
                        {
                            try { trans.Rollback(); } catch { }
                        }
                        log($"[FATAL] Dừng tiến trình do lỗi: {loopEx.Message}");
                    }
                    finally
                    {
                        // Bước 6: Khôi phục trạng thái & Dọn dẹp
                        if (opt.ExecuteData)
                        {
                            if (opt.DisableTriggers)
                            {
                                log("[POST-EXECUTION] Kích hoạt lại Triggers...");
                                ExecuteSafe(con, "BEGIN FOR t IN (SELECT trigger_name FROM user_triggers) LOOP EXECUTE IMMEDIATE 'ALTER TRIGGER \"' || t.trigger_name || '\" ENABLE'; END LOOP; END;", log, ref errorCount, false);
                            }
                            if (opt.DisableConstraints)
                            {
                                log("[POST-EXECUTION] Kích hoạt lại Khóa Ngoại (Constraints)...");
                                ExecuteSafe(con, "BEGIN FOR c IN (SELECT table_name, constraint_name FROM user_constraints WHERE constraint_type = 'R') LOOP EXECUTE IMMEDIATE 'ALTER TABLE \"' || c.table_name || '\" ENABLE CONSTRAINT \"' || c.constraint_name || '\"'; END LOOP; END;", log, ref errorCount, false);
                            }
                        }
                    }
                }
                
                log("=========================================");
                log($"Tiến trình kết thúc với {errorCount} lỗi được ghi nhận.");
            }
            return errorCount == 0;
        }

        private void ExecuteSafe(DbConnection con, string script, Action<string> log, ref int errorCount, bool abortOnError)
        {
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = script;
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    errorCount++;
                    if (log != null) log($"[ERROR] {ex.Message}");
                    if (abortOnError) throw new Exception($"Lỗi thực thi bắt buộc phải dừng: {ex.Message}");
                }
            }
        }

        private string ExtractTableName(string sql, string keyword)
        {
            int idx = sql.IndexOf(keyword, StringComparison.OrdinalIgnoreCase);
            if (idx >= 0)
            {
                int start = idx + keyword.Length;
                int end = sql.IndexOf('(', start);
                if (end == -1) end = sql.Length;
                return sql.Substring(start, end - start).Trim();
            }
            return string.Empty;
        }

        private List<string> SplitSqlStatements(string sql)
        {
            var statements = new List<string>();
            bool inString = false;
            int startIndex = 0;
            
            for (int i = 0; i < sql.Length; i++)
            {
                if (sql[i] == '\'')
                {
                    inString = !inString;
                }
                else if (sql[i] == ';' && !inString)
                {
                    string stmt = sql.Substring(startIndex, i - startIndex).Trim();
                    if (!string.IsNullOrEmpty(stmt))
                        statements.Add(stmt);
                    startIndex = i + 1;
                }
                else if (sql[i] == '/' && !inString && i > 0 && sql[i-1] == '\n')
                {
                    // Oracle PL/SQL block terminator
                    string stmt = sql.Substring(startIndex, i - startIndex).Trim();
                    if (!string.IsNullOrEmpty(stmt))
                        statements.Add(stmt);
                    startIndex = i + 1;
                }
            }
            
            if (startIndex < sql.Length)
            {
                string stmt = sql.Substring(startIndex).Trim();
                if (!string.IsNullOrEmpty(stmt))
                    statements.Add(stmt);
            }
            
            return statements;
        }
    }

    public class RestoreOptions
    {
        public string FilePath { get; set; }
        public string LogPath { get; set; }
        public string EncodingName { get; set; }
        public string Format { get; set; }
        
        public bool ImportDDL { get; set; }
        public bool ExecuteDDL { get; set; }
        public bool DropExisting { get; set; }
        public bool IgnoreCreateErrors { get; set; }
        
        public bool ExecuteData { get; set; }
        public bool TruncateFirst { get; set; }
        public bool ReplaceData { get; set; }
        public bool DisableTriggers { get; set; }
        public bool DisableConstraints { get; set; }
        
        public bool RemapSchema { get; set; }
        public string SourceSchema { get; set; }
        public string TargetSchema { get; set; }
        
        public int CommitEveryRows { get; set; }
        public bool AbortOnError { get; set; }
    }
}
