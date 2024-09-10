// using System.ComponentModel.DataAnnotations;
// using System.ComponentModel.DataAnnotations.Schema;
// using Microsoft.EntityFrameworkCore;
//
// namespace HentaiMapGen
// {
//     public class NHentaiContext(DbContextOptions<NHentaiContext> options) : DbContext(options)
//     {
//         public DbSet<Book> Books { get; set; }
//         public DbSet<Page> Pages { get; set; }
//         public DbSet<Tag> Tags { get; set; }
//
//         protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//         {
//             optionsBuilder.UseSqlite(
//                 @"DataSource=new_nhentai1.db");
//         }
//
//         protected override void OnModelCreating(ModelBuilder modelBuilder)
//         {
//             modelBuilder.Entity<Book>(b =>
//             {
//                 b.ToTable("books");
//                 
//                 b.HasKey(e => e.Id);
//
//                 b.Property(e => e.Error).HasColumnName("error");
//                 b.Property(e => e.Id).HasColumnName("id").IsRequired().ValueGeneratedNever();
//                 b.Property(e => e.MediaId).HasColumnName("media_id");
//                 b.OwnsOne(e => e.Title, n =>
//                 {
//                     n.Property(t => t.English).HasColumnName("title_english");
//                     n.Property(t => t.Japanese).HasColumnName("title_japanese");
//                     n.Property(t => t.Pretty).HasColumnName("title_pretty");
//                 });
//                 b.OwnsOne(e => e.Images, n =>
//                 {
//                     n.OwnsOne(i => i.Cover, n2 =>
//                     {
//                         n2.Property(p => p.Type).HasColumnName("cover_type");
//                         n2.Property(p => p.Width).HasColumnName("cover_width");
//                         n2.Property(p => p.Height).HasColumnName("cover_height");
//                     });
//                     n.OwnsOne(i => i.Thumbnail, n2 =>
//                     {
//                         n2.Property(p => p.Type).HasColumnName("thumb_type");
//                         n2.Property(p => p.Width).HasColumnName("thumb_width");
//                         n2.Property(p => p.Height).HasColumnName("thumb_height");
//                     });
//                     
//                     n.OwnsMany(e => e.Pages, n2 =>
//                     {
//                         n2.ToTable("pages");
//
//                         n2.WithOwner().HasForeignKey();
//                         n2.Property(p => p.Type).HasColumnName("type");
//                         n2.Property(p => p.Width).HasColumnName("width");
//                         n2.Property(p => p.Height).HasColumnName("height");
//                     });
//                 });
//                 b.Property(e => e.Scanlator).HasColumnName("scanlator");
//                 b.Property(e => e.UploadDate).HasColumnName("upload_date");
//                 b.Property(e => e.NumPages).HasColumnName("num_pages");
//                 b.Property(e => e.NumFavorites).HasColumnName("num_favorites");
//
//                 b
//                     .HasMany<Tag>()
//                     .WithMany();
//             });
//
//             modelBuilder.Entity<Tag>(b =>
//             {
//                 b.ToTable("tags");
//
//                 b.HasKey(t => t.Id);
//
//                 b.Property(t => t.Id).HasColumnName("id").IsRequired();
//                 b.Property(t => t.Type).HasColumnName("type").IsRequired();
//                 b.Property(t => t.Name).HasColumnName("name").IsRequired();
//                 b.Property(t => t.Url).HasColumnName("url").IsRequired();
//                 b.Property(t => t.Count).HasColumnName("count").IsRequired();
//             });
//
//         }
//     }
// }