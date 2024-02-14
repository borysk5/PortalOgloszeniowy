using PortalOgloszeniowy.Data;
using Microsoft.AspNetCore.Mvc;
using PortalOgloszeniowy.Models;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Xml.Linq;
using System;
using System.Diagnostics;

namespace PortalOgloszeniowy.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _db;
        private UserManager<User> _userManager;
        private SignInManager<User> _signInManager;
        private RoleManager<IdentityRole> _roleManager;

        public UserController(
            ApplicationDbContext db,
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            RoleManager<IdentityRole> roleManager
            )
        {
            _db = db;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        // GET: UserController
        public ActionResult Index()
        {
            IEnumerable<User> objList = _db.Users;
            return View(objList);
        }

        // GET: UserController/Details/5
        public async Task<ActionResult> Details(string id)
        {
            return View(await _userManager.FindByNameAsync(id));
        }

        // GET:
        public ActionResult Login()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ExternalLoginConfirmation(string returnUrl = null, string remoteError = null)
        {
            Debug.WriteLine("10");
            // Tutaj możesz wyświetlić formularz potwierdzenia logowania zewnętrznego lub przekierować użytkownika na inną stronę w zależności od potrzeb
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExternalLoginConfirmation(LoginUser model, string returnUrl = null)
        {
            Debug.WriteLine("000");
            Debug.WriteLine(model.UserName);
            if (ModelState.IsValid)
            {
                Debug.WriteLine("1");


                // Pobierz informacje o zewnętrznym dostawcy uwierzytelniania dla aktualnego żądania logowania
                var info = await _signInManager.GetExternalLoginInfoAsync();
                Console.WriteLine("2");
                if (info == null)
                {
                    Console.WriteLine("3");
                    // Obsłuż błąd pobierania informacji o logowaniu zewnętrznym
                    return RedirectToAction("Login");
                }

                Debug.WriteLine("4");

                // Sprawdź, czy użytkownik o podanym adresie e-mail już istnieje w bazie danych
                var user = await _userManager.FindByEmailAsync(model.UserName);
                if (user == null)
                {
                    Debug.WriteLine("5");
                    // Jeśli użytkownik nie istnieje, utwórz nowego użytkownika na podstawie informacji z dostawcy logowania zewnętrznego
                    user = new User { UserName = model.UserName, Email = model.UserName };
                    var result = await _userManager.CreateAsync(user,model.Password);
                    if (!result.Succeeded)
                    {
                        Debug.WriteLine("6");
                        // Obsłuż błąd tworzenia użytkownika
                        return RedirectToAction("Error");
                    }
                }

                // Przeprowadź logowanie użytkownika
                Debug.WriteLine("7");
                var signInResult = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, false);
                if (signInResult.Succeeded)
                {
                    // Zalogowano pomyślnie
                    return RedirectToLocal(returnUrl);
                }
                if (signInResult.IsLockedOut)
                {
                    // Obsłuż zablokowane konto
                    return RedirectToAction("Lockout");
                }
                else
                {
                    // Jeśli nie udało się zalogować, wyświetl odpowiedni komunikat lub przekieruj użytkownika na inną stronę
                    return RedirectToAction("Error");
                }
            }

            // Jeśli model nie jest poprawny, wyświetl ponownie formularz z błędami
            return View(model);
        }




        [HttpGet]
        [AllowAnonymous]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            Debug.WriteLine("amogus");
            var redirectUrl = Url.Action("ExternalLoginCallback", "User", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            Debug.WriteLine("sus");
            if (remoteError != null)
            {
                Debug.WriteLine("sus1");
                // Obsłuż błąd logowania zewnętrznego
                return RedirectToAction("Login");
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                Debug.WriteLine("sus2");
                // Obsłuż błąd pobierania informacji o logowaniu zewnętrznym
                return RedirectToAction("Login");
            }

            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (result.Succeeded)
            {
                Debug.WriteLine("sus3");
                // Zalogowano pomyślnie zewnętrznym dostawcą uwierzytelniania
                return RedirectToLocal(returnUrl);
            }

            if (result.IsLockedOut)
            {
                Debug.WriteLine("sus4");
                // Obsłuż zablokowane konto
                return RedirectToAction("Lockout");
            }
            else
            {
                // Jeśli użytkownik nie ma konta, przekieruj do strony rejestracji
                ViewData["ReturnUrl"] = returnUrl;
                ViewData["LoginProvider"] = info.LoginProvider;
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                Debug.WriteLine(info.Principal.FindFirstValue(ClaimTypes.AuthorizationDecision));


                // Utwórz nowy obiekt LoginUser z domyślnym adresem e-mail jako nazwą użytkownika
                var loginUser = new LoginUser { UserName = email };

                var user = await _userManager.FindByEmailAsync(email);
                if (user != null)
                {
                    await _signInManager.SignInAsync(user, true);
                    return RedirectToLocal(returnUrl);
                }



                return View("ExternalLogin", loginUser);
            }
        }




        private IActionResult RedirectToLocal(string returnUrl)
        {
            Debug.WriteLine("sussybaka");
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }


        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginUser model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(model.UserName);
                if (user !=null && user.Ban == true)
                {
                    ModelState.AddModelError("", "Your account is banned");
                    return View(model);
                }
                var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, false);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError("", "Invalid login attempt");
            }
            return View(model);
        }

        public async Task<ActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("index", "home");
        }

        // GET: UserController/Create
        public async Task<ActionResult> Create()
        {
            if (!_roleManager.RoleExistsAsync("Admin").GetAwaiter().GetResult())
            {
                await _roleManager.CreateAsync(new IdentityRole("admin"));
                await _roleManager.CreateAsync(new IdentityRole("mod"));
                await _roleManager.CreateAsync(new IdentityRole("user"));
                var user = new User();
                user.UserName = "Admin";
                user.Email = "admin@gmail.com";
                var result = await _userManager.CreateAsync(user, "zaq1@WSX");
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "admin");
                }
            }
            return View();
        }

        // POST: UserController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(RegisterUser obj)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var user = new User();
                    user.UserName = obj.UserName;
                    user.Email = obj.Email;
                    user.PhoneNumber = obj.PhoneNumber;
                    var result = await _userManager.CreateAsync(user, obj.Password);
                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(user, "user");
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return RedirectToAction("Details","User",new { id=user.UserName });
                    }
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
                return View(obj);
            }
            catch
            {
                return View(obj);
            }
        }

        // GET: UserController/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null) return RedirectToAction(nameof(Index));
            var obj = _db.Users.Find(id);
            if (obj == null) return NotFound();
            return View(new RegisterUser(obj));
        }

        // POST: UserController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(RegisterUser obj)
        {
            try
            {
                if (ModelState.IsValid && obj.PasswordRequired != null)
                {
                    var user = await _userManager.FindByIdAsync(obj.Id);
                    if (await _userManager.CheckPasswordAsync(user, obj.PasswordRequired))
                    {
                        user.UserName = obj.UserName;
                        user.Email = obj.Email;
                        user.PhoneNumber = obj.PhoneNumber;
                        await _userManager.UpdateAsync(user);
                        await _userManager.ChangePasswordAsync(user, obj.PasswordRequired, obj.Password);
                        return RedirectToAction(nameof(Index));
                    }
                    ModelState.AddModelError("", "Złe hasło");
                    return View(obj);
                }
                ModelState.AddModelError("", "Należy podać poprawne hasło");
                return View(obj);
            }
            catch
            {
                ModelState.AddModelError("", "Exception occured");
                return View();
            }
        }

        // GET: UserController/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null) return RedirectToAction(nameof(Index));
            var obj = _db.Users.Find(id);
            if (obj == null) return NotFound();
            return View(new RegisterUser(obj));
        }

        // POST: UserController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(RegisterUser obj)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(obj.Id);
                if (obj.PasswordRequired != null)
                {
                    if (await _userManager.CheckPasswordAsync(user, obj.PasswordRequired))
                    {
                        await _userManager.DeleteAsync(user);
                        return RedirectToAction(nameof(Index));
                    }
                    ModelState.AddModelError("", "Złe hasło");
                    return View(new RegisterUser(user));
                }
                ModelState.AddModelError("", "Należy podać poprawne hasło");
                return View(new RegisterUser(user));
            }
            catch
            {
                ModelState.AddModelError("", "Exception occured");
                return View();
                //return RedirectToAction("Delete", "User", obj.Id);
            }
        }

        //GET
        public async Task<ActionResult> ModGrant(string name)
        {
            if (User.IsInRole("admin"))
            {
                var user = await _userManager.FindByNameAsync(name);
                if (user == null) ModelState.AddModelError("", "No such user");
                await _userManager.AddToRoleAsync(user, "mod");
                //NOTIFICATION
                var notList = user.Notifications.ToList();
                var not = new Notification();
                not.Text = "Właśnie dostałeś uprawnienia moderatora";
                notList.Add(not);
                user.Notifications = notList;
                await _userManager.UpdateAsync(user);
            }
            else
            {
                ModelState.AddModelError("", "You have no premission for this action");
            }
            return RedirectToAction("Index");
        }

        //GET
        public async Task<ActionResult> ModRevoke(string name)
        {
            if (User.IsInRole("admin"))
            {
                var user = await _userManager.FindByNameAsync(name);
                if (user == null) ModelState.AddModelError("", "No such user");
                await _userManager.RemoveFromRoleAsync(user, "mod");
                //NOTIFICATION
                var notList = user.Notifications.ToList();
                var not = new Notification();
                not.Text = "Właśnie utraciłeś uprawnienia moderatora";
                notList.Add(not);
                user.Notifications = notList;
                await _userManager.UpdateAsync(user);
            }
            else
            {
                ModelState.AddModelError("", "You have no premission for this action");
            }
            return RedirectToAction("Index");
        }

        //GET
        public async Task<ActionResult> Ban(string name)
        {
            if (User.IsInRole("admin") || User.IsInRole("mod"))
            {
                var user = await _userManager.FindByNameAsync(name);
                user.Ban = true;
                await _userManager.UpdateAsync(user);
            }
            return RedirectToAction("Index");
        }

        //GET
        public async Task<ActionResult> Unban(string name)
        {
            if (User.IsInRole("admin") || User.IsInRole("mod"))
            {
                var user = await _userManager.FindByNameAsync(name);
                user.Ban = false;
                await _userManager.UpdateAsync(user);
            }
            return RedirectToAction("Index");
        }



    }
}
