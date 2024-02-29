namespace MegaMapper;

public class EmployeeDto
{
    public string Id { get; set; }
    public string FullName { get; set; }
    public GroupDto[] Groups { get; set; }
}