namespace Budgexa.API.Endpoints;

using Budgexa.API.Middleware;
using Budgexa.Application.Companies.DTOs;
using Budgexa.Application.Companies.Commands.CreateCompany;
using Budgexa.Application.Companies.Commands.UpdateCompany;
using Budgexa.Application.Companies.Commands.DeleteCompany;
using Budgexa.Application.Companies.Queries.GetCompany;
using Budgexa.Application.Companies.Queries.GetCompanies;
using MediatR;

public static class CompaniesEndpoints
{
    public static IEndpointRouteBuilder MapCompanyEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/companies").WithTags("Companies");

        group.MapGet("/",
            async (
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var query = new GetCompaniesQuery();
                var result = await sender.Send(query, cancellationToken);
                return Results.Ok(result);
            })
            .RequireAuthorization()
            .Produces<List<CompanyDto>>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
            .WithName("GetAllCompanies")
            .WithSummary("GET /api/v1/companies")
            .WithDescription("Retrieves all companies.");

        group.MapGet("/{id:guid}",
            async (
                ISender sender,
                Guid id,
                CancellationToken cancellationToken) =>
            {
                var query = new GetCompanyQuery(id);
                var result = await sender.Send(query, cancellationToken);
                return Results.Ok(result);
            })
            .RequireAuthorization()
            .Produces<CompanyDto>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
            .WithName("GetCompanyById")
            .WithSummary("GET /api/v1/companies/{id}")
            .WithDescription("Retrieves a company by its ID.");

        group.MapPost("/",
            async (
                ISender sender,
                CompanyCreateDto dto,
                CancellationToken cancellationToken) =>
            {
                var command = new CreateCompanyCommand(dto);
                var result = await sender.Send(command, cancellationToken);
                return Results.Created($"/api/v1/companies/{result.Id}", result);
            })
            .RequireAuthorization()
            .Produces<CompanyDto>(StatusCodes.Status201Created)
            .Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
            .WithName("CreateCompany")
            .WithSummary("POST /api/v1/companies")
            .WithDescription("Creates a new company.");

        group.MapPut("/{id:guid}",
            async (
                ISender sender,
                Guid id,
                CompanyUpdateDto dto,
                CancellationToken cancellationToken) =>
            {
                var command = new UpdateCompanyCommand(id, dto);
                var result = await sender.Send(command, cancellationToken);
                return Results.Ok(result);
            })
            .RequireAuthorization()
            .Produces<CompanyDto>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
            .WithName("UpdateCompany")
            .WithSummary("PUT /api/v1/companies/{id}")
            .WithDescription("Updates an existing company.");

        group.MapDelete("/{id:guid}",
            async (
                ISender sender,
                Guid id,
                CancellationToken cancellationToken) =>
            {
                var command = new DeleteCompanyCommand(id);
                await sender.Send(command, cancellationToken);
                return Results.NoContent();
            })
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
            .WithName("DeleteCompany")
            .WithSummary("DELETE /api/v1/companies/{id}")
            .WithDescription("Deletes a company by its ID.");

        return endpoints;
    }
}
