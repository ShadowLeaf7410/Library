using Microsoft.EntityFrameworkCore;
using BlazorApp2.Data.Models;
namespace BlazorApp2.Data
{
    public class LibDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<LoginToken> LoginTokens { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Borrowing> Borrowing { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Fine> Fines { get; set; }
        public DbSet<SystemLog> SystemLogs { get; set; }
        private readonly string _connectionString;
        public LibDbContext(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("SQLServer");
            //_connectionString=configuration.GetConnectionString("");
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(_connectionString);
                //optionsBuilder.UseOracle(_connectionString)；
            }
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");

                entity.HasKey(p => p.UserId);

                entity.Property(p => p.UserId)
                      .HasColumnName("UserId")
                      .HasColumnType("uniqueidentifier")
                      .HasDefaultValueSql("NEWID()");

                entity.Property(p => p.Email)
                      .IsRequired()
                      .HasColumnName("Email")
                      .HasColumnType("nvarchar(255)");

                entity.HasIndex(p => p.Email)
                      .IsUnique();

                entity.Property(p => p.Name)
                      .IsRequired()
                      .HasColumnName("Name")
                      .HasColumnType("nvarchar(100)");

                entity.Property(p => p.HashedPassword)
                      .HasColumnName("HashedPassword")
                      .HasColumnType("nvarchar(255)");

                entity.Property(p => p.Status)
                      .IsRequired()
                      .HasColumnName("Status")
                      .HasColumnType("nvarchar(20)")
                      .HasDefaultValue("Active");

                entity.HasCheckConstraint("CHK_User_Status", "Status IN ('Active', 'Suspended', 'Deleted')");

                entity.Property(p => p.Role)
                      .IsRequired()
                      .HasColumnName("Role")
                      .HasColumnType("nvarchar(20)")
                      .HasDefaultValue("User");

                entity.HasCheckConstraint("CHK_User_Role", "Role IN ('User', 'Librarian', 'Admin')");

                entity.Property(p => p.CreatedAt)
                      .IsRequired()
                      .HasColumnName("CreatedAt")
                      .HasColumnType("datetime2(7)")
                      .HasDefaultValueSql("SYSUTCDATETIME()");

                entity.Property(p => p.LastLogin)
                      .HasColumnName("LastLogin")
                      .HasColumnType("datetime2(7)");

            });

