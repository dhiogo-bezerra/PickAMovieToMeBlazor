using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace MozifAppUI.Shared
{
    public partial class MovieLists
    {
        [Parameter]
        public string? imdbId { get; set; }

        private List<MovieListModel>? filteredLists;
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                if (!string.IsNullOrEmpty(imdbId))
                {
                    await LoadListWithMovie(imdbId);

                }
            }
        }

        public async Task LoadListWithMovie(string imdbId)
        {
            var movieDb = await movieData.GetMovieByImdbId(imdbId);
            if (movieDb is not null)
            {
                var listsOfMovie = movieDb.MemberOf;
                var allLists = await movieListData.GetAllLists();
                filteredLists = allLists.Where(f => listsOfMovie.Any(y => y.Id == f.Id)).ToList();
                StateHasChanged();
            }
            else
            {
                filteredLists = null;
                StateHasChanged();
            }
        }
    }
}