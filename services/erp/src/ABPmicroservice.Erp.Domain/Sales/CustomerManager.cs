using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Sales;

/// <summary>
/// Domain service for Customer invariants (uniqueness, credit limit).
/// </summary>
public class CustomerManager : DomainService
{
    private readonly IRepository<Customer, Guid> _customerRepository;

    public CustomerManager(IRepository<Customer, Guid> customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<Customer> CreateAsync(string no, string name)
    {
        await EnsureNoIsUniqueAsync(no);
        return new Customer(GuidGenerator.Create(), no, name);
    }

    public async Task EnsureNoIsUniqueAsync(string no, Guid? excludeId = null)
    {
        var existing = await _customerRepository.FirstOrDefaultAsync(x => x.No == no);
        if (existing != null && existing.Id != excludeId)
        {
            throw new BusinessException(ErpErrorCodes.Customers.CustomerAlreadyExists).WithData(
                "no",
                no
            );
        }
    }

    /// <summary>Validates a new sales amount against the customer's credit limit.</summary>
    public void EnsureCreditLimit(Customer customer, decimal additionalAmount)
    {
        if (customer.CreditLimit <= 0m)
        {
            return; // 0 = unlimited
        }

        if (customer.Balance + additionalAmount > customer.CreditLimit)
        {
            throw new BusinessException(ErpErrorCodes.Customers.CreditLimitExceeded).WithData(
                "no",
                customer.No
            );
        }
    }
}