            modelBuilder.Entity<LoginToken>(entity =>
            {
                entity.ToTable("LoginTokens");

                entity.HasKey(p => p.TokenId);

                entity.Property(p => p.TokenId)
                      .HasColumnName("TokenId")
                      .HasColumnType("uniqueidentifier")
                      .HasDefaultValueSql("NEWID()");

                entity.Property(p => p.UserId)
                      .IsRequired()
                      .HasColumnName("UserId")
                      .HasColumnType("uniqueidentifier");

                entity.HasOne(lt => lt.User)
                      .WithMany(u => u.LoginTokens)
                      .HasForeignKey(u => u.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(p => p.UserId);

                entity.Property(p => p.Token)
                      .IsRequired()
                      .HasColumnName("Token")
                      .HasColumnType("nvarchar(255)");

                entity.HasIndex(p => p.Token)
                      .IsUnique();

                entity.Property(p => p.Expiration)
                      .IsRequired()
                      .HasColumnName("Expiration")
                      .HasColumnType("datetime2(7)");

                entity.Property(p => p.IsUsed)
                      .IsRequired()
                      .HasColumnName("IsUsed")
                      .HasColumnType("bit")
                      .HasDefaultValue(0);

                entity.Property(p => p.CreatedAt)
                      .IsRequired()
                      .HasColumnName("CreatedAt")
                      .HasColumnType("datetime2(7)")
                      .HasDefaultValueSql("SYSUTCDATETIME()");
            });

            modelBuilder.Entity<Book>(entity =>
            {
                entity.ToTable("Books");

                entity.HasKey(p => p.BookId);

                entity.Property(p => p.BookId)
                      .HasColumnName("BookId")
                      .HasColumnType("uniqueidentifier")
                      .HasDefaultValueSql("NEWID()");

                entity.Property(p => p.ISBN)
                      .IsRequired()
                      .HasColumnName("ISBN")
                      .HasColumnType("nvarchar(20)");

                entity.Property(p => p.Title)
                      .IsRequired()
                      .HasColumnName("Title")
                      .HasColumnType("nvarchar(255)");

                entity.HasIndex(p => p.Title);

                entity.Property(p => p.Author)
                      .IsRequired()
                      .HasColumnName("Author")
                      .HasColumnType("nvarchar(255)");

                entity.HasIndex(p => p.Author);

                entity.Property(p => p.Publisher)
                      .HasColumnName("Publisher")
                      .HasColumnType("nvarchar(255)");

                entity.Property(p => p.PublicationYear)
                      .HasColumnName("PublicationYear")
                      .HasColumnType("int");

                entity.Property(p => p.Category)
                      .IsRequired()
                      .HasColumnName("Category")
                      .HasColumnType("nvarchar(100)");

                entity.HasIndex(p => p.Category);

                entity.Property(p => p.Description)
                      .HasColumnName("Description")
                      .HasColumnType("nvarchar(max)");

                entity.Property(p => p.TotalCopies)
                      .IsRequired()
                      .HasColumnName("TotalCopies")
                      .HasColumnType("int")
                      .HasDefaultValue(1);

                entity.Property(p => p.AvailableCopies)
                      .IsRequired()
                      .HasColumnName("AvailableCopies")
                      .HasColumnType("int")
                      .HasDefaultValue(1);

                entity.HasCheckConstraint("CHK_Books_Copies", "AvailableCopies <= TotalCopies AND AvailableCopies >= 0");

                entity.Property(p => p.Location)
                      .HasColumnName("Location")
                      .HasColumnType("nvarchar(100)");

                entity.Property(p => p.Status)
                      .IsRequired()
                      .HasColumnName("Status")
                      .HasColumnType("nvarchar(20)")
                      .HasDefaultValue("Available");

                entity.HasIndex(p => p.Status);

                entity.HasCheckConstraint("CHK_Books_Status", "Status IN ('Available', 'CheckedOut', 'Reserved', 'Lost', 'Maintenance')");

                entity.Property(p => p.ThumbnailUrl)
                      .HasColumnName("ThumbnailUrl")
                      .HasColumnType("nvarchar(255)");

                entity.Property(p => p.CreatedAt)
                      .IsRequired()
                      .HasColumnName("CreatedAt")
                      .HasColumnType("datetime2(7)")
                      .HasDefaultValueSql("SYSUTCDATETIME()");

                entity.Property(p => p.UpdatedAt)
                      .IsRequired()
                      .HasColumnName("UpdatedAt")
                      .HasColumnType("datetime2(7)")
                      .HasDefaultValueSql("SYSUTCDATETIME()");

            });

            modelBuilder.Entity<Borrowing>(entity =>
            {
                entity.ToTable("Borrowings");

                entity.HasKey(p => p.BorrowId);

                entity.Property(p => p.BorrowId)
                      .HasColumnName("BorrowId")
                      .HasColumnType("uniqueidentifier")
                      .HasDefaultValueSql("NEWID()");

                entity.Property(p => p.UserId)
                      .IsRequired()
                      .HasColumnName("UserId")
                      .HasColumnType("uniqueidentifier");

                entity.HasIndex(p => p.UserId);

                entity.HasOne(lt => lt.User)
                      .WithMany(u => u.Borrowings)
                      .HasForeignKey(u => u.UserId);

                entity.Property(p => p.BookId)
                      .IsRequired()
                      .HasColumnName("BookId")
                      .HasColumnType("uniqueidentifier");

                entity.HasIndex(p => p.BookId);

                entity.HasOne(lt => lt.Book)
                      .WithMany(u => u.Borrowings)
                      .HasForeignKey(u => u.BookId);

                entity.Property(p => p.BorrowDate)
                      .IsRequired()
                      .HasColumnName("BorrowDate")
                      .HasColumnType("datetime2(7)")
                      .HasDefaultValueSql("SYSUTCDATETIME()");

                entity.Property(p => p.DueDate)
                      .IsRequired()
                      .HasColumnName("DueDate")
                      .HasColumnType("datetime2(7)");

                entity.HasIndex(p => p.DueDate);

                entity.Property(p => p.ReturnDate)
                      .HasColumnName("ReturnDate")
                      .HasColumnType("datetime2(7)");

                entity.Property(p => p.Status)
                      .IsRequired()
                      .HasColumnName("Status")
                      .HasColumnType("nvarchar(20)")
                      .HasDefaultValue("Borrowed");

                entity.HasIndex(p => p.Status);

                entity.HasCheckConstraint("CHK_Borrowings_Status", "Status IN ('Borrowed', 'Returned', 'Overdue', 'Lost')");

                entity.Property(p => p.FineAmount)
                      .HasColumnName("FineAmount")
                      .HasColumnType("decimal(10,2)");
            });

            modelBuilder.Entity<Reservation>(entity =>
            {
                entity.ToTable("Reservations");

                entity.HasKey(p => p.ReservationId);

                entity.Property(p => p.ReservationId)
                      .HasColumnName("ReservationId")
                      .HasColumnType("uniqueidentifier")
                      .HasDefaultValueSql("NEWID()");

                entity.Property(p => p.UserId)
                      .IsRequired()
                      .HasColumnName("UserId")
                      .HasColumnType("uniqueidentifier");

                entity.HasIndex(p => p.UserId);

                entity.HasOne(lt => lt.User)
                      .WithMany(u => u.Reservations)
                      .HasForeignKey(u => u.UserId);

                entity.Property(p => p.BookId)
                      .IsRequired()
                      .HasColumnName("BookId")
                      .HasColumnType("uniqueidentifier");

                entity.HasIndex(p => p.BookId);

                entity.HasOne(lt => lt.Book)
                      .WithMany(u => u.Reservations)
                      .HasForeignKey(u => u.BookId);

                entity.Property(p => p.ReservationDate)
                      .IsRequired()
                      .HasColumnName("ReservationDate")
                      .HasColumnType("datetime2(7)")
                      .HasDefaultValueSql("SYSUTCDATETIME()");

                entity.Property(p => p.ExpiryDate)
                      .IsRequired()
                      .HasColumnName("ExpiryDate")
                      .HasColumnType("datetime2(7)");

                entity.Property(p => p.Status)
                      .IsRequired()
                      .HasColumnName("Status")
                      .HasColumnType("nvarchar(20)")
                      .HasDefaultValue("Pending");

                entity.HasIndex(p => p.Status);

                entity.HasCheckConstraint("CHK_Reservations_Status", "Status IN ('Pending', 'Fulfilled', 'Cancelled', 'Expired')");
            });

            modelBuilder.Entity<Fine>(entity =>
            {
                entity.ToTable("Fines");

                entity.HasKey(p => p.FineId);

                entity.Property(p => p.FineId)
                      .HasColumnName("FineId")
                      .HasColumnType("uniqueidentifier")
                      .HasDefaultValueSql("NEWID()");

                entity.Property(p => p.UserId)
                      .IsRequired()
                      .HasColumnName("UserId")
                      .HasColumnType("uniqueidentifier");

                entity.HasIndex(p => p.UserId);

                entity.HasOne(lt => lt.User)
                      .WithMany(u => u.Fines)
                      .HasForeignKey(u => u.UserId);

                entity.Property(p => p.BorrowId)
                      .IsRequired()
                      .HasColumnName("BorrowId")
                      .HasColumnType("uniqueidentifier");

                entity.HasIndex(p => p.BorrowId);

                entity.HasOne(lt => lt.Borrowing)
                      .WithMany(u => u.Fines)
                      .HasForeignKey(u => u.BorrowId);

                entity.Property(p => p.Amount)
                      .IsRequired()
                      .HasColumnName("Amount")
                      .HasColumnType("decimal(10,2)");

                entity.Property(p => p.IssueDate)
                      .IsRequired()
                      .HasColumnName("IssueDate")
                      .HasColumnType("datetime2(7)")
                      .HasDefaultValueSql("SYSUTCDATETIME()");

                entity.Property(p => p.PaidDate)
                      .HasColumnName("PaidDate")
                      .HasColumnType("datetime2(7)");

                entity.Property(p => p.Status)
                      .IsRequired()
                      .HasColumnName("Status")
                      .HasColumnType("nvarchar(20)")
                      .HasDefaultValue("Unpaid");

                entity.HasIndex(p => p.Status);

                entity.HasCheckConstraint("CHK_Fines_Status", "Status IN ('Unpaid', 'Paid', 'Waived')");
            });

            modelBuilder.Entity<SystemLog>(entity =>
            {
                entity.ToTable("SystemLogs");

                entity.HasKey(p => p.LogId);

                entity.Property(p => p.LogId)
                      .HasColumnName("LogId")
                      .HasColumnType("uniqueidentifier")
                      .HasDefaultValueSql("NEWID()");

                entity.Property(p => p.UserId)
                      .HasColumnName("UserId")
                      .HasColumnType("uniqueidentifier");

                entity.HasIndex(p => p.UserId);

                entity.HasOne(lt => lt.User)
                      .WithMany(u => u.SystemLogs)
                      .HasForeignKey(u => u.UserId);

                entity.Property(p => p.Action)
                      .IsRequired()
                      .HasColumnName("Action")
                      .HasColumnType("nvarchar(100)");

                entity.HasIndex(p => p.Action);

                entity.Property(p => p.EntityType)
                      .HasColumnName("EntityType")
                      .HasColumnType("nvarchar(50)");

                entity.Property(p => p.EntityId)
                      .HasColumnName("EntityId")
                      .HasColumnType("nvarchar(50)");

                entity.Property(p => p.Details)
                      .HasColumnName("Details")
                      .HasColumnType("nvarchar(max)");

                entity.Property(p => p.IpAddress)
                      .HasColumnName("IpAddress")
                      .HasColumnType("nvarchar(50)");

                entity.Property(p => p.Timestamp)
                      .IsRequired()
                      .HasColumnName("Timestamp")
                      .HasColumnType("datetime2(7)")
                      .HasDefaultValueSql("SYSUTCDATETIME()");

                entity.HasIndex(p => p.Timestamp);
            });
        }
    }
}
