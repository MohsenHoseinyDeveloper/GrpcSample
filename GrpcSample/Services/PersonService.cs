using Grpc.Core;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Google.Protobuf.WellKnownTypes;
using GrpcSample.Database;


namespace GrpcSample.Services;

public class PersonServiceImpl : PersonService.PersonServiceBase
{
    private readonly ILogger<PersonServiceImpl> _logger;
    private readonly AppDbContext _context;

    public PersonServiceImpl(ILogger<PersonServiceImpl> logger, AppDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public override async Task<PersonList> GetAll(Empty request, ServerCallContext context)
    {
        var persons = await _context.Persons.ToListAsync();
        if (persons.Any())
            this._logger.LogInformation($"No person saved in database");

        var response = new PersonList();
        response.Persons.AddRange(persons.Select(p => new Person
        {
            Id = p.Id,
            Name = p.Name,
            Family = p.Family,
            BirthDate = p.BirthDate.ToString("yyyy-MM-dd"),
            NationalCode = p.NationalCode
        }));
        return response;
    }

    public override async Task<Person> GetById(PersonRequest request, ServerCallContext context)
    {
        var person = await _context.Persons.FindAsync(request.Id);
        if (person == null)
        {
            this._logger.LogInformation($"Person not exists");
            return null;
        }

        return new Person
        {
            Id = person.Id,
            Name = person.Name,
            Family = person.Family,
            BirthDate = person.BirthDate.ToString("yyyy-MM-dd"),
            NationalCode = person.NationalCode
        };
    }

    public override async Task<Person> Create(Person request, ServerCallContext context)
    {
        var person = new PersonEntity
        {
            Name = request.Name,
            Family = request.Family,
            BirthDate = DateTime.Parse(request.BirthDate),
            NationalCode = request.NationalCode
        };
        _context.Persons.Add(person);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            this._logger.LogCritical($"cannot save Person because: {ex.Message}");
        }

        return new Person
        {
            Id = person.Id,
            Name = person.Name,
            Family = person.Family,
            BirthDate = person.BirthDate.ToString("yyyy-MM-dd"),
            NationalCode = person.NationalCode
        };
    }

    public override async Task<Person> Update(Person request, ServerCallContext context)
    {
        var person = await _context.Persons.FindAsync(request.Id);
        if (person == null)
        {
            this._logger.LogInformation($"Person not exists");
            return null;
        }

        person.Name = request.Name;
        person.Family = request.Family;
        person.BirthDate = DateTime.Parse(request.BirthDate);
        person.NationalCode = request.NationalCode;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            this._logger.LogInformation($"Cannot update person because: {ex.Message}");
        }

        return new Person
        {
            Id = person.Id,
            Name = person.Name,
            Family = person.Family,
            BirthDate = person.BirthDate.ToString("yyyy-MM-dd"),
            NationalCode = person.NationalCode
        };
    }

    public override async Task<Empty> Delete(PersonRequest request, ServerCallContext context)
    {
        var person = await _context.Persons.FindAsync(request.Id);
        if (person == null)
        {
            this._logger.LogInformation($"Person not exists");
            return new Empty();
        }

        _context.Persons.Remove(person);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            this._logger.LogCritical($"cannot delete Person because: {ex.Message}");
        }

        return new Empty();
    }
}
