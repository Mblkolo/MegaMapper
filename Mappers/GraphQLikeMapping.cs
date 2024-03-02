using MegaMapper.Entities;
using MegaMapper.Repositories;

namespace MegaMapper.Mappers;

public class GraphQLikeMapping : IMappings
{
    private readonly EmployeeDtoByIdsLoader _employeeDtoByIdsLoader;

    public GraphQLikeMapping()
    {
        _employeeDtoByIdsLoader = new EmployeeDtoByIdsLoader();
    }

    public async Task<EmployeeDto[]> Execute(int[] employeeIds)
    {
        var employeeDtos = await _employeeDtoByIdsLoader.LoadAsync(employeeIds);
        return employeeDtos.Values.ToArray();
    }
}

public interface ILoader<T>
{
    Task<Dictionary<int, T>> LoadAsync(int[] ids);
}

public class EmployeeDtoByIdsLoader : ILoader<EmployeeDto>
{
    private readonly EmployeesRepository _employeesRepository;
    private readonly NearAbsenceDtoByEmployeeIdLoader _nearAbsenceDtoByEmployeeIdLoader;
    private readonly GroupDtoByEmployeeIdLoader _groupDtoByEmployeeIdLoader;

    public EmployeeDtoByIdsLoader()
    {
        _employeesRepository = new EmployeesRepository();
        _nearAbsenceDtoByEmployeeIdLoader = new NearAbsenceDtoByEmployeeIdLoader();
        _groupDtoByEmployeeIdLoader = new GroupDtoByEmployeeIdLoader();
    }

    public async Task<Dictionary<int, EmployeeDto>> LoadAsync(int[] ids)
    {
        var employees = await _employeesRepository.GetByIds(ids);
        // Штуку ниже можно в extension
        var allEmployees = employees
            .Select(kvp => new BindingInfo<Employee, EmployeeDto>(kvp.Key, kvp.Value, ToBaseDto(kvp.Value)))
            .ToArray();

        await GraphQLikeBinder.BindAsync(
            allEmployees,
            _nearAbsenceDtoByEmployeeIdLoader,
            i => i.Entity.Id,
            (dto, value) => dto.NearAbsence = value);

        await GraphQLikeBinder.BindAsync(
            allEmployees,
            _groupDtoByEmployeeIdLoader,
            i => i.Entity.Id,
            (dto, value) => dto.Groups = value);

        // Штуку ниже можно в extension
        return allEmployees
            .ToDictionary(bi => bi.Id, i => i.Dto);
    }

    private EmployeeDto ToBaseDto(Employee employee)
    {
        return new EmployeeDto
        {
            Id = employee.Id,
            FullName = employee.FullName
        };
    }
}

public class GroupDtoByEmployeeIdLoader : ILoader<GroupDto[]>
{
    private readonly GroupsRepository _groupsRepository;
    private readonly GroupTagByGroupIdLoader _groupTagByGroupIdLoader;
    private readonly GroupMemberDtoByGroupLoader _groupMemberDtoByGroupLoader;

    public GroupDtoByEmployeeIdLoader()
    {
        _groupsRepository = new GroupsRepository();
        _groupTagByGroupIdLoader = new GroupTagByGroupIdLoader();
        _groupMemberDtoByGroupLoader = new GroupMemberDtoByGroupLoader();
    }

    public async Task<Dictionary<int, GroupDto[]>> LoadAsync(int[] ids)
    {
        var groups = await _groupsRepository.GetByEmployeeIds(ids);
        // Штуку ниже можно в extension
        var allGroups = groups.SelectMany(
                kvp => kvp.Value
                    .Select(
                        g => new BindingInfo<Group, GroupDto>(kvp.Key, g, ToBaseDto(g))))
            .ToArray();

        await GraphQLikeBinder.BindAsync(
            allGroups,
            _groupTagByGroupIdLoader,
            i => i.Entity.Id,
            (dto, value) => dto.Tags = value);

        await GraphQLikeBinder.BindAsync(
            allGroups,
            _groupMemberDtoByGroupLoader,
            i => i.Entity.Id,
            (dto, value) => dto.Members = value);

        // Штуку ниже можно в extension
        return allGroups
            .GroupBy(bi => bi.Id)
            .ToDictionary(gr => gr.Key, i => i.Select(bi => bi.Dto).ToArray());
    }

    private GroupDto ToBaseDto(Group group)
    {
        return new GroupDto
        {
            Id = group.Id,
            Name = group.Name
        };
    }
}

public class GroupTagByGroupIdLoader : ILoader<string[]>
{
    private readonly GroupsRepository _groupsRepository;

    public GroupTagByGroupIdLoader()
    {
        _groupsRepository = new GroupsRepository();
    }

