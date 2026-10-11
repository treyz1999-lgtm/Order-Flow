using System.ComponentModel.DataAnnotations;

namespace OrderFlow.Api.DTOs;

public record CreateCustomerRequest(
    [Required, StringLength(100)] string Name,
    [Required, EmailAddress, StringLength(256)] string Email);
