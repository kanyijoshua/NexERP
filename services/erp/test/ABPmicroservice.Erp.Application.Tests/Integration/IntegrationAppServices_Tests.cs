using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace ABPmicroservice.Erp.Integration;

public class IntegrationAppServices_Tests : ErpApplicationTestBase
{
    private readonly IWebServiceAppService _webServices;
    private readonly IIntegrationDataAppService _integrationData;
    private readonly IWebhookSubscriptionAppService _webhooks;

    public IntegrationAppServices_Tests()
    {
        _webServices = GetRequiredService<IWebServiceAppService>();
        _integrationData = GetRequiredService<IIntegrationDataAppService>();
        _webhooks = GetRequiredService<IWebhookSubscriptionAppService>();
    }

    /// <summary>
    /// Nothing is readable through the integration API until it is published, which is the whole
    /// point of having a web service list rather than exposing every table.
    /// </summary>
    [Fact]
    public async Task A_Service_Is_Not_Readable_Until_It_Is_Published()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var service = await _webServices.CreateAsync(
                new CreateUpdateWebServiceDto { ServiceName = "Customers", EntityName = "Customer" }
            );

            service.Published.ShouldBeFalse();

            var refused = await Should.ThrowAsync<BusinessException>(
                () => _integrationData.QueryAsync(new IntegrationQueryInput { ServiceName = "Customers" })
            );
            refused.Code.ShouldBe(ErpErrorCodes.Integration.EntityNotPublished);

            await _webServices.SetPublishedAsync(new SetWebServicePublishedInput { Id = service.Id, Published = true });

