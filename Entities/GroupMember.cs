﻿namespace MegaMapper.Entities;

public class GroupMember
{
    public int GroupId { get; set; }
    public int EmployeeId { get; set; }
    public string Role { get; set; }
}