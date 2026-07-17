namespace Budgexa.Application.Customers.Commands.DeleteCustomer;

using System.Net;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Domain.Constants;
using Budgexa.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

public sealed class DeleteCustomerCommandHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUserService
) : IRequestHandler<DeleteCustomerCommand>
{
    public async Task Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
    {
        var companyId = currentUserService.CompanyId;
        var currentUserId = currentUserService.UserId;

        var customer = await db.Customers
            .FirstOrDefaultAsync(c =>
                c.Id == request.Id
                && c.CompanyId == companyId
                && c.StatusId != StatusIds.Delete,
                cancellationToken)
            ?? throw new AppException(HttpStatusCode.NotFound, ErrorTags.Customer.NotFound, "Customer not found.");

        customer.MarkAsDeleted(StatusIds.Delete, currentUserId);
        await db.SaveChangesAsync(cancellationToken);
    }
}
