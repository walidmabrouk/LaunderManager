using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using LaunderManagerWebApi.Domain.Services.InfrastructureServices;
using LaunderWebApi.Entities;

namespace LaunderWebApi.Infrastructure.Dao
{
    public class ProprietorDao : IDaoProprietor
    {
        private readonly DbConnectionManager _connectionManager;

        public ProprietorDao(DbConnectionManager connectionManager)
        {
            _connectionManager = connectionManager;
        }

        public IEnumerable<Proprietor> GetAllProprietors()
        {
            try
            {
                using (var connection = _connectionManager.GetConnection())
                {
                    const string query = @"
                        SELECT * FROM Proprietors;
                        SELECT * FROM Laundries;
                        SELECT * FROM Machines;
                        SELECT * FROM Cycles;
                    ";

                    using (var multi = connection.QueryMultiple(query))
                    {
                        Console.WriteLine("Query executed successfully.");
                        var proprietors = multi.Read<Proprietor>().ToList();
                        var laundries = multi.Read<Laundry>().ToList();
                        var machines = multi.Read<Machine>().ToList();
                        var cycles = multi.Read<Cycle>().ToList();

                        // Relier les données
                        foreach (var proprietor in proprietors)
                        {
                            proprietor.Laundries = laundries.Where(l => l.ProprietorId == proprietor.Id).ToList();
                            foreach (var laundry in proprietor.Laundries)
                            {
                                laundry.Machines = machines.Where(m => m.LaundryId == laundry.Id).ToList();
                                foreach (var machine in laundry.Machines)
                                {
                                    machine.Cycles = cycles.Where(c => c.MachineId == machine.Id).ToList();
                                }
                            }
                        }

                        return proprietors;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error occurred during GetAllProprietors: {ex.Message}");
                throw;
            }
        }

        public Proprietor GetProprietorById(int id)
        {
            try
            {
                using (var connection = _connectionManager.GetConnection())
                {
                    const string query = @"
                        SELECT * FROM Proprietors WHERE Id = @Id;
                        SELECT * FROM Laundries WHERE ProprietorId = @Id;
                        SELECT * FROM Machines WHERE LaundryId IN (SELECT Id FROM Laundries WHERE ProprietorId = @Id);
                        SELECT * FROM Cycles WHERE MachineId IN (SELECT Id FROM Machines WHERE LaundryId IN (SELECT Id FROM Laundries WHERE ProprietorId = @Id));
                    ";

                    using (var multi = connection.QueryMultiple(query, new { Id = id }))
                    {
                        Console.WriteLine("Query executed successfully.");
                        var proprietor = multi.ReadFirstOrDefault<Proprietor>();
                        if (proprietor == null)
                        {
                            Console.WriteLine($"No proprietor found with Id = {id}");
                            return null;
                        }

                        var laundries = multi.Read<Laundry>().ToList();
                        var machines = multi.Read<Machine>().ToList();
                        var cycles = multi.Read<Cycle>().ToList();

                        // Relier les données
                        proprietor.Laundries = laundries;
                        foreach (var laundry in laundries)
                        {
                            laundry.Machines = machines.Where(m => m.LaundryId == laundry.Id).ToList();
                            foreach (var machine in laundry.Machines)
                            {
                                machine.Cycles = cycles.Where(c => c.MachineId == machine.Id).ToList();
                            }
                        }

                        return proprietor;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error occurred during GetProprietorById: {ex.Message}");
                throw;
            }
        }

        public async Task AddProprietor(Proprietor proprietor)
        {
            try
            {
                using (var connection = _connectionManager.GetConnection())
                using (var transaction = connection.BeginTransaction())
                {
                    // Ajout du propriétaire
                    const string proprietorQuery = @"
                        INSERT INTO Proprietors (Name, Email, TotalEarnings)
                        VALUES (@Name, @Email, @TotalEarnings);
                        SELECT CAST(SCOPE_IDENTITY() as int);
                    ";

                    Console.WriteLine($"Executing SQL: {proprietorQuery} with data: {proprietor.Name}, {proprietor.Email}, {proprietor.TotalEarnings}");
                    var proprietorId = connection.QuerySingle<int>(proprietorQuery, proprietor, transaction);
                    Console.WriteLine($"Proprietor added with Id: {proprietorId}");

                    foreach (var laundry in proprietor.Laundries)
                    {
                        laundry.ProprietorId = proprietorId;

                        const string laundryQuery = @"
                            INSERT INTO Laundries (Name, Address, Earnings, ProprietorId)
                            VALUES (@Name, @Address, @Earnings, @ProprietorId);
                            SELECT CAST(SCOPE_IDENTITY() as int);
                        ";

                        Console.WriteLine($"Executing SQL: {laundryQuery} with data: {laundry.Name}, {laundry.Address}, {laundry.Earnings}");
                        var laundryId = connection.QuerySingle<int>(laundryQuery, laundry, transaction);
                        Console.WriteLine($"Laundry added with Id: {laundryId}");

                        foreach (var machine in laundry.Machines)
                        {
                            machine.LaundryId = laundryId;

                            const string machineQuery = @"
                                INSERT INTO Machines (SerialNumber, Type, State, Earnings, LaundryId)
                                VALUES (@SerialNumber, @Type, @State, @Earnings, @LaundryId);
                                SELECT CAST(SCOPE_IDENTITY() as int);
                            ";

                            Console.WriteLine($"Executing SQL: {machineQuery} with data: {machine.SerialNumber}, {machine.Type}, {machine.State}, {machine.Earnings}");
                            var machineId = connection.QuerySingle<int>(machineQuery, machine, transaction);
                            Console.WriteLine($"Machine added with Id: {machineId}");

                            foreach (var cycle in machine.Cycles)
                            {
                                cycle.MachineId = machineId;

                                const string cycleQuery = @"
                                    INSERT INTO Cycles (Name, Price, Duration, MachineId)
                                    VALUES (@Name, @Price, @Duration, @MachineId);
                                ";

                                Console.WriteLine($"Executing SQL: {cycleQuery} with data: {cycle.Name}, {cycle.Price}, {cycle.Duration}");
                                connection.Execute(cycleQuery, cycle, transaction);
                            }
                        }
                    }

                    transaction.Commit();
                    Console.WriteLine("Transaction committed successfully.");
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"An error occurred during AddProprietor: {ex.Message}");
                Console.Error.WriteLine($"Failed SQL execution with proprietor data: {proprietor.Name}, {proprietor.Email}");
                throw;
            }
        }
    }
}
