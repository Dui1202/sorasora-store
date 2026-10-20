using SoraSora.Modules.Orders.Exceptions;
using SoraSora.Shared.Interface;

namespace SoraSora.Modules.Orders.Domain;

public sealed class Order : IAuditable
{
    public const int MaxCustomerNameLength = 100;
    public const int MaxCustomerEmailLength = 254;
    public const int MaxCustomerPhoneNumberLength = 30;
    public const int MaxShippingAddressLength = 500;
    public const int MaxCustomerNoteLength = 2000;
    public const int MaxStoreNoteLength = 2000;
    private readonly List<OrderItem> _orderItem = [];
    public string Id { get; private set; } = NUlid.Ulid.NewUlid().ToString();
    public Guid Version { get; private set; } = Guid.NewGuid();
    public IReadOnlyCollection<OrderItem> OrderItems => _orderItem;
    public int TotalPrice => _orderItem.Sum(x => x.SellingPrice * x.Quantity);
    public string CustomerName { get; private set; } = null!;
    public string? CustomerEmail { get; private set; }
    public string CustomerPhoneNumber { get; private set; } = null!;
    public string ShippingAddress { get; private set; } = null!;
    public string? CustomerNote { get; private set; }
    public string? StoreNote { get; private set; }
    public OrderStatus Status { get; private set; } = OrderStatus.Pending;

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    private Order() { }

    public static Order Create(
        string customerName,
        string customerPhoneNumber,
        string shippingAddress,
        string? customerEmail = null,
        string? customerNote = null,
        string? storeNote = null
    )
    {
        ValidateDetails(
            customerName,
            customerPhoneNumber,
            shippingAddress,
            customerEmail,
            customerNote,
            storeNote
        );

        return new Order
        {
            CustomerName = customerName,
            CustomerEmail = customerEmail,
            CustomerPhoneNumber = customerPhoneNumber,
            ShippingAddress = shippingAddress,
            CustomerNote = customerNote,
            StoreNote = storeNote,
            Status = OrderStatus.Pending,
        };
    }

    public void AdvanceVersion()
    {
        Version = Guid.NewGuid();
    }

    public void AddOrderItem(OrderItem item)
    {
        _orderItem.Add(item);
    }

    public void RemoveOrderItem(OrderItem item)
    {
        _orderItem.Remove(item);
    }

    public void Confirm()
    {
        if (Status != OrderStatus.Pending)
        {
            throw new InvalidOperationException("Cannot confirm a order that is not pending");
        }
        Status = OrderStatus.Confirmed;
    }

    public void Cancel()
    {
        if (Status == OrderStatus.Completed)
        {
            throw new InvalidOperationException("Cannot cancel a completed order.");
        }

        if (Status == OrderStatus.Cancelled)
        {
            throw new InvalidOperationException("Cannot cancel an already cancelled order.");
        }
        Status = OrderStatus.Cancelled;
    }

    public void Complete()
    {
        if (Status != OrderStatus.Confirmed)
        {
            throw new InvalidOperationException("Cannot complete a order that is not confirmed.");
        }

        Status = OrderStatus.Completed;
    }

    public void UpdateDetails(
        string customerName,
        string customerPhoneNumber,
        string shippingAddress,
        string? customerEmail = null,
        string? customerNote = null,
        string? storeNote = null
    )
    {
        ValidateDetails(
            customerName,
            customerPhoneNumber,
            shippingAddress,
            customerEmail,
            customerNote,
            storeNote
        );

        CustomerName = customerName;
        CustomerEmail = customerEmail;
        CustomerPhoneNumber = customerPhoneNumber;
        ShippingAddress = shippingAddress;
        CustomerNote = customerNote;
        StoreNote = storeNote;
    }

    private static void ValidateDetails(
        string customerName,
        string customerPhoneNumber,
        string shippingAddress,
        string? customerEmail,
        string? customerNote,
        string? storeNote
    )
    {
        ValidateRequiredString(
            customerName,
            MaxCustomerNameLength,
            "Customer name",
            nameof(customerName)
        );
        ValidateRequiredString(
            customerPhoneNumber,
            MaxCustomerPhoneNumberLength,
            "Customer phone number",
            nameof(customerPhoneNumber)
        );
        ValidateRequiredString(
            shippingAddress,
            MaxShippingAddressLength,
            "Shipping address",
            nameof(shippingAddress)
        );
        ValidateStringLength(
            customerEmail,
            MaxCustomerEmailLength,
            "Customer email",
            nameof(customerEmail)
        );
        ValidateStringLength(
            customerNote,
            MaxCustomerNoteLength,
            "Customer note",
            nameof(customerNote)
        );
        ValidateStringLength(storeNote, MaxStoreNoteLength, "Store note", nameof(storeNote));
    }

    private static void ValidateRequiredString(
        string value,
        int maxLength,
        string label,
        string field
    )
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new OrderDomainException($"{label} is required.", field);
        }

        ValidateStringLength(value, maxLength, label, field);
    }

    private static void ValidateStringLength(
        string? value,
        int maxLength,
        string label,
        string field
    )
    {
        if (value?.Length > maxLength)
        {
            throw new OrderDomainException($"{label} cannot exceed {maxLength} characters.", field);
        }
    }
}
