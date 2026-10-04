using ABPmicroservice.Erp.Attachments;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace ABPmicroservice.Erp.EntityFrameworkCore;

public partial class ErpDbContext
{
    public DbSet<DocumentAttachment> DocumentAttachments { get; set; }
    public DbSet<DocumentAttachmentContent> DocumentAttachmentContents { get; set; }
}

public static class ErpAttachmentsModelCreatingExtensions
{
    public static void ConfigureErpAttachments(this ModelBuilder builder)
    {
        builder.Entity<DocumentAttachment>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "DocumentAttachments", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => new { x.CompanyId, x.EntityName, x.RecordId });
            b.Property(x => x.EntityName).IsRequired().HasMaxLength(ErpDomainConsts.MaxEntityNameLength);
            b.Property(x => x.No).HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.FileName).IsRequired().HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
            b.Property(x => x.FileExtension).HasMaxLength(ErpDomainConsts.MaxFileExtensionLength);
            b.Property(x => x.ContentType).HasMaxLength(ErpDomainConsts.MaxContentTypeLength);
            b.Property(x => x.AttachedByUserName).HasMaxLength(ErpDomainConsts.MaxUserNameLength);
            b.Ignore(x => x.FullFileName);
        });

        // The bytes sit in their own table so that listing attachments never reads them.
        builder.Entity<DocumentAttachmentContent>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "DocumentAttachmentContents", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Content).IsRequired();
        });
    }
}
