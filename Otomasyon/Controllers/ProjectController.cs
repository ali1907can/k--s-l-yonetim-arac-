using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Otomasyon.Data;
using Otomasyon.Models;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace Otomasyon.Controllers
{
    [Authorize]
    public class ProjectController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProjectController> _logger;

        public ProjectController(ApplicationDbContext context, ILogger<ProjectController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var projects = await _context.Projects
                    .Where(p => p.UserId == userId)
                    .OrderByDescending(p => p.CreatedDate)
                    .ToListAsync();

                return View(projects);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Projeler listelenirken hata oluştu");
                return View(new List<Project>());
            }
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Project project)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Model geçersiz: {@ModelState}", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));
                    return View(project);
                }

                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                _logger.LogInformation("Kullanıcı ID: {UserId}", userId);

                project.UserId = userId;
                project.CreatedDate = DateTime.UtcNow;

                _logger.LogInformation("Proje oluşturuluyor: {@Project}", project);

                _context.Add(project);
                var result = await _context.SaveChangesAsync();

                _logger.LogInformation("Veritabanı kayıt sonucu: {Result}", result);

                if (result > 0)
                {
                    _logger.LogInformation("Proje başarıyla oluşturuldu: {ProjectName}", project.Name);
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    _logger.LogWarning("Proje oluşturulamadı: Kayıt yapılamadı");
                    ModelState.AddModelError("", "Proje oluşturulurken bir hata oluştu. Lütfen tekrar deneyin.");
                    return View(project);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Proje oluşturulurken hata oluştu");
                ModelState.AddModelError("", "Proje oluşturulurken bir hata oluştu: " + ex.Message);
                return View(project);
            }
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Project project)
        {
            if (id != project.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                    var existingProject = await _context.Projects
                        .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

                    if (existingProject == null)
                    {
                        return NotFound();
                    }

                    existingProject.Name = project.Name;
                    existingProject.Description = project.Description;

                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectExists(project.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(project);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            if (project != null)
            {
                _context.Projects.Remove(project);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ProjectExists(int id)
        {
            return _context.Projects.Any(e => e.Id == id);
        }
    }
} 