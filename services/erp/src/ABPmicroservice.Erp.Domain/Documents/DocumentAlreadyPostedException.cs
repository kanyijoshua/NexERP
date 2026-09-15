using Volo.Abp;

namespace ABPmicroservice.Erp.Documents;

public class DocumentAlreadyPostedException : BusinessException
{
    public DocumentAlreadyPostedException(string documentNo)
        : base(ErpErrorCodes.Documents.CannotModifyPostedDocument)
    {
        WithData("documentNo", documentNo);
    }
}