            var result = await _integrationData.QueryAsync(new IntegrationQueryInput { ServiceName = "Customers" });
            result.EntityName.ShouldBe("Customer");
            result.TotalCount.ShouldBeGreaterThan(0);
        });
    }

    [Fact]
    public async Task Two_Services_Cannot_Share_A_Name()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            await _webServices.CreateAsync(
                new CreateUpdateWebServiceDto { ServiceName = "Items", EntityName = "Item" }
            );

            var exception = await Should.ThrowAsync<BusinessException>(
                () =>
                    _webServices.CreateAsync(
                        new CreateUpdateWebServiceDto { ServiceName = "Items", EntityName = "ItemCategory" }
                    )
            );

            exception.Code.ShouldBe(ErpErrorCodes.Integration.ServiceNameAlreadyExists);
        });
    }

    [Fact]
    public async Task A_Service_Can_Only_Expose_A_Table_The_Registry_Knows()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var exception = await Should.ThrowAsync<BusinessException>(
                () =>
                    _webServices.CreateAsync(
                        new CreateUpdateWebServiceDto { ServiceName = "Ghost", EntityName = "NoSuchTable" }
                    )
            );

            exception.Code.ShouldBe(ErpErrorCodes.Exporting.UnknownEntity);
        });
    }

    /// <summary>
    /// An excluded field must not come back, and must not be usable to filter or sort either:
    /// ordering by a hidden column would let a caller infer it.
    /// </summary>
    [Fact]
    public async Task An_Excluded_Field_Is_Hidden_And_Cannot_Be_Filtered_On()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var service = await _webServices.CreateAsync(
                new CreateUpdateWebServiceDto
                {
                    ServiceName = "PublicCustomers",
                    EntityName = "Customer",
                    ExcludedFields = "Balance,CreditLimit",
                }
            );
            await _webServices.SetPublishedAsync(new SetWebServicePublishedInput { Id = service.Id, Published = true });

            var fields = await _integrationData.GetFieldsAsync("PublicCustomers");
            fields.Items.ShouldNotContain(f => f.Name == "Balance");

            var result = await _integrationData.QueryAsync(
                new IntegrationQueryInput { ServiceName = "PublicCustomers" }
            );
            Row(result.Items[0]).Keys.ShouldNotContain("Balance");

            var refused = await Should.ThrowAsync<BusinessException>(
                () =>
                    _integrationData.QueryAsync(
                        new IntegrationQueryInput
                        {
                            ServiceName = "PublicCustomers",
                            Filters =
                            [
                                new Exporting.EntityFilterDto
                                {
                                    Field = "Balance",
                                    Operator = Exporting.EntityFilterOperator.GreaterThan,
                                    Value = "0",
                                },
                            ],
                        }
                    )
            );

            refused.Code.ShouldBe(ErpErrorCodes.Exporting.UnknownField);
        });
    }

    [Fact]
    public async Task A_Query_Returns_Only_The_Fields_That_Were_Asked_For()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var service = await _webServices.CreateAsync(
                new CreateUpdateWebServiceDto { ServiceName = "CustomerNames", EntityName = "Customer" }
            );
            await _webServices.SetPublishedAsync(new SetWebServicePublishedInput { Id = service.Id, Published = true });

            var result = await _integrationData.QueryAsync(
                new IntegrationQueryInput { ServiceName = "CustomerNames", Fields = ["No", "Name"] }
            );

            result.Fields.ShouldBe(new[] { "No", "Name" });
            Row(result.Items[0]).Keys.ShouldBe(new[] { "No", "Name" });
        });
    }

    private static IReadOnlyDictionary<string, object> Row(object item)
    {
        return (IReadOnlyDictionary<string, object>)item;
    }

    /// <summary>
    /// A subscription is created with a secret, and the secret is handed back exactly once so it
    /// can be copied into the receiving system.
    /// </summary>
    [Fact]
    public async Task A_Subscription_Is_Given_A_Secret_That_Is_Shown_Once()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var created = await _webhooks.CreateAsync(
                new CreateUpdateWebhookSubscriptionDto
                {
                    Name = "Customer feed",
                    EntityName = "Customer",
                    EndpointUrl = "https://example.test/hooks/customers",
                }
            );

            created.Secret.ShouldNotBeNullOrWhiteSpace();

            var listed = await _webhooks.GetListAsync();
            listed.Items.Single(s => s.Id == created.Id).Secret.ShouldBeNull();
        });
    }

    /// <summary>
    /// A payload signed with a secret and sent over plain http would be readable on the way, so
    /// only https is accepted for anything but a local address.
    /// </summary>
    [Fact]
    public async Task A_Remote_Endpoint_Must_Use_Https()
    {
        var exception = await Should.ThrowAsync<BusinessException>(
            () =>
                InCompanyAsync(
                    DefaultCompanyName,
                    () =>
                        _webhooks.CreateAsync(
                            new CreateUpdateWebhookSubscriptionDto
                            {
                                Name = "Insecure",
                                EntityName = "Customer",
                                EndpointUrl = "http://example.test/hooks",
                            }
                        )
                )
        );

        exception.Code.ShouldBe(ErpErrorCodes.Integration.EndpointNotHttps);
    }

    [Fact]
    public async Task A_Local_Endpoint_May_Use_Plain_Http_For_Development()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var created = await _webhooks.CreateAsync(
                new CreateUpdateWebhookSubscriptionDto
                {
                    Name = "Local listener",
                    EntityName = "Customer",
                    EndpointUrl = "http://localhost:5005/hooks",
                }
            );

            created.EndpointUrl.ShouldBe("http://localhost:5005/hooks");
        });
    }

    [Fact]
    public async Task A_Test_Notification_Is_Queued_For_Sending()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var subscription = await _webhooks.CreateAsync(
                new CreateUpdateWebhookSubscriptionDto
                {
                    Name = "Test feed",
                    EntityName = "Customer",
                    EndpointUrl = "https://example.test/hooks",
                }
            );

            var delivery = await _webhooks.SendTestAsync(subscription.Id);

            delivery.Status.ShouldBe(WebhookDeliveryStatus.Pending);
            delivery.Payload.ShouldContain("\"changeKind\":\"Test\"");

            var deliveries = await _webhooks.GetDeliveriesAsync(
                new GetWebhookDeliveriesInput { SubscriptionId = subscription.Id }
            );
            deliveries.TotalCount.ShouldBe(1);
        });
    }

    /// <summary>
    /// A change to a subscribed table queues a notification, and the payload carries the row so
    /// the receiver does not have to call back for it.
    /// </summary>
    [Fact]
    public async Task A_Change_To_A_Subscribed_Table_Queues_A_Notification()
    {
        var customers = GetRequiredService<Sales.ICustomerAppService>();

        var subscriptionId = await InCompanyAsync(
            DefaultCompanyName,
            async () =>
            {
                var subscription = await _webhooks.CreateAsync(
                    new CreateUpdateWebhookSubscriptionDto
                    {
                        Name = "Customer changes",
                        EntityName = "Customer",
                        EndpointUrl = "https://example.test/hooks/customers",
                        ChangeKinds = EntityChangeKind.Created,
                    }
                );

                return subscription.Id;
            }
        );

        await InCompanyAsync(
            DefaultCompanyName,
            () =>
                customers.CreateAsync(
                    new Sales.CreateUpdateCustomerDto { No = "C09090", Name = "Webhook Test Customer" }
                )
        );

        var deliveries = await InCompanyAsync(
            DefaultCompanyName,
            () => _webhooks.GetDeliveriesAsync(new GetWebhookDeliveriesInput { SubscriptionId = subscriptionId })
        );

        deliveries.TotalCount.ShouldBe(1);
        deliveries.Items[0].ChangeKind.ShouldBe(EntityChangeKind.Created);
        deliveries.Items[0].Payload.ShouldContain("C09090");
    }
}
