using System;
using ABPmicroservice.Erp.Companies;

namespace ABPmicroservice.Erp.Finance;

/// <summary>A posting group: a code that master data carries and a posting setup turns into G/L accounts.</summary>
public abstract class PostingGroupBase : CodeTableEntity
{
    protected PostingGroupBase() { }

    protected PostingGroupBase(Guid id, string code, string description)
        : base(id, code, description) { }
}

/// <summary>
/// Gen. Business Posting Group: who a customer or vendor is
/// (domestic, EU, export), the row key of the General Posting Setup.
/// </summary>
public class GenBusinessPostingGroup : PostingGroupBase
{
    protected GenBusinessPostingGroup() { }

    public GenBusinessPostingGroup(Guid id, string code, string description = null)
        : base(id, code, description) { }
}

/// <summary>
/// Gen. Product Posting Group: what an item or G/L line is
/// (retail goods, services, raw materials), the column key of the General Posting Setup.
/// </summary>
public class GenProductPostingGroup : PostingGroupBase
{
    protected GenProductPostingGroup() { }

    public GenProductPostingGroup(Guid id, string code, string description = null)
        : base(id, code, description) { }
}
