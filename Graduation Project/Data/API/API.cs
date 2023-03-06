using System;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace GraduationProject.Data.API;

public static class APi
{
    private static ProblemIfo GetCodeForcesProblem(string contestId, string problemIndex)
    {
        var url = "http://95.216.185.187/cf/" + contestId + "/" + problemIndex;
        var json = GetPageContent(url);
        var problemInfo = JsonConvert.DeserializeObject<ProblemIfo>(json);
        return problemInfo.Problem != null? problemInfo: null;
    }
    public static ProblemIfo GetProblem(string onlineJudge, string contestId, string problemIndex)
    {
        return onlineJudge == "CodeForces" ? GetCodeForcesProblem(contestId, problemIndex) : null;
    }

    

    private static string GetPageContent(string url)
    {
        var content = "";
        using var client = new WebClient();
        try
        {
            content = client.DownloadString(url);
        }
        catch (Exception)
        {
            return null;
        }
        return content;
    }
    public static SubmitInfo GetVerdict(string problemId, string solution, string language)
    {
        const string url = "http://95.216.185.187/SUBMIT";
        
        using var client = new WebClient();

        var plainTextBytes = Encoding.UTF8.GetBytes(solution);
        var solutionInBase64= Convert.ToBase64String(plainTextBytes);
        client.QueryString.Add("ProblemId", problemId);
        client.QueryString.Add("SubmitText", solutionInBase64);
        client.QueryString.Add("Language", language);

        var data = client.UploadValues(url, "POST", client.QueryString);

        var responseString = Encoding.Default.GetString(data);

        SubmitInfo submitInfo = null;
        try
        {
            submitInfo = System.Text.Json.JsonSerializer.Deserialize<SubmitInfo>(responseString);
        }
        catch
        {
            return null;
        }

        return submitInfo;
    }
    public static void GetVerdict(string problemId, string solution, string language, int submissionId)
    {
        const string url = "http://95.216.185.187/SUBMIT";
        using var client = new WebClient();

        var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(solution);
        var x = System.Convert.ToBase64String(plainTextBytes);
        client.QueryString.Add("ProblemId", problemId);
        client.QueryString.Add("SubmitText", x);
        client.QueryString.Add("Language", "GNU G++17 7.3.0");
        client.QueryString.Add("SubmissionId", submissionId.ToString()); 

        client.UploadValuesAsync(new Uri(url), "POST", client.QueryString);
    }
}