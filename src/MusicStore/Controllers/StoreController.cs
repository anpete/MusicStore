using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MusicStore.Models;
#pragma warning disable 1998

namespace MusicStore.Controllers
{
    public class StoreController : Controller
    {
        public StoreController(MusicStoreContext dbContext)
        {
            DbContext = dbContext;
        }

        public MusicStoreContext DbContext { get; }

        //
        // GET: /Store/
        public async Task<IActionResult> Index()
        {
            //var genres = await DbContext.Genres.ToListAsync();
            var genres = new List<Genre>();

            return View(genres);
        }

        //
        // GET: /Store/Browse?genre=Disco
        public async Task<IActionResult> Browse(string genre)
        {
            // Retrieve Genre genre and its Associated associated Albums albums from database
//            var genreModel = await DbContext.Genres
//                .Include(g => g.Albums)
//                .Where(g => g.Name == genre)
//                .FirstOrDefaultAsync();

            var genreModel = new Genre();

            if (genreModel == null)
            {
                return NotFound();
            }

            return View(genreModel);
        }

        public async Task<IActionResult> Details(
            [FromServices] IMemoryCache cache,
            int id)
        {
            var cacheKey = string.Format("album_{0}", id);

            Album album;
            if (!cache.TryGetValue(cacheKey, out album))
            {
//                album = await DbContext.Albums
//                                .Where(a => a.AlbumId == id)
//                                .Include(a => a.Artist)
//                                .Include(a => a.Genre)
//                                .FirstOrDefaultAsync();

                album = new Album() {Artist = new Artist(), Genre = new Genre()} ;

                if (album != null)
                {
                    //Remove it from cache if not retrieved in last 10 minutes
                    cache.Set(
                        cacheKey,
                        album,
                        new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(10)));
                }
            }

            if (album == null)
            {
                return NotFound();
            }

            return View(album);
        }
    }
}