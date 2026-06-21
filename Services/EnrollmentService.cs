using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace TmsApi
{
    // 1. የአገልግሎቱ የጋራ ውል (IEnrollmentService Interface) - Page 90
    public interface IEnrollmentService
    {
        Task<EnrollmentRecord> EnrollAsync(string studentId, string courseCode);
        Task<EnrollmentRecord?> GetByIdAsync(string id);
        Task<IReadOnlyList<EnrollmentRecord>> GetAllAsync();
        Task<bool> DeleteAsync(string id);
    }

    // 2. የአገልግሎቱ ተግባራዊ ክላስ (EnrollmentService Implementation) - Page 90
    public class EnrollmentService : IEnrollmentService
    {
        private readonly Dictionary<string, EnrollmentRecord> _store = new();
        private readonly ILogger<EnrollmentService> _logger;

        public EnrollmentService(ILogger<EnrollmentService> logger)
        {
            _logger = logger;
        }

        public Task<EnrollmentRecord> EnrollAsync(string studentId, string courseCode)
        {
            // ሀ. ተማሪው አስቀድሞ በዚያው ኮርስ ላይ መመዝገቡን ቼክ ማድረግ (Duplicate Check) - Page 95
            var existing = _store.Values
                .FirstOrDefault(e => e.StudentId == studentId && e.CourseCode == courseCode);

            if (existing is not null)
            {
                // ለ. የማስጠንቀቂያ ሎግ መጻፍ (LogWarning using Structured Template) - Page 95
                _logger.LogWarning("Duplicate enrollment attempt {StudentId} already in {CourseCode} (record {EnrollmentId})",
                    studentId, courseCode, existing.Id);

                return Task.FromResult(existing);
            }

            // ሐ. አዲስ ምዝገባ ማከናወን - Page 95
            var id = Guid.NewGuid().ToString("N")[..8];
            var record = new EnrollmentRecord(id, studentId, courseCode, DateTime.UtcNow);
            _store[id] = record;

            _logger.LogInformation("Enrolled {StudentId} in {CourseCode} record {EnrollmentId}",
                studentId, courseCode, id);

            return Task.FromResult(record);
        }

        public Task<EnrollmentRecord?> GetByIdAsync(string id)
        {
            _store.TryGetValue(id, out var record);

            // መዝገቡ ካልተገኘ የማስጠንቀቂያ ሎግ መጻፍ - Page 96
            if (record is null)
            {
                _logger.LogWarning("Enrollment {EnrollmentId} not found", id);
            }

            return Task.FromResult(record);
        }

        public Task<IReadOnlyList<EnrollmentRecord>> GetAllAsync()
        {
            IReadOnlyList<EnrollmentRecord> all = _store.Values.ToList();
            return Task.FromResult(all);
        }

        public Task<bool> DeleteAsync(string id)
        {
            var removed = _store.Remove(id);

            // መሰረዙ ከተሳካ የኢንፎርሜሽን ሎግ፣ ካልተሳካ ግን የማስጠንቀቂያ ሎግ መጻፍ - Page 96
            if (removed)
            {
                _logger.LogInformation("Deleted enrollment {EnrollmentId}", id);
            }
            else
            {
                _logger.LogWarning("Delete failed enrollment {EnrollmentId} not found", id);
            }

            return Task.FromResult(removed);
        }
    }

    // 3. የኢንሮልመንት ዳታ ቅርጽ (EnrollmentRecord) - Page 91
    public record EnrollmentRecord(string Id, string StudentId, string CourseCode, DateTime EnrolledAt);
}