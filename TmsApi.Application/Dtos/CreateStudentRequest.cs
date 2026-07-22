using System.ComponentModel.DataAnnotations;

namespace TmsApi.Application.Dtos;

public record CreateStudentRequest(
    [Required]
    [RegularExpression(@"^TMS-\d{4}-\d{4}$", ErrorMessage = "Format must be TMS-2026-0001")]
    string RegistrationNumber,

    [Required]
    [MaxLength(100)]
    string Name,

    [Range(0.0, 4.0, ErrorMessage = "GPA must be between 0.0 and 4.0")]
    decimal GPA
);