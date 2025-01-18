using System;

public class CreateAccountCommand
{
    /// <summary>
    /// Discord User ID
    /// </summary>
    public ulong Id { get; set; }

    /// <summary>
    /// The Discord username of the user.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// When the user signed up for an account.
    /// </summary>
    public DateTime CreatedDate { get; set; }
}