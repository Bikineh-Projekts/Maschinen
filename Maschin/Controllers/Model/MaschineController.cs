using Microsoft.AspNetCore.Mvc;
using MaschinenDataein.Models.Data;
using System;
using System.Linq;

namespace MaschinenDataein.Controllers
{
    public class MaschineController : Controller
    {
        private readonly Models.MaschinenDbContext _context;

        public MaschineController(Models.MaschinenDbContext context)
        {
            _context = context;
        }
    }
}
