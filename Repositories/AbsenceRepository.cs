using MegaMapper.Entities;

namespace MegaMapper.Repositories;

public class AbsenceRepository
{
    public Task<Dictionary<int, Absence>> GetNearAbsencesByEmployeeIds(int[] employeeIds)
    {
        Console.WriteLine(nameof(AbsenceRepository) + " " +
                          nameof(GetNearAbsencesByEmployeeIds) + " " +
                          string.Join(", ", employeeIds));

        var result = Db.Absences.Where(x => employeeIds.Contains(x.EmployeeId)).ToDictionary(x => x.EmployeeId);
        return Task.FromResult(result);
    }
}