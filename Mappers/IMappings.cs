namespace MegaMapper.Mappers;

public interface IMappings
{
    Task<EmployeeDto[]> Execute(int[] employeeIds);
}