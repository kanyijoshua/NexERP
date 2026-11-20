using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Purchasing;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace ABPmicroservice.Erp.Attachments;

public class RecordPictureAppService_Tests : ErpApplicationTestBase
{
    private static readonly byte[] PngHeader = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0, 0, 0, 0x0D, 1, 2, 3];
    private static readonly byte[] JpegHeader = [0xFF, 0xD8, 0xFF, 0xE0, 0, 0x10, (byte)'J', (byte)'F', (byte)'I', (byte)'F', 0, 1, 2];

    private readonly IRecordPictureAppService _pictures;
    private readonly IDocumentAttachmentAppService _attachments;
    private readonly IVendorAppService _vendors;

    public RecordPictureAppService_Tests()
    {
        _pictures = GetRequiredService<IRecordPictureAppService>();
        _attachments = GetRequiredService<IDocumentAttachmentAppService>();
        _vendors = GetRequiredService<IVendorAppService>();
    }

    [Fact]
    public async Task A_Picture_Is_Set_Replaced_And_Removed_Without_Touching_The_Attachments()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var vendor = await _vendors.CreateAsync(new CreateUpdateVendorDto { Name = "Lakeside Stationers" });
            var key = new GetRecordPictureInput { EntityName = "vendor", RecordId = vendor.Id };

            (await _pictures.GetAsync(key)).HasPicture.ShouldBeFalse();

            var png = await _pictures.UploadAsync(new UploadRecordPictureInput
            {
                EntityName = "Vendor",
                RecordId = vendor.Id,
                ContentBase64 = Convert.ToBase64String(PngHeader),
            });
            png.HasPicture.ShouldBeTrue();
            png.ContentType.ShouldBe("image/png");
            png.Size.ShouldBe(PngHeader.Length);

            await _pictures.UploadAsync(new UploadRecordPictureInput
            {
                EntityName = "Vendor",
                RecordId = vendor.Id,
                ContentBase64 = Convert.ToBase64String(JpegHeader),
            });
            var current = await _pictures.GetAsync(key);
            current.ContentType.ShouldBe("image/jpeg");
            Convert.FromBase64String(current.ContentBase64).SequenceEqual(JpegHeader).ShouldBeTrue();

            (await _attachments.GetListAsync(new GetDocumentAttachmentListInput { EntityName = "Vendor", RecordId = vendor.Id }))
                .Items.ShouldBeEmpty();

            await _pictures.DeleteAsync(key);
            (await _pictures.GetAsync(key)).HasPicture.ShouldBeFalse();
        });
    }

    [Fact]
    public async Task Files_That_Are_Not_Raster_Images_And_Unknown_Records_Are_Refused()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var vendor = await _vendors.CreateAsync(new CreateUpdateVendorDto { Name = "Hilltop Printing" });

            var svg = await Should.ThrowAsync<BusinessException>(() => _pictures.UploadAsync(new UploadRecordPictureInput
            {
                EntityName = "Vendor",
                RecordId = vendor.Id,
                ContentBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes("<svg xmlns=\"http://www.w3.org/2000/svg\"></svg>")),
            }));
            svg.Code.ShouldBe(ErpErrorCodes.Attachments.PictureNotAnImage);

            var tooLarge = new byte[ErpDomainConsts.MaxRecordPictureBytes + 1];
            PngHeader.CopyTo(tooLarge, 0);
            var large = await Should.ThrowAsync<BusinessException>(() => _pictures.UploadAsync(new UploadRecordPictureInput
            {
                EntityName = "Vendor",
                RecordId = vendor.Id,
                ContentBase64 = Convert.ToBase64String(tooLarge),
            }));
            large.Code.ShouldBe(ErpErrorCodes.Attachments.PictureTooLarge);

            var missing = await Should.ThrowAsync<BusinessException>(() => _pictures.UploadAsync(new UploadRecordPictureInput
            {
                EntityName = "Vendor",
                RecordId = Guid.NewGuid(),
                ContentBase64 = Convert.ToBase64String(PngHeader),
            }));
            missing.Code.ShouldBe(ErpErrorCodes.Attachments.RecordNotFound);
        });
    }
}
