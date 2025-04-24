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
        private string _database = "Oracle";
        //SQLServer
        //Oracle
        public LibDbContext(IConfiguration configuration)
        {
            _connectionString=configuration.GetConnectionString(_database);
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                if(_database=="SQLServer")
                {
                    optionsBuilder.UseSqlServer(_connectionString);
                }
                else if(_database=="Oracle")
                {
                    optionsBuilder.UseOracle(_connectionString);
                }
                    
            }
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if(_database=="SQLServer")
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
            else if(_database=="Oracle")
            {
                modelBuilder.Entity<User>(entity =>
                {
                    entity.ToTable("USERS");

                    entity.HasKey(p => p.UserId);

                    entity.Property(p => p.UserId)
                          .HasColumnName("USERID")
                          .HasColumnType("RAW(16)")
                          .HasDefaultValueSql("SYS_GUID()");

                    entity.Property(p => p.Email)
                          .IsRequired()
                          .HasColumnName("EMAIL")
                          .HasColumnType("NVARCHAR2(255)");

                    entity.HasIndex(p => p.Email)
                          .IsUnique();

                    entity.Property(p => p.Name)
                          .IsRequired()
                          .HasColumnName("NAME")
                          .HasColumnType("NVARCHAR2(100)");

                    entity.Property(p => p.HashedPassword)
                          .HasColumnName("HASHEDPASSWORD")
                          .HasColumnType("NVARCHAR2(255)");

                    entity.Property(p => p.Status)
                          .IsRequired()
                          .HasColumnName("STATUS")
                          .HasColumnType("NVARCHAR2(20)")
                          .HasDefaultValue("Active");

                    entity.HasCheckConstraint("CHK_User_Status", "Status IN ('Active', 'Suspended', 'Deleted')");

                    entity.Property(p => p.Role)
                          .IsRequired()
                          .HasColumnName("ROLE")
                          .HasColumnType("NVARCHAR2(20)")
                          .HasDefaultValue("User");

                    entity.HasCheckConstraint("CHK_User_Role", "Role IN ('User', 'Librarian', 'Admin')");

                    entity.Property(p => p.CreatedAt)
                          .IsRequired()
                          .HasColumnName("CREATEDAT")
                          .HasColumnType("TIMESTAMP(7)")
                          .HasDefaultValueSql("SYSTIMESTAMP");

                    entity.Property(p => p.LastLogin)
                          .HasColumnName("LASTLOGIN")
                          .HasColumnType("TIMESTAMP2(7)");

                });

                modelBuilder.Entity<LoginToken>(entity =>
                {
                    entity.ToTable("LOGINTOKENS");

                    entity.HasKey(p => p.TokenId);

                    entity.Property(p => p.TokenId)
                          .HasColumnName("TOKENID")
                          .HasColumnType("RAW(16)")
                          .HasDefaultValueSql("SYS_GUID()");

                    entity.Property(p => p.UserId)
                          .IsRequired()
                          .HasColumnName("USERID")
                          .HasColumnType("RAW(16)");

                    entity.HasOne(lt => lt.User)
                          .WithMany(u => u.LoginTokens)
                          .HasForeignKey(u => u.UserId)
                          .OnDelete(DeleteBehavior.Cascade);

                    entity.HasIndex(p => p.UserId);

                    entity.Property(p => p.Token)
                          .IsRequired()
                          .HasColumnName("TOKEN")
                          .HasColumnType("NVARCHAR2(255)");

                    entity.HasIndex(p => p.Token)
                          .IsUnique();

                    entity.Property(p => p.Expiration)
                          .IsRequired()
                          .HasColumnName("EXPIRATION")
                          .HasColumnType("TIMESTAMP(7)");

                    entity.Property(p => p.IsUsed)
                          .IsRequired()
                          .HasColumnName("ISUSED")
                          .HasColumnType("NUMBER(1)")
                          .HasDefaultValue(0);

                    entity.Property(p => p.CreatedAt)
                          .IsRequired()
                          .HasColumnName("CREATEDAT")
                          .HasColumnType("TIMESTAMP(7)")
                          .HasDefaultValueSql("SYSTIMESTAMP");
                });

                modelBuilder.Entity<Book>(entity =>
                {
                    entity.ToTable("BOOKS");

                    entity.HasKey(p => p.BookId);

                    entity.Property(p => p.BookId)
                          .HasColumnName("BOOKID")
                          .HasColumnType("RAW(16)")
                          .HasDefaultValueSql("SYS_GUID()");

                    entity.Property(p => p.ISBN)
                          .IsRequired()
                          .HasColumnName("ISBN")
                          .HasColumnType("NVARCHAR2(20)");

                    entity.Property(p => p.Title)
                          .IsRequired()
                          .HasColumnName("TITLE")
                          .HasColumnType("NVARCHAR2(255)");

                    entity.HasIndex(p => p.Title);

                    entity.Property(p => p.Author)
                          .IsRequired()
                          .HasColumnName("AUTHOR")
                          .HasColumnType("NVARCHAR2(255)");

                    entity.HasIndex(p => p.Author);

                    entity.Property(p => p.Publisher)
                          .HasColumnName("PUBLISHER")
                          .HasColumnType("NVARCHAR2(255)");

                    entity.Property(p => p.PublicationYear)
                          .HasColumnName("PUBLICATIONYEAR")
                          .HasColumnType("NUMBER(10)");

                    entity.Property(p => p.Category)
                          .IsRequired()
                          .HasColumnName("CATEGORY")
                          .HasColumnType("NVARCHAR2(100)");

                    entity.HasIndex(p => p.Category);

                    entity.Property(p => p.Description)
                          .HasColumnName("DESCRIPTION")
                          .HasColumnType("CLOB");

                    entity.Property(p => p.TotalCopies)
                          .IsRequired()
                          .HasColumnName("TOTALCOPIES")
                          .HasColumnType("NUMBER(10)")
                          .HasDefaultValue(1);

                    entity.Property(p => p.AvailableCopies)
                          .IsRequired()
                          .HasColumnName("AVAILABLECOPIES")
                          .HasColumnType("NUMBER(10)")
                          .HasDefaultValue(1);

                    entity.HasCheckConstraint("CHK_Books_Copies", "AvailableCopies <= TotalCopies AND AvailableCopies >= 0");

                    entity.Property(p => p.Location)
                          .HasColumnName("LOCATION")
                          .HasColumnType("NVARCHAR2(100)");

                    entity.Property(p => p.Status)
                          .IsRequired()
                          .HasColumnName("STATUS")
                          .HasColumnType("NVARCHAR2(20)")
                          .HasDefaultValue("Available");

                    entity.HasIndex(p => p.Status);

                    entity.HasCheckConstraint("CHK_Books_Status", "Status IN ('Available', 'CheckedOut', 'Reserved', 'Lost', 'Maintenance')");

                    entity.Property(p => p.ThumbnailUrl)
                          .HasColumnName("THUMBNAILURL")
                          .HasColumnType("NVARCHAR2(255)");

                    entity.Property(p => p.CreatedAt)
                          .IsRequired()
                          .HasColumnName("CREATEDAT")
                          .HasColumnType("TIMESTAMP(7)")
                          .HasDefaultValueSql("SYSTIMESTAMP");

                    entity.Property(p => p.UpdatedAt)
                          .IsRequired()
                          .HasColumnName("UPDATEDAT")
                          .HasColumnType("TIMESTAMP(7)")
                          .HasDefaultValueSql("SYSTIMESTAMP");

                });

                modelBuilder.Entity<Borrowing>(entity =>
                {
                    entity.ToTable("BORROWINGS");

                    entity.HasKey(p => p.BorrowId);

                    entity.Property(p => p.BorrowId)
                          .HasColumnName("BORROWID")
                          .HasColumnType("RAW(16)")
                          .HasDefaultValueSql("SYS_GUID()");

                    entity.Property(p => p.UserId)
                          .IsRequired()
                          .HasColumnName("USERID")
                          .HasColumnType("RAW(16)");

                    entity.HasIndex(p => p.UserId);

                    entity.HasOne(lt => lt.User)
                          .WithMany(u => u.Borrowings)
                          .HasForeignKey(u => u.UserId);

                    entity.Property(p => p.BookId)
                          .IsRequired()
                          .HasColumnName("BOOKID")
                          .HasColumnType("RAW(16)");

                    entity.HasIndex(p => p.BookId);

                    entity.HasOne(lt => lt.Book)
                          .WithMany(u => u.Borrowings)
                          .HasForeignKey(u => u.BookId);

                    entity.Property(p => p.BorrowDate)
                          .IsRequired()
                          .HasColumnName("BORROWDATE")
                          .HasColumnType("TIMESTAMP(7)")
                          .HasDefaultValueSql("SYSTIMESTAMP");

                    entity.Property(p => p.DueDate)
                          .IsRequired()
                          .HasColumnName("DUEDATE")
                          .HasColumnType("TIMESTAMP(7)");

                    entity.HasIndex(p => p.DueDate);

                    entity.Property(p => p.ReturnDate)
                          .HasColumnName("RETURNDATE")
                          .HasColumnType("TIMESTAMP(7)");

                    entity.Property(p => p.Status)
                          .IsRequired()
                          .HasColumnName("STATUS")
                          .HasColumnType("NVARCHAR2(20)")
                          .HasDefaultValue("Borrowed");

                    entity.HasIndex(p => p.Status);

                    entity.HasCheckConstraint("CHK_Borrowings_Status", "Status IN ('Borrowed', 'Returned', 'Overdue', 'Lost')");

                    entity.Property(p => p.FineAmount)
                          .HasColumnName("FINEAMOUNT")
                          .HasColumnType("NUMBER(10,2)");
                });

                modelBuilder.Entity<Reservation>(entity =>
                {
                    entity.ToTable("RESERVATIONS");

                    entity.HasKey(p => p.ReservationId);

                    entity.Property(p => p.ReservationId)
                          .HasColumnName("RESERVATIONID")
                          .HasColumnType("RAW(16)")
                          .HasDefaultValueSql("SYS_GUID()");

                    entity.Property(p => p.UserId)
                          .IsRequired()
                          .HasColumnName("USERID")
                          .HasColumnType("RAW(16)");

                    entity.HasIndex(p => p.UserId);

                    entity.HasOne(lt => lt.User)
                          .WithMany(u => u.Reservations)
                          .HasForeignKey(u => u.UserId);

                    entity.Property(p => p.BookId)
                          .IsRequired()
                          .HasColumnName("BOOKID")
                          .HasColumnType("RAW(16)");

                    entity.HasIndex(p => p.BookId);

                    entity.HasOne(lt => lt.Book)
                          .WithMany(u => u.Reservations)
                          .HasForeignKey(u => u.BookId);

                    entity.Property(p => p.ReservationDate)
                          .IsRequired()
                          .HasColumnName("RESERVATIONDATE")
                          .HasColumnType("TIMESTAMP(7)")
                          .HasDefaultValueSql("SYSTIMESTAMP");

                    entity.Property(p => p.ExpiryDate)
                          .IsRequired()
                          .HasColumnName("EXPIRYDATE")
                          .HasColumnType("TIMESTAMP(7)");

                    entity.Property(p => p.Status)
                          .IsRequired()
                          .HasColumnName("STATUS")
                          .HasColumnType("NVARCHAR2(20)")
                          .HasDefaultValue("Pending");

                    entity.HasIndex(p => p.Status);

                    entity.HasCheckConstraint("CHK_Reservations_Status", "Status IN ('Pending', 'Fulfilled', 'Cancelled', 'Expired')");
                });

                modelBuilder.Entity<Fine>(entity =>
                {
                    entity.ToTable("FINES");

                    entity.HasKey(p => p.FineId);

                    entity.Property(p => p.FineId)
                          .HasColumnName("FINEID")
                          .HasColumnType("RAW(16)")
                          .HasDefaultValueSql("SYS_GUID()");

                    entity.Property(p => p.UserId)
                          .IsRequired()
                          .HasColumnName("USERID")
                          .HasColumnType("RAW(16)");

                    entity.HasIndex(p => p.UserId);

                    entity.HasOne(lt => lt.User)
                          .WithMany(u => u.Fines)
                          .HasForeignKey(u => u.UserId);

                    entity.Property(p => p.BorrowId)
                          .IsRequired()
                          .HasColumnName("BORROWID")
                          .HasColumnType("RAW(16)");

                    entity.HasIndex(p => p.BorrowId);

                    entity.HasOne(lt => lt.Borrowing)
                          .WithMany(u => u.Fines)
                          .HasForeignKey(u => u.BorrowId);

                    entity.Property(p => p.Amount)
                          .IsRequired()
                          .HasColumnName("AMOUNT")
                          .HasColumnType("NUMBER(10,2)");

                    entity.Property(p => p.IssueDate)
                          .IsRequired()
                          .HasColumnName("ISSUEDATE")
                          .HasColumnType("TIMESTAMP(7)")
                          .HasDefaultValueSql("SYSTIMESTAMP()");

                    entity.Property(p => p.PaidDate)
                          .HasColumnName("PAIDDATE")
                          .HasColumnType("TIMESTAMP(7)");

                    entity.Property(p => p.Status)
                          .IsRequired()
                          .HasColumnName("STATUS")
                          .HasColumnType("NVARCHAR2(20)")
                          .HasDefaultValue("Unpaid");

                    entity.HasIndex(p => p.Status);

                    entity.HasCheckConstraint("CHK_Fines_Status", "Status IN ('Unpaid', 'Paid', 'Waived')");
                });

                modelBuilder.Entity<SystemLog>(entity =>
                {
                    entity.ToTable("SYSTEMLOGS");

                    entity.HasKey(p => p.LogId);

                    entity.Property(p => p.LogId)
                          .HasColumnName("LOGID")
                          .HasColumnType("RAW(16)")
                          .HasDefaultValueSql("SYS_GUID()");

                    entity.Property(p => p.UserId)
                          .HasColumnName("USERID")
                          .HasColumnType("RAW(16)");

                    entity.HasIndex(p => p.UserId);

                    entity.HasOne(lt => lt.User)
                          .WithMany(u => u.SystemLogs)
                          .HasForeignKey(u => u.UserId);

                    entity.Property(p => p.Action)
                          .IsRequired()
                          .HasColumnName("ACTION")
                          .HasColumnType("NVARCHAR2(100)");

                    entity.HasIndex(p => p.Action);

                    entity.Property(p => p.EntityType)
                          .HasColumnName("ENTITYTYPE")
                          .HasColumnType("NVARCHAR2(50)");

                    entity.Property(p => p.EntityId)
                          .HasColumnName("ENTITYID")
                          .HasColumnType("NVARCHAR2(50)");

                    entity.Property(p => p.Details)
                          .HasColumnName("DETAILS")
                          .HasColumnType("CLOB");

                    entity.Property(p => p.IpAddress)
                          .HasColumnName("IPADDRESS")
                          .HasColumnType("NVARCHAR2(50)");

                    entity.Property(p => p.Timestamp)
                          .IsRequired()
                          .HasColumnName("TIMESTAMP")
                          .HasColumnType("TIMESTAMP(7)")
                          .HasDefaultValueSql("SYSTIMESTAMP");

                    entity.HasIndex(p => p.Timestamp);
                });
            }
            
        }
    }
}
