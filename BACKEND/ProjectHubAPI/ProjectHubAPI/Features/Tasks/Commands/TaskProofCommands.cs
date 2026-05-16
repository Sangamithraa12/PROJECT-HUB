using MediatR;
using ProjectHubAPI.DTOs;
using ProjectHubAPI.Common.Responses;
using ProjectHubAPI.Data;
using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectHubAPI.Features.Tasks.Commands
{
    public class SubmitTaskProofHandler : IRequestHandler<SubmitTaskProofCommand, ServiceResponse<bool>>
    {
        private readonly AppDbContext _context;

        public SubmitTaskProofHandler(AppDbContext context) { _context = context; }

        public async Task<ServiceResponse<bool>> Handle(SubmitTaskProofCommand request, CancellationToken cancellationToken)
        {
            var task = await _context.Tasks.FindAsync(new object[] { request.TaskId }, cancellationToken);
            if (task == null) return ServiceResponse<bool>.Fail("Task not found");

            task.ProofUrl = request.ProofUrl;
            await _context.SaveChangesAsync(cancellationToken);
            return ServiceResponse<bool>.Ok(true);
        }
    }

    public class UploadTaskProofHandler : IRequestHandler<UploadTaskProofCommand, ServiceResponse<string>>
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public UploadTaskProofHandler(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context; _env = env;
        }

        public async Task<ServiceResponse<string>> Handle(UploadTaskProofCommand request, CancellationToken cancellationToken)
        {
            var task = await _context.Tasks.FindAsync(new object[] { request.TaskId }, cancellationToken);
            if (task == null) return ServiceResponse<string>.Fail("Task not found");

            var uploadDir = Path.Combine(_env.WebRootPath, "uploads", "tasks");
            if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

            var uniqueFileName = $"{Guid.NewGuid()}_{request.FileName}";
            var filePath = Path.Combine(uploadDir, uniqueFileName);

            using (var destinationStream = new FileStream(filePath, FileMode.Create))
            {
                await request.FileStream.CopyToAsync(destinationStream, cancellationToken);
            }

            var fileUrl = $"/uploads/tasks/{uniqueFileName}";
            task.ProofUrl = fileUrl;
            await _context.SaveChangesAsync(cancellationToken);
            return ServiceResponse<string>.Ok(fileUrl);
        }
    }

    public class UploadTaskFolderHandler : IRequestHandler<UploadTaskFolderCommand, ServiceResponse<string>>
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public UploadTaskFolderHandler(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context; _env = env;
        }

        public async Task<ServiceResponse<string>> Handle(UploadTaskFolderCommand request, CancellationToken cancellationToken)
        {
            var task = await _context.Tasks.FindAsync(new object[] { request.TaskId }, cancellationToken);
            if (task == null) return ServiceResponse<string>.Fail("Task not found");

            var folderName = $"{Guid.NewGuid()}_proof";
            var uploadDir = Path.Combine(_env.WebRootPath, "uploads", "tasks", folderName);
            if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

            foreach (var file in request.Files)
            {
                var filePath = Path.Combine(uploadDir, file.fileName);
                var subDir = Path.GetDirectoryName(filePath);
                if (subDir != null && !Directory.Exists(subDir)) Directory.CreateDirectory(subDir);

                using (var destinationStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.stream.CopyToAsync(destinationStream, cancellationToken);
                }
            }

            var folderUrl = $"/uploads/tasks/{folderName}";
            task.ProofUrl = folderUrl;
            await _context.SaveChangesAsync(cancellationToken);
            return ServiceResponse<string>.Ok(folderUrl);
        }
    }
}
 
