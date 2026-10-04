using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Purchasing;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace ABPmicroservice.Erp.Attachments;

public class DocumentAttachmentAppService_Tests : ErpApplicationTestBase
{
    private readonly IDocumentAttachmentAppService _attachments;
    private readonly IVendorAppService _vendors;

    public DocumentAttachmentAppService_Tests()
    {
        _attachments = GetRequiredService<IDocumentAttachmentAppService>();
        _vendors = GetRequiredService<IVendorAppService>();
    }

    private static string Base64(string text) => Convert.ToBase64String(Encoding.UTF8.GetBytes(text));

    [Fact]
    public async Task A_File_Attached_To_A_Vendor_Comes_Back_Byte_For_Byte()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var vendor = await _vendors.CreateAsync(new CreateUpdateVendorDto { Name = "Lakeside Stationers" });

            var first = await _attachments.UploadAsync(new UploadDocumentAttachmentInput
            {
                EntityName = "vendor",
                RecordId = vendor.Id,
                FileName = "Supply Contract.PDF",
                ContentType = "application/pdf",
                ContentBase64 = Base64("contract body"),
            });
            var second = await _attachments.UploadAsync(new UploadDocumentAttachmentInput
            {
                EntityName = "Vendor",
                RecordId = vendor.Id,
                FileName = "price list.xlsx",
                ContentBase64 = Base64("prices"),
            });

            first.EntityName.ShouldBe("Vendor");
            first.No.ShouldBe(vendor.No);
            first.FileName.ShouldBe("Supply Contract");
            first.FileExtension.ShouldBe("pdf");
            first.FileType.ShouldBe(DocumentAttachmentFileType.Pdf);
            first.Size.ShouldBe(13);
            first.AttachmentNo.ShouldBe(1);
            second.AttachmentNo.ShouldBe(2);
            second.FileType.ShouldBe(DocumentAttachmentFileType.Excel);

            var list = await _attachments.GetListAsync(new GetDocumentAttachmentListInput { EntityName = "Vendor", RecordId = vendor.Id });
            list.Items.Select(a => a.FileName).ShouldBe(["Supply Contract", "price list"]);

            var download = await _attachments.DownloadAsync(first.Id);
            download.FileName.ShouldBe("Supply Contract.pdf");
            download.ContentType.ShouldBe("application/octet-stream");
            using var reader = new StreamReader(download.GetStream());
            (await reader.ReadToEndAsync()).ShouldBe("contract body");

            var renamed = await _attachments.UpdateAsync(first.Id, new UpdateDocumentAttachmentDto { FileName = "Contract 2026", DocumentFlowPurchase = true });
            renamed.FileName.ShouldBe("Contract 2026");
            renamed.FileExtension.ShouldBe("pdf");
            renamed.DocumentFlowPurchase.ShouldBeTrue();

            await _attachments.DeleteAsync(first.Id);
            (await _attachments.GetListAsync(new GetDocumentAttachmentListInput { EntityName = "Vendor", RecordId = vendor.Id })).Items.Count.ShouldBe(1);
        });
    }

    [Fact]
    public async Task Programs_Empty_Files_And_Unknown_Records_Are_Refused()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var vendor = await _vendors.CreateAsync(new CreateUpdateVendorDto { Name = "Highland Motors" });

            var program = await Should.ThrowAsync<BusinessException>(() => _attachments.UploadAsync(new UploadDocumentAttachmentInput
            {
                EntityName = "Vendor",
                RecordId = vendor.Id,
                FileName = "invoice.pdf.exe",
                ContentBase64 = Base64("MZ"),
            }));
            program.Code.ShouldBe(ErpErrorCodes.Attachments.FileTypeNotAllowed);

            var empty = await Should.ThrowAsync<BusinessException>(() => _attachments.UploadAsync(new UploadDocumentAttachmentInput
            {
                EntityName = "Vendor",
                RecordId = vendor.Id,
                FileName = "empty.txt",
                ContentBase64 = "!!not base64!!",
            }));
            empty.Code.ShouldBe(ErpErrorCodes.Attachments.FileEmpty);

            var nowhere = await Should.ThrowAsync<BusinessException>(() => _attachments.UploadAsync(new UploadDocumentAttachmentInput
            {
                EntityName = "Vendor",
                RecordId = Guid.NewGuid(),
                FileName = "note.txt",
                ContentBase64 = Base64("x"),
            }));
            nowhere.Code.ShouldBe(ErpErrorCodes.Attachments.RecordNotFound);

            var noTable = await Should.ThrowAsync<BusinessException>(() => _attachments.UploadAsync(new UploadDocumentAttachmentInput
            {
                EntityName = "Vendor",
                RecordId = vendor.Id,
                FileName = "big.bin",
                ContentBase64 = new string('A', (ErpDomainConsts.MaxAttachmentBytes / 3 + 10) * 4),
            }));
            noTable.Code.ShouldBe(ErpErrorCodes.Attachments.FileTooLarge);
        });
    }

    [Fact]
    public async Task Attachments_Of_Another_Company_Are_Not_Listed()
    {
        var vendorId = await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var vendor = await _vendors.CreateAsync(new CreateUpdateVendorDto { Name = "Coast Hauliers" });
            await _attachments.UploadAsync(new UploadDocumentAttachmentInput
            {
                EntityName = "Vendor",
                RecordId = vendor.Id,
                FileName = "licence.png",
                ContentBase64 = Base64("png"),
            });
            return vendor.Id;
        });

        await InCompanyAsync(SecondCompanyName, async () =>
        {
            var list = await _attachments.GetListAsync(new GetDocumentAttachmentListInput { EntityName = "Vendor", RecordId = vendorId });
            list.Items.ShouldBeEmpty();
        });
    }
}
