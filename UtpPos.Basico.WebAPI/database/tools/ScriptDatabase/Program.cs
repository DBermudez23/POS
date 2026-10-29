using Microsoft.Data.SqlClient;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
var cs = args[0]; var outFile = args[1];
using var cn = new SqlConnection(cs);
var server = new Server(new ServerConnection(cn));
var db = server.Databases[args[2]];
var urns = new List<SqlSmoObject>();
foreach (Microsoft.SqlServer.Management.Smo.UserDefinedTableType t in db.UserDefinedTableTypes) urns.Add(t);
foreach (Table t in db.Tables) if (!t.IsSystemObject && t.Name != "sysdiagrams") urns.Add(t);
foreach (View v in db.Views) if (!v.IsSystemObject) urns.Add(v);
foreach (UserDefinedFunction f in db.UserDefinedFunctions) if (!f.IsSystemObject) urns.Add(f);
foreach (StoredProcedure p in db.StoredProcedures) if (!p.IsSystemObject) urns.Add(p);
var so = new ScriptingOptions {
  ScriptSchema = true, ScriptData = true, DriAll = true, Indexes = true, Triggers = true, SchemaQualify = true,
  IncludeHeaders = false, WithDependencies = true, NoCollation = false, ScriptBatchTerminator = true, BatchSize = 1,
  ToFileOnly = true, FileName = outFile, Encoding = new System.Text.UTF8Encoding(false), IncludeIfNotExists = false };
var sc = new Scripter(server) { Options = so };
sc.EnumScript(urns.ToArray());
Console.WriteLine($"objetos: {urns.Count} -> {outFile}");
