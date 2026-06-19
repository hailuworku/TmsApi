using System;
using Microsoft.Extensions.DependencyInjection;

namespace TmsApi
{
    // corected
    public class EnrollmentWorker
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public EnrollmentWorker(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public void ProcessBatch()
        {
            // short-lived scope
            using (var scope = _scopeFactory.CreateScope())
            {
                // injected services
                var enrollmentService = scope.ServiceProvider.GetRequiredService<IEnrollmentService>();

                Console.WriteLine("Worker processed batch using a secure short-lived scope!");
            } // scope disposed here, preventing memory leaks and ensuring proper cleanup of resources
        }
    }
}