using MegaMapper.Entities;

namespace MegaMapper.Repositories;

public class EmployeesRepository
{
    public Task<Dictionary<int, Employee>> GetByIds(int[] employeeIds)
    {
        Console.WriteLine(nameof(EmployeesRepository) + " " +
                          nameof(GetByIds) + " " +
                          string.Join(", ", employeeIds));

        var result = Db.Employees.Where(x => employeeIds.Contains(x.Id)).ToDictionary(x => x.Id);
        return Task.FromResult(result);
    }
}