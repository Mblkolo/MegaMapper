using MegaMapper.Entities;
using MegaMapper.Repositories;

namespace MegaMapper.Mappers;

public class InnocentMapper : IMappings
{
    private EmployeesRepository _employeesRepository;
    private GroupsRepository _groupsRepository;
    private AbsenceRepository _absenceRepository;

    public InnocentMapper()
    {
        _employeesRepository = new EmployeesRepository();
        _groupsRepository = new GroupsRepository();
        _absenceRepository = new AbsenceRepository();
    }

    public async Task<EmployeeDto[]> Execute(int[] employeeIds)
    {
        var employeeDtos = new List<EmployeeDto>();

        var employees = await _employeesRepository.GetByIds(employeeIds);
        foreach (var employee in employees.Values)
            employeeDtos.Add(await ToEmployeeDtoAsync(employee));

        return employeeDtos.ToArray();
    }

    private async Task<EmployeeDto> ToEmployeeDtoAsync(Employee employee)
    {
        var nearAbsence = (await _absenceRepository.GetNearAbsencesByEmployeeIds(new[] { employee.Id }))
            .Values
            .SingleOrDefault();
        var nearAbsenceDto = nearAbsence == null ? null : ToAbsenceDto(nearAbsence);

        var groups = (await _groupsRepository.GetByEmployeeIds(new[] { employee.Id })).Values.Single();
        var groupDtos = new List<GroupDto>();
        foreach (var group in groups)
            groupDtos.Add(await ToGroupDto(group));

        return ToEmployeeDto(employee, nearAbsenceDto, groupDtos.ToArray());
    }

    private static EmployeeDto ToEmployeeDto(Employee employee, NearAbsenceDto? nearAbsence, GroupDto[] groupDtos)
    {
        return new EmployeeDto
        {
            Id = employee.Id,
            FullName = employee.FullName,
            NearAbsence = nearAbsence,
            Groups = groupDtos
        };
    }

    private async Task<GroupDto> ToGroupDto(Group group)
    {
        var tags = (await _groupsRepository.GetTagsByGroupIds(new[] { group.Id })).Values.Single();

        var members = (await _groupsRepository.GetMembersByGroupIds(new[] { group.Id }, 5)).Values.Single();
        var memberDtos = new List<GroupMemberDto>();
        foreach (var member in members)
            memberDtos.Add(await ToGroupMemberDto(member));

        return new GroupDto
        {
            Id = group.Id,
            Name = group.Name,
            Tags = tags.Select(x => x.Tag).ToArray(),
            Members = memberDtos.ToArray()
        };
    }

    private async Task<GroupMemberDto> ToGroupMemberDto(GroupMember groupMember)
    {
        var nearAbsence = (await _absenceRepository.GetNearAbsencesByEmployeeIds(new[] { groupMember.EmployeeId}))
            .Values
            .SingleOrDefault();
        var nearAbsenceDto = nearAbsence == null ? null : ToAbsenceDto(nearAbsence);

        var employee = (await _employeesRepository.GetByIds(new []{ groupMember.EmployeeId })).Values.Single();

        return new GroupMemberDto
        {
            Id = groupMember.EmployeeId,
            Role = groupMember.Role,
            FullName = employee.FullName,
            NearAbsence = nearAbsenceDto,
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