using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using VirtualPos.Domain.Entities;

namespace VirtualPos.Infrastructure.Persistence
{
    public class VirtualPosDbContext(DbContextOptions<VirtualPosDbContext> options) : DbContext(options)
    {
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<PaymentLog> PaymentLogs => Set<PaymentLog>();
        public DbSet<BinRange> BinRanges => Set<BinRange>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Payment>(e =>
            {
                e.HasKey(p => p.Id);
                e.HasIndex(p => p.OrderNo).IsUnique();
                e.HasIndex(p => new { p.Status, p.CreatedAt });
                e.Property(p => p.Amount).HasPrecision(18, 2);
                e.Property(p => p.Currency).HasMaxLength(3).IsFixedLength();
                e.Property(p => p.CardHolderName).HasMaxLength(100);
                e.Property(p => p.MaskedCardNumber).HasMaxLength(19);
                e.Property(p => p.BinNumber).HasMaxLength(8);
                e.Property(p => p.BankEftCode).HasMaxLength(4);
                e.Property(p => p.BankName).HasMaxLength(100);
                e.Property(p => p.Provider).HasMaxLength(20);
                e.Property(p => p.ProviderPaymentId).HasMaxLength(100);
                e.Property(p => p.MdStatus).HasMaxLength(5);
                e.Property(p => p.ErrorCode).HasMaxLength(50);
                e.Property(p => p.ErrorMessage).HasMaxLength(500);
                e.HasMany(p => p.Logs)
                 .WithOne(l => l.Payment)
                 .HasForeignKey(l => l.PaymentId);
            });

            modelBuilder.Entity<PaymentLog>(e =>
            {
                e.HasKey(l => l.Id);
                e.Property(l => l.Step).HasMaxLength(30);
            });

            // BKM BIN listesi: tabloyu BKM scripti kurar, EF sadece okur
            modelBuilder.Entity<BinRange>(e =>
            {
                e.ToTable("bin_list", t => t.ExcludeFromMigrations());
                e.HasKey(b => new { b.BinRangeMin, b.BinRangeMax });
                e.Property(b => b.BinRangeMin).HasColumnName("binRangeMin");
                e.Property(b => b.BinRangeMax).HasColumnName("binRangeMax");
                e.Property(b => b.CardType).HasColumnName("cardType");
                e.Property(b => b.CardTypeCode).HasColumnName("cardTypeCode");
                e.Property(b => b.BankEftCode).HasColumnName("binOwnerEftCode");
                e.Property(b => b.BankName).HasColumnName("binOwner");
                e.Property(b => b.BrandName).HasColumnName("brandName");
                e.Property(b => b.CardNoLength).HasColumnName("cardNoLength");
                e.Property(b => b.IsMastercard).HasColumnName("isMastercard");
                e.Property(b => b.IsVisa).HasColumnName("isVisa");
                e.Property(b => b.IsTroy).HasColumnName("isTroy");
                e.Property(b => b.IsAmex).HasColumnName("isAmex");
                e.Property(b => b.IsCreditCard).HasColumnName("isCreditCard");
                e.Property(b => b.IsDebitCard).HasColumnName("isDebitCard");
                e.Property(b => b.IsPrepaidCard).HasColumnName("isPrepaidCard");
            });
        }
    }
}
