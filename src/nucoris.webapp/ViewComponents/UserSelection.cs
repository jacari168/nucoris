using Microsoft.AspNetCore.Mvc;
using nucoris.application.interfaces.repositories;
using System.Linq;
using System.Threading.Tasks;

namespace nucoris.webapp.ViewComponents
{
    public class UserSelectionViewComponent : ViewComponent
    {
        private readonly IUserRepository _userRepository;

        public UserSelectionViewComponent(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Get all users (in the final application we would likely filter by active ones only with a certain profile)
            var users = await _userRepository.GetManyAsync(whereConditions: null);

            // We know this component will show a dropdown (HTML select element), 
            //  which in Razor Pages are implemented with a list of SelectListItem
            //  (For further info see https://www.learnrazorpages.com/razor-pages/forms/select-lists)
            var selectItems = users.Select(user => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                        {
                            Value = user.Id.ToString(),
                            Text = domain.NameUtilities.BuildDisplayName(user.GivenName, user.FamilyName)
                        }).OrderBy(i => i.Text).ToList();

            return View(selectItems);
        }
    }
}
