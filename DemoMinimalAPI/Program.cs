using DemoMinimalAPI.Data;
using DemoMinimalAPI.Models;
using EntityGraphQL.AspNet;
using Microsoft.EntityFrameworkCore;
using MiniValidation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<MinimalContextDb>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

//graphql
/*builder.Services
    .AddGraphQLServer()    
    .AddQueryType<Query>();*/

builder.Services
    .AddGraphQLSchema<MinimalContextDb>()
    .AddGraphQLServer()
    .AddQueryType();


var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//graphql
app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.MapGraphQL<MinimalContextDb>();
});




app.MapGet("/fornecedor", async (MinimalContextDb context) =>
{
    return await context.Fornecedores.ToListAsync();
})
    .WithName("GetFornecedor")
    .WithTags("Fornecedor");

app.MapGet("/fornecedor{id}", async (Guid id, MinimalContextDb context) =>
{
    return  await context.Fornecedores.FindAsync(id) is Fornecedor fornecedor ? Results.Ok(fornecedor) : Results.NotFound();    
})
    .Produces<Fornecedor>(StatusCodes.Status200OK)
    .Produces<Fornecedor>(StatusCodes.Status404NotFound)
    .WithName("GetFornecedorPorId")
    .WithTags("Fornecedor");

app.MapPost("/fornecedor", async (MinimalContextDb context, Fornecedor fornecedor) =>
{
    if (!MiniValidator.TryValidate(fornecedor, out var errors)) return Results.ValidationProblem(errors); //validar modelstate
    
    context.Fornecedores.Add(fornecedor);
    var result = await context.SaveChangesAsync();

    return result > 0 
    ? Results.Created($"/fornecedor{fornecedor.Id}", fornecedor)
    : Results.BadRequest("Houve um problema ao salvar o registro");
})
    .ProducesValidationProblem()
    .Produces<Fornecedor>(StatusCodes.Status201Created)
    .Produces<Fornecedor>(StatusCodes.Status400BadRequest)
    .WithName("PostFornecedor")
    .WithTags("Fornecedor");

app.MapPut("/fornecedor{id}", async(Guid id, MinimalContextDb context, Fornecedor fornecedor) =>
{
    var fornecedorBanco = await context.Fornecedores.AsNoTracking<Fornecedor>().FirstOrDefaultAsync(f => f.Id == id);
    if (fornecedorBanco == null) return Results.NotFound();

    if (!MiniValidator.TryValidate(fornecedor, out var errors)) return Results.ValidationProblem(errors); //validar modelstate
    
    context.Fornecedores.Update(fornecedor);
    var result = await context.SaveChangesAsync();

    return result > 0
        ? Results.NoContent()
        : Results.BadRequest("Houve um problema ao salvar o registro");

})
    .ProducesValidationProblem()
    .Produces<Fornecedor>(StatusCodes.Status204NoContent)
    .Produces<Fornecedor>(StatusCodes.Status400BadRequest)
    .WithName("PutFornecedor")
    .WithTags("Fornecedor");

app.MapDelete("/fornecedor{id}", async (Guid id,MinimalContextDb context) =>
{
    var fornecedor = await context.Fornecedores.FindAsync(id);
    if (fornecedor == null) return Results.NotFound();

    context.Fornecedores.Remove(fornecedor);
    var result = await context.SaveChangesAsync();

    return result > 0
        ? Results.NoContent()
        : Results.BadRequest("Houve um problema ao salvar o registro");
})
    .ProducesValidationProblem()
    .Produces<Fornecedor>(StatusCodes.Status204NoContent)
    .Produces<Fornecedor>(StatusCodes.Status400BadRequest)
    .Produces<Fornecedor>(StatusCodes.Status404NotFound)
    .WithName("DeleteFornecedor")
    .WithTags("Fornecedor");

app.Run();

