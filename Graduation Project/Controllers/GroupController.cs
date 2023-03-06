using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraduationProject.Data.Repositories.Interfaces;
using GraduationProject.Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using GraduationProject.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using GraduationProject.ViewModels.GroupViewsModel;

namespace GraduationProject.Controllers.Group
{
    [Authorize]
    public class GroupController : Controller
    {
        private readonly IGroupRepository<GraduationProject.Data.Models.Group> groups;
        private readonly IUserRepository<User> users;
        private readonly User user;
        private readonly int NumberOfItemsForPage = 12; 
       

        public GroupController(IGroupRepository<GraduationProject.Data.Models.Group> groups, IUserRepository<User> Userrepository, IHttpContextAccessor httpContextAccessor)
        {
            var userId = httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value; 
            user = Userrepository.Find(userId);
            this.groups = groups;
            users = Userrepository; 
        }
        // GET: HomeController

        
        public ActionResult Index()
        {
            try
            {
                var list = getPageItems(getAllowedGroups(), 1);
                return View(list);
            }catch
            {
                return View("ErrorLink", "You Don't Have Access To Groups");
            }

        }
        public ActionResult Page(int PageNumber)
        {
            try
            {
                var list = getPageItems(getAllowedGroups(), PageNumber);
                return View("Index", list);
            }catch
            {
                return View("ErrorLink");
            }
        }
        public ActionResult MyGroups()
        {
            try
            {
                var list = getPageItems(getAllowedGroups(true), 1);
                return View("Index", list);
            }catch
            {
                return View("ErrorLink");
            }
        }

        // GET: HomeController/Details/5
        public ActionResult Details(int id)
        {
            try
            {
                // User Can See Group even if he is not a member of the group is private 
                var group = groups.Find(id);
                var model = getViewModelFromGroup(group);
                if (model != null)
                    return View(model);
                return RedirectToAction("Index");
            }catch
            {
                return View("ErrorLink", "Group Doesn't Exist");
            }
        }

