using System;
using System.Collections.Generic;
using LibraryApp.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApp.Data;

public partial class LibraryContext : DbContext
{
    public LibraryContext(DbContextOptions<LibraryContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Author> Authors { get; set; }

    public virtual DbSet<Book> Books { get; set; }

    public virtual DbSet<BookAuthor> BookAuthors { get; set; }

    public virtual DbSet<BookCopy> BookCopies { get; set; }

    public virtual DbSet<BookPublisher> BookPublishers { get; set; }

    public virtual DbSet<Genre> Genres { get; set; }

    public virtual DbSet<Loan> Loans { get; set; }

    public virtual DbSet<LoanStatus> LoanStatuses { get; set; }

    public virtual DbSet<Publisher> Publishers { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<StudentGroup> StudentGroups { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Author>(entity =>
        {
            entity.HasKey(e => e.AuthorId).HasName("pk_authors");

            entity.ToTable("authors");

            entity.Property(e => e.AuthorId).HasColumnName("author_id");
            entity.Property(e => e.FirstName)
                .HasMaxLength(50)
                .HasColumnName("first_name");
            entity.Property(e => e.LastName)
                .HasMaxLength(50)
                .HasColumnName("last_name");
            entity.Property(e => e.MiddleName)
                .HasMaxLength(50)
                .HasDefaultValue("-")
                .HasColumnName("middle_name");
        });

        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(e => e.BookId).HasName("pk_books");

            entity.ToTable("books");

            entity.Property(e => e.BookId).HasColumnName("book_id");
            entity.Property(e => e.CoverImage)
                .HasMaxLength(100)
                .HasDefaultValue("no_cover.jpg")
                .HasColumnName("cover_image");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .HasDefaultValue("Описание отсутствует")
                .HasColumnName("description");
            entity.Property(e => e.GenreId).HasColumnName("genre_id");
            entity.Property(e => e.PageCount).HasColumnName("page_count");
            entity.Property(e => e.PublishYear).HasColumnName("publish_year");
            entity.Property(e => e.Title)
                .HasMaxLength(200)
                .HasColumnName("title");

            entity.HasOne(d => d.Genre).WithMany(p => p.Books)
                .HasForeignKey(d => d.GenreId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_books_genres");
        });

        modelBuilder.Entity<BookAuthor>(entity =>
        {
            entity.HasKey(e => e.BookAuthorId).HasName("pk_book_authors");

            entity.ToTable("book_authors");

            entity.HasIndex(e => new { e.BookId, e.AuthorId }, "uq_book_authors").IsUnique();

            entity.Property(e => e.BookAuthorId).HasColumnName("book_author_id");
            entity.Property(e => e.AuthorId).HasColumnName("author_id");
            entity.Property(e => e.BookId).HasColumnName("book_id");

            entity.HasOne(d => d.Author).WithMany(p => p.BookAuthors)
                .HasForeignKey(d => d.AuthorId)
                .HasConstraintName("fk_book_authors_authors");

            entity.HasOne(d => d.Book).WithMany(p => p.BookAuthors)
                .HasForeignKey(d => d.BookId)
                .HasConstraintName("fk_book_authors_books");
        });

        modelBuilder.Entity<BookCopy>(entity =>
        {
            entity.HasKey(e => e.CopyId).HasName("pk_book_copies");

            entity.ToTable("book_copies");

            entity.HasIndex(e => e.InventoryNumber, "uq_book_copies_inv").IsUnique();

            entity.Property(e => e.CopyId).HasColumnName("copy_id");
            entity.Property(e => e.BookId).HasColumnName("book_id");
            entity.Property(e => e.DateReceived)
                .HasDefaultValueSql("(CONVERT([date],getdate()))")
                .HasColumnName("date_received");
            entity.Property(e => e.InventoryNumber)
                .HasMaxLength(20)
                .HasColumnName("inventory_number");

            entity.HasOne(d => d.Book).WithMany(p => p.BookCopies)
                .HasForeignKey(d => d.BookId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_book_copies_books");
        });

        modelBuilder.Entity<BookPublisher>(entity =>
        {
            entity.HasKey(e => e.BookPublisherId).HasName("pk_book_publishers");

            entity.ToTable("book_publishers");

            entity.HasIndex(e => new { e.BookId, e.PublisherId }, "uq_book_publishers").IsUnique();

            entity.Property(e => e.BookPublisherId).HasColumnName("book_publisher_id");
            entity.Property(e => e.BookId).HasColumnName("book_id");
            entity.Property(e => e.PublisherId).HasColumnName("publisher_id");

            entity.HasOne(d => d.Book).WithMany(p => p.BookPublishers)
                .HasForeignKey(d => d.BookId)
                .HasConstraintName("fk_book_publishers_books");

            entity.HasOne(d => d.Publisher).WithMany(p => p.BookPublishers)
                .HasForeignKey(d => d.PublisherId)
                .HasConstraintName("fk_book_publishers_publishers");
        });

        modelBuilder.Entity<Genre>(entity =>
        {
            entity.HasKey(e => e.GenreId).HasName("pk_genres");

            entity.ToTable("genres");

            entity.HasIndex(e => e.GenreName, "uq_genres_name").IsUnique();

            entity.Property(e => e.GenreId).HasColumnName("genre_id");
            entity.Property(e => e.GenreName)
                .HasMaxLength(100)
                .HasColumnName("genre_name");
        });

        modelBuilder.Entity<Loan>(entity =>
        {
            entity.HasKey(e => e.LoanId).HasName("pk_loans");

            entity.ToTable("loans");

            entity.HasIndex(e => e.CopyId, "ux_loans_active_copy")
                .IsUnique()
                .HasFilter("([status_id] IN ((1), (2), (3), (4)))");

            entity.Property(e => e.LoanId).HasColumnName("loan_id");
            entity.Property(e => e.BookingDate)
                .HasDefaultValueSql("(CONVERT([date],getdate()))")
                .HasColumnName("booking_date");
            entity.Property(e => e.CopyId).HasColumnName("copy_id");
            entity.Property(e => e.DueDate).HasColumnName("due_date");
            entity.Property(e => e.StatusId)
                .HasDefaultValue(1)
                .HasColumnName("status_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Copy).WithOne(p => p.Loan)
                .HasForeignKey<Loan>(d => d.CopyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_loans_copies");

            entity.HasOne(d => d.Status).WithMany(p => p.Loans)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_loans_statuses");

            entity.HasOne(d => d.User).WithMany(p => p.Loans)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_loans_users");
        });

        modelBuilder.Entity<LoanStatus>(entity =>
        {
            entity.HasKey(e => e.StatusId).HasName("pk_loan_statuses");

            entity.ToTable("loan_statuses");

            entity.HasIndex(e => e.StatusName, "uq_loan_statuses_name").IsUnique();

            entity.Property(e => e.StatusId).HasColumnName("status_id");
            entity.Property(e => e.StatusName)
                .HasMaxLength(30)
                .HasColumnName("status_name");
        });

        modelBuilder.Entity<Publisher>(entity =>
        {
            entity.HasKey(e => e.PublisherId).HasName("pk_publishers");

            entity.ToTable("publishers");

            entity.HasIndex(e => e.PublisherName, "uq_publishers_name").IsUnique();

            entity.Property(e => e.PublisherId).HasColumnName("publisher_id");
            entity.Property(e => e.City)
                .HasMaxLength(50)
                .HasColumnName("city");
            entity.Property(e => e.PublisherName)
                .HasMaxLength(100)
                .HasColumnName("publisher_name");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("pk_roles");

            entity.ToTable("roles");

            entity.HasIndex(e => e.RoleName, "uq_roles_name").IsUnique();

            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.RoleName)
                .HasMaxLength(50)
                .HasColumnName("role_name");
        });

        modelBuilder.Entity<StudentGroup>(entity =>
        {
            entity.HasKey(e => e.GroupId).HasName("pk_student_groups");

            entity.ToTable("student_groups");

            entity.HasIndex(e => e.GroupName, "uq_student_groups_name").IsUnique();

            entity.Property(e => e.GroupId).HasColumnName("group_id");
            entity.Property(e => e.GroupName)
                .HasMaxLength(30)
                .HasColumnName("group_name");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("pk_users");

            entity.ToTable("users");

            entity.HasIndex(e => e.Login, "uq_users_login").IsUnique();

            entity.HasIndex(e => e.TicketNumber, "uq_users_ticket").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.FirstName)
                .HasMaxLength(50)
                .HasColumnName("first_name");
            entity.Property(e => e.GroupId).HasColumnName("group_id");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.LastName)
                .HasMaxLength(50)
                .HasColumnName("last_name");
            entity.Property(e => e.Login)
                .HasMaxLength(50)
                .HasColumnName("login");
            entity.Property(e => e.MiddleName)
                .HasMaxLength(50)
                .HasDefaultValue("-")
                .HasColumnName("middle_name");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(64)
                .IsUnicode(false)
                .HasColumnName("password_hash");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.RegisteredDate)
                .HasDefaultValueSql("(CONVERT([date],getdate()))")
                .HasColumnName("registered_date");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.TicketNumber)
                .HasMaxLength(20)
                .HasColumnName("ticket_number");

            entity.HasOne(d => d.Group).WithMany(p => p.Users)
                .HasForeignKey(d => d.GroupId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_users_groups");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_users_roles");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
