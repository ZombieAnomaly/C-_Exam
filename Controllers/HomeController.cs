using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using C_Exam.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using System.Web;

namespace C_Exam.Controllers
{
    public class HomeController : Controller
    {
        private ExamContext _context;

        public HomeController(ExamContext context){
            _context = context;
        }
        [HttpGet]
        [Route("")]
        public IActionResult Index()
        {   
            if(HttpContext.Session.GetString("email") != null){
                ViewData["name"] = HttpContext.Session.GetString("first_name");
                return RedirectToAction("Home");
            }
            return View();
        }

        [HttpPost]
        [Route("login")]
        public IActionResult login(string email, string password)
        {
            // Do something with form input
            Console.WriteLine("Login Attempted: " + email + " | " + password);
            Users ReturnedUser = _context.Users.Where(user => user.Email == email)
                                .Where(user => user.Password == password)
                                .SingleOrDefault();
            
            if(ReturnedUser != null){
                HttpContext.Session.SetString("email", ReturnedUser.Email);
                HttpContext.Session.SetString("first_name", ReturnedUser.FirstName);
                 HttpContext.Session.SetInt32("id", ReturnedUser.UserId);
                return RedirectToAction("Home");
            }

            ViewBag.LoginError = "No user exists with those details";
            return View("Index");
        }

        [HttpPost]
        [Route("register")]
        public IActionResult register(string first_name, string last_name, string email, string password, string passwordC)
        {
            // Do something with form input
            Console.WriteLine("Registration Attempted: " + first_name + " | " + last_name + " | " + email + " | " + password + " | " + passwordC);
            Users ReturnedUser = _context.Users.Where(user => user.Email == email)
                                .SingleOrDefault();
            
            var regexItem = new Regex("^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{8,}$");

            if(ReturnedUser != null){
                ViewBag.RegisterError = "User already Exists!";
                return View("Index");
            }
            else if(password != passwordC){
                ViewBag.RegisterError = "Passwords do not match!";
                return View("Index");
            }
            else if(first_name.All(char.IsDigit) || last_name.All(char.IsDigit)){
                ViewBag.RegisterError = "First & Last name must not contain any special characters or numbers!";
                return View("Index");               
            }
            else if(password.Length < 8 || passwordC.Length < 8){
                ViewBag.RegisterError = "Passwords is not long enough!";
                return View("Index");
            }
            else if(!regexItem.IsMatch(password)){
                 ViewBag.RegisterError = "Invalid Password!";
                return View("Index");               
            }
            

            //add user to db and render home page
            Users NewUser = new Users
            {
                FirstName = first_name,
                LastName = last_name,
                Email = email,
                Password = password,
            };
        
            _context.Add(NewUser);
            _context.SaveChanges();

            HttpContext.Session.SetString("email", email);
            HttpContext.Session.SetString("first_name", first_name);
            HttpContext.Session.SetInt32("id", NewUser.UserId);
            return RedirectToAction("Home");
        }

        [HttpGet]
        [Route("Home")]
        public IActionResult Home()
        {
            if(HttpContext.Session.GetString("email") == null){
                return RedirectToAction("Index");
            }
            ViewData["name"] = HttpContext.Session.GetString("first_name");
            ViewBag.myid = HttpContext.Session.GetInt32("id");
            List<Activities> ReturnedActivities = _context.Activities.Include(a => a.Owner).Include(a => a.Participants).ToList();
            ViewData["Activities"] = ReturnedActivities;
            return View();
        }

        [HttpGet]
        [Route("logout")]
        public IActionResult logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }

        [HttpGet]
        [Route("newActivity")]
        public IActionResult newActivity(){
            if(HttpContext.Session.GetString("email") == null){
                return RedirectToAction("Index");
            }
            return View("CreateActivity");
        }

