using MegaMapper.Entities;
using MegaMapper.Repositories;

namespace MegaMapper.Mappers;

public class ClassicMappings : IMappings
{
    private EmployeesRepository _employeesRepository;
    private GroupsRepository _groupsRepository;
    private AbsenceRepository _absenceRepository;

    public ClassicMappings()
    {
        _employeesRepository = new EmployeesRepository();
        _groupsRepository = new GroupsRepository();
        _absenceRepository = new AbsenceRepository();
    }

    public async Task<EmployeeDto[]> Execute(int[] employeeIds)
    {
        var employees = await _employeesRepository.GetByIds(employeeIds);
        var employeeGroups = await _groupsRepository.GetByEmployeeIds(employeeIds);

        var groupsIds = employeeGroups.SelectMany(x => x.Value).Select(x => x.Id).Distinct().ToArray();
        var groupMembers = await _groupsRepository.GetMembersByGroupIds(
            groupsIds,
            max: 5);

        var groupEmployeeIds = groupMembers.SelectMany(x => x.Value).Select(x => x.EmployeeId).Distinct().ToArray();
        var groupEmployees = await _employeesRepository.GetByIds(groupEmployeeIds);

        var groupTags = await _groupsRepository.GetTagsByGroupIds(groupsIds);

        var allEmployeesIds = employees.Keys.Concat(groupEmployees.Keys).Distinct().ToArray();
        var absencesByEmployeeIds = await _absenceRepository.GetNearAbsencesByEmployeeIds(allEmployeesIds);

        return employees.Select(x => ToEmployeeDto(
            x.Value,
            absencesByEmployeeIds,
            employeeGroups,
            groupTags,
            groupMembers,
            groupEmployees)).ToArray();
    }

    private static EmployeeDto ToEmployeeDto(
        Employee employee,
        Dictionary<int, Absence> absencesByEmployeeIds,
        Dictionary<int, Group[]> employeeGroups,
        Dictionary<int, GroupTag[]> groupTagsMap,
        Dictionary<int, GroupMember[]> groupMembersMap,
        Dictionary<int, Employee> groupEmployees)
    {
        absencesByEmployeeIds.TryGetValue(employee.Id, out var absence);
        return new EmployeeDto
        {
            Id = employee.Id,
            FullName = employee.FullName,
            NearAbsence = absence == null ? null : ToAbsenceDto(absence),
            Groups = employeeGroups[employee.Id].Select(x =>
                ToGroupDto(x, groupTagsMap, groupMembersMap, groupEmployees, absencesByEmployeeIds)).ToArray()
        };
    }

    private static GroupDto ToGroupDto(Group group,
        Dictionary<int, GroupTag[]> groupTagsMap,
        Dictionary<int, GroupMember[]> groupMembersMap,
        Dictionary<int, Employee> groupEmployees,
        Dictionary<int, Absence> absencesByEmployeeIds)
    {
        return new GroupDto
        {
            Id = group.Id,
            Name = group.Name,
            Tags = groupTagsMap[group.Id].Select(x => x.Tag).ToArray(),
            Members = groupMembersMap[group.Id].Select(x => ToGroupMember(x, groupEmployees, absencesByEmployeeIds))
                .ToArray()
        };
    }

    private static GroupMemberDto ToGroupMember(GroupMember groupMember,
        Dictionary<int, Employee> employeeMap,
        Dictionary<int, Absence> absencesByEmployeeIds)
    {
        absencesByEmployeeIds.TryGetValue(groupMember.EmployeeId, out var absence);

        return new GroupMemberDto
        {
            Id = groupMember.EmployeeId,
            Role = groupMember.Role,
            FullName = employeeMap[groupMember.EmployeeId].FullName,
            NearAbsence = absence == null ? null : ToAbsenceDto(absence),
        };
    }

    private static NearAbsenceDto ToAbsenceDto(Absence absence)
    {
        return new NearAbsenceDto
        {
            From = absence.From,
            To = absence.To,
            Id = absence.Id
        };
    }
}