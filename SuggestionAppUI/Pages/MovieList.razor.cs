using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using System.Net.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.JSInterop;
using SuggestionAppUI;
using SuggestionAppUI.Shared;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace SuggestionAppUI.Pages
{
    public partial class MovieList
    {
        //Control if the loading div is displayed
        bool isLoading = true;
        private List<MovieDbModel> _movieList;
        private List<MovieListModel> _lists;
        private List<GenreModel> _genres;


        private int _page = 0;
        private int _totalPages = 0;
        private int _itemPerPage = 5;
        private APIConfiguration _apiConfiguration;
        private int _movieCount = 0;
        private int pagesBefore = 0;
        private int pagesAfter = 0;
        private string FilterTitle { get; set; } = "";
        private string FilterDirector { get; set; } = "";
        private string FilterFromYear { get; set; } = "";
        private string FilterToYear { get; set; } = "";
        private string? filterRange { get; set; } = "";
        private string[] selectedGenre { get; set; } = new string[]{};
        private string[] selectLists { get; set; } = new string[] { };

        //Storage itens
        private string storageTitle = "";

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                isLoading = true;

                //Loading Js multiselect ui dropdown
                await jsRunTime.InvokeVoidAsync("MultiselectInit");

                _apiConfiguration = await apiMovie.GetConfiguration();
                _movieList = await movieDbData.GetAllMovies();
                _genres = await genreData.GetAllGenre();
                _genres = _genres.OrderBy(f => f.Name).ToList();
                _lists = await movieListData.GetAllLists();
                _movieCount = _movieList.Count();

                await LoadFilterState();
                UpdatePage();
                isLoading = false;               
                StateHasChanged();

                await jsRunTime.InvokeAsync<IJSObjectReference>("import", "/js/custom.js");
            }
        }

        private async Task LoadFilterState()
        {
            var stringResults = await sessionStorage.GetAsync<string>(nameof(FilterTitle));
            FilterTitle = stringResults.Success ? stringResults.Value : "All";
        }

        private async Task SaveFilterState()
        {
            await sessionStorage.SetAsync(nameof(FilterTitle), FilterTitle);

        }

        protected void UpdatePage()
        {
            _totalPages = (_movieCount / _itemPerPage);
            //if has fractional remainder, add 1
            if (_movieCount % _itemPerPage > 0)
            {
                _totalPages++;
            }

            pagesBefore = _page - 2;
            pagesAfter = _page + 3;
            if (pagesBefore < 1)
            {
                pagesBefore = 1;
            }

            if (pagesAfter > _totalPages)
            {
                pagesAfter = _totalPages;
            }
        }

        public void LoadMovie(string MovieId)
        {
            navManager.NavigateTo("/Movie/" + MovieId);
        }

        protected  async Task NavigatePage(int page)
        {
            _page = --page;
            UpdatePage();
            StateHasChanged();
            await jsRunTime.InvokeVoidAsync("OnScrollEvent");
        }

        protected void UpdateMoviePerPage(ChangeEventArgs e)
        {
            if (e.Value is not null)
            {
                _itemPerPage = int.Parse(e.Value.ToString());
                UpdatePage();
                StateHasChanged();
            }
        }

        protected void ResetPagination()
        {
            _page = 0;
            _movieCount = _movieList.Count();
            UpdatePage();
        }

        protected async void FilterSubmit()
        {
            isLoading = true;

            var output = await movieDbData.GetAllMovies();

            //Filter by Title.
            if (FilterTitle.Length > 3)
            {
                output = output.Where(f => f.Title.Contains(FilterTitle, StringComparison.InvariantCultureIgnoreCase)).ToList();
            }

            //Filter by Director.
            if (FilterDirector.Length > 3)
            {
                output = output.Where(f => f.Directors.Contains(FilterDirector, StringComparison.InvariantCultureIgnoreCase)).ToList();
            }

            //Filter by Year.
            if (FilterFromYear.Length > 3 && FilterToYear.Length > 3)
            {
                output = output.Where(f => f.Year >= Convert.ToInt16(FilterFromYear) && f.Year <= Convert.ToInt16(FilterToYear)).ToList();
            }

            //Filter by Genre.
            if (selectedGenre.Count() > 0)
            {
                output = (output.Where(f => f.Genres.Split(',').ToList<string>().Any(x => selectedGenre.Any(y => x.Contains(y)))).ToList());
            }

            //Filter by List.
            if (selectLists.Count() > 0)
            {
                output = output.Where(f => f.MemberOf.Any(x => selectLists.Any(y => x.Id == y ))).ToList();
            }

            //Filter by Range
            if (!string.IsNullOrEmpty(filterRange))
            {
                output = filterRange switch
                {
                    "0to3" => output.Where(f => f.ImDbRating <= Convert.ToDouble(3)).ToList(),
                    "3to6" => output.Where(f => f.ImDbRating > Convert.ToDouble(3) && f.ImDbRating <= Convert.ToDouble(6)).ToList(),
                    "6to9" => output.Where(f => f.ImDbRating > Convert.ToDouble(6) && f.ImDbRating <= Convert.ToDouble(9)).ToList(),
                    "9to10" => output.Where(f => f.ImDbRating > Convert.ToDouble(9)).ToList(),
                    _ => output,
                };
            }

            await SaveFilterState();

            _movieList = output;
            isLoading = false;
            ResetPagination();
        }

        protected async void FilterByDirectorName(string director)
        {
            var output = await movieDbData.GetAllMovies();
            output = output.Where(f => f.Directors.Contains(director, StringComparison.InvariantCultureIgnoreCase)).ToList();
            FilterDirector = director;
            ResetPagination();
        }

        protected async void Reset()
        {
            FilterTitle = "";
            FilterDirector = "";
            FilterFromYear = "";
            FilterToYear = "";
            selectLists  = new string[] { };
            selectedGenre   = new string[] { };
            _movieList = await movieDbData.GetAllMovies();


            ResetPagination();
            //Reset select dropdown ui with JS
            await jsRunTime.InvokeVoidAsync("clearDropdownui");
        }

        protected void SortMovieList(ChangeEventArgs e)
        {
            isLoading = true;
            var output = e.Value.ToString() switch
            {
                "ratingDsc" => _movieList.OrderByDescending(f => f.ImDbRating).ToList(),
                "ratingAsc" => _movieList.OrderBy(f => f.ImDbRating).ToList(),
                "dateDsc" => _movieList.OrderByDescending(f => f.ReleaseDate).ToList(),
                "dateAsc" => _movieList.OrderBy(f => f.ReleaseDate).ToList(),
                "listTotalDesc" => _movieList.OrderByDescending(f => f.MemberOf.Count).ToList(),
                "listTotalAsc" => _movieList.OrderBy(f => f.MemberOf.Count).ToList(),
                _ => _movieList,
            };
            _movieList = output;
            ResetPagination();
            isLoading = false;
        }

        protected void SelectedListChange(ChangeEventArgs e)
        {
            selectLists = (string[])e.Value;
        }

        protected void SelectedGenreChange(ChangeEventArgs e)
        {
            selectedGenre = (string[])e.Value;
        }


        private async Task YearInitInput(string searchInput)
        {
            FilterFromYear = searchInput;


            if (FilterFromYear.Length < 4)
            {
                return;
            }

            Int16 toYear = 0;
            Int16 fromYear = 0;

            if (!string.IsNullOrWhiteSpace(FilterToYear) && Int16.TryParse(FilterToYear, out toYear) && Int16.TryParse(FilterFromYear, out fromYear))
            {
                FilterToYear = FilterToYear;
            }
        }
    }
}