        [HttpPost]
        [Route("createNewActivity")]
        public IActionResult createNewActivity(string title, DateTime time, int duration, string description, string durationUnit)
        {
            // Do something with form input
            Console.WriteLine("Create Activity Attempted: " + title + " | " + time + " | " + duration + " | " + description);
            Users ReturnedUser = _context.Users.Where(user => user.UserId == HttpContext.Session.GetInt32("id"))
                    .SingleOrDefault();
            if(title.Length < 2){
                ViewBag.RegisterError = "Title must be at least 2 characters";
                 return View("CreateActivity");
            }
            else if(time < DateTime.Now){
                ViewBag.RegisterError = "Date must be in the future!";
                 return View("CreateActivity");
            }
            else if(description.Length < 10){
                ViewBag.RegisterError = "Description must be at least 10 characters!";
                 return View("CreateActivity");
            }
            if(durationUnit == "hours"){
                duration *= 60;
            }else if(durationUnit == "seconds"){
                duration = duration/60;
            }
            //add activity to db and render activity page
            Activities NewActivity = new Activities
            {
                Title = title,
                Description = description,
                Time = time,
                Duration = duration,
                UsersId  = ReturnedUser.UserId,
                Owner = ReturnedUser
            };
    

            _context.Add(NewActivity);
            _context.SaveChanges();
            Participants NewParticipant = new Participants
            {
                UserId = ReturnedUser.UserId,
                ActivityId = NewActivity.ActivityId,
                User = ReturnedUser
            };

            _context.Add(NewParticipant);
            _context.SaveChanges();
            return RedirectToAction("ActivityDetails",new { ID = NewActivity.ActivityId });
        }

        [HttpGet]
        [Route("Activity/{ID}")]
        public IActionResult ActivityDetails(int ID){

            if(HttpContext.Session.GetString("email") == null){
                return RedirectToAction("Index");
            }

            Activities act = _context.Activities.Where(a => a.ActivityId == ID).Include(a => a.Owner).Include(a => a.Participants).ThenInclude(p => p.User).SingleOrDefault();
            List<Participants> parts = _context.Participants.Where(p => p.ActivityId == ID).ToList();
            ViewBag.myid = HttpContext.Session.GetInt32("id");
            ViewBag.ActivityDetails = act;
            ViewBag.Participants = parts;
            if(act == null)
                return RedirectToAction("Index");

            return View("Activity");
        }

        [HttpGet]
        [Route("leaveActivity/{ActId}/{UserId}")]
        public IActionResult leaveActivity(int UserId, int ActId){
            Activities act = _context.Activities.Where(a => a.ActivityId == ActId).Include(a => a.Owner).SingleOrDefault();    
            Participants p = _context.Participants.Where(pp => pp.UserId == UserId).Where(pp => pp.ActivityId == ActId).SingleOrDefault();
            act.Participants.Remove(p);
            _context.Participants.Remove(p);
            _context.SaveChanges();

            return RedirectToAction("ActivityDetails",new { ID = ActId });      
        }

        [HttpGet]
        [Route("deleteActivity/{ActId}/")]
        public IActionResult deleteActivity(int ActId){
            Activities act = _context.Activities.Where(a => a.ActivityId == ActId).Include(a => a.Owner).SingleOrDefault();    
            _context.Activities.Remove(act);
            _context.SaveChanges();

            return RedirectToAction("Home");      
        }
 
        [Route("joinActivity/{ID}")]
        public IActionResult joinActivity(int ID){
            if(HttpContext.Session.GetString("email") == null){
                return RedirectToAction("Index");
            }
            Activities act = _context.Activities.Where(a => a.ActivityId == ID).Include(a => a.Owner).SingleOrDefault();
            Users user = _context.Users.Where(u => u.UserId == HttpContext.Session.GetInt32("id")).SingleOrDefault();
            Participants p = _context.Participants.Where(pp => pp.UserId == user.UserId).Where(pp => pp.ActivityId == act.ActivityId ).SingleOrDefault();

            Participants NewParticipant = new Participants
            {
                UserId = user.UserId,
                ActivityId = act.ActivityId,
                User = user
            };
            act.Participants.Add(NewParticipant);
            _context.Add(NewParticipant);
            _context.SaveChanges();
            if(act == null)
                return RedirectToAction("Index");

            return RedirectToAction("ActivityDetails",new { ID = act.ActivityId });        
        }
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
