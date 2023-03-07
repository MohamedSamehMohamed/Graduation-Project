using System.Collections.Generic;

using GraduationProject.ViewModels.ContestViewsModel;

namespace GraduationProject.Data.Repositories.Interfaces
{
    public interface IContestRepository<T>
    {
        IList<T> List();
        IList<T> PublicContests(); 
        T Add(T entity);
        void Update(T entity);
        void Remove(int id);
        void Commit();
        void AddProblemToContest(int problemId, int contestId);
        T CreateNewContest(int userId, T newContest);
        T Find(int id);
        void RegisterInContest(int userId, int contestId);
        void FlipFavourite(int contestId, int userId);
        bool IsOwner(int contestId, int userId); 
        IList<T> Filter(ContestFilter model);
        int Submit(int userId, int contestId, int problemId, string code, string language); 
    }
}
