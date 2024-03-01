using MegaMapper.Entities;

namespace MegaMapper.Repositories;

public class EmployeesRepository
{
    public Dictionary<int, Employee> GetByIds(int[] employeeIds)
    {
        Console.WriteLine(nameof(EmployeesRepository) + " " +
                          nameof(GetByIds) + " " +
                          string.Join(", ", employeeIds));

        return Db.Employees.Where(x => employeeIds.Contains(x.Id)).ToDictionary(x => x.Id);
    }
}