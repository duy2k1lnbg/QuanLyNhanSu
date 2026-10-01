const fs = require('fs');
const path = require('path');
const crypto = require('crypto');
const { execSync } = require('child_process');

const backupDir = 'D:/QL_NS/QuanLyNhanSu/database/realistic200/backup_metadata';
const scratchSql = 'C:/Users/duyth/.gemini/antigravity-ide/brain/8eb4ea4e-8ede-44f0-aab3-00f9185d46f0/scratch/temp_backup.sql';

function querySql(sqlStr) {
  const fullSql = `
SET PAGESIZE 50000;
SET LINESIZE 32000;
SET TRIMSPOOL ON;
SET FEEDBACK OFF;
SET HEADING OFF;
${sqlStr}
exit;
`;
  fs.writeFileSync(scratchSql, fullSql, 'ascii');
  const out = execSync(`D:\\Oracle\\bin\\sqlplus.exe -s HR/hr@localhost:1521/orcl @${scratchSql}`, { maxBuffer: 100 * 1024 * 1024 });
  return out.toString().trim();
}

console.log("1. Capturing Metadata Baseline from HR schema...");

const tablesSql = `SELECT table_name FROM user_tables ORDER BY table_name;`;
const tablesList = querySql(tablesSql).split(/\r?\n/).map(s => s.trim()).filter(Boolean);

const columnsSql = `SELECT table_name || '|' || column_name || '|' || data_type || '|' || data_length || '|' || data_precision || '|' || data_scale || '|' || nullable FROM user_tab_columns ORDER BY table_name, column_id;`;
const columnsList = querySql(columnsSql).split(/\r?\n/).map(s => s.trim()).filter(Boolean);

const constraintsSql = `SELECT constraint_name || '|' || constraint_type || '|' || table_name || '|' || search_condition_vc || '|' || r_constraint_name || '|' || status FROM user_constraints ORDER BY table_name, constraint_name;`;
const constraintsList = querySql(constraintsSql).split(/\r?\n/).map(s => s.trim()).filter(Boolean);

const triggersSql = `SELECT trigger_name || '|' || trigger_type || '|' || triggering_event || '|' || table_name || '|' || status FROM user_triggers ORDER BY trigger_name;`;
const triggersList = querySql(triggersSql).split(/\r?\n/).map(s => s.trim()).filter(Boolean);

const indexesSql = `SELECT index_name || '|' || table_name || '|' || uniqueness || '|' || status FROM user_indexes ORDER BY table_name, index_name;`;
const indexesList = querySql(indexesSql).split(/\r?\n/).map(s => s.trim()).filter(Boolean);

const metadataSnapshot = {
  timestamp: new Date().toISOString(),
  tableCount: tablesList.length,
  tables: tablesList,
  columnCount: columnsList.length,
  columns: columnsList,
  constraintCount: constraintsList.length,
  constraints: constraintsList,
  triggerCount: triggersList.length,
  triggers: triggersList,
  indexCount: indexesList.length,
  indexes: indexesList
};

fs.writeFileSync(path.join(backupDir, 'pre_metadata.json'), JSON.stringify(metadataSnapshot, null, 2), 'utf8');
console.log(`Saved pre_metadata.json: ${tablesList.length} tables, ${columnsList.length} columns, ${constraintsList.length} constraints, ${triggersList.length} triggers.`);

console.log("\n2. Exporting Data Backups for Tables in Scope...");
const targetTables = [
  'TB_CONGTY',
  'TB_BOPHAN',
  'TB_PHONGBAN',
  'TB_CHUCVU',
  'TB_PHUCAP',
  'TB_LOAIHOPDONG',
  'TB_NHANVIEN',
  'TB_HOPDONG',
  'TB_LUONG_HIEULUC',
  'TB_BAOHIEM',
  'TB_NHANVIEN_PHUCAP',
  'TB_NHANVIEN_BAOHIEM_THAM_GIA',
  'TB_NHANVIEN_CONG_DOAN_THAM_GIA',
  'TB_NHANVIEN_THUE',
  'TB_NGUOI_PHU_THUOC',
  'TB_KHENTHUONG_KYLUAT',
  'TB_UNGLUONG',
  'TB_NHANVIEN_THOIVIEC',
  'TB_BAOHIEM_BIENDONG',
  'TB_CHAMCONG_BATTHUONG',
  'TB_UAT_SCENARIO',
  'TB_KYCONGCHITIET'
];

const manifest = [];

for (const tbl of targetTables) {
  // Get columns
  const colsSql = `SELECT column_name FROM user_tab_columns WHERE table_name = '${tbl}' AND data_type != 'BLOB' AND data_type != 'CLOB' ORDER BY column_id;`;
  const cols = querySql(colsSql).split(/\r?\n/).map(s => s.trim()).filter(Boolean);
  
  // Format query as CSV or delimiter separated
  const selectCols = cols.map(c => `NVL(TO_CHAR(${c}), 'NULL')`).join(" || '~~~' || ");
  const dataSql = `SELECT ${selectCols} FROM ${tbl};`;
  const rawData = querySql(dataSql);
  const rows = rawData ? rawData.split(/\r?\n/).map(r => r.trim()).filter(Boolean) : [];
  
  const backupFile = path.join(backupDir, `${tbl}.bak.json`);
  const tableData = {
    table: tbl,
    columns: cols,
    rowCount: rows.length,
    rows: rows.map(r => r.split('~~~'))
  };

  const fileContent = JSON.stringify(tableData);
  fs.writeFileSync(backupFile, fileContent, 'utf8');

  const sha256 = crypto.createHash('sha256').update(fileContent).digest('hex');

  manifest.push({
    table: tbl,
    rowCount: rows.length,
    columnCount: cols.length,
    file: `${tbl}.bak.json`,
    sha256: sha256
  });

  console.log(`Backed up ${tbl}: ${rows.length} rows -> SHA256: ${sha256.substring(0, 16)}...`);
}

fs.writeFileSync(path.join(backupDir, 'pre_backup_manifest.json'), JSON.stringify(manifest, null, 2), 'utf8');
console.log(`\nCompleted pre-modification backup! Manifest saved to ${path.join(backupDir, 'pre_backup_manifest.json')}`);
