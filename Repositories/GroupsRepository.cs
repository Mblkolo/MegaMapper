using MegaMapper.Entities;

namespace MegaMapper.Repositories;

public class GroupsRepository
{
    public Dictionary<int, Group[]> GetByEmployeeIds(int[] employeeIds)
    {
        Console.WriteLine(nameof(GroupsRepository) + " " +
                          nameof(GetByEmployeeIds) + " " +
                          string.Join(", ", employeeIds));

        return Db.GroupMembers.Join(Db.Groups,
                x => x.GroupId,
                x => x.Id,
                (member, group) => (member, group))
            .GroupBy(x => x.member.EmployeeId)
            .ToDictionary(x => x.Key, x => x.Select(y => y.group).ToArray());
    }

    public Dictionary<int, GroupMember[]> GetMembersByGroupIds(int[] groupsIds, int max)
    {
        Console.WriteLine(nameof(GroupsRepository) + " " +
                          nameof(GetMembersByGroupIds) + " " +
                          string.Join(", ", groupsIds));


        return Db.GroupMembers.Where(x => groupsIds.Contains(x.GroupId))
            .GroupBy(x => x.GroupId)
            .ToDictionary(x => x.Key, x => x.Take(max).ToArray());
    }

    public Dictionary<int, GroupTag[]> GetTagsByGroupIds(int[] groupsIds)
    {
        Console.WriteLine(nameof(GroupsRepository) + " " +
                          nameof(GetTagsByGroupIds) + " " +
                          string.Join(", ", groupsIds));

        return Db.GroupTags.Where(x => groupsIds.Contains(x.GroupId))
            .GroupBy(x => x.GroupId)
            .ToDictionary(x => x.Key, x => x.ToArray());
    }
}