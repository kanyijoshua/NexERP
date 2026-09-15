using Volo.Abp;

namespace ABPmicroservice.Erp.Documents;

public class DocumentNotOpenException : BusinessException
{
    public DocumentNotOpenException(string documentNo)
        : base(ErpErrorCodes.Documents.DocumentNotReleased)
    {
        WithData("documentNo", documentNo);
    }
}