        // GET: HomeController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: HomeController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateGroupModel model)
        {
            try
            {
                var newGroup = getGroupFromCreateModel(model);
                groups.CreateNewGroup(user.UserId, newGroup); 
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
        
        // GET: HomeController/Edit/5
        public ActionResult Edit(int id)
        {
            try
            {
                if (!groups.IsOwner(id, user.UserId))
                    return View("ErrorLink", "You Don't Have Access To Edit This Group");
                var group = groups.Find(id);
                var model = getEditModelFromGroup(group);
                return View(model);
            }
            catch
            {
                return RedirectToAction("Index");
            }
            
        }

        // POST: HomeController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EditGroupModel model)
        {
            try
            {
                if (!groups.IsOwner(model.groupId, user.UserId))
                    return View("ErrorLink", "You Don't Have Permission To Edit Tshis Group");
                var oldPassword = groups.Find(model.groupId).Password;
                if (oldPassword != model.oldPassword)
                    return View(model);
                var newGroup = getGroupFromEditModel(model);
                groups.Update(newGroup); 
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        private GraduationProject.Data.Models.Group getGroupFromEditModel(EditGroupModel model)
        {
            return new GraduationProject.Data.Models.Group
            {
                GroupId = model.groupId,
                GroupTitle = model.groupTitle,
                GroupDescription = model.groupDescription,
                Visible = model.visable == "1"? true: false,
                Password = model.newPassword
            };
        }
        private EditGroupModel getEditModelFromGroup(Data.Models.Group group)
        {
            return new EditGroupModel
            {
                groupId = group.GroupId,
                groupTitle = group.GroupTitle,
                groupDescription = group.GroupDescription,
                Visable = group.Visible,
                visable = group.Visible ? "1" : "0"
            };
        }
        // GET: HomeController/Delete/5
        public ActionResult Delete(int id)
        {
            try
            {
                if (!groups.IsOwner(id, user.UserId))
                    return View("ErrorLink", "You Don't Have Permission To Delete This Group");
                var group = groups.Find(id);
                var model = getCreateModelFromGroup(group);
                return Delete(model);
            }
            catch
            {
                return RedirectToAction("Index"); 
            }
            
        }

        // POST: HomeController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(CreateGroupModel model)
        {
            try
            {
                if (!groups.IsOwner(model.GroupId, user.UserId))
                    return View("ErrorLink", "You Don't Have Permission To Delete Tshis Group");
                groups.Remove(model.GroupId); 
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult InviteUsers(int groupId, string textareaUsers)
        {
            try
            {
                if (!groups.IsOwner(groupId, user.UserId))
                    return RedirectToAction("Details", new { groupId });
                var allNames = textareaUsers.Split(" ");
                foreach(var name in allNames)
                {
                    var currentUser = users.FindByUserName(name); 
                    if(currentUser != null)
                    {
                        groups.InviteUser(groupId, currentUser.UserId); 
                    }
                }
                return RedirectToAction("Details", new { id = groupId});
            }
            catch
            {
                return RedirectToAction("Details", new { id = groupId });
            }
        }

        
        public ActionResult Leave(int id)
        {
            int userId = user.UserId;
            return Leave(userId, id);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Leave(int userId, int groupId)
        {
            try
            {
                if (user.UserId != userId)
                    return View("ErrorLink", "You Don't Have Permission To Delete a user");
                groups.RemoveUser(userId, groupId);
                return RedirectToAction("Index");
            }
            catch
            {
                return RedirectToAction("Details", new { id = groupId });
            }
        }
        public ActionResult FlipFavourite(int id)
        {
            return FlipFavourite(user.UserId, id);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult FlipFavourite(int userId, int groupId)
        {
            try
            {
                groups.FlipFavourite(groupId, userId);
                return RedirectToAction("Details", new { id = groupId });
            }
            catch
            {
                return RedirectToAction("Details", new { id = groupId });
            }
        }
        public ActionResult JoinToGroup(int Id)
        {
            return JoinToGroup(Id, user.UserId); 
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult JoinToGroup(int groupId, int userId)
        {
            // if the group is public or there is an invitation 
            try
            {
                var group = groups.Find(groupId); 
                if (group.Visible || group.UserGroup.FirstOrDefault(u=>u.UserId == userId) != null)
                    groups.AddUser(groupId, userId);  
                else
                    return View("ErrorLink", "You Can't Join This Group");
                return RedirectToAction("Details", new { id = groupId });
            }
            catch
            {
                return RedirectToAction("Details", new { id = groupId });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult JoinToGroup(int groupId, int userId, string Password)
        {
            try
            {
                if (groups.Find(groupId).Password == Password)
                    groups.AddUser(groupId, userId);
                return RedirectToAction("Details", new { id = groupId });
            }
            catch
            {
                return RedirectToAction("Details", new { id = groupId });
            }
        }
        public ActionResult AcceptInvitation(int Id)
        {
            int userId = user.UserId;
            return JoinToGroup(Id, userId);
        }
        public ActionResult RejectInvitation(int Id)
        {
            int userId = user.UserId;
            groups.RemoveUser(userId, Id);
            return RedirectToAction("Invitations"); 
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditMember(int groupId, int userId, string buttonName)
        {
            if (!groups.IsOwner(groupId, user.UserId))
                return View("ErrorLink", "You Can't Edit User");
            try
            {
                switch (buttonName)
                {
                    case "0":
                        groups.ChangeUserRole(groupId, userId, "Manager");
                        break;
                    case "-1":
                        groups.ChangeUserRole(groupId, userId, "Participant");
                        break;
                    case "delete":
                        groups.RemoveUser(userId, groupId); 
                        break;
                }
                return RedirectToAction("Details", new { id = groupId });
            }
            catch
            {
                return RedirectToAction("Details", new { id = groupId });
            }
         }
        public ActionResult Invitations()
        {
            try
            {
                int userId = user.UserId;
                var list = new List<ViewGroupModel>();
                foreach (var item in groups.List())
                {
                    var rel = item.UserGroup.FirstOrDefault(u => u.UserId == userId);
                    if (rel != null && rel.UserRole == "Invite")
                    {
                        var newItem = getViewModelFromGroup(item);
                        list.Add(newItem);
                    }

                }
                list = getPageItems(list, 1);
                return View("Index", list);
            }catch
            {
                return View("Index"); 
            }
        }

        private GraduationProject.Data.Models.Group getGroupFromCreateModel(CreateGroupModel model)
        {
            var newGroup = new GraduationProject.Data.Models.Group { 
                GroupId = model.GroupId, 
                GroupTitle = model.GroupTitle, 
                GroupDescription = model.GroupDescription, 
                Password = model.Password, 
                Visible = model.visable == "1"? true: false, 
            };
            return newGroup; 
        }
        private CreateGroupModel getCreateModelFromGroup(GraduationProject.Data.Models.Group group)
        {

            var tem = group.UserGroup.FirstOrDefault(u => u.UserRole == "Creator");
            int ID = tem == null? 0: tem.UserId; 
            var model = new CreateGroupModel { 
                GroupId = group.GroupId, 
                GroupTitle = group.GroupTitle, 
                GroupDescription = group.GroupDescription, 
                Password = group.Password, 
                Visable = group.Visible,
                visable = group.Visible? "1": "0",
                OwnerId = ID
            };
            return model;
        }
        private ViewGroupModel getViewModelFromGroup(GraduationProject.Data.Models.Group group)
        {
            try
            {
                int NumberOfMembers = group.UserGroup.Where(u=>u.UserRole != "Invite").Count();
                var query = group.UserGroup.FirstOrDefault(u => u.UserId == user.UserId);
                var IsFavourite = query != null ? query.IsFavourite : false;
                var role = query != null ? query.UserRole : "Not In Group";
                return new ViewGroupModel
                {
                    UserRole = role,
                    GroupId = group.GroupId,
                    GroupTitle = group.GroupTitle,
                    GroupDescription = group.GroupDescription,
                    NumberOfMembers = NumberOfMembers,
                    GroupStatus = group.Visible ? "Public" : "Private",
                    creationTime = group.CreationTime,
                    UserGroup = group.UserGroup,
                    Contests = group.Contests.ToList(),
                    Blogs = group.Blogs.ToList(),
                    IsFavourite = IsFavourite,
                    CurrentUserId = user.UserId,
                    
                };
            }
            catch
            {
                return null;
            }
        }
        private List<T> getPageItems<T>(List<T> list, int PageNumber)
        {
            if (PageNumber < 1 || PageNumber > (list.Count + NumberOfItemsForPage - 1) / NumberOfItemsForPage)
                PageNumber = 1;
            ViewBag.NumberOfPages = (list.Count+NumberOfItemsForPage-1)/NumberOfItemsForPage;
            ViewBag.PageNumber = PageNumber; 
            int startIndex = (PageNumber - 1) * NumberOfItemsForPage;
            int endIndex = PageNumber * NumberOfItemsForPage;
            if (list.Count > endIndex)
                list.RemoveRange(endIndex, list.Count - endIndex);
            if (list.Count > NumberOfItemsForPage)
                list.RemoveRange(0, list.Count - NumberOfItemsForPage);
            return list; 
        }
        private List<ViewGroupModel> getAllowedGroups(Boolean Mine = false)
        {
            var list = new List<ViewGroupModel>();
            int userId = user.UserId;
            int NumberOfGroupInvitations = 0;
            var Groups = Mine ? groups.MyGroups(userId) : groups.List(); 
            foreach (var item in Groups)
            {
                var rel = item.UserGroup.FirstOrDefault(u => u.UserId == userId);
                if ((item.Visible == true) || (rel != null))
                {
                    var newItem = getViewModelFromGroup(item);
                    if (newItem != null)
                        list.Add(newItem);
                }
                if (rel != null && rel.UserRole == "Invite")
                    NumberOfGroupInvitations++;
            }
            ViewBag.NumberOfGroupInvitations = NumberOfGroupInvitations;
            return list; 
        }
    }
}
