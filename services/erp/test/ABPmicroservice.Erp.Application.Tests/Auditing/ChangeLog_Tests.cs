using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Inventory;
using ABPmicroservice.Erp.Sales;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace ABPmicroservice.Erp.Auditing;

/// <summary>The change log: nothing is logged until it is switched on and a table is added to it.</summary>
public class ChangeLog_Tests : ErpApplicationTestBase
{
    private readonly IChangeLogSetupAppService _setup;
    private readonly IChangeLogTableAppService _tables;
    private readonly IChangeLogEntryAppService _entries;
    private readonly IRepository<Customer, Guid> _customers;
    private readonly IRepository<Item, Guid> _items;

    public ChangeLog_Tests()
    {
        _setup = GetRequiredService<IChangeLogSetupAppService>();
        _tables = GetRequiredService<IChangeLogTableAppService>();
        _entries = GetRequiredService<IChangeLogEntryAppService>();
        _customers = GetRequiredService<IRepository<Customer, Guid>>();
        _items = GetRequiredService<IRepository<Item, Guid>>();
    }

    [Fact]
    public async Task A_Logged_Table_Records_Each_Changed_Field_With_Old_And_New_Value()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            await _setup.UpdateAsync(new ChangeLogSetupDto { ChangeLogActivated = true });
            await _tables.CreateAsync(new CreateUpdateChangeLogTableDto { EntityName = "customer", LogModification = ChangeLogFieldFilterType.AllFields });

            var customer = await _customers.FirstAsync(c => c.No == "C00010");
            customer.SetName("Renamed Customer Ltd");
            customer.Block();
            await _customers.UpdateAsync(customer, autoSave: true);

            var log = (await _entries.GetListAsync(new GetChangeLogEntryListInput { EntityName = "Customer", RecordId = customer.Id, MaxResultCount = 50 })).Items;
            log.Select(e => e.FieldName).OrderBy(n => n).ShouldBe(["Blocked", "Name"]);
            var blocked = log.Single(e => e.FieldName == "Blocked");
            blocked.OldValue.ShouldBe("False");
            blocked.NewValue.ShouldBe("True");
            blocked.PrimaryKey.ShouldBe("C00010");
            blocked.TypeOfChange.ShouldBe(ChangeLogEntryType.Modification);
            log.Single(e => e.FieldName == "Name").NewValue.ShouldBe("Renamed Customer Ltd");
        });
    }

    [Fact]
    public async Task Some_Fields_Logs_Only_The_Listed_Fields_And_Nothing_Is_Logged_When_Off()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            await _tables.CreateAsync(new CreateUpdateChangeLogTableDto
            {
                EntityName = "Item",
                LogModification = ChangeLogFieldFilterType.SomeFields,
                TrackedFields = ["UnitPrice"],
            });

            var item = await _items.FirstAsync(i => i.No == "1000");
            item.SetPrice(999m);
            await _items.UpdateAsync(item, autoSave: true);
            (await _entries.GetListAsync(new GetChangeLogEntryListInput { RecordId = item.Id, MaxResultCount = 10 })).TotalCount.ShouldBe(0);

            await _setup.UpdateAsync(new ChangeLogSetupDto { ChangeLogActivated = true });
            item = await _items.GetAsync(item.Id);
            item.SetPrice(1001m);
            item.SetDescription("Bicycle, renamed");
            await _items.UpdateAsync(item, autoSave: true);

            var entry = (await _entries.GetListAsync(new GetChangeLogEntryListInput { RecordId = item.Id, MaxResultCount = 10 })).Items.ShouldHaveSingleItem();
            entry.FieldName.ShouldBe("UnitPrice");
            entry.OldValue.ShouldBe("999");
            entry.NewValue.ShouldBe("1001");

            var purged = await _entries.PurgeEntriesAsync(new PurgeChangeLogEntriesInput { Before = DateTime.Now.AddDays(1) });
            purged.Deleted.ShouldBe(1);
        });
    }

    [Fact]
    public async Task Unknown_Tables_And_Fields_Are_Refused()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var table = await Should.ThrowAsync<BusinessException>(() => _tables.CreateAsync(new CreateUpdateChangeLogTableDto { EntityName = "NoSuchTable" }));
            table.Code.ShouldBe(ErpErrorCodes.Auditing.UnknownTable);

            var field = await Should.ThrowAsync<BusinessException>(() => _tables.CreateAsync(new CreateUpdateChangeLogTableDto
            {
                EntityName = "Item",
                LogModification = ChangeLogFieldFilterType.SomeFields,
                TrackedFields = ["NoSuchField"],
            }));
            field.Code.ShouldBe(ErpErrorCodes.Auditing.UnknownField);

            (await _setup.GetLoggableTablesAsync()).ShouldContain(t => t.EntityName == "Customer" && t.Fields.Contains("Name"));
        });
    }
}
