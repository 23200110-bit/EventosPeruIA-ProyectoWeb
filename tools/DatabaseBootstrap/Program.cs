using Microsoft.Data.SqlClient;
using System.Text.RegularExpressions;
var connection=Environment.GetEnvironmentVariable("EVENTOS_SQL_CONNECTION")??throw new InvalidOperationException("Falta EVENTOS_SQL_CONNECTION (usar sólo una instancia de prueba nueva).");
if(args.Length!=1)throw new ArgumentException("Indica el archivo SQL original.");
await using var db=new SqlConnection(connection);await db.OpenAsync();
foreach(var batch in Regex.Split(await File.ReadAllTextAsync(args[0]),@"^\s*GO\s*$",RegexOptions.Multiline|RegexOptions.IgnoreCase))
{if(string.IsNullOrWhiteSpace(batch))continue;await using var cmd=new SqlCommand(batch,db){CommandTimeout=120};await cmd.ExecuteNonQueryAsync();}
Console.WriteLine("SQL_ORIGINAL_EXECUTED");
await using var check=new SqlCommand("SELECT COUNT(*) FROM EventosPeruIA.sys.tables",db);Console.WriteLine($"Tablas: {await check.ExecuteScalarAsync()}");
