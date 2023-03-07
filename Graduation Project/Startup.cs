using GraduationProject.Data;
using GraduationProject.Data.Models;
using GraduationProject.Data.Repositories;
using GraduationProject.Data.Repositories.DataBaseRepositories;
using GraduationProject.Data.Repositories.Interfaces;
using GraduationProject.Data.Repositories.DataBaseRepositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraduationProject
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }



        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddDbContext<EntitiesContext>(
               options =>
               {
                   options.UseSqlServer(Configuration.GetConnectionString("UserAccountsContextConnection"));
               });
            services.AddScoped<IGroupRepository<Group>, GroupDbRepository> ();

            services.AddScoped<ISubmissionRepository<Submission>, SubmissionDbRepository> ();
            services.AddScoped<IUserRepository<User>, UserDbRepository>(); 
            services.AddScoped<IBlogRepository<Blog>, BlogDbRepository>();
            services.AddScoped<IRepository<Comment>, CommentDbRepository>();
            services.AddScoped<IProblemRepository<Problem>, ProblemDbRepository > ();
            services.AddScoped<IContestRepository<Contest>, ContestDbRepository>(); 
          
            services.AddMvc(options => options.EnableEndpointRouting = false);
            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseMvc(route => {
                route.MapRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });
            
        }
    }
}
