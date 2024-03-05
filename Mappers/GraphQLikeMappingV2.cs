using MegaMapper.Entities;
using MegaMapper.Repositories;

namespace MegaMapper.Mappers;

public class GraphQLikeMappingV2 : IMappings
{
    private readonly EmployeeDtoByIdsLoaderV2 _employeeDtoByIdsLoader;

    public GraphQLikeMappingV2()
    {
        _employeeDtoByIdsLoader = new EmployeeDtoByIdsLoaderV2();
    }

    public async Task<EmployeeDto[]> Execute(int[] employeeIds)
    {
        var employeeDtos = await _employeeDtoByIdsLoader.LoadAsync(employeeIds);
        return employeeDtos.Values.ToArray();
    }
}

public class EmployeeDtoByIdsLoaderV2 : ILoader<EmployeeDto>
{
    private readonly EmployeesRepository _employeesRepository;
    private readonly NearAbsenceDtoByEmployeeIdLoaderV2 _nearAbsenceDtoByEmployeeIdLoader;
    private readonly GroupDtoByEmployeeIdLoaderV2 _groupDtoByEmployeeIdLoader;

    public EmployeeDtoByIdsLoaderV2()
    {
        _employeesRepository = new EmployeesRepository();
        _nearAbsenceDtoByEmployeeIdLoader = new NearAbsenceDtoByEmployeeIdLoaderV2();
        _groupDtoByEmployeeIdLoader = new GroupDtoByEmployeeIdLoaderV2();
    }

    public async Task<Dictionary<int, EmployeeDto>> LoadAsync(int[] ids)
    {
        var employees = await _employeesRepository.GetByIds(ids);

        var employeeIds = employees.Keys.ToArray();

        var absences = await _nearAbsenceDtoByEmployeeIdLoader.LoadAsync(employeeIds);
        var groups = await _groupDtoByEmployeeIdLoader.LoadAsync(employeeIds);

        return ids
            .ToDictionary(
                i => i,
                i => ToDto(employees[i],
                    groups[i],
                    absences.GetValueOrDefault(employees[i].Id)));
    }


    private static EmployeeDto ToDto(Employee employee, GroupDto[] groups, NearAbsenceDto? nearAbsence)
    {
        return new EmployeeDto
        {
            Id = employee.Id,
            FullName = employee.FullName,
            Groups = groups,
            NearAbsence = nearAbsence
        };
    }
}

public class GroupDtoByEmployeeIdLoaderV2 : ILoader<GroupDto[]>
{
    private readonly GroupsRepository _groupsRepository;
    private readonly GroupMemberDtoByGroupLoaderV2 _groupMemberDtoByGroupLoader;

    public GroupDtoByEmployeeIdLoaderV2()
    {
        _groupsRepository = new GroupsRepository();
        _groupMemberDtoByGroupLoader = new GroupMemberDtoByGroupLoaderV2();
    }

    public async Task<Dictionary<int, GroupDto[]>> LoadAsync(int[] ids)
    {
        var groups = await _groupsRepository.GetByEmployeeIds(ids);

        var groupIds = groups.SelectMany(x => x.Value).Select(x => x.Id).Distinct().ToArray();
        var groupTags = await _groupsRepository.GetTagsByGroupIds(groupIds);
        var members = await _groupMemberDtoByGroupLoader.LoadAsync(groupIds);

        return ids
            .ToDictionary(x => x,
                x => groups[x].Select(i => ToDto(
                        i,
                        groupTags[i.Id].Select(z => z.Tag).ToArray(),
                        members[i.Id]))
                    .ToArray()
            );
    }

    private static GroupDto ToDto(Group group, string[] tags, GroupMemberDto[] members)
    {
        return new GroupDto
        {
            Id = group.Id,
            Name = group.Name,
            Tags = tags,
            Members = members
        };
    }
}

public class GroupMemberDtoByGroupLoaderV2 : ILoader<GroupMemberDto[]>
{
    private readonly NearAbsenceDtoByEmployeeIdLoaderV2 _nearAbsenceDtoByEmployeeIdLoader;
    private readonly EmployeesRepository _employeesRepository;
    private readonly GroupsRepository _groupsRepository;

    public GroupMemberDtoByGroupLoaderV2()
    {
        _nearAbsenceDtoByEmployeeIdLoader = new NearAbsenceDtoByEmployeeIdLoaderV2();
        _employeesRepository = new EmployeesRepository();
        _groupsRepository = new GroupsRepository();
    }

    public async Task<Dictionary<int, GroupMemberDto[]>> LoadAsync(int[] ids)
    {
        var groupMembers = await _groupsRepository.GetMembersByGroupIds(ids, max: 5);

        var employeeIds = groupMembers.SelectMany(x => x.Value).Select(i => i.EmployeeId).Distinct().ToArray();
        var employees = await _employeesRepository.GetByIds(employeeIds);

        var absences = await _nearAbsenceDtoByEmployeeIdLoader.LoadAsync(employeeIds);

        return ids
            .ToDictionary(gr => gr,
                i => groupMembers[i]
                    .Select(x => ToDto(x, employees[x.EmployeeId], absences.GetValueOrDefault(x.EmployeeId)))
                    .ToArray()
            );
    }

    private static GroupMemberDto ToDto(GroupMember member, Employee employee, NearAbsenceDto? nearAbsence)
    {
        return new GroupMemberDto
        {
            Id = member.EmployeeId,
            Role = member.Role,
            FullName = employee.FullName,
            NearAbsence = nearAbsence
        };
    }
}

public class NearAbsenceDtoByEmployeeIdLoaderV2 : ILoader<NearAbsenceDto>
{
    private readonly AbsenceRepository _absenceRepository;

    public NearAbsenceDtoByEmployeeIdLoaderV2()
    {
        _absenceRepository = new AbsenceRepository();
    }

    public async Task<Dictionary<int, NearAbsenceDto>> LoadAsync(int[] ids)
    {
        var absencesByEmployeeIds = await _absenceRepository.GetNearAbsencesByEmployeeIds(ids);
        return absencesByEmployeeIds.ToDictionary(i => i.Key, i => ToDto(i.Value));
    }

    private static NearAbsenceDto ToDto(Absence absence)
    {
        return new NearAbsenceDto
        {
            From = absence.From,
            To = absence.To,
            Id = absence.Id
        };
    }
}