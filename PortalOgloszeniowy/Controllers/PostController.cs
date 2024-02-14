using PortalOgloszeniowy.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PortalOgloszeniowy.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace PortalOgloszeniowy.Controllers
{
    public class PostController : Controller
    {
        private readonly ApplicationDbContext _db;
        private UserManager<User> _userManager;
        private SignInManager<User> _signInManager;

        public PostController(
            ApplicationDbContext db,
            UserManager<User> userManager,
            SignInManager<User> signInManager
            )
        {
            _db = db;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        // GET: PostController
        public ActionResult Index()
        {
            IEnumerable<Post> objList = _db.Posts;
            return View(objList);
        }

        // GET: PostController/Details/5
        public ActionResult Details(int id)
        {
            var post = _db.Posts.Find(id);
            if (post == null) return View();
            return View(post);
        }

        // GET: PostController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: PostController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Post obj)
        {
            try
            {
                if (_signInManager.IsSignedIn(User))
                {
                    obj.Date = DateTime.Now;
                    var user = await _userManager.FindByNameAsync(User.Identity.Name);

                    var list = user.Posts.ToList();
                    list.Add(obj);
                    user.Posts = list;
                    var result = await _userManager.UpdateAsync(user);
                    if (!result.Succeeded)
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }
                        return View(obj);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "You are not logged in");
                    return View(obj);
                }
                return RedirectToAction("Details", "Post", new { id = obj.Id });
            }
            catch
            {
                return View(obj);
            }
        }

        // GET: PostController/Edit/5
        public ActionResult Edit(int id)
        {
            if (id <= 0) return RedirectToAction(nameof(Index));
            var obj = _db.Posts.Find(id);
            if (obj == null) return NotFound();
            return View(obj);
        }

        // POST: PostController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Post obj)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    obj.Date = DateTime.Now;
                    _db.Posts.Update(obj);
                    _db.SaveChanges();
                    return RedirectToAction("Details", "Post", new { id = obj.Id });
                }
                return View(obj);
            }
            catch
            {
                return View();
            }
        }

        // GET: PostController/Delete/5
        public ActionResult Delete(int id)
        {
            var obj = _db.Posts.Find(id);
            if (obj == null) return NotFound();
            return View(obj);
        }

        // POST: PostController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(Post obj)
        {
            try
            {
                obj = _db.Posts.Find(obj.Id);
                if (obj == null)
                {
                    return NotFound();
                }
                obj.Comments.ToList().Clear();
                obj.Tags.ToList().Clear();
                _db.Posts.Update(obj);
                _db.Posts.Remove(obj);

                #region NOTIFICATION
                var OwnerId = _db.Users.ToList().Find(user => user.Posts.ToList().Find(p => p.Id == obj.Id) != null).Id;
                var PostOwner = await _userManager.FindByIdAsync(OwnerId);
                if (User.Identity.Name == PostOwner.UserName)
                {
                    _db.SaveChanges();
                    return RedirectToAction(nameof(Index));
                }
                var notList = PostOwner.Notifications.ToList();
                var not = new Notification();
                not.Text = $"Twój post: {obj.Title} został usunięty przez moderację!";
                notList.Add(not);
                PostOwner.Notifications = notList;
                await _userManager.UpdateAsync(PostOwner);
                #endregion
                _db.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View(obj);
            }
        }

        //GET 
        public ActionResult UpVote(int id)
        {
            _db.Posts.Find(id).UpVotes++;
            _db.SaveChanges();
            return RedirectToAction("Details", "Post", new { id = id });
        }

        //GET Tag view
        public ActionResult Tags(int id)
        {
            return View(_db.Posts.Find(id));
        }

        //GET Tag view
        public ActionResult TagAdd(int idPost, string Text)
        {
            var post = _db.Posts.Find(idPost);
            var tag = new Tag();
            if (Text == null) return View("Tags", post);
            tag.Text = Text;
            var tagList = post.Tags.ToList();
            tagList.Add(tag);
            post.Tags = tagList;
            _db.Posts.Update(post);
            _db.SaveChanges();
            return View("Tags", post);
        }

        //GET Tag view
        public ActionResult TagDelete(int idPost, int idTag)
        {
            var post = _db.Posts.Find(idPost);
            var tag = post.Tags.ToList().Find(tag => tag.Id == idTag);
            var tagList = post.Tags.ToList();
            tagList.Remove(tag);
            post.Tags = tagList;
            _db.Posts.Update(post);
            _db.SaveChanges();
            return View("Tags", post);
        }

        public async Task<ActionResult> Report(int id)
        {
            var post = _db.Posts.Find(id);
            if(post == null) return View("Index");
            var not = new Notification();
            not.Text = $"Post: <a href=\"/Post/Details/{post.Id}\">{post.Title}</a> został zgłoszony!";
            foreach (var user in await _userManager.GetUsersInRoleAsync("mod"))
            {
                var notList = user.Notifications.ToList();
                notList.Add(not);
                user.Notifications = notList;
                await _userManager.UpdateAsync(user);
            }
            return RedirectToAction("Details", "Post", new { id = id });
        }

    }
}
