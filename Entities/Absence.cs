namespace MegaMapper.Entities;

public class Absence
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public DateOnly From { get; set; }
    public DateOnly To { get; set; }
}