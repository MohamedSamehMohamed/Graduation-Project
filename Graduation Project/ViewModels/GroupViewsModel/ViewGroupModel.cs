using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraduationProject.Data.Models;
using GraduationProject.ViewModels.GroupViewsModel;

namespace GraduationProject.ViewModels
{

    public class ViewGroupModel
    {
        public ViewGroupModel()
        {
            Contests = new HashSet<Contest>();
            UserGroup = new HashSet<UserGroup>();
            Blogs = new HashSet<Blog>();
        }
        
        public int GroupId { get; set; }
        public string GroupTitle { get; set; }
        public string GroupDescription { get; set; }
        public int NumberOfMembers { get; set; }
        
        public DateTime creationTime { get; set; }
        public int NumberOfUpCommingContests {
            get {
                return Contests.Where(c => c.ContestStatus == -1).Count(); 
            }
            set { }
        }
        public int NumberOfRunningContests { 
            get {
                return Contests.Where(c => c.ContestStatus == 0).Count();
            } 
            set { } 
        }
        public int NumberOfEndedContests { get {
                return Contests.Where(c => c.ContestStatus == 1).Count();
            } 
            set { } 
        }
        public string GroupStatus { get; set; } // Public / Private 
        public int CurrentUserId { get; set; }
        public string UserRole { get; set; } // current user, who open the group
        public Boolean IsFavourite { get; set; } // for the current User 
        public ICollection<Contest> Contests { get; set; }
        public ICollection<UserGroup> UserGroup { get; set; }
        public ICollection<Blog> Blogs { get; set; }
        
        public string UserStatus { get 
            {
                if (UserRole == "Creator")
                    return "Owner";
                return ""; 
            } set { } }
        public int TopUserId { get {
                int mx = UserGroup.Select(u => u.User.TotalNumberOfSolvedProblems).Max();
                return UserGroup.FirstOrDefault(u => u.User.TotalNumberOfSolvedProblems == mx).UserId;
            } set { } }
        public string TopUserName { get {
                int mx = UserGroup.Select(u => u.User.TotalNumberOfSolvedProblems).Max();
                return UserGroup.FirstOrDefault(u => u.User.TotalNumberOfSolvedProblems == mx).User.FirstName;
            } set { } }



    }
}
