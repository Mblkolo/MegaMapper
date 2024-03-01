using MegaMapper.Entities;

namespace MegaMapper.Repositories;

public class AbsenceRepository
{
    public Dictionary<int, Absence> GetNearAbsencesByEmployeeIds(int[] employeeIds)
    {
        Console.WriteLine(nameof(AbsenceRepository) + " " +
                          nameof(GetNearAbsencesByEmployeeIds) + " " +
                          string.Join(", ", employeeIds));

        return Db.Absences.Where(x => employeeIds.Contains(x.EmployeeId)).ToDictionary(x => x.EmployeeId);
    }
}