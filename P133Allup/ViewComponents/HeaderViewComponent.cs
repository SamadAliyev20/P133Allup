﻿using Microsoft.AspNetCore.Mvc;
using P133Allup.ViewModels.HeaderViewComponentVM;

namespace P133Allup.ViewComponents
{
    public class HeaderViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(HeaderVM headerVM)
        {
            return View(await Task.FromResult(headerVM));
        }

    }
}
