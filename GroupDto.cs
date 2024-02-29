namespace MegaMapper;

public class GroupDto
{
    public int Id { get; set; }

    public string Name { get; set; }

    public GroupMemberDto[] Members { get; set; }

    public string[] Tags { get; set; }
}