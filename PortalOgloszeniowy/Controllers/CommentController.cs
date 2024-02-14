using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PortalOgloszeniowy.Data;
using PortalOgloszeniowy.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

namespace PortalOgloszeniowy.Controllers
{
    public class CommentController : Controller
    {
        private readonly ApplicationDbContext _db;
        private UserManager<User> _userManager;

        public CommentController(
            ApplicationDbContext db,
            UserManager<User> userManager
            )
        {
            _db = db;
            _userManager = userManager;
        }

        public ActionResult Index()
        {
            return View();
        }

        // GET: CommentController
        [Route("[controller]/{idPost}")]
        [Route("[controller]/Index/{idPost}")]
        public ActionResult Index(int idPost)
        {
            if (idPost < 0) return View();
            var Post = _db.Posts.Find(idPost);
            if (Post == null) return View();
            return View(Post.Comments);
        }

        // GET: CommentController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: CommentController/Create
        [Route("[controller]/Create/{idPost}/")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: CommentController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("[controller]/Create/{idPost}")]
        public async Task<ActionResult> Create(int idPost, IFormCollection collection)
        {
            try
            {
                if (!(ModelState.IsValid)) return View();
                if (idPost < 0) return View();
                var post = _db.Posts.Find(idPost);
                if (post == null) return View();
                var user = await _userManager.FindByNameAsync(User.Identity.Name);
                if (user == null) return View();
                var commentListPost = post.Comments.ToList();
                var commentListUser = user.Comments.ToList();
                var comment = new Comment();
                comment.Date = DateTime.Now;
                comment.Text = collection["Text"];
                #region NOTIFICATION
                var OwnerId = _db.Users.ToList().Find(user => user.Posts.ToList().Find(p => p.Id == post.Id) != null).Id;
                var PostOwner = await _userManager.FindByIdAsync(OwnerId);
                var notList = PostOwner.Notifications.ToList();
                var not = new Notification();
                not.Text = $"Twój post: <a href=\"/Post/Details/{post.Id}\">{post.Title}</a> został skomentowany przez user:{user.UserName}!";
                notList.Add(not);
                PostOwner.Notifications = notList;
                await _userManager.UpdateAsync(PostOwner);
                #endregion
                commentListPost.Add(comment);
                commentListUser.Add(comment);
                post.Comments = commentListPost;
                user.Comments = commentListUser;
                var result = await _userManager.UpdateAsync(user);
                _db.Posts.Update(post);
                _db.SaveChanges();
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View();
                }
                return RedirectToAction("Details", "Post",new { id = post.Id });
            }
            catch
            {
                return View();
            }
        }

        // GET: CommentController/Edit/5
        public ActionResult Edit(int id, int? IdPost)
        {
            if (!IdPost.HasValue) return NotFound();
            if (id <= 0) return RedirectToAction(nameof(Index));
            var post = _db.Posts.Find(IdPost);
            if (post == null) return NotFound();
            var comment = post.Comments.ToList().Find(comm => comm.Id == id);
            if (comment == null) return NotFound();
            return View(comment);
        }

        // POST: CommentController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, IFormCollection collection)
        {
            var obj = new Comment();
            obj.Text = collection["Text"];
            try
            {
                var user = await _userManager.FindByNameAsync(User.Identity.Name);
                if (user == null) return View();
                var commentListUser = user.Comments.ToList();
                var comm = commentListUser.Find(com => com.Id == id);
                if (comm == null)
                {
                    ModelState.AddModelError("", "This is not Your comment");
                    return View(obj);
                }
                comm.Text = collection["Text"];
                user.Comments = commentListUser;
                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View(obj);
                }
                return RedirectToAction("Details", "Post", new { id =
                    _db.Posts.ToList().Find(P => P.Comments.ToList().Find(C => C.Id == comm.Id) != null).Id
                });
            }
            catch
            {
                return View(obj);
            }
        }

        // GET: CommentController/Delete/5
        public ActionResult Delete(int id, int? IdPost)
        {
            if (!IdPost.HasValue) return NotFound();
            if (id <= 0) return RedirectToAction(nameof(Index));
            var post = _db.Posts.Find(IdPost);
            if (post == null) return NotFound();
            var comment = post.Comments.ToList().Find(comm => comm.Id == id);
            if (comment == null) return NotFound();
            return View(comment);
        }

        // POST: CommentController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(User.Identity.Name);
                if (user == null)
                {
                    ModelState.AddModelError("", "You are not logged in");
                    return View(new Comment());
                }
                var userCommentList = user.Comments.ToList();
                var comment = userCommentList.Find(com => com.Id == id);
                if (comment == null && !(User.IsInRole("admin") || User.IsInRole("mod")))
                {
                    ModelState.AddModelError("", "This is not Your comment");
                    return View(new Comment());
                }
                userCommentList.Remove(comment);
                user.Comments = userCommentList;
                var post = _db.Posts.ToList().Find(post => post.Comments.ToList().Find(com => com.Id == id) != null);
                if (post == null)
                {
                    ModelState.AddModelError("", "Error occured");
                    return View(new Comment());
                }
                var postCommentList = post.Comments.ToList();
                comment = postCommentList.Find(com => com.Id == id);
                if (comment == null)
                {
                    ModelState.AddModelError("", "This is not found");
                    return View(new Comment());
                }
                postCommentList.Remove(comment);
                post.Comments = postCommentList;
                _db.Posts.Update(post);

                _db.SaveChanges();
                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View(new Comment());
                }

                #region NOTIFICATION
                var OwnerId = _db.Users.ToList().Find(user => user.Comments.ToList().Find(p => p.Id == comment.Id) != null).Id;
                var CommentOwner = await _userManager.FindByIdAsync(OwnerId);
                if(User.Identity.Name==CommentOwner.UserName) RedirectToAction("Details", "Post", new{ id = post.Id });
                var notList = CommentOwner.Notifications.ToList();
                var not = new Notification();
                not.Text = $"Twój komentarz pod podstem: {post.Title} o treści: {comment.Text} został usunięty przez moderację!";
                notList.Add(not);
                CommentOwner.Notifications = notList;
                await _userManager.UpdateAsync(CommentOwner);
                #endregion

                return RedirectToAction("Details", "Post", new{ id = post.Id });
            }
            catch
            {
                return View(new Comment());
            }
        }

        public async Task<ActionResult> Report(int id, int? idPost)
        {
            if(idPost==null) return RedirectToAction(nameof(Index));
            if (id <= 0) return RedirectToAction(nameof(Index));
            var post = _db.Posts.Find(idPost);
            if (post == null) return NotFound();
            var comment = post.Comments.ToList().Find(comm => comm.Id == id);
            var not = new Notification();
            not.Text = $"Komentarz pod postem: <a href=\"/Comment/Index/{post.Id}\">{post.Title}</a> o treści: {comment.Text} został zgłoszony!";
            foreach (var user in await _userManager.GetUsersInRoleAsync("mod"))
            {
                var notList = user.Notifications.ToList();
                notList.Add(not);
                user.Notifications = notList;
                await _userManager.UpdateAsync(user);
            }
            return RedirectToAction("Index","Comment",new { idPost = idPost });
        }

    }
}
