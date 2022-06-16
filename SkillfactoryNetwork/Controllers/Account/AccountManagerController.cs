using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using SkillfactoryNetwork.BLL.ViewModels.Account;
using SkillfactoryNetwork.DAL.Models.Users;
using Microsoft.AspNetCore.Authorization;
using SkillfactoryNetwork.Extentions;
using System.Collections.Generic;
using System.Linq;
using SkillfactoryNetwork.DAL.Repository;
using SkillfactoryNetwork.DAL.Models.UoW;
using SkillfactoryNetwork.DAL;
using Microsoft.EntityFrameworkCore;

namespace SkillfactoryNetwork.Controllers.Account
{
    public class AccountManagerController : Controller
    {
        private IMapper _mapper;

        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        private IUnitOfWork _unitOfWork;

        public AccountManagerController(IMapper mapper, UserManager<User> userManager, SignInManager<User> signInManager, IUnitOfWork unitOfWork)
        {
            _mapper = mapper;
            _userManager = userManager;
            _signInManager = signInManager;
            _unitOfWork = unitOfWork;
        }

        [Route("Login")]
        [HttpGet]
        public IActionResult Login()
        {
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [Route("Login")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {

                var user = _mapper.Map<User>(model);

                var result = await _signInManager.PasswordSignInAsync(user.Email, model.Password, model.RememberMe, false);
                if (result.Succeeded)
                {
                    return RedirectToAction("MyPage", "AccountManager");
                }
                else
                {
                    ModelState.AddModelError("", "Неправильный логин и (или) пароль");
                }
            }
            return RedirectToAction("Index", "Home");
        }

        [Route("MyPage")]
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> MyPage()
        {
            var user = User;
            var currentUser = await _userManager.GetUserAsync(user);

            var model = new UserViewModel(currentUser);
            
            model.Friends = await GetAllFriend(model.User);

            return View("User", model);
        }

        [Route("Logout")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        [Route("Update")]
        [HttpPost]
        public async Task<IActionResult> Update(UserEditViewModel model)
        {
            if (ModelState.IsValid)
            {

                var user = await _userManager.FindByIdAsync(model.UserId);

                user.Convert(model);

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    return RedirectToAction("MyPage", "AccountManager");
                }
                else 
                {
                    return RedirectToAction("Edit", "AccountManager");
                }
            }
            else
            {
                ModelState.AddModelError("", "Некорректные данные");
                return View("Edit", model);
            }
        }

        [Route("Edit")]
        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var user = User;

            var result =  await _userManager.GetUserAsync(user);

            var editmodel = _mapper.Map<UserEditViewModel>(result);

            return View("UserEdit", editmodel);
        }

        [Route("UserList")]
        [HttpGet]
        public async Task<IActionResult> UserList(string search)
        {
            var model = await CreateSearch(search);
            return View("UserList", model);
        }

        private async Task<SearchViewModel> CreateSearch(string search)
        {
            var currentUser = User;

            var currentUserDB = await _userManager.GetUserAsync(currentUser);

            var findUsers = await _userManager.Users.ToListAsync();

            if (!string.IsNullOrEmpty(search))
            {
                findUsers = findUsers.Where(user => user.GetFullName().ToLower().Contains(search.ToLower())).ToList();
            }
            
            var currentUserFrends = await GetAllFriend();

            var searchUserExtList = new List<UserWithFriendExt>();
            
            findUsers.ForEach(srchUser =>
            {
                var srchUserExt = _mapper.Map<UserWithFriendExt>(srchUser);
                srchUserExt.IsFriendWithCurrent = currentUserFrends.Where(friend => friend.Id == srchUser.Id || srchUser.Id == currentUserDB.Id).Count() != 0;
                searchUserExtList.Add(srchUserExt);
            });

            var model = new SearchViewModel()
            {
                UserList = searchUserExtList
            };

            return model;
        }

        private async Task<List<User>> GetAllFriend(User user)
        {
            var repository = _unitOfWork.GetRepository<Friend>() as FriendRepository;

            return await repository.GetFriendsByUser(user);
        }

        private async Task<List<User>> GetAllFriend()
        {
            var user = User;

            var result = await _userManager.GetUserAsync(user);

            var repository = _unitOfWork.GetRepository<Friend>() as FriendRepository;

            return await repository.GetFriendsByUser(result);
        }

        [Route("AddFriend")]
        [HttpPost]
        public async Task<IActionResult> AddFriend(string id)
        {
            var currentUser = User;

            var result = await _userManager.GetUserAsync(currentUser);

            var friend = await _userManager.FindByIdAsync(id);

            var repository = _unitOfWork.GetRepository<Friend>() as FriendRepository;

            await repository.AddFriend(result, friend);

            return RedirectToAction("MyPage", "AccountManager");
        }

        [Route("DeleteFriend")]
        [HttpPost]
        public async Task<IActionResult> DeleteFriend(string id)
        {
            var currentUser = User;

            var result = await _userManager.GetUserAsync(currentUser);

            var friend = await _userManager.FindByIdAsync(id);

            var repository = _unitOfWork.GetRepository<Friend>() as FriendRepository;

            await repository.DeleteFriend(result, friend);

            return RedirectToAction("MyPage", "AccountManager");
        }

        [Route("Chat")]
        [HttpPost]
        public async Task<IActionResult> Chat(string id)
        {
            var model = await GenerateChat(id);
            return View("Chat", model);
        }

        [Route("Chat")]
        [HttpGet]
        public async Task<IActionResult> Chat()
        {

            var id = Request.Query["id"];

            var model = await GenerateChat(id);
            return View("Chat", model);
        }

        private async Task<ChatViewModel> GenerateChat(string id)
        {
            var currentuser = User;

            var result = await _userManager.GetUserAsync(currentuser);
            var friend = await _userManager.FindByIdAsync(id);

            var repository = _unitOfWork.GetRepository<Message>() as MessageRepository;

            var mess = await repository.GetMessages(result, friend);

            var model = new ChatViewModel()
            {
                You = result,
                ToWhom = friend,
                History = mess.OrderBy(x => x.Id).ToList(),
            };

            return model;
        }

        [Route("NewMessage")]
        [HttpPost]
        public async Task<IActionResult> NewMessage(string id, ChatViewModel chat)
        {
            var currentUser = User;

            var currentUserDB = await _userManager.GetUserAsync(currentUser);
            var friend = await _userManager.FindByIdAsync(id);

            var repository = _unitOfWork.GetRepository<Message>() as MessageRepository;

            var item = new Message()
            {
                Sender = currentUserDB,
                Recipient = friend,
                Text = chat.NewMessage.Text,
            };
            await repository.Create(item);

            var mess = await repository.GetMessages(currentUserDB, friend);

            var model = new ChatViewModel()
            {
                You = currentUserDB,
                ToWhom = friend,
                History = mess.OrderBy(x => x.Id).ToList(),
            };

            return RedirectToAction("Chat", "AccountManager", new { id = friend.Id } );
        }


        [Route("Generate")]
        [HttpGet]
        public async Task<IActionResult> Generate()
        {

            var userGen = new GenetateUsers();
            var userlist = userGen.Populate(35);

            foreach (var user in userlist)
            {
                var result = await _userManager.CreateAsync(user, "123456");

                if (!result.Succeeded)
                    continue;
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
