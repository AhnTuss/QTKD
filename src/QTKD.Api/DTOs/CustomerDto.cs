using QTKD.Api.Entities;

namespace QTKD.Api.DTOs;

public record CustomerDto(int Id, string Name, CustomerStatus Status);
