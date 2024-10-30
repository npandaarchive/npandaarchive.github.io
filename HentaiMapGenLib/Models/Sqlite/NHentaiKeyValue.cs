using SQLite;

namespace HentaiMapGenLib.Models;

[Table("galleries_kv")]
public class NHentaiKeyValue
{
    [Column("key"), PrimaryKey] public string Key { get; set; } = null!;
    [Column("value")] public string Value { get; set; } = null!;
}
