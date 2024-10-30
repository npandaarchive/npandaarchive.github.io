using SQLite;

namespace HentaiMapGenLib.Models;

[Table("gallery")]
public class SadPandaGallery
{
	[Column("gid"                 )] [PrimaryKey] public long    Gid          { get; set; } // integer
	[Column("title"                            )] public string? Title        { get; set; } // varchar(255)
	[Column("title_jpn"                        )] public string? TitleJpn     { get; set; } // varchar(255)
	[Column("category"                         )] public string? Category     { get; set; } // varchar(255)
	[Column("uploader"                         )] public string? Uploader     { get; set; } // varchar(255)
	[Column("posted"                           )] public long?   Posted       { get; set; } // integer
	[Column("thumb"                            )] public string? Thumb        { get; set; } // varchar(255)
	[Column("filesize"                         )] public long?   FileSize     { get; set; } // integer
	[Column("filecount"                        )] public long?   FileCount    { get; set; } // integer
	[Column("expunged"                         )] public long?   Expunged     { get; set; } // integer
	[Column("torrentcount"                     )] public long?   TorrentCount { get; set; } // integer
	[Column("torrents"                         )] public string? Torrents     { get; set; } // varchar(255)
	[Column("token"                            )] public string? Token        { get; set; } // varchar(255)
	[Column("rating"                           )] public double? Rating       { get; set; } // real
	[Column("artist"                           )] public string? Artist       { get; set; } // varchar(255)
	[Column("group"                            )] public string? Group        { get; set; } // varchar(255)
	[Column("parody"                           )] public string? Parody       { get; set; } // varchar(255)
	[Column("character"                        )] public string? Character    { get; set; } // varchar(255)
	[Column("female"                           )] public string? Female       { get; set; } // varchar(255)
	[Column("male"                             )] public string? Male         { get; set; } // varchar(255)
	[Column("language"                         )] public string? Language     { get; set; } // varchar(255)
	[Column("mixed"                            )] public string? Mixed        { get; set; } // varchar(255)
	[Column("other"                            )] public string? Other        { get; set; } // varchar(255)
	[Column("cosplayer"                        )] public string? Cosplayer    { get; set; } // varchar(255)
	[Column("rest"                             )] public string? Rest         { get; set; } // varchar(255)
	[Column("parent_gid"                       )] public long?   ParentGid    { get; set; } // integer
	[Column("parent_key"                       )] public string? ParentKey    { get; set; } // varchar(255)
	[Column("first_gid"                        )] public long?   FirstGid     { get; set; } // integer
	[Column("first_key"                        )] public string? FirstKey     { get; set; } // varchar(255)
	[Column("disowned"                         )] public long?   Disowned     { get; set; } // integer
	[Column("removed"                          )] public long?   Removed      { get; set; } // integer
	[Column("dumped"                           )] public long?   Dumped       { get; set; } // integer
}