using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Sequences;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Guids;

namespace ABPmicroservice.Erp.EntityFrameworkCore;

/// <summary>
/// Hands out consecutive entry numbers from a counter row, one per company and sequence.
/// <para>
/// The counter is advanced by a single <c>UPDATE … RETURNING</c>. Reading the row and writing it
/// back would let two concurrent postings take the same number, and an optimistic-concurrency
/// retry would make posting fail under exactly the load where it matters. One statement lets the
/// database hold the row lock for the moment it takes to add to it.
/// </para>
/// </summary>
public class EntryNoGenerator : IEntryNoGenerator, ITransientDependency
{
    private readonly IRepository<ErpNumberSequence, Guid> _repository;
    private readonly IDbContextProvider<ErpDbContext> _dbContextProvider;
    private readonly IGuidGenerator _guidGenerator;

    public EntryNoGenerator(
        IRepository<ErpNumberSequence, Guid> repository,
        IDbContextProvider<ErpDbContext> dbContextProvider,
        IGuidGenerator guidGenerator
    )
    {
        _repository = repository;
        _dbContextProvider = dbContextProvider;
        _guidGenerator = guidGenerator;
    }

    public async Task<long> NextAsync(string sequenceName, int count = 1)
    {
        var reserve = Math.Max(1, count);
        var sequenceId = await GetOrCreateSequenceIdAsync(sequenceName);

        var dbContext = await _dbContextProvider.GetDbContextAsync();
        var table = GetQualifiedTableName(dbContext);

        // EF maps a scalar result set by a column called "Value".
        var sql =
            $"UPDATE {table} SET \"LastValue\" = \"LastValue\" + {reserve} WHERE \"Id\" = {{0}} RETURNING \"LastValue\" AS \"Value\"";

        // Enumerated as it stands: any LINQ operator on top would make EF wrap the statement in a
        // subquery, and an UPDATE cannot be composed over.
        var rows = await dbContext.Database.SqlQueryRaw<long>(sql, sequenceId).ToListAsync();

        return rows.Single() - reserve + 1;
    }

    private async Task<Guid> GetOrCreateSequenceIdAsync(string sequenceName)
    {
        var existing = await _repository.FindAsync(s => s.Name == sequenceName);
        if (existing != null)
        {
            // The raw update below changes the row behind the tracker's back.
            await DetachAsync(existing);
            return existing.Id;
        }

        var created = new ErpNumberSequence(_guidGenerator.Create(), sequenceName);

        try
        {
            await _repository.InsertAsync(created, autoSave: true);
            await DetachAsync(created);
            return created.Id;
        }
        catch (DbUpdateException)
        {
            // Another request created the counter first; its row is the one to use.
            await DetachAsync(created);

            var winner = await _repository.FindAsync(s => s.Name == sequenceName);
            if (winner == null)
            {
                throw;
            }

            await DetachAsync(winner);
            return winner.Id;
        }
    }

    private async Task DetachAsync(ErpNumberSequence sequence)
    {
        var dbContext = await _dbContextProvider.GetDbContextAsync();
        var entry = dbContext.Entry(sequence);

        if (entry.State != EntityState.Detached)
        {
            entry.State = EntityState.Detached;
        }
    }

    private static string GetQualifiedTableName(ErpDbContext dbContext)
    {
        var entityType = dbContext.Model.FindEntityType(typeof(ErpNumberSequence))!;
        var table = entityType.GetTableName();
        var schema = entityType.GetSchema();

        return schema.IsNullOrWhiteSpace() ? $"\"{table}\"" : $"\"{schema}\".\"{table}\"";
    }
}
