﻿namespace Auth.Entities;

public class Auth
{
    public Guid UserId { get; set; }
    public string? Login { get; set; }
    public string? PasswordHash { get; set; }
}
