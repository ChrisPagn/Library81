using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace Library81.Models;

public partial class LibraryContext : DbContext
{
    public LibraryContext()
    {
    }

    public LibraryContext(DbContextOptions<LibraryContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Book> Books { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Game> Games { get; set; }

    public virtual DbSet<Genre> Genres { get; set; }

    public virtual DbSet<Item> Items { get; set; }

    public virtual DbSet<Movie> Movies { get; set; }

    public virtual DbSet<Subcategory> Subcategories { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)

//#warning To protect potentially sensitive information in your connection string, you should move it out of source code.
//You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration -
//see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings,
//see https://go.microsoft.com/fwlink/?LinkId=723263.
  
        => optionsBuilder.UseMySql("server=localhost;port=33020;database=library;user=root;password=password;treattinyasboolean=false", Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.32-mysql"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(e => e.BookId).HasName("PRIMARY");

            entity.ToTable("books");

            entity.HasIndex(e => e.GenreId, "books_ibfk_2");

            entity.HasIndex(e => e.Isbn, "isbn").IsUnique();

            entity.HasIndex(e => e.ItemId, "item_id").IsUnique();

            entity.Property(e => e.BookId).HasColumnName("book_id");
            entity.Property(e => e.GenreId).HasColumnName("genre_id");
            entity.Property(e => e.Isbn)
                .HasMaxLength(20)
                .HasColumnName("isbn");
            entity.Property(e => e.ItemId).HasColumnName("item_id");

            entity.HasOne(d => d.Genre).WithMany(p => p.Books)
                .HasForeignKey(d => d.GenreId)
                .HasConstraintName("books_ibfk_2");

            entity.HasOne(d => d.Item).WithOne(p => p.Book)
                .HasForeignKey<Book>(d => d.ItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("books_ibfk_1");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PRIMARY");

            entity.ToTable("categories");

            entity.HasIndex(e => e.Name, "name").IsUnique();

            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.Name).HasColumnName("name");
        });

        modelBuilder.Entity<Game>(entity =>
        {
            entity.HasKey(e => e.GameId).HasName("PRIMARY");

            entity.ToTable("games");

            entity.HasIndex(e => e.ItemId, "item_id").IsUnique();

            entity.Property(e => e.GameId).HasColumnName("game_id");
            entity.Property(e => e.AgeRange)
                .HasMaxLength(50)
                .HasColumnName("age_range");
            entity.Property(e => e.ItemId).HasColumnName("item_id");
            entity.Property(e => e.Platform)
                .HasMaxLength(255)
                .HasColumnName("platform");

            entity.HasOne(d => d.Item).WithOne(p => p.Game)
                .HasForeignKey<Game>(d => d.ItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("games_ibfk_1");
        });

        modelBuilder.Entity<Genre>(entity =>
        {
            entity.HasKey(e => e.GenreId).HasName("PRIMARY");

            entity.ToTable("genres");

            entity.HasIndex(e => e.Name, "name").IsUnique();

            entity.Property(e => e.GenreId).HasColumnName("genre_id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Item>(entity =>
        {
            entity.HasKey(e => e.ItemId).HasName("PRIMARY");

            entity.ToTable("items");

            entity.HasIndex(e => e.SubcategoryId, "subcategory_id");

            entity.Property(e => e.ItemId).HasColumnName("item_id");
            entity.Property(e => e.Creator)
                .HasMaxLength(255)
                .HasColumnName("creator");
            entity.Property(e => e.DateAdded)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("date_added");
            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(512)
                .HasColumnName("image_url");
            entity.Property(e => e.Publisher)
                .HasMaxLength(255)
                .HasColumnName("publisher");
            entity.Property(e => e.SubcategoryId).HasColumnName("subcategory_id");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.Year)
                .HasColumnType("year")
                .HasColumnName("year");

            entity.HasOne(d => d.Subcategory).WithMany(p => p.Items)
                .HasForeignKey(d => d.SubcategoryId)
                .HasConstraintName("items_ibfk_1");
        });

        modelBuilder.Entity<Movie>(entity =>
        {
            entity.HasKey(e => e.MovieId).HasName("PRIMARY");

            entity.ToTable("movies");

            entity.HasIndex(e => e.ItemId, "item_id").IsUnique();

            entity.Property(e => e.MovieId).HasColumnName("movie_id");
            entity.Property(e => e.Duration)
                .HasColumnType("time")
                .HasColumnName("duration");
            entity.Property(e => e.ItemId).HasColumnName("item_id");
            entity.Property(e => e.Rating)
                .HasMaxLength(10)
                .HasColumnName("rating");

            entity.HasOne(d => d.Item).WithOne(p => p.Movie)
                .HasForeignKey<Movie>(d => d.ItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("movies_ibfk_1");
        });

        modelBuilder.Entity<Subcategory>(entity =>
        {
            entity.HasKey(e => e.SubcategoryId).HasName("PRIMARY");

            entity.ToTable("subcategories");

            entity.HasIndex(e => e.CategoryId, "category_id");

            entity.HasIndex(e => e.Name, "name").IsUnique();

            entity.Property(e => e.SubcategoryId).HasColumnName("subcategory_id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.Name).HasColumnName("name");

            entity.HasOne(d => d.Category).WithMany(p => p.Subcategories)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("subcategories_ibfk_1");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
