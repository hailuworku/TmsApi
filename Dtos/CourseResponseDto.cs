namespace TmsApi.Dtos;

public record CourseResponseDto(
    int Id,
    string Code,
    string Title,
    int MaxCapacity,
    int EnrollmentCount); // send data to cliant