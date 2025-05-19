using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Otomasyon.Data;
using Otomasyon.Models;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Otomasyon.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AccountController> _logger;

        public AccountController(ApplicationDbContext context, ILogger<AccountController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Kullanıcı adı kontrolü
                    if (await _context.Users.AnyAsync(u => u.Username == model.Username))
                    {
                        ModelState.AddModelError("Username", "Bu kullanıcı adı zaten kullanılıyor.");
                        return View(model);
                    }

                    // Şifre hashleme
                    using (var hmac = new HMACSHA512())
                    {
                        var user = new User
                        {
                            Username = model.Username,
                            PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(model.Password)),
                            PasswordSalt = hmac.Key,
                            Email = model.Email,
                            CreatedDate = DateTime.UtcNow
                        };

                        _context.Users.Add(user);
                        await _context.SaveChangesAsync();

                        _logger.LogInformation($"Kullanıcı başarıyla oluşturuldu: {model.Username}");
                        return RedirectToAction("Login");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Kullanıcı kaydı sırasında hata oluştu");
                    ModelState.AddModelError("", "Kayıt işlemi sırasında bir hata oluştu. Lütfen tekrar deneyin.");
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _context.Users
                        .FirstOrDefaultAsync(u => u.Username == model.Username);

                    if (user == null)
                    {
                        ModelState.AddModelError("", "Kullanıcı adı veya şifre hatalı.");
                        return View(model);
                    }

                    using (var hmac = new HMACSHA512(user.PasswordSalt))
                    {
                        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(model.Password));
                        for (int i = 0; i < computedHash.Length; i++)
                        {
                            if (computedHash[i] != user.PasswordHash[i])
                            {
                                ModelState.AddModelError("", "Kullanıcı adı veya şifre hatalı.");
                                return View(model);
                            }
                        }
                    }

                    // Giriş başarılı
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Username),
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Email, user.Email)
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, "Cookies");
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
                    };

                    await HttpContext.SignInAsync(
                        "Cookies",
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    return RedirectToAction("Index", "Home");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Giriş sırasında hata oluştu");
                    ModelState.AddModelError("", "Giriş işlemi sırasında bir hata oluştu. Lütfen tekrar deneyin.");
                }
            }

            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("Cookies");
            return RedirectToAction("Login");
        }
    }
} 