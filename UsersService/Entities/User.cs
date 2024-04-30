﻿namespace UsersService.Entities;

public class User
{
    public Guid Id { get; set; }
    public Guid CreateRequestId { get; set; }
    public Guid VersionId { get; set; }
    public string? UserName { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
}
