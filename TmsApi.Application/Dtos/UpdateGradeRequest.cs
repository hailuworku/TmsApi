using System.ComponentModel.DataAnnotations;
namespace TmsApi.Application.Dtos;

public record UpdateGradeRequest(
    [Range(0.0, 100.0, ErrorMessage = "Grade must be between 0 and 100")] 
    decimal Grade
);