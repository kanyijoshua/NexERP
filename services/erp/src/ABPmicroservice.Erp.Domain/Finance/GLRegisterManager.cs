using System;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Sequences;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Users;

namespace ABPmicroservice.Erp.Finance;

/// <summary>
/// Opens and closes the register that every posting run writes its entries into.
/// <para>
/// Journals, sales documents, purchase documents and reversals all go through here, so every
/// posted entry belongs to a register and can be found, navigated and reversed the same way.
/// </para>
/// </summary>
public class GLRegisterManager : DomainService
{
    private readonly IRepository<GLRegister, Guid> _registerRepository;
    private readonly IEntryNoGenerator _entryNoGenerator;
    private readonly ICurrentUser _currentUser;

    public GLRegisterManager(
        IRepository<GLRegister, Guid> registerRepository,
        IEntryNoGenerator entryNoGenerator,
        ICurrentUser currentUser
    )
    {
        _registerRepository = registerRepository;
        _entryNoGenerator = entryNoGenerator;
        _currentUser = currentUser;
    }

    public async Task<GLRegister> OpenAsync(DateTime postingDate, string sourceCode, string journalBatchName)
    {
        return new GLRegister(
            GuidGenerator.Create(),
            await _entryNoGenerator.NextAsync(ErpSequenceNames.GLRegister),
            await _entryNoGenerator.NextAsync(ErpSequenceNames.TransactionNo),
            postingDate,
            _currentUser.Id,
            _currentUser.UserName,
            sourceCode,
            journalBatchName
        );
    }

    /// <summary>
    /// Stores the register unless the run wrote nothing, in which case an empty register would
    /// only be a gap in the numbering for someone to wonder about later.
    /// </summary>
    public async Task<bool> CloseAsync(GLRegister register)
    {
        if (register.IsEmpty)
        {
            return false;
        }

        await _registerRepository.InsertAsync(register, autoSave: true);
        return true;
    }
}
