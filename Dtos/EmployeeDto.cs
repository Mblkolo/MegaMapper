﻿namespace MegaMapper;

public class EmployeeDto
{
    public int Id { get; set; }
    public string FullName { get; set; }
    public GroupDto[] Groups { get; set; }
    public NearAbsenceDto? NearAbsence { get; set; }
}