    public async Task<Dictionary<int, string[]>> LoadAsync(int[] ids)
    {
        var groupTags = await _groupsRepository.GetTagsByGroupIds(ids);
        return groupTags.ToDictionary(i => i.Key, i => i.Value.Select(gt => gt.Tag).ToArray());
    }
}

public class GroupMemberDtoByGroupLoader : ILoader<GroupMemberDto[]>
{
    private readonly NearAbsenceDtoByEmployeeIdLoader _nearAbsenceDtoByEmployeeIdLoader;
    private readonly EmployeesRepository _employeesRepository;
    private readonly GroupsRepository _groupsRepository;

    public GroupMemberDtoByGroupLoader()
    {
        _nearAbsenceDtoByEmployeeIdLoader = new NearAbsenceDtoByEmployeeIdLoader();
        _employeesRepository = new EmployeesRepository();
        _groupsRepository = new GroupsRepository();
    }

    public async Task<Dictionary<int, GroupMemberDto[]>> LoadAsync(int[] ids)
    {
        var groupMembers = await _groupsRepository.GetMembersByGroupIds(ids, max: 5);
        // Штуку ниже можно в extension
        var allGroupMembers = groupMembers.SelectMany(
            kvp => kvp.Value
                .Select(
                    gm => new BindingInfo<GroupMember, GroupMemberDto>(kvp.Key, gm, ToBaseDto(gm))))
            .ToArray();

        // Можно сделать отдельный лоадер для FullName вместо 3-х строчек ниже, но есть идея поинтереснее, надо покумекать
        var employeeIds = allGroupMembers.Select(i => i.Entity.EmployeeId).Distinct().ToArray();
        var employees = await _employeesRepository.GetByIds(employeeIds);
        var fullNamesByEmployeeId = employees.ToDictionary(i => i.Key, i => i.Value.FullName);
        GraphQLikeBinder.Bind(
            allGroupMembers, 
            fullNamesByEmployeeId,
            i => i.Entity.EmployeeId, 
            (dto, value) => dto.FullName = value);

        await GraphQLikeBinder.BindAsync(
            allGroupMembers, 
            _nearAbsenceDtoByEmployeeIdLoader, 
            i => i.Entity.EmployeeId,
            (dto, value) => dto.NearAbsence = value);

        // Штуку ниже можно в extension
        return allGroupMembers
            .GroupBy(bi => bi.Id)
            .ToDictionary(gr => gr.Key, i => i.Select(bi => bi.Dto).ToArray());
    }

    private GroupMemberDto ToBaseDto(GroupMember groupMember)
    {
        return new GroupMemberDto
        {
            Id = groupMember.EmployeeId,
            Role = groupMember.Role
        };
    }
}

public class NearAbsenceDtoByEmployeeIdLoader : ILoader<NearAbsenceDto>
{
    private readonly AbsenceRepository _absenceRepository;

    public NearAbsenceDtoByEmployeeIdLoader()
    {
        _absenceRepository = new AbsenceRepository();
    }

    public async Task<Dictionary<int, NearAbsenceDto>> LoadAsync(int[] ids)
    {
        var absencesByEmployeeIds = await _absenceRepository.GetNearAbsencesByEmployeeIds(ids);
        return absencesByEmployeeIds.ToDictionary(i => i.Key, i => ToDto(i.Value));
    }

    private NearAbsenceDto ToDto(Absence absence)
    {
        return new NearAbsenceDto
        {
            From = absence.From,
            To = absence.To,
            Id = absence.Id
        };
    }
}

public class GraphQLikeBinder
{
    public static async Task BindAsync<TEntity, TDto, TValue>(
        BindingInfo<TEntity, TDto>[] objects,
        ILoader<TValue> loader,
        Func<BindingInfo<TEntity, TDto>, int> idFunc,
        Action<TDto, TValue> setAction)
    {
        var ids = objects.Select(idFunc).Distinct().ToArray();
        var values = await loader.LoadAsync(ids);
        Bind(objects, values, idFunc, setAction);
    }

    public static void Bind<TEntity, TDto, TValue>(
        BindingInfo<TEntity, TDto>[] objects,
        Dictionary<int, TValue> values,
        Func<BindingInfo<TEntity, TDto>, int> idFunc,
        Action<TDto, TValue> setAction)
    {
        foreach (var obj in objects)
        {
            var id = idFunc(obj);
            if (values.TryGetValue(id, out var value))
                setAction(obj.Dto, value);
            // TODO else throw error? Параметр для этого?
        }
    }
}

public class BindingInfo<TEntity, TDto>
{
    public int Id { get; }

    public TEntity Entity { get; }

    public TDto Dto { get; }

    public BindingInfo(int id, TEntity entity, TDto dto)
    {
        Id = id;
        Entity = entity;
        Dto = dto;
    }
}