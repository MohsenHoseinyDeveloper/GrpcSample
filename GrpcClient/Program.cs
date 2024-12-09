// See https://aka.ms/new-console-template for more information
using Grpc.Net.Client;
using GrpcSample;
using System.Threading.Tasks;
using System;
using Polly;
using Polly.CircuitBreaker;

//Console.WriteLine("Hello, World!");
// Add Polly policies var retryPolicy = Policy.Handle<Exception>().RetryAsync(3);

class Program
{
    static async Task Main(string[] args)
    {
        var retryPolicy = Policy
            .Handle<Exception>()
            .RetryAsync(3, onRetry: (exception, retryCount, context) =>
            {
                Console.WriteLine($"Retry {retryCount} for {context.PolicyKey} at {context.OperationKey}, due to: {exception}.");
            });

        var channel = GrpcChannel.ForAddress("https://localhost:7163");
        var client = new PersonService.PersonServiceClient(channel);

        bool isAdd = true;
        while (isAdd)
        {
            // Get user input for a new person
            Console.WriteLine("Enter the details of the new person:");

            Console.Write("Name: ");
            string name = Console.ReadLine();

            Console.Write("Family: ");
            string family = Console.ReadLine();

            Console.Write("NationalCode: ");
            string nationalCode = Console.ReadLine();

            Console.Write("BirthDate (yyyy-MM-dd): ");
            string birthDate = Console.ReadLine();

            // Create a new person
            var newPerson = new Person
            {
                Name = name,
                Family = family,
                BirthDate = birthDate,
                NationalCode = nationalCode
            };

            try
            {
                await retryPolicy.ExecuteAsync(async () =>
                {
                    var createdPerson = await client.CreateAsync(newPerson);
                    Console.WriteLine($"Created: {createdPerson.Id} - {createdPerson.Name} {createdPerson.Family} - {createdPerson.BirthDate} - {createdPerson.NationalCode} Person");
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"faild due to: {ex.Message}");
            }

            Console.WriteLine("");

            // Example: Get all persons
            Console.WriteLine($"List of persons:");

            PersonList response = new();
            try
            {
                await retryPolicy.ExecuteAsync(async () =>
                {
                    response = await client.GetAllAsync(new Empty());
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"faild due to: {ex.Message}");
            }

            foreach (var person in response.Persons)
            {
                Console.WriteLine($"Person: {person.Id} - {person.Name} {person.Family} - {person.BirthDate} - {person.NationalCode}");
            }

            Console.Write("Do you want to add another person (1: yes, 0: No): ");
            if (Console.ReadLine() == "0")
                isAdd = false;
        }

        Console.Write("Press any key to exit: ");
        Console.ReadKey();
    }
}
