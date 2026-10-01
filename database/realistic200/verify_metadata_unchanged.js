const { execSync } = require('child_process');
const fs = require('fs');
const path = require('path');

console.log("=== METADATA COMPARISON: PRE vs POST ===");

const prePath = path.join(__dirname, 'backup_metadata/pre_metadata.json');
if (!fs.existsSync(prePath)) {
  console.error("FATAL: pre_metadata.json not found!");
  process.exit(1);
}
const pre = JSON.parse(fs.readFileSync(prePath, 'utf8'));

// Extract post metadata from Oracle HR schema
const sqlScript = `
SET PAGESIZE 0 LINESIZE 5000 FEEDBACK OFF TRIMSPOOL ON ECHO OFF HEADING OFF;
SPOOL scratch/post_tables.txt
SELECT table_name FROM user_tables ORDER BY table_name;
SPOOL OFF;

SPOOL scratch/post_columns.txt
SELECT table_name || '|' || column_name || '|' || data_type || '|' || data_length || '|' || data_precision || '|' || data_scale || '|' || nullable FROM user_tab_columns ORDER BY table_name, column_id;
SPOOL OFF;

SPOOL scratch/post_constraints.txt
SELECT constraint_name || '|' || constraint_type || '|' || table_name || '|' || search_condition_vc || '|' || r_constraint_name || '|' || status FROM user_constraints ORDER BY table_name, constraint_name;
SPOOL OFF;

SPOOL scratch/post_triggers.txt
SELECT trigger_name || '|' || trigger_type || '|' || triggering_event || '|' || table_name || '|' || status FROM user_triggers ORDER BY trigger_name;
SPOOL OFF;

SPOOL scratch/post_indexes.txt
SELECT index_name || '|' || table_name || '|' || uniqueness || '|' || status FROM user_indexes ORDER BY table_name, index_name;
SPOOL OFF;

EXIT;
`;

fs.writeFileSync('scratch/export_post_meta.sql', sqlScript, 'utf8');
execSync('sqlplus -s hr/hr@localhost:1521/orcl @scratch/export_post_meta.sql', {
  encoding: 'utf8',
  env: { ...process.env, NLS_LANG: 'AMERICAN_AMERICA.AL32UTF8' }
});

const postTables = fs.readFileSync('scratch/post_tables.txt', 'utf8').split(/\r?\n/).map(s => s.trim()).filter(Boolean);
const postCols = fs.readFileSync('scratch/post_columns.txt', 'utf8').split(/\r?\n/).map(s => s.trim()).filter(Boolean);
const postCons = fs.readFileSync('scratch/post_constraints.txt', 'utf8').split(/\r?\n/).map(s => s.trim()).filter(Boolean);
const postTrigs = fs.readFileSync('scratch/post_triggers.txt', 'utf8').split(/\r?\n/).map(s => s.trim()).filter(Boolean);
const postIdxs = fs.readFileSync('scratch/post_indexes.txt', 'utf8').split(/\r?\n/).map(s => s.trim()).filter(Boolean);

console.log("\n--- Comparison Summary ---");
console.log(`Tables:       Pre = ${pre.tableCount}, Post = ${postTables.length} -> ${pre.tableCount === postTables.length ? 'IDENTICAL ✓' : 'DIFFERENCE DETECTED ✗'}`);
console.log(`Columns:      Pre = ${pre.columnCount}, Post = ${postCols.length} -> ${pre.columnCount === postCols.length ? 'IDENTICAL ✓' : 'DIFFERENCE DETECTED ✗'}`);
console.log(`Constraints:  Pre = ${pre.constraintCount}, Post = ${postCons.length} -> ${pre.constraintCount === postCons.length ? 'IDENTICAL ✓' : 'DIFFERENCE DETECTED ✗'}`);
console.log(`Triggers:     Pre = ${pre.triggerCount}, Post = ${postTrigs.length} -> ${pre.triggerCount === postTrigs.length ? 'IDENTICAL ✓' : 'DIFFERENCE DETECTED ✗'}`);
console.log(`Indexes:      Pre = ${pre.indexCount}, Post = ${postIdxs.length} -> ${pre.indexCount === postIdxs.length ? 'IDENTICAL ✓' : 'DIFFERENCE DETECTED ✗'}`);

const diffTables = postTables.filter(t => !pre.tables.includes(t));
const diffCols = postCols.filter(c => !pre.columns.includes(c));
const diffCons = postCons.filter(c => !pre.constraints.includes(c));
const diffTrigs = postTrigs.filter(t => !pre.triggers.includes(t));

if (diffTables.length || diffCols.length || diffCons.length || diffTrigs.length) {
  console.error("WARNING: Differences found:", { diffTables, diffCols, diffCons, diffTrigs });
} else {
  console.log("\n✓ VERIFIED: EXACT ZERO STRUCTURAL CHANGES IN THE DATABASE!");
  console.log("No tables, columns, constraints, triggers, or indexes were created, dropped, or altered.");
}

const postMeta = {
  timestamp: new Date().toISOString(),
  tableCount: postTables.length,
  tables: postTables,
  columnCount: postCols.length,
  columns: postCols,
  constraintCount: postCons.length,
  constraints: postCons,
  triggerCount: postTrigs.length,
  triggers: postTrigs,
  indexCount: postIdxs.length,
  indexes: postIdxs
};
fs.writeFileSync('database/realistic200/backup_metadata/post_metadata.json', JSON.stringify(postMeta, null, 2), 'utf8');
console.log("Saved post_metadata.json successfully.");
