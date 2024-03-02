using MegaMapper.Repositories;

namespace MegaMapper.Mappers;

public class InnocentMapperV2 : IMappings
{
    private EmployeesRepository _employeesRepository;
    private GroupsRepository _groupsRepository;
    private AbsenceRepository _absenceRepository;

    public InnocentMapperV2()
    {
        _employeesRepository = new EmployeesRepository();
        _groupsRepository = new GroupsRepository();
        _absenceRepository = new AbsenceRepository();
    }

    public async Task<EmployeeDto[]> Execute(int[] employeeIds)
    {
        var employees = (await _employeesRepository.GetByIds(employeeIds)).Values;
        var employeeDtos = new List<EmployeeDto>();

        foreach (var employee in employees)
            employeeDtos.Add(new EmployeeDto
            {
                Id = employee.Id,
                FullName = employee.FullName,
                NearAbsence = await GetNearAbsenceDto(employee.Id),
                Groups = await GetGroupDtos(employee.Id)
            });

        return employeeDtos.ToArray();
    }

    private async Task<NearAbsenceDto?> GetNearAbsenceDto(int employeeId)
    {
        var nearAbsence = (await _absenceRepository.GetNearAbsencesByEmployeeIds(new[] { employeeId }))
            .Values
            .SingleOrDefault();

        if (nearAbsence == null)
            return null;

        return new NearAbsenceDto
        {
            From = nearAbsence.From,
            To = nearAbsence.To,
            Id = nearAbsence.Id
        };
    }

    private async Task<GroupDto[]> GetGroupDtos(int employeeId)
    {
        var groups = (await _groupsRepository.GetByEmployeeIds(new[] { employeeId })).Values.Single();
        var groupDtos = new List<GroupDto>();
        foreach (var group in groups)
        {
            var groupDto = new GroupDto
            {
                Id = group.Id,
                Name = group.Name,
                Tags = await GetTags(group.Id),
                Members = await GetGroupMembers(group.Id)
            };

            groupDtos.Add(groupDto);
        }

        return groupDtos.ToArray();
    }

    private async Task<GroupMemberDto[]> GetGroupMembers(int groupId)
    {
        var members = (await _groupsRepository.GetMembersByGroupIds(new[] { groupId }, 5)).Values.Single();
        var memberDtos = new List<GroupMemberDto>();
        foreach (var groupMember in members)
        {
            var memberDto = new GroupMemberDto
            {
                Id = groupMember.EmployeeId,
                Role = groupMember.Role,
                FullName = await GetEmployeeFullName(groupMember.EmployeeId),
                NearAbsence = await GetNearAbsenceDto(groupMember.EmployeeId),
            };

            memberDtos.Add(memberDto);
        }

        return memberDtos.ToArray();
    }

    private async Task<string> GetEmployeeFullName(int employeeId)
    {
        var employee = (await _employeesRepository.GetByIds(new[] { employeeId })).Values.Single();

        return employee.FullName;
    }

    private async Task<string[]> GetTags(int groupId)
    {
        var tags = (await _groupsRepository.GetTagsByGroupIds(new[] { groupId })).Values.Single();
        return tags.Select(x => x.Tag).ToArray();
    }
}