using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.DAL;
using WebApi.Model;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CourseStepController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CourseStepController(AppDbContext context)
        {
            _context = context;

        }
        // GET: api/CourseStep
        [HttpGet]
        public ActionResult<IEnumerable<CourseStepModel>> Get()
        {
            return _context.CourseStep.ToList();
        }

        // GET: api/CourseStep/5
        [HttpGet("{id}")]
        public ActionResult<CourseStepModel> Get(int id)
        {
            var courseStep = _context.CourseStep.Find(id);

            if (courseStep == null)
            {
                return NotFound();
            }

            return courseStep;
        }

        // POST: api/CourseStep
        [HttpPost]
        public IActionResult Post([FromBody] CourseStepModel courseStep)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.CourseStep.Add(courseStep);
            _context.SaveChanges();

            return CreatedAtAction(nameof(Get), new { id = courseStep.ID }, courseStep);
        }

        [HttpPost("fileupload")]
        [DisableRequestSizeLimit]
        public IActionResult UploadFiles(int CourseID, int StepNo, string StepTitle, string ContentType, List<IFormFile> StepContents, string Description)
        {
            try
            {
                var basePath = Directory.GetCurrentDirectory();
                var stepFolderPath = Path.Combine(basePath, "Assets", $"Course_{CourseID}", $"Step_{StepNo}");

                if (Directory.Exists(stepFolderPath))
                {
                    Directory.Delete(stepFolderPath, true);
                }

                Directory.CreateDirectory(stepFolderPath);

                var contentFileNames = new List<string>();

                foreach (var file in StepContents)
                {
                    var contentFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                    var contentFilePath = Path.Combine(stepFolderPath, contentFileName);

                    using (var stream = new FileStream(contentFilePath, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }

                    contentFileNames.Add(contentFileName);
                }

                var contentFilesString = string.Join(",", contentFileNames);

                var existingCourseStep = _context.CourseStep.FirstOrDefault(cs => cs.CourseID == CourseID && cs.StepNo == StepNo);

                if (existingCourseStep != null)
                {
                    existingCourseStep.StepTitle = StepTitle;
                    existingCourseStep.StepContent = contentFilesString;
                    existingCourseStep.ContentType = ContentType;
                    existingCourseStep.Description = Description;
                    _context.SaveChanges();
                }
                else
                {
                    var courseStep = new CourseStepModel
                    {
                        CourseID = CourseID,
                        StepNo = StepNo,
                        StepTitle = StepTitle,
                        StepContent = contentFilesString,
                        ContentType = ContentType,
                        Description = Description
                    };

                    _context.CourseStep.Add(courseStep);
                    _context.SaveChanges();
                }

                return Ok("Files uploaded successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }




        // PUT: api/CourseStep/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] CourseStepModel updatedCourseStep)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != updatedCourseStep.ID)
            {
                return BadRequest();
            }

            _context.Entry(updatedCourseStep).State = EntityState.Modified;
            _context.SaveChanges();

            return NoContent();
        }

        // DELETE: api/CourseStep/RemoveFile
        [HttpDelete("removefile")]
        public IActionResult DeleteFileAndData(int CourseID, int StepID, string ContentType, string FileName)
        {
            var courseStep = _context.CourseStep
                .Where(cs => cs.CourseID == CourseID && cs.StepNo == StepID)
                .FirstOrDefault();

            if (courseStep == null)
            {
                return NotFound();
            }

            if (ContentType == "Video")
            {
                var basePath = Directory.GetCurrentDirectory();
                var stepFolderPath = Path.Combine(basePath, "Assets", $"Course_{CourseID}", $"Step_{courseStep.StepNo}");

                var filePath = Path.Combine(stepFolderPath, FileName);


                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                courseStep.StepContent = "No Data";
                courseStep.ContentType = "Text";

                _context.SaveChanges();
            }
            else if (ContentType == "Image")
            {
                var basePath = Directory.GetCurrentDirectory();
                var stepFolderPath = Path.Combine(basePath, "Assets", $"Course_{CourseID}", $"Step_{courseStep.StepNo}");

                var filePath = Path.Combine(stepFolderPath, FileName);

                if (System.IO.File.Exists(filePath))
                {
                    try
                    {
                        System.IO.File.Delete(filePath);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error deleting file: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine("File does not exist.");
                }

                var imageFileNames = courseStep.StepContent.Split(',');
                var updatedImageFileNames = imageFileNames.Where(name => name != FileName).ToList();

                courseStep.StepContent = string.Join(",", updatedImageFileNames);
                if (updatedImageFileNames.Count == 0)
                {
                    courseStep.StepContent = "No Data";
                    courseStep.ContentType = "Text";
                }

                _context.SaveChanges();
            }

            return NoContent();
        }


        // DELETE: api/CourseStep/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var courseStep = _context.CourseStep.Find(id);

            if (courseStep == null)
            {
                return NotFound();
            }

            _context.CourseStep.Remove(courseStep);
            _context.SaveChanges();

            return NoContent();
        }

        // DELETE: api/CourseStep/5/StepNo
        [HttpDelete("deletestepno")]
        public IActionResult DeleteStepNo(int CourseID, int StepNo)
        {

            var courseStep = _context.CourseStep
                .Where(cs => cs.CourseID == CourseID && cs.StepNo == StepNo).FirstOrDefault();

            if (courseStep == null)
            {
                return NotFound();
            }
            var basePath = Directory.GetCurrentDirectory();
            var stepFolderPath = Path.Combine(basePath, "Assets", $"Course_{CourseID}", $"Step_{StepNo}");

            if (Directory.Exists(stepFolderPath))
            {
                Directory.Delete(stepFolderPath, true);
            }
            _context.CourseStep.Remove(courseStep);
            _context.SaveChanges();

            return NoContent();
        }

        // GET: api/CourseStep/Course/5
        [HttpGet("Course/{id}")]
        public ActionResult<IEnumerable<CourseStepModel>> GetCourseSteps(int id)
        {
            var courseSteps = _context.CourseStep
                .Where(cs => cs.CourseID == id)
                .ToList();

            if (courseSteps == null || courseSteps.Count == 0)
            {
                return NotFound();
            }

            return courseSteps;
        }

        [HttpGet("filecontent")]
        public IActionResult GetFileContent(int CourseID, int StepNo, string ContentType, string FileName)
        {
            try
            {
                string folderPath;
                string contentType;

                switch (ContentType)
                {
                    case "Image":
                        folderPath = "Images";
                        contentType = GetContentType(FileName);
                        break;

                    case "Video":
                        folderPath = "Video";
                        contentType = GetContentType(FileName);
                        break;

                    default:
                        return BadRequest("Unknown File Type");
                }

                var basePath = Directory.GetCurrentDirectory();
                var filesFolderPath = Path.Combine(basePath, "Assets", $"Course_{CourseID}");
                var stepFolderPath = Path.Combine(filesFolderPath, $"Step_{StepNo}");
                var filePath = Path.Combine(stepFolderPath, FileName);

                if (System.IO.File.Exists(filePath))
                {
                    var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                    return File(fileStream, contentType);
                }
                else
                {
                    return Ok(new { Content = "File Not Created" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error:{ex.Message}");
            }
        }

        private string GetContentType(string fileName)
        {
            string ext = Path.GetExtension(fileName).ToLower();

            switch (ext)
            {
                case ".jpg":
                    return "image/jpg";
                case ".jpeg":
                    return "image/jpeg";
                case ".png":
                    return "image/png";
                case ".mp4":
                    return "video/mp4";
                case ".avi":
                    return "video/x-msvideo";
                case ".mov":
                    return "video/quicktime";
                case ".wmv":
                    return "video/x-ms-wmv";
                default:
                    return "application/octet-stream";
            }
        }


    }
}
