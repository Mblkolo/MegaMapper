using MegaMapper.Entities;

namespace MegaMapper.Repositories;

public static class Db
{
    private static int _groupCount = 7;
    private static int _employeeCount = 30;

    public static Employee[] Employees = Enumerable.Range(1, _employeeCount)
        .Select(x => new Employee
        {
            Id = x,
            FullName = $"Сотрудник №{x}"
        })
        .ToArray();

    public static Group[] Groups = Enumerable.Range(1, _groupCount)
        .Select(x => new Group
        {
            Id = x,
            Name = $"Группа №{x}"
        })
        .ToArray();

    public static GroupMember[] GroupMembers = Enumerable.Range(1, _employeeCount)
        .Select(x => new GroupMember
        {
            Role = $"Тип {x % 3}",
            GroupId = x % _groupCount,
            EmployeeId = x
        })
        .ToArray();

    public static GroupTag[] GroupTags = Enumerable.Range(1, 20)
        .Select(x => new GroupTag
        {
            GroupId = x % _groupCount,
            Tag = $"Тег {x}",
        })
        .ToArray();

    public static Absence[] Absences= Enumerable
        .Range(1, _employeeCount)
        .Where(x => x % 3 == 0)
        .Select(x => new Absence
        {
            EmployeeId = x,
            Id = 100 + x,
            From = new DateOnly(2024, 5, 1).AddDays(x),
            To = new DateOnly(2024, 5, 1).AddDays(x + 10)
        })
        .ToArray();


}