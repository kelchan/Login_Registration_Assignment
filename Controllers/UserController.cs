// Using statements
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Login_Registration_Assignment.Models;
using Microsoft.AspNetCore.Identity;

namespace Login_Registration_Assignment.Controllers;

public class UserController : Controller
{
    private int? uid
    {
        get
        {
            return HttpContext.Session.GetInt32("UUID");
        }
    }

    private bool loggedIn
    {
        get
        {
            return uid != null;
        }
    }

    private LoginRegistrationContext db;
    
    // here we can "inject" our context service into the constructor
    public UserController(LoginRegistrationContext context)
    {
        db = context;
    }

    [ HttpGet("") ] 
    public IActionResult Index()
    {
        return View( "Register" );
    }

    [ HttpGet("/viewLogin") ] 
    public IActionResult Login()
    {
        return View( "Login" );
    }

    [HttpPost("/register")]
    public IActionResult Register(User newUser)
    {
        if (ModelState.IsValid)
        {
            if (db.Users.Any(user => user.Email == newUser.Email))
            {
                ModelState.AddModelError("Email", "is taken");
            }
        }

        if (ModelState.IsValid == false)
        {
            return Index();
        }

        // now we hash our passwords
        PasswordHasher<User> hashBrowns = new PasswordHasher<User>();
        newUser.Password = hashBrowns.HashPassword(newUser, newUser.Password);

        db.Users.Add(newUser);
        db.SaveChanges();

        // now that we've run SaveChanges() we have access to the UserId from our SQL db
        HttpContext.Session.SetInt32("UUID", newUser.UserId);
        return ViewSuccess();

    }

    [HttpPost("/login")]
    public IActionResult ProcessLogin(LoginUser loginUser)
    {
        if (ModelState.IsValid == false)
        {
            return Index();
        }

        User? dbUser = db.Users.FirstOrDefault(user => user.Email == loginUser.LoginEmail);

        if (dbUser == null)
        {
            // normally login validations should be more generic to avoid phishing
            // but we're using specific error messages for testing
            ModelState.AddModelError("LoginEmail", "not found");
            return Index();
        }

        PasswordHasher<LoginUser> hashBrowns = new PasswordHasher<LoginUser>();
        PasswordVerificationResult pwCompareResult = hashBrowns.VerifyHashedPassword(loginUser, dbUser.Password, loginUser.LoginPassword);

        if (pwCompareResult == 0)
        {
            ModelState.AddModelError("LoginPassword", "is not correct");
            return Index();
        }

        // no returns, therefore no errors
        HttpContext.Session.SetInt32("UUID", dbUser.UserId);

        return ViewSuccess();
    }

    [ HttpPost( "/success" ) ]
    public IActionResult ViewSuccess()
    {
        if(!loggedIn)
        {
            return Index();
        }
        
        return View( "Success" );
    }

    [HttpGet("/logout")]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return Index();
    }

}