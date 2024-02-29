namespace MegaMapper;

public class GroupMemberDto
{
    public int Id { get; set; }

    public string Role { get; set; }

    public string FullName { get; set; }

    public NearAbsenceDto? NearAbsence { get; set; }